#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Stops the Archie DeepWiki development environment and cleans up all processes
.DESCRIPTION
    Gracefully stops the API service and cleans up background processes
.PARAMETER Force
    Force kill processes without graceful shutdown
.PARAMETER KeepNeo4j
    [DEPRECATED] Legacy parameter - no longer used (removed Neo4j dependency)
.EXAMPLE
    .\Stop-ArchieDevEnvironment.ps1
.EXAMPLE
    .\Stop-ArchieDevEnvironment.ps1 -Force
#>

[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$KeepNeo4j
)

$script:LogPath = "$PSScriptRoot\logs\stop-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
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

function Get-SavedProcesses {
    if (Test-Path $script:ProcessFile) {
        try {
            return Get-Content $script:ProcessFile | ConvertFrom-Json
        } catch {
            return @{}
        }
    }
    return @{}
}

function Clear-ProcessFile {
    if (Test-Path $script:ProcessFile) {
        Remove-Item $script:ProcessFile -Force -ErrorAction SilentlyContinue
    }
}

function Stop-FrontendService {
    Write-LoggedOutput "Stopping frontend service..." -Type Info
    
    $stopped = $false
    
    # Try to stop from saved process info first
    $processInfo = Get-SavedProcesses
    if ($processInfo.Frontend -and $processInfo.Frontend.ProcessId) {
        try {
            $process = Get-Process -Id $processInfo.Frontend.ProcessId -ErrorAction SilentlyContinue
            if ($process) {
                Write-LoggedOutput "Stopping saved frontend process (PID: $($process.Id))" -Type Info
                
                if ($Force) {
                    $process.Kill()
                } else {
                    $process.CloseMainWindow()
                    if (-not $process.WaitForExit(10000)) {
                        Write-LoggedOutput "Graceful shutdown timed out, force killing..." -Type Warning
                        $process.Kill()
                    }
                }
                $stopped = $true
                Write-LoggedOutput "Frontend service stopped successfully" -Type Success
            }
        } catch {
            Write-LoggedOutput "Error stopping saved frontend process: $($_.Exception.Message)" -Type Warning
        }
    }
    
    # Find and stop any node processes running the frontend
    $frontendProcesses = Get-Process -Name node -ErrorAction SilentlyContinue | Where-Object {
        try {
            $commandLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)").CommandLine
            return $commandLine -and ($commandLine.Contains("frontend") -or $commandLine.Contains("next"))
        } catch {
            return $false
        }
    }
    
    foreach ($process in $frontendProcesses) {
        try {
            Write-LoggedOutput "Stopping frontend process (PID: $($process.Id))" -Type Info
            
            if ($Force) {
                $process.Kill()
            } else {
                $process.CloseMainWindow()
                if (-not $process.WaitForExit(10000)) {
                    Write-LoggedOutput "Graceful shutdown timed out, force killing..." -Type Warning
                    $process.Kill()
                }
            }
            $stopped = $true
            Write-LoggedOutput "Frontend process stopped" -Type Success
        } catch {
            Write-LoggedOutput "Error stopping frontend process: $($_.Exception.Message)" -Type Warning
        }
    }
    
    if (-not $stopped) {
        Write-LoggedOutput "No frontend processes found running" -Type Info
    }
}

function Stop-ApiService {
    Write-LoggedOutput "Stopping API service..." -Type Info
    
    $stopped = $false
    
    # Try to stop from saved process info first
    $processInfo = Get-SavedProcesses
    if ($processInfo.Api -and $processInfo.Api.ProcessId) {
        try {
            $process = Get-Process -Id $processInfo.Api.ProcessId -ErrorAction SilentlyContinue
            if ($process) {
                Write-LoggedOutput "Stopping saved API process (PID: $($process.Id))" -Type Info
                
                if ($Force) {
                    $process.Kill()
                } else {
                    $process.CloseMainWindow()
                    if (-not $process.WaitForExit(10000)) {
                        Write-LoggedOutput "Graceful shutdown timed out, force killing..." -Type Warning
                        $process.Kill()
                    }
                }
                $stopped = $true
                Write-LoggedOutput "API service stopped successfully" -Type Success
            }
        } catch {
            Write-LoggedOutput "Error stopping saved process: $($_.Exception.Message)" -Type Warning
        }
    }
    
    # Find and stop any dotnet processes running Archie.Api
    $apiProcesses = Get-Process -Name dotnet -ErrorAction SilentlyContinue | Where-Object {
        try {
            $commandLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)").CommandLine
            return $commandLine -and $commandLine.Contains("Archie.Api")
        } catch {
            return $false
        }
    }
    
    foreach ($process in $apiProcesses) {
        try {
            Write-LoggedOutput "Stopping API process (PID: $($process.Id))" -Type Info
            
            if ($Force) {
                $process.Kill()
            } else {
                $process.CloseMainWindow()
                if (-not $process.WaitForExit(10000)) {
                    Write-LoggedOutput "Graceful shutdown timed out, force killing..." -Type Warning
                    $process.Kill()
                }
            }
            $stopped = $true
            Write-LoggedOutput "API process stopped" -Type Success
        } catch {
            Write-LoggedOutput "Error stopping API process: $($_.Exception.Message)" -Type Warning
        }
    }
    
    if (-not $stopped) {
        Write-LoggedOutput "No API processes found running" -Type Info
    }
}

