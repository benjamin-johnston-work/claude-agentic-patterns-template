#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Archie DeepWiki Development Environment Setup Script
.DESCRIPTION
    Automates the installation and configuration of all required dependencies
    for local development of the Archie DeepWiki platform.
.PARAMETER Clean
    Performs a clean installation, removing existing configurations
.PARAMETER SkipValidation
    Skips post-installation validation checks
.PARAMETER ConfigOnly
    Only updates configuration files, skips software installation
.EXAMPLE
    .\Setup-ArchieDevEnvironment.ps1
.EXAMPLE
    .\Setup-ArchieDevEnvironment.ps1 -Clean -Verbose
#>

[CmdletBinding()]
param(
    [switch]$Clean,
    [switch]$SkipValidation,
    [switch]$ConfigOnly
)

# Script configuration
$script:ScriptVersion = "1.0.0"
$script:LogPath = "$PSScriptRoot\logs\setup-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$script:ConfigPath = "$PSScriptRoot\config"

function Write-LoggedOutput {
    param(
        [string]$Message,
        [ValidateSet("Info", "Warning", "Error", "Success")]
        [string]$Type = "Info"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Type] $Message"
    
    switch ($Type) {
        "Info" { Write-Host $logMessage -ForegroundColor White }
        "Warning" { Write-Warning $Message }
        "Error" { Write-Host $logMessage -ForegroundColor Red }
        "Success" { Write-Host $logMessage -ForegroundColor Green }
    }
    
    # Append to log file
    if (-not (Test-Path (Split-Path $script:LogPath))) {
        New-Item -ItemType Directory -Path (Split-Path $script:LogPath) -Force | Out-Null
    }
    Add-Content -Path $script:LogPath -Value $logMessage
}

function Test-IsAdministrator {
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Install-Chocolatey {
    Write-LoggedOutput "Checking for Chocolatey package manager..." -Type Info
    
    if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
        Write-LoggedOutput "Installing Chocolatey package manager..." -Type Info
        Set-ExecutionPolicy Bypass -Scope Process -Force
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
        Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
        
        # Refresh environment variables
        $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
        
        if (Get-Command choco -ErrorAction SilentlyContinue) {
            Write-LoggedOutput "Chocolatey installed successfully" -Type Success
        } else {
            throw "Failed to install Chocolatey"
        }
    } else {
        Write-LoggedOutput "Chocolatey already installed" -Type Success
    }
}

function Install-DotNetSDK {
    Write-LoggedOutput "Checking .NET SDK installation..." -Type Info
    
    $dotnetVersion = & dotnet --version 2>$null
    if ($dotnetVersion -and $dotnetVersion.StartsWith("9.")) {
        Write-LoggedOutput ".NET 9.0 SDK already installed: $dotnetVersion" -Type Success
        return
    }
    
    Write-LoggedOutput "Installing .NET 9.0 SDK..." -Type Info
    choco install dotnet-sdk -y --version=9.0.101
    
    # Refresh environment
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
    
    $dotnetVersion = & dotnet --version 2>$null
    if ($dotnetVersion -and $dotnetVersion.StartsWith("9.")) {
        Write-LoggedOutput ".NET 9.0 SDK installed successfully: $dotnetVersion" -Type Success
    } else {
        throw "Failed to install .NET 9.0 SDK"
    }
}

function Install-Docker {
    Write-LoggedOutput "Checking Docker installation..." -Type Info
    
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        $dockerVersion = & docker --version 2>$null
        Write-LoggedOutput "Docker already installed: $dockerVersion" -Type Success
        
        # Check if Docker is running
        try {
            & docker info 2>$null | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-LoggedOutput "Docker is not running. Please start Docker Desktop." -Type Warning
                return
            }
            Write-LoggedOutput "Docker is running" -Type Success
        } catch {
            Write-LoggedOutput "Docker check failed: $($_.Exception.Message)" -Type Warning
        }
        return
    }
    
    Write-LoggedOutput "Installing Docker Desktop..." -Type Info
    choco install docker-desktop -y
    
    # Refresh environment
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
    
    Write-LoggedOutput "Docker installed. Please restart your machine and run Docker Desktop before continuing." -Type Warning
    Write-LoggedOutput "After restart, you'll need to run 'docker login' to authenticate with Docker Hub." -Type Info
}

