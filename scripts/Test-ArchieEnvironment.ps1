<#
.SYNOPSIS
    Validates the Archie development environment setup
.DESCRIPTION
    Runs comprehensive tests to ensure all components are properly configured including:
    - .NET SDK, Git, Node.js installations
    - Azure AI Search connectivity
    - Azure OpenAI connectivity
    - GitHub API connectivity
    - Project build validation
    - Configuration file validation
.EXAMPLE
    .\Test-ArchieEnvironment.ps1
.EXAMPLE
    .\Test-ArchieEnvironment.ps1 -Detailed -OutputFormat JSON
#>

[CmdletBinding()]
param(
    [switch]$Detailed,
    [ValidateSet("Console", "JSON", "XML")]
    [string]$OutputFormat = "Console"
)

function Test-ComponentInstallation {
    param(
        [string]$ComponentName,
        [string]$Command,
        [string]$ExpectedPattern
    )
    
    try {
        $output = & $Command 2>$null
        $success = if ($ExpectedPattern) { $output -match $ExpectedPattern } else { $LASTEXITCODE -eq 0 }
        
        return @{
            Component = $ComponentName
            Status = if ($success) { "PASS" } else { "FAIL" }
            Output = $output
            Command = $Command
        }
    } catch {
        return @{
            Component = $ComponentName
            Status = "FAIL" 
            Error = $_.Exception.Message
            Command = $Command
        }
    }
}

function Test-ServiceHealth {
    param(
        [string]$ServiceName
    )
    
    try {
        $service = Get-Service $ServiceName -ErrorAction Stop
        return @{
            Service = $ServiceName
            Status = $service.Status
            StartType = $service.StartType
        }
    } catch {
        return @{
            Service = $ServiceName
            Status = "NOT_FOUND"
            Error = $_.Exception.Message
        }
    }
}

function Test-AzureAISearchConnectivity {
    Write-Output "Testing Azure AI Search connectivity..."
    
    $projectRoot = Split-Path -Parent $PSScriptRoot
    $configPath = "$projectRoot\src\Archie.Api\appsettings.Development.json"
    
    if (-not (Test-Path $configPath)) {
        return @{
            Test = "Azure AI Search Connectivity"
            Status = "FAIL"
            Error = "Configuration file not found: $configPath"
        }
    }
    
    try {
        $config = Get-Content $configPath -Raw | ConvertFrom-Json
        
        if (-not $config.AzureSearch -or -not $config.AzureSearch.ServiceUrl) {
            return @{
                Test = "Azure AI Search Connectivity"
                Status = "FAIL"
                Error = "Azure AI Search configuration missing"
            }
        }
        
        $searchUrl = $config.AzureSearch.ServiceUrl.TrimEnd('/') + "/indexes?api-version=2024-07-01"
        $headers = @{
            'api-key' = $config.AzureSearch.AdminKey
            'Content-Type' = 'application/json'
        }
        
        $response = Invoke-RestMethod -Uri $searchUrl -Method GET -Headers $headers -TimeoutSec 10
        
        return @{
            Test = "Azure AI Search Connectivity"
            Status = "PASS"
            Result = "Connected successfully"
            IndexCount = $response.value.Count
            ServiceUrl = $config.AzureSearch.ServiceUrl
        }
    } catch {
        return @{
            Test = "Azure AI Search Connectivity"
            Status = "FAIL"
            Error = $_.Exception.Message
        }
    }
}

function Test-AzureOpenAIConnectivity {
    Write-Output "Testing Azure OpenAI connectivity..."
    
    $projectRoot = Split-Path -Parent $PSScriptRoot
    $configPath = "$projectRoot\src\Archie.Api\appsettings.Development.json"
    
    if (-not (Test-Path $configPath)) {
        return @{
            Test = "Azure OpenAI Connectivity"
            Status = "FAIL"
            Error = "Configuration file not found"
        }
    }
    
    try {
        $config = Get-Content $configPath -Raw | ConvertFrom-Json
        
        if (-not $config.AzureOpenAI -or -not $config.AzureOpenAI.Endpoint) {
            return @{
                Test = "Azure OpenAI Connectivity"
                Status = "FAIL"
                Error = "Azure OpenAI configuration missing"
            }
        }
        
        # Test a simple API call to validate connectivity - use models endpoint for OpenAI
        $deploymentUrl = "$($config.AzureOpenAI.Endpoint.TrimEnd('/'))/openai/models?api-version=$($config.AzureOpenAI.ApiVersion)"
        $headers = @{
            'api-key' = $config.AzureOpenAI.ApiKey
            'Content-Type' = 'application/json'
        }
        
        $response = Invoke-RestMethod -Uri $deploymentUrl -Method GET -Headers $headers -TimeoutSec 10
        $embeddingDeployment = $response.data | Where-Object { $_.id -eq $config.AzureOpenAI.EmbeddingDeploymentName }
        
        return @{
            Test = "Azure OpenAI Connectivity"
            Status = "PASS"
            Result = "Connected successfully"
            DeploymentCount = $response.data.Count
            EmbeddingDeploymentFound = ($embeddingDeployment -ne $null)
            Endpoint = $config.AzureOpenAI.Endpoint
        }
    } catch {
        return @{
            Test = "Azure OpenAI Connectivity"
            Status = "FAIL"
            Error = $_.Exception.Message
        }
    }
}

