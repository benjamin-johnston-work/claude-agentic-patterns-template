#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Starts the Archie DeepWiki development environment with proper process management
.DESCRIPTION
    Starts the API service with log streaming and process tracking. Validates Azure services configuration.
.PARAMETER NoLogs
    Start services without streaming logs to console
.PARAMETER ApiOnly
    Legacy parameter - kept for compatibility (no longer used)
.EXAMPLE
    .\Start-ArchieDevEnvironment.ps1
.EXAMPLE
    .\Start-ArchieDevEnvironment.ps1 -NoLogs
#>

[CmdletBinding()]
param(
    [switch]$NoLogs,
    [switch]$ApiOnly
)

$script:LogPath = "$PSScriptRoot\logs\start-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$script:ProcessFile = "$PSScriptRoot\logs\.archie-processes.json"

function Write-LoggedOutput {
    param(
        [string]$Message,
        [ValidateSet("Info", "Warning", "Error", "Success")]
        [string]$Type = "Info"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Type] $Message"
    
    switch ($Type) {
        "Info" { Write-Host $logMessage -ForegroundColor Cyan }
        "Warning" { Write-Host $logMessage -ForegroundColor Yellow }
        "Error" { Write-Host $logMessage -ForegroundColor Red }
        "Success" { Write-Host $logMessage -ForegroundColor Green }
    }
    
    if (-not (Test-Path (Split-Path $script:LogPath))) {
        New-Item -ItemType Directory -Path (Split-Path $script:LogPath) -Force | Out-Null
    }
    Add-Content -Path $script:LogPath -Value $logMessage
}

function Save-ProcessInfo {
    param($ProcessInfo)
    
    if (-not (Test-Path (Split-Path $script:ProcessFile))) {
        New-Item -ItemType Directory -Path (Split-Path $script:ProcessFile) -Force | Out-Null
    }
    
    $ProcessInfo | ConvertTo-Json -Depth 3 | Set-Content -Path $script:ProcessFile
}

function Get-SavedProcesses {
    if (Test-Path $script:ProcessFile) {
        try {
            $content = Get-Content $script:ProcessFile | ConvertFrom-Json
            return [PSCustomObject]$content
        } catch {
            return [PSCustomObject]@{}
        }
    }
    return [PSCustomObject]@{}
}

function Test-Prerequisites {
    Write-LoggedOutput "Checking prerequisites..." -Type Info
    
    # Check .NET SDK
    try {
        $dotnetVersion = & dotnet --version 2>$null
        if (-not $dotnetVersion -or -not $dotnetVersion.StartsWith("9.")) {
            throw ".NET 9.0 SDK not found or incorrect version: $dotnetVersion"
        }
        Write-LoggedOutput "[OK] .NET 9.0 SDK: $dotnetVersion" -Type Success
    } catch {
        throw "Prerequisites check failed: $($_.Exception.Message)"
    }
    
    # Check Node.js
    try {
        $nodeVersion = & node --version 2>$null
        if (-not $nodeVersion) {
            throw "Node.js not found"
        }
        Write-LoggedOutput "[OK] Node.js: $nodeVersion" -Type Success
    } catch {
        throw "Node.js not found - required for frontend development"
    }
    
    # Validate Azure services configuration
    $apiProjectPath = "$PSScriptRoot\..\src\Archie.Api"
    $developmentConfig = "$apiProjectPath\appsettings.Development.json"
    
    if (-not (Test-Path $developmentConfig)) {
        throw "Configuration file not found: $developmentConfig"
    }
    
    try {
        $config = Get-Content $developmentConfig -Raw | ConvertFrom-Json
        
        if (-not $config.AzureSearch -or -not $config.AzureSearch.ServiceUrl) {
            throw "Azure AI Search configuration missing or invalid"
        }
        Write-LoggedOutput "[OK] Azure AI Search configuration found" -Type Success
        
        if (-not $config.AzureOpenAI -or -not $config.AzureOpenAI.Endpoint) {
            throw "Azure OpenAI configuration missing or invalid"
        }
        Write-LoggedOutput "[OK] Azure OpenAI configuration found" -Type Success
        
        if (-not $config.GitHub) {
            Write-LoggedOutput "[WARNING] GitHub configuration missing - rate limits may apply" -Type Warning
        } else {
            Write-LoggedOutput "[OK] GitHub configuration found" -Type Success
        }
        
    } catch {
        throw "Configuration validation failed: $($_.Exception.Message)"
    }
}