function Test-AzureServices {
    Write-LoggedOutput "Validating Azure services configuration..." -Type Info
    
    $apiProjectPath = "$PSScriptRoot\..\src\Archie.Api"
    $developmentConfig = "$apiProjectPath\appsettings.Development.json"
    
    if (-not (Test-Path $developmentConfig)) {
        throw "Configuration file not found: $developmentConfig"
    }
    
    try {
        $config = Get-Content $developmentConfig -Raw | ConvertFrom-Json
        
        # Validate Azure AI Search configuration
        if (-not $config.AzureSearch) {
            throw "AzureSearch configuration section missing"
        }
        if (-not $config.AzureSearch.ServiceUrl -or $config.AzureSearch.ServiceUrl -eq "") {
            throw "AzureSearch ServiceUrl not configured"
        }
        if (-not $config.AzureSearch.AdminKey -or $config.AzureSearch.AdminKey -eq "") {
            throw "AzureSearch AdminKey not configured"
        }
        Write-LoggedOutput "✓ Azure AI Search configuration validated" -Type Success
        
        # Validate Azure OpenAI configuration
        if (-not $config.AzureOpenAI) {
            throw "AzureOpenAI configuration section missing"
        }
        if (-not $config.AzureOpenAI.Endpoint -or $config.AzureOpenAI.Endpoint -eq "") {
            throw "AzureOpenAI Endpoint not configured"
        }
        if (-not $config.AzureOpenAI.ApiKey -or $config.AzureOpenAI.ApiKey -eq "") {
            throw "AzureOpenAI ApiKey not configured"
        }
        Write-LoggedOutput "✓ Azure OpenAI configuration validated" -Type Success
        
        # Validate GitHub configuration
        if (-not $config.GitHub) {
            throw "GitHub configuration section missing"
        }
        if (-not $config.GitHub.DefaultAccessToken -or $config.GitHub.DefaultAccessToken -eq "") {
            Write-LoggedOutput "⚠ GitHub access token not configured - rate limits will apply" -Type Warning
        } else {
            Write-LoggedOutput "✓ GitHub API configuration validated" -Type Success
        }
        
        Write-LoggedOutput "Azure services configuration validation completed" -Type Success
        return $true
        
    } catch {
        Write-LoggedOutput "Azure services configuration validation failed: $($_.Exception.Message)" -Type Error
        throw "Configuration validation failed"
    }
}

function Test-AzureConnectivity {
    Write-LoggedOutput "Testing Azure services connectivity..." -Type Info
    
    # Test Azure AI Search connectivity
    try {
        $apiProjectPath = "$PSScriptRoot\..\src\Archie.Api"
        $developmentConfig = "$apiProjectPath\appsettings.Development.json"
        $config = Get-Content $developmentConfig -Raw | ConvertFrom-Json
        
        $searchUrl = $config.AzureSearch.ServiceUrl.TrimEnd('/') + "/indexes"
        $headers = @{
            'api-key' = $config.AzureSearch.AdminKey
            'Content-Type' = 'application/json'
        }
        
        $response = Invoke-RestMethod -Uri $searchUrl -Method GET -Headers $headers -TimeoutSec 10
        Write-LoggedOutput "✓ Azure AI Search connectivity verified" -Type Success
        
    } catch {
        Write-LoggedOutput "⚠ Azure AI Search connectivity test failed: $($_.Exception.Message)" -Type Warning
        Write-LoggedOutput "  This may be due to network connectivity or incorrect configuration" -Type Info
    }
    
    # Test GitHub API connectivity
    try {
        $githubResponse = Invoke-RestMethod -Uri "https://api.github.com/rate_limit" -Method GET -TimeoutSec 10
        Write-LoggedOutput "✓ GitHub API connectivity verified (Rate limit: $($githubResponse.rate.remaining)/$($githubResponse.rate.limit))" -Type Success
    } catch {
        Write-LoggedOutput "⚠ GitHub API connectivity test failed: $($_.Exception.Message)" -Type Warning
    }
}

function Install-Git {
    Write-LoggedOutput "Checking Git installation..." -Type Info
    
    if (Get-Command git -ErrorAction SilentlyContinue) {
        $gitVersion = & git --version
        Write-LoggedOutput "Git already installed: $gitVersion" -Type Success
        return
    }
    
    Write-LoggedOutput "Installing Git for Windows..." -Type Info
    choco install git -y
    
    # Refresh environment
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
    
    if (Get-Command git -ErrorAction SilentlyContinue) {
        $gitVersion = & git --version
        Write-LoggedOutput "Git installed successfully: $gitVersion" -Type Success
    } else {
        throw "Failed to install Git"
    }
}