function Test-ProjectBuild {
    Write-Output "Testing project build..."
    
    $projectRoot = Split-Path -Parent $PSScriptRoot
    
    try {
        Push-Location $projectRoot
        $buildOutput = & dotnet build --configuration Debug --verbosity quiet 2>&1
        $buildSuccess = $LASTEXITCODE -eq 0
        
        return @{
            Test = "Project Build"
            Status = if ($buildSuccess) { "PASS" } else { "FAIL" }
            Output = $buildOutput
            ExitCode = $LASTEXITCODE
        }
    } catch {
        return @{
            Test = "Project Build"
            Status = "FAIL"
            Error = $_.Exception.Message
        }
    } finally {
        Pop-Location
    }
}

function Test-GitHubConnectivity {
    Write-Output "Testing GitHub API connectivity..."
    
    try {
        $response = Invoke-RestMethod -Uri "https://api.github.com/rate_limit" -Method GET -TimeoutSec 10
        
        return @{
            Test = "GitHub API Connectivity"
            Status = "PASS"
            Result = "Connected successfully"
            RateLimit = "$($response.rate.remaining)/$($response.rate.limit)"
            ResetTime = ([System.DateTimeOffset]::FromUnixTimeSeconds($response.rate.reset)).ToString("yyyy-MM-dd HH:mm:ss")
        }
    } catch {
        return @{
            Test = "GitHub API Connectivity"
            Status = "FAIL"
            Error = $_.Exception.Message
        }
    }
}

function Test-ConfigurationFiles {
    Write-Output "Testing configuration files..."
    
    $projectRoot = Split-Path -Parent $PSScriptRoot
    $configFiles = @(
        "$projectRoot\src\Archie.Api\appsettings.json",
        "$projectRoot\src\Archie.Api\appsettings.Development.json"
    )
    
    $results = @()
    foreach ($configFile in $configFiles) {
        if (Test-Path $configFile) {
            try {
                $config = Get-Content $configFile -Raw | ConvertFrom-Json
                $hasAzureSearch = $config.AzureSearch -ne $null
                $hasAzureOpenAI = $config.AzureOpenAI -ne $null
                $hasGitHub = $config.GitHub -ne $null
                $hasIndexing = $config.Indexing -ne $null
                
                $results += @{
                    File = Split-Path -Leaf $configFile
                    Status = "PASS"
                    HasAzureSearchConfig = $hasAzureSearch
                    HasAzureOpenAIConfig = $hasAzureOpenAI
                    HasGitHubConfig = $hasGitHub
                    HasIndexingConfig = $hasIndexing
                    AzureSearchUrl = if ($hasAzureSearch) { $config.AzureSearch.ServiceUrl } else { "Not configured" }
                }
            } catch {
                $results += @{
                    File = Split-Path -Leaf $configFile
                    Status = "FAIL"
                    Error = "Invalid JSON format"
                }
            }
        } else {
            $results += @{
                File = Split-Path -Leaf $configFile
                Status = "FAIL" 
                Error = "File not found"
            }
        }
    }
    
    return $results
}

# Main execution
$validationResults = @{
    Timestamp = Get-Date
    Components = @()
    AzureSearchTest = $null
    AzureOpenAITest = $null
    GitHubTest = $null
    BuildTest = $null
    ConfigurationTest = @()
}

Write-Output "Running Archie Environment Validation..."
Write-Output "=========================================="

# Test component installations
$components = @(
    @{ Name = ".NET SDK"; Command = "dotnet"; Args = @("--version"); Pattern = "^9\." }
    @{ Name = "Git"; Command = "git"; Args = @("--version") }
    @{ Name = "Node.js"; Command = "node"; Args = @("--version") }
    @{ Name = "Chocolatey"; Command = "choco"; Args = @("--version") }
)

foreach ($component in $components) {
    try {
        if ($component.Args) {
            $output = & $component.Command $component.Args 2>$null
        } else {
            $output = & $component.Command 2>$null
        }
        
        $success = if ($component.Pattern) { 
            $output -match $component.Pattern 
        } else { 
            $LASTEXITCODE -eq 0 
        }
        
        $testResult = @{
            Component = $component.Name
            Status = if ($success) { "PASS" } else { "FAIL" }
            Output = $output
            Command = "$($component.Command) $($component.Args -join ' ')"
        }
    } catch {
        $testResult = @{
            Component = $component.Name
            Status = "FAIL" 
            Error = $_.Exception.Message
            Command = "$($component.Command) $($component.Args -join ' ')"
        }
    }
    
    $validationResults.Components += $testResult
    
    $status = if ($testResult.Status -eq "PASS") { "[PASS]" } else { "[FAIL]" }
    Write-Output "$status $($component.Name): $($testResult.Status)"
    
    if ($Detailed -and $testResult.Output) {
        Write-Output "  Version: $($testResult.Output)"
    }
    if ($Detailed -and $testResult.Error) {
        Write-Output "  Error: $($testResult.Error)"
    }
}