function Start-ApiService {
    Write-LoggedOutput "Starting Archie API service..." -Type Info
    
    $projectPath = "$PSScriptRoot\..\src\Archie.Api"
    
    if (-not (Test-Path "$projectPath\Archie.Api.csproj")) {
        throw "API project not found at $projectPath"
    }
    
    # Kill any existing dotnet processes for this project
    $existingProcesses = Get-Process -Name dotnet -ErrorAction SilentlyContinue | Where-Object {
        $_.CommandLine -and $_.CommandLine.Contains("Archie.Api")
    }
    
    foreach ($proc in $existingProcesses) {
        Write-LoggedOutput "Stopping existing API process (PID: $($proc.Id))" -Type Warning
        $proc.Kill()
        $proc.WaitForExit(5000)
    }
    
    # Ensure fresh build before starting API service
    Write-LoggedOutput "Building API project to ensure latest changes..." -Type Info
    Push-Location $projectPath
    try {
        $buildProcess = Start-Process -FilePath "dotnet" -ArgumentList "build", "--configuration", "Development" -Wait -PassThru -WindowStyle Hidden
        
        if ($buildProcess.ExitCode -ne 0) {
            throw "API build failed with exit code $($buildProcess.ExitCode)"
        }
        Write-LoggedOutput "API project built successfully" -Type Success
        
        # Start the API service
        $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--configuration", "Development" -PassThru -WindowStyle Hidden
        
        if ($apiProcess) {
            Write-LoggedOutput "API service started (PID: $($apiProcess.Id))" -Type Success
            
            # Save process information
            $processInfo = Get-SavedProcesses
            $processInfo | Add-Member -MemberType NoteProperty -Name "Api" -Value @{
                ProcessId = $apiProcess.Id
                StartTime = Get-Date
                ProjectPath = $projectPath
            } -Force
            Save-ProcessInfo $processInfo
            
            # Wait for API to be fully ready
            Write-LoggedOutput "Waiting for API to be ready..." -Type Info
            $timeout = 60
            $elapsed = 0
            $apiReady = $false
            
            do {
                Start-Sleep -Seconds 2
                $elapsed += 2
                
                # Check if process is still running
                if ($apiProcess.HasExited) {
                    throw "API service failed to start (exit code: $($apiProcess.ExitCode))"
                }
                
                # Test API endpoint
                try {
                    $response = Invoke-WebRequest -Uri "https://localhost:7001/graphql" -Method GET -SkipCertificateCheck -TimeoutSec 5 2>$null
                    if ($response.StatusCode -eq 200) {
                        $apiReady = $true
                    }
                } catch {
                    # Try HTTP if HTTPS fails
                    try {
                        $response = Invoke-WebRequest -Uri "http://localhost:5001/graphql" -Method GET -TimeoutSec 5 2>$null
                        if ($response.StatusCode -eq 200) {
                            $apiReady = $true
                        }
                    } catch {
                        # API not ready yet, continue waiting
                    }
                }
                
                if ($elapsed -ge 40 -and -not $apiReady) {
                    Write-LoggedOutput "API is taking longer than usual to start..." -Type Warning
                }
            } while (-not $apiReady -and $elapsed -lt $timeout)
            
            if ($apiReady) {
                Write-LoggedOutput "API service is ready at https://localhost:7001 and http://localhost:5001" -Type Success
            } else {
                Write-LoggedOutput "Warning: API may not be fully ready yet (timeout after ${timeout}s)" -Type Warning
            }
            return $apiProcess
        } else {
            throw "Failed to start API process"
        }
    } finally {
        Pop-Location
    }
}