function Install-NodeJS {
    Write-LoggedOutput "Checking Node.js installation..." -Type Info
    
    if (Get-Command node -ErrorAction SilentlyContinue) {
        $nodeVersion = & node --version
        Write-LoggedOutput "Node.js already installed: $nodeVersion" -Type Success
        return
    }
    
    Write-LoggedOutput "Installing Node.js LTS..." -Type Info
    choco install nodejs-lts -y
    
    # Refresh environment
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
    
    if (Get-Command node -ErrorAction SilentlyContinue) {
        $nodeVersion = & node --version
        Write-LoggedOutput "Node.js installed successfully: $nodeVersion" -Type Success
    } else {
        throw "Failed to install Node.js"
    }
}

function Initialize-ArchieEnvironment {
    Write-LoggedOutput "Initializing Archie development environment..." -Type Info
    
    # Create logs directory if it doesn't exist
    $logsDir = "$PSScriptRoot\logs"
    if (-not (Test-Path $logsDir)) {
        New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
        Write-LoggedOutput "Created logs directory: $logsDir" -Type Info
    }
    
    # Validate Azure services configuration and connectivity
    Test-AzureServices
    Test-AzureConnectivity
    
    Write-LoggedOutput "Environment initialization completed" -Type Success
}

function Update-ArchieConfiguration {
    Write-LoggedOutput "Validating Archie configuration files..." -Type Info
    
    $apiProjectPath = "$PSScriptRoot\..\src\Archie.Api"
    $developmentConfig = "$apiProjectPath\appsettings.Development.json"
    
    if (Test-Path $developmentConfig) {
        try {
            $config = Get-Content $developmentConfig -Raw | ConvertFrom-Json
            
            # Validate required Azure services configuration exists
            $requiredSections = @("AzureSearch", "AzureOpenAI", "GitHub", "Indexing")
            $missingCount = 0
            
            foreach ($section in $requiredSections) {
                if (-not $config.$section) {
                    Write-LoggedOutput "⚠ Missing configuration section: $section" -Type Warning
                    $missingCount++
                } else {
                    Write-LoggedOutput "✓ Configuration section validated: $section" -Type Success
                }
            }
            
            if ($missingCount -eq 0) {
                Write-LoggedOutput "All required configuration sections present" -Type Success
            } else {
                Write-LoggedOutput "$missingCount configuration section(s) missing - may cause runtime errors" -Type Warning
            }
            
        } catch {
            Write-LoggedOutput "Error validating configuration: $($_.Exception.Message)" -Type Error
            throw "Configuration validation failed"
        }
    } else {
        Write-LoggedOutput "Warning: appsettings.Development.json not found at $developmentConfig" -Type Warning
        throw "Configuration file not found"
    }
}

function Test-EnvironmentHealth {
    Write-LoggedOutput "Running environment health checks..." -Type Info
    
    $healthResults = @{}
    
    # Test .NET SDK
    try {
        $dotnetVersion = & dotnet --version 2>$null
        $healthResults.DotNet = @{ Status = "OK"; Version = $dotnetVersion }
    } catch {
        $healthResults.DotNet = @{ Status = "FAIL"; Error = $_.Exception.Message }
    }
    
    # Test Git
    try {
        $gitVersion = & git --version 2>$null
        $healthResults.Git = @{ Status = "OK"; Version = $gitVersion }
    } catch {
        $healthResults.Git = @{ Status = "FAIL"; Error = $_.Exception.Message }
    }
    
    # Test Node.js
    try {
        $nodeVersion = & node --version 2>$null
        $healthResults.NodeJS = @{ Status = "OK"; Version = $nodeVersion }
    } catch {
        $healthResults.NodeJS = @{ Status = "FAIL"; Error = $_.Exception.Message }
    }
    
    # Test Docker
    try {
        $dockerVersion = & docker --version 2>$null
        $healthResults.Docker = @{ Status = "OK"; Version = $dockerVersion }
    } catch {
        $healthResults.Docker = @{ Status = "FAIL"; Error = "Docker not installed or not running" }
    }
    
    # Test Azure AI Search connectivity
    try {
        $apiProjectPath = "$PSScriptRoot\..\src\Archie.Api"
        $developmentConfig = "$apiProjectPath\appsettings.Development.json"
        $config = Get-Content $developmentConfig -Raw | ConvertFrom-Json
        
        $searchUrl = $config.AzureSearch.ServiceUrl.TrimEnd('/') + "/indexes"
        $headers = @{
            'api-key' = $config.AzureSearch.AdminKey
            'Content-Type' = 'application/json'
        }
        
        $response = Invoke-RestMethod -Uri $searchUrl -Method GET -Headers $headers -TimeoutSec 10
        $healthResults.AzureSearchConnectivity = @{ Status = "OK"; Result = "Connected - $($response.value.Count) indexes found" }
    } catch {
        $healthResults.AzureSearchConnectivity = @{ Status = "FAIL"; Error = "Cannot connect to Azure AI Search: $($_.Exception.Message)" }
    }
    
    # Test GitHub API connectivity
    try {
        $githubResponse = Invoke-RestMethod -Uri "https://api.github.com/rate_limit" -Method GET -TimeoutSec 10
        $healthResults.GitHubConnectivity = @{ Status = "OK"; Result = "Connected - Rate limit: $($githubResponse.rate.remaining)/$($githubResponse.rate.limit)" }
    } catch {
        $healthResults.GitHubConnectivity = @{ Status = "FAIL"; Error = "Cannot connect to GitHub API: $($_.Exception.Message)" }
    }
    
    # Test project restore and build
    try {
        Push-Location "$PSScriptRoot\.."
        
        # First restore packages
        Write-LoggedOutput "Restoring NuGet packages..." -Type Info
        $restoreResult = & dotnet restore --verbosity quiet 2>$null
        if ($LASTEXITCODE -ne 0) {
            $healthResults.ProjectBuild = @{ Status = "FAIL"; Error = "Package restore failed" }
        } else {
            # Then build
            $buildResult = & dotnet build --configuration Debug --verbosity quiet 2>$null
            if ($LASTEXITCODE -eq 0) {
                $healthResults.ProjectBuild = @{ Status = "OK"; Result = "Build successful" }
            } else {
                $healthResults.ProjectBuild = @{ Status = "FAIL"; Error = "Build failed" }
            }
        }
    } catch {
        $healthResults.ProjectBuild = @{ Status = "FAIL"; Error = $_.Exception.Message }
    } finally {
        Pop-Location
    }
    
    # Display results
    Write-LoggedOutput "`n=== Environment Health Check Results ===" -Type Info
    foreach ($component in $healthResults.Keys) {
        $result = $healthResults[$component]
        if ($result.Status -eq "OK" -or $result.Status -eq "Running") {
            Write-LoggedOutput "$component`: $($result.Status) - $($result.Version)$($result.Result)" -Type Success
        } else {
            Write-LoggedOutput "$component`: $($result.Status) - $($result.Error)" -Type Error
        }
    }
    
    $failedComponents = $healthResults.Values | Where-Object { $_.Status -eq "FAIL" }
    if ($failedComponents.Count -eq 0) {
        Write-LoggedOutput "`nAll components are healthy!" -Type Success
        return $true
    } else {
        Write-LoggedOutput "`n$($failedComponents.Count) component(s) failed health checks" -Type Error
        return $false
    }
}

# Main execution
try {
    Write-LoggedOutput "Starting Archie DeepWiki Development Environment Setup v$script:ScriptVersion" -Type Info
    Write-LoggedOutput "Log file: $script:LogPath" -Type Info
    
    if (-not (Test-IsAdministrator)) {
        throw "This script requires administrator privileges. Please run as administrator."
    }
    
    if (-not $ConfigOnly) {
        # Install dependencies
        Install-Chocolatey
        Install-DotNetSDK
        Install-Git
        Install-NodeJS
        
        # Initialize environment
        Initialize-ArchieEnvironment
    }
    
    # Update configuration
    Update-ArchieConfiguration
    
    # Run health checks
    if (-not $SkipValidation) {
        $healthResult = Test-EnvironmentHealth
        if (-not $healthResult) {
            Write-LoggedOutput "Some components failed health checks. Review the log for details." -Type Warning
        }
    }
    
    Write-LoggedOutput "`nArchie development environment setup completed!" -Type Success
    Write-LoggedOutput "Next steps:" -Type Info
    Write-LoggedOutput "1. Run '.\scripts\Start-ArchieDevEnvironment.ps1' to start the API service" -Type Info
    Write-LoggedOutput "2. Access API at https://localhost:7001" -Type Info
    Write-LoggedOutput "3. Access GraphQL playground at https://localhost:7001/graphql" -Type Info
    Write-LoggedOutput "4. Use '.\scripts\Test-ArchieEnvironment.ps1' to validate Azure services connectivity" -Type Info
    
} catch {
    Write-LoggedOutput "Setup failed: $($_.Exception.Message)" -Type Error
    Write-LoggedOutput "Check the log file for detailed error information: $script:LogPath" -Type Error
    exit 1
}