# Test Azure AI Search connectivity
$azureSearchTest = Test-AzureAISearchConnectivity
$validationResults.AzureSearchTest = $azureSearchTest
$status = if ($azureSearchTest.Status -eq "PASS") { "[PASS]" } else { "[FAIL]" }
Write-Output "$status $($azureSearchTest.Test): $($azureSearchTest.Status)"
if ($Detailed -and $azureSearchTest.Error) {
    Write-Output "  Error: $($azureSearchTest.Error)"
}
if ($Detailed -and $azureSearchTest.Result) {
    Write-Output "  Result: $($azureSearchTest.Result)"
}

# Test Azure OpenAI connectivity
$azureOpenAITest = Test-AzureOpenAIConnectivity
$validationResults.AzureOpenAITest = $azureOpenAITest
$status = if ($azureOpenAITest.Status -eq "PASS") { "[PASS]" } else { "[FAIL]" }
Write-Output "$status $($azureOpenAITest.Test): $($azureOpenAITest.Status)"
if ($Detailed -and $azureOpenAITest.Error) {
    Write-Output "  Error: $($azureOpenAITest.Error)"
}
if ($Detailed -and $azureOpenAITest.Result) {
    Write-Output "  Result: $($azureOpenAITest.Result)"
}

# Test GitHub API connectivity
$githubTest = Test-GitHubConnectivity
$validationResults.GitHubTest = $githubTest
$status = if ($githubTest.Status -eq "PASS") { "[PASS]" } else { "[FAIL]" }
Write-Output "$status $($githubTest.Test): $($githubTest.Status)"
if ($Detailed -and $githubTest.Error) {
    Write-Output "  Error: $($githubTest.Error)"
}
if ($Detailed -and $githubTest.Result) {
    Write-Output "  Result: $($githubTest.Result)"
    Write-Output "  Rate Limit: $($githubTest.RateLimit) (resets at $($githubTest.ResetTime))"
}

# Test project build
$buildTest = Test-ProjectBuild
$validationResults.BuildTest = $buildTest
$status = if ($buildTest.Status -eq "PASS") { "[PASS]" } else { "[FAIL]" }
Write-Output "$status $($buildTest.Test): $($buildTest.Status)"
if ($Detailed -and $buildTest.Error) {
    Write-Output "  Error: $($buildTest.Error)"
}
if ($Detailed -and $buildTest.Output -and $buildTest.Status -eq "FAIL") {
    Write-Output "  Build Output: $($buildTest.Output | Select-Object -First 3)"
}

# Test configuration files
$configTests = Test-ConfigurationFiles
$validationResults.ConfigurationTest = $configTests
foreach ($configTest in $configTests) {
    $status = if ($configTest.Status -eq "PASS") { "[PASS]" } else { "[FAIL]" }
    $fileName = if ($configTest.File) { $configTest.File } else { "Unknown" }
    Write-Output "$status Configuration ($fileName): $($configTest.Status)"
    
    if ($Detailed -and $configTest.Status -eq "PASS") {
        Write-Output "  Azure Search: $($configTest.HasAzureSearchConfig)"
        Write-Output "  Azure OpenAI: $($configTest.HasAzureOpenAIConfig)" 
        Write-Output "  GitHub: $($configTest.HasGitHubConfig)"
        Write-Output "  Indexing: $($configTest.HasIndexingConfig)"
    }
    if ($Detailed -and $configTest.Error) {
        Write-Output "  Error: $($configTest.Error)"
    }
}

# Output results in requested format
if ($OutputFormat -eq "JSON") {
    $validationResults | ConvertTo-Json -Depth 10
} elseif ($OutputFormat -eq "XML") {
    $validationResults | ConvertTo-Xml -As String
} else {
    Write-Output "`nValidation Summary:"
    Write-Output "=================="
    
    $totalTests = $validationResults.Components.Count + 3 + 1 + $validationResults.ConfigurationTest.Count  # Components + Azure services + Build + Config
    $azureSearchTestPassed = if ($validationResults.AzureSearchTest.Status -eq "PASS") { 1 } else { 0 }
    $azureOpenAITestPassed = if ($validationResults.AzureOpenAITest.Status -eq "PASS") { 1 } else { 0 }
    $githubTestPassed = if ($validationResults.GitHubTest.Status -eq "PASS") { 1 } else { 0 }
    $buildTestPassed = if ($validationResults.BuildTest.Status -eq "PASS") { 1 } else { 0 }
    
    $passedTests = ($validationResults.Components | Where-Object { $_.Status -eq "PASS" }).Count + 
                   $azureSearchTestPassed +
                   $azureOpenAITestPassed +
                   $githubTestPassed +
                   $buildTestPassed +
                   ($validationResults.ConfigurationTest | Where-Object { $_.Status -eq "PASS" }).Count
    
    Write-Output "Passed: $passedTests/$totalTests tests"
    
    if ($passedTests -eq $totalTests) {
        Write-Output "[SUCCESS] Environment is ready for development!"
    } else {
        Write-Output "[FAILED] Some tests failed. Check the details above."
        exit 1
    }
}