function Start-FrontendService {
    Write-LoggedOutput "Starting frontend service..." -Type Info
    
    $frontendPath = "$PSScriptRoot\..\src\frontend"
    
    if (-not (Test-Path "$frontendPath\package.json")) {
        throw "Frontend project not found at $frontendPath"
    }
    
    # Kill any existing node processes for this project
    $existingProcesses = Get-Process -Name node -ErrorAction SilentlyContinue | Where-Object {
        $_.CommandLine -and $_.CommandLine.Contains("frontend")
    }
    
    foreach ($proc in $existingProcesses) {
        Write-LoggedOutput "Stopping existing frontend process (PID: $($proc.Id))" -Type Warning
        $proc.Kill()
        $proc.WaitForExit(5000)
    }
    
    # Ensure npm dependencies are installed
    Push-Location $frontendPath
    try {
        Write-LoggedOutput "Installing/updating npm dependencies..." -Type Info
        $npmInstall = Start-Process -FilePath "cmd" -ArgumentList "/c", "npm", "install" -Wait -PassThru -WindowStyle Hidden -WorkingDirectory $frontendPath
        
        if ($npmInstall.ExitCode -ne 0) {
            throw "npm install failed with exit code $($npmInstall.ExitCode)"
        }
        Write-LoggedOutput "npm dependencies installed successfully" -Type Success
        
        # Start the frontend development server
        $frontendProcess = Start-Process -FilePath "cmd" -ArgumentList "/c", "npm", "run", "dev" -PassThru -WindowStyle Hidden -WorkingDirectory $frontendPath
        
        if ($frontendProcess) {
            Write-LoggedOutput "Frontend service started (PID: $($frontendProcess.Id))" -Type Success
            
            # Save process information
            $processInfo = Get-SavedProcesses
            $processInfo | Add-Member -MemberType NoteProperty -Name "Frontend" -Value @{
                ProcessId = $frontendProcess.Id
                StartTime = Get-Date
                ProjectPath = $frontendPath
            } -Force
            Save-ProcessInfo $processInfo
            
            # Wait for frontend to be ready
            Write-LoggedOutput "Waiting for frontend to be ready..." -Type Info
            $timeout = 120
            $elapsed = 0
            $frontendReady = $false
            
            do {
                Start-Sleep -Seconds 3
                $elapsed += 3
                
                # Check if process is still running (give it more time initially)
                if ($frontendProcess.HasExited -and $elapsed -gt 10) {
                    throw "Frontend service failed to start (exit code: $($frontendProcess.ExitCode))"
                }
                
                # Test common frontend ports
                $portsToTry = @(3000, 3001, 3002, 3003, 3004)
                foreach ($port in $portsToTry) {
                    try {
                        $response = Invoke-WebRequest -Uri "http://localhost:$port" -Method GET -TimeoutSec 3 2>$null
                        if ($response.StatusCode -eq 200) {
                            $frontendReady = $true
                            Write-LoggedOutput "Frontend service is ready at http://localhost:$port" -Type Success
                            break
                        }
                    } catch {
                        # Continue trying other ports
                    }
                }
                
                if ($frontendReady) {
                    break
                }
                
                if ($elapsed -ge 30 -and -not $frontendReady) {
                    Write-LoggedOutput "Frontend is taking longer than usual to start..." -Type Warning
                }
            } while (-not $frontendReady -and $elapsed -lt $timeout)
            
            if ($frontendReady) {
                Write-LoggedOutput "Frontend service is ready at http://localhost:3000" -Type Success
            } else {
                Write-LoggedOutput "Warning: Frontend may not be fully ready yet (timeout after ${timeout}s)" -Type Warning
            }
            return $frontendProcess
        } else {
            throw "Failed to start frontend process"
        }
    } finally {
        Pop-Location
    }
}

function Start-LogStreaming {
    param($ApiProcess, $FrontendProcess)
    
    if ($NoLogs) {
        Write-LoggedOutput "Log streaming disabled" -Type Info
        return
    }
    
    Write-LoggedOutput "Starting log streaming..." -Type Info
    Write-LoggedOutput "Press Ctrl+C to stop all services" -Type Warning
    Write-LoggedOutput "----------------------------------------" -Type Info
    
    # Stream API logs
    if ($apiProcess) {
        Start-Job -Name "ApiLogs" -ScriptBlock {
            param($ProcessId, $ProjectPath)
            
            # Try to read from dotnet output
            $logPath = "$ProjectPath\logs"
            if (Test-Path $logPath) {
                Get-ChildItem "$logPath\*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | ForEach-Object {
                    Get-Content $_.FullName -Wait | ForEach-Object {
                        Write-Host "[API] $_" -ForegroundColor Green
                    }
                }
            }
        } -ArgumentList $apiProcess.Id, (Resolve-Path "$PSScriptRoot\..\src\Archie.Api").Path | Out-Null
    }
    
    # Wait for user interruption
    try {
        Write-LoggedOutput "Services running. Press Ctrl+C to stop..." -Type Info
        while ($true) {
            Start-Sleep -Seconds 2
            
            # Check if Frontend process is still running (only monitor frontend, API might exit normally)
            if ($frontendProcess -and $frontendProcess.HasExited) {
                Write-LoggedOutput "Frontend process has exited unexpectedly" -Type Error
                break
            }
        }
    } catch [System.Management.Automation.PipelineStoppedException] {
        Write-LoggedOutput "Received stop signal" -Type Warning
    } finally {
        # Clean up background jobs
        Get-Job | Remove-Job -Force 2>$null
    }
}

# Main execution
try {
    Write-LoggedOutput "Starting Archie DeepWiki Development Environment" -Type Success
    Write-LoggedOutput "Log file: $script:LogPath" -Type Info
    
    # Always stop any existing services first to ensure clean startup
    Write-LoggedOutput "Stopping any existing services..." -Type Info
    try {
        $stopScript = Join-Path $PSScriptRoot "Stop-ArchieDevEnvironment.ps1"
        & $stopScript -Force 2>$null
        Write-LoggedOutput "Existing services stopped" -Type Success
    } catch {
        Write-LoggedOutput "No existing services found to stop" -Type Info
    }
    
    # Test prerequisites
    Test-Prerequisites
    
    # Start API service
    $apiProcess = Start-ApiService
    
    # Start Frontend service  
    $frontendProcess = Start-FrontendService
    
    Write-LoggedOutput "`n=== Services Started Successfully ===" -Type Success
    Write-LoggedOutput "API Endpoint: https://localhost:7001" -Type Info
    Write-LoggedOutput "GraphQL Playground: https://localhost:7001/graphql" -Type Info
    Write-LoggedOutput "Frontend: http://localhost:3000" -Type Info
    Write-LoggedOutput "Azure AI Search: ract-archie-search-dev.search.windows.net" -Type Info
    Write-LoggedOutput "Azure OpenAI: ract-ai-foundry-dev.openai.azure.com" -Type Info
    
    # Start log streaming
    Start-LogStreaming -ApiProcess $apiProcess -FrontendProcess $frontendProcess
    
} catch {
    Write-LoggedOutput "Failed to start development environment: $($_.Exception.Message)" -Type Error
    Write-LoggedOutput "Check the log file for detailed error information: $script:LogPath" -Type Error
    
    # Clean up any started processes  
    try {
        $stopScript = Join-Path $PSScriptRoot "Stop-ArchieDevEnvironment.ps1"
        & $stopScript -Force
    } catch {
        # Ignore stop script errors during cleanup
    }
    
    exit 1
}