function Show-StopSummary {
    Write-LoggedOutput "Archie development services have been stopped" -Type Info
    Write-LoggedOutput "- API service (backend)" -Type Info
    Write-LoggedOutput "- Frontend service" -Type Info
    Write-LoggedOutput "Azure services (AI Search, OpenAI) remain available" -Type Info
    
    if ($Force) {
        Write-LoggedOutput "All related processes have been force terminated" -Type Warning
    }
}

function Stop-BackgroundJobs {
    Write-LoggedOutput "Cleaning up background jobs..." -Type Info
    
    $jobs = Get-Job -ErrorAction SilentlyContinue
    if ($jobs) {
        foreach ($job in $jobs) {
            try {
                Write-LoggedOutput "Stopping job: $($job.Name)" -Type Info
                Stop-Job $job -PassThru | Remove-Job -Force
            } catch {
                Write-LoggedOutput "Error stopping job $($job.Name): $($_.Exception.Message)" -Type Warning
            }
        }
        Write-LoggedOutput "Background jobs cleaned up" -Type Success
    } else {
        Write-LoggedOutput "No background jobs found" -Type Info
    }
}

function Stop-DotNetProcesses {
    Write-LoggedOutput "Cleaning up any remaining dotnet processes..." -Type Info
    
    # Find any dotnet processes that might be related to our project
    $projectPath = Resolve-Path "$PSScriptRoot\..\src\Archie.Api"
    $dotnetProcesses = Get-Process -Name dotnet -ErrorAction SilentlyContinue | Where-Object {
        try {
            $commandLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)").CommandLine
            return $commandLine -and ($commandLine.Contains("Archie") -or $commandLine.Contains($projectPath.Path))
        } catch {
            return $false
        }
    }
    
    if ($dotnetProcesses) {
        foreach ($process in $dotnetProcesses) {
            try {
                Write-LoggedOutput "Stopping dotnet process (PID: $($process.Id))" -Type Warning
                $process.Kill()
                $process.WaitForExit(5000)
            } catch {
                Write-LoggedOutput "Error stopping dotnet process: $($_.Exception.Message)" -Type Warning
            }
        }
        Write-LoggedOutput "Dotnet processes cleaned up" -Type Success
    } else {
        Write-LoggedOutput "No related dotnet processes found" -Type Info
    }
}

# Main execution
try {
    Write-LoggedOutput "Stopping Archie DeepWiki Development Environment" -Type Warning
    Write-LoggedOutput "Log file: $script:LogPath" -Type Info
    
    # Stop services
    Stop-BackgroundJobs
    Stop-FrontendService
    Stop-ApiService
    
    # Extra cleanup if Force is specified
    if ($Force) {
        Stop-DotNetProcesses
    }
    
    # Clear process tracking file
    Clear-ProcessFile
    
    # Show summary
    Show-StopSummary
    
    Write-LoggedOutput "`nAPI service stopped successfully" -Type Success
    Write-LoggedOutput "To start again: .\Start-ArchieDevEnvironment.ps1" -Type Info
    
} catch {
    Write-LoggedOutput "Error during shutdown: $($_.Exception.Message)" -Type Error
    Write-LoggedOutput "Check the log file for detailed error information: $script:LogPath" -Type Error
    exit 1
}