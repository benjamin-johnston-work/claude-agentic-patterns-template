#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Resets the Archie development environment
.DESCRIPTION
    Stops running processes, validates configurations, and cleans temporary files.
    Does not uninstall software - only resets runtime state.
.PARAMETER Confirm
    Skips confirmation prompts
.EXAMPLE
    .\Reset-ArchieEnvironment.ps1 -Confirm
#>

[CmdletBinding()]
param(
    [switch]$Confirm
)

function Reset-ArchieProcesses {
    Write-Output "Cleaning up Archie processes..."
    
    try {
        # Stop any running API processes
        $apiProcesses = Get-Process -Name dotnet -ErrorAction SilentlyContinue | Where-Object {
            try {
                $commandLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)").CommandLine
                return $commandLine -and $commandLine.Contains("Archie.Api")
            } catch {
                return $false
            }
        }
        
        if ($apiProcesses) {
            foreach ($process in $apiProcesses) {
                Write-Output "Stopping API process (PID: $($process.Id))"
                $process.Kill()
                $process.WaitForExit(5000)
            }
            Write-Output "Archie API processes stopped"
        } else {
            Write-Output "No Archie API processes found running"
        }
        
        # Clean up any background jobs
        $jobs = Get-Job -ErrorAction SilentlyContinue
        if ($jobs) {
            foreach ($job in $jobs) {
                Stop-Job $job -PassThru | Remove-Job -Force
            }
            Write-Output "Background jobs cleaned up"
        }
        
    } catch {
        Write-Warning "Error cleaning up processes: $($_.Exception.Message)"
    }
}

function Reset-Configuration {
    Write-Output "Validating configuration files..."
    
    $projectRoot = Split-Path -Parent $PSScriptRoot
    $developmentConfig = "$projectRoot\src\Archie.Api\appsettings.Development.json"
    
    if (Test-Path $developmentConfig) {
        try {
            # Validate that the configuration file is valid JSON
            $config = Get-Content $developmentConfig -Raw | ConvertFrom-Json
            
            # Check for required Azure services configuration
            $requiredSections = @("AzureSearch", "AzureOpenAI", "GitHub", "Indexing")
            $missingCount = 0
            
            foreach ($section in $requiredSections) {
                if (-not $config.$section) {
                    Write-Warning "Configuration section missing: $section"
                    $missingCount++
                }
            }
            
            if ($missingCount -eq 0) {
                Write-Output "All required configuration sections present"
            } else {
                Write-Warning "$missingCount configuration sections missing - may need manual configuration"
            }
            
        } catch {
            Write-Warning "Configuration file validation failed: $($_.Exception.Message)"
            Write-Output "Configuration file may be corrupted and need manual repair"
        }
    } else {
        Write-Warning "Configuration file not found: $developmentConfig"
    }
}

function Clean-TempDirectories {
    Write-Output "Cleaning temporary directories and logs..."
    
    # Clean temp directories
    $tempDirs = @(
        "C:\Temp\Archie",
        "$env:TEMP\archie*"
    )
    
    foreach ($dir in $tempDirs) {
        if (Test-Path $dir) {
            Remove-Item -Path $dir -Recurse -Force -ErrorAction SilentlyContinue
            Write-Output "Cleaned: $dir"
        }
    }
    
    # Clean script logs
    $scriptLogsDir = "$PSScriptRoot\logs"
    if (Test-Path $scriptLogsDir) {
        $oldLogs = Get-ChildItem $scriptLogsDir -File | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-7) }
        if ($oldLogs) {
            $oldLogs | Remove-Item -Force
            Write-Output "Cleaned old log files ($($oldLogs.Count) files)"
        }
    }
    
    # Clean process tracking file
    $processFile = "$PSScriptRoot\logs\.archie-processes.json"
    if (Test-Path $processFile) {
        Remove-Item $processFile -Force
        Write-Output "Cleaned process tracking file"
    }
}

# Main execution
if (-not $Confirm) {
    $response = Read-Host "This will stop Archie processes and clean temporary files. Continue? (y/N)"
    if ($response -ne 'y' -and $response -ne 'Y') {
        Write-Output "Operation cancelled"
        exit 0
    }
}

Write-Output "Resetting Archie development environment..."
Write-Output "==========================================="

Reset-ArchieProcesses
Reset-Configuration
Clean-TempDirectories

Write-Output "`nEnvironment reset completed!"
Write-Output "Run Setup-ArchieDevEnvironment.ps1 to validate the environment."
Write-Output "Run Start-ArchieDevEnvironment.ps1 to start the API service."