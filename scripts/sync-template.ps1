# Manual Template Sync Script
# Use this to manually sync changes to the template repository

param()

# Configuration
$MainRepoPath = Get-Location
$TemplateRepoUrl = "https://github.com/benjamin-johnston-work/claude-agentic-patterns-template.git"
$TempDir = [System.IO.Path]::GetTempPath() + [System.Guid]::NewGuid().ToString()
$TemplateDir = Join-Path $TempDir "template-repo"

Write-Host "Starting template sync process..." -ForegroundColor Yellow

# Validate we're in the right directory
if (!(Test-Path ".claude") -or !(Test-Path ".github/workflows/sync-template.yml")) {
    Write-Host "Error: This script must be run from the main repository root" -ForegroundColor Red
    exit 1
}

# Create temp directory
New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

# Clone template repository
Write-Host "Cloning template repository..." -ForegroundColor Yellow
git clone $TemplateRepoUrl $TemplateDir
Set-Location $TemplateDir

# Ensure we're on main branch
try {
    git checkout main 2>$null
} catch {
    git checkout -b main
}

# Go back to main repo
Set-Location $MainRepoPath

# Sync files (excluding docs and specific files)
Write-Host "Syncing files..." -ForegroundColor Yellow

# Remove all files from template except .git
Get-ChildItem $TemplateDir -Exclude ".git" | Remove-Item -Recurse -Force

# Copy everything except docs and template-specific files
$ExcludePatterns = @('docs', '.git')
Get-ChildItem -Path . -Recurse | Where-Object { 
    $_.FullName -notmatch 'docs\\' -and 
    $_.FullName -notmatch '\.git\\' -and
    $_.Name -ne 'README.md' -and
    $_.Name -ne 'sync-template.ps1' -and
    $_.FullName -notmatch '\.github\\workflows\\sync-template\.yml'
} | Copy-Item -Destination { 
    $relativePath = $_.FullName.Substring($MainRepoPath.Path.Length + 1)
    Join-Path $TemplateDir $relativePath
} -Force -Recurse

# Create template-specific README
$ReadmeContent = @"
# Claude Agentic Patterns Template

This is a template repository containing Claude Code agent configurations and patterns.

## Quick Start

1. Click "Use this template" to create your repository
2. Customize the agents in ``.claude/agents/`` for your project
3. Modify commands in ``.claude/commands/`` as needed
4. Start using Claude Code with your custom agent setup

## What's Included

- **Agents**: Pre-configured specialized agents for different tasks
- **Commands**: Slash commands for common development workflows  
- **Patterns**: Implementation patterns following Claude Code best practices

## Documentation

For full documentation and implementation details, see the main repository:
https://github.com/benjamin-johnston-work/agenticpatterns

## Usage

This template provides a production-ready Claude Code setup. Customize the agents and commands to match your project's specific needs.

---

*Template manually synced from main repository*
"@

Set-Content -Path (Join-Path $TemplateDir "README.md") -Value $ReadmeContent

# Commit and push changes
Set-Location $TemplateDir

Write-Host "Committing changes..." -ForegroundColor Yellow
git add .

# Check if there are changes to commit
$Changes = git diff --staged --name-only
if (!$Changes) {
    Write-Host "No changes to sync" -ForegroundColor Green
    Remove-Item -Path $TempDir -Recurse -Force
    exit 0
}

# Count changes
$ChangedFiles = ($Changes | Measure-Object).Count
Write-Host "Found $ChangedFiles changed files" -ForegroundColor Yellow

# Commit with timestamp
$CommitMessage = "Manual sync from main repository - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
git commit -m $CommitMessage

# Push changes
Write-Host "Pushing changes..." -ForegroundColor Yellow
git push origin main

# Create release tag for major updates
if ($ChangedFiles -gt 5) {
    $Tag = "v$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Write-Host "Creating release tag: $Tag" -ForegroundColor Yellow
    $TagMessage = "Manual template sync - major update with $ChangedFiles changes"
    git tag -a $Tag -m $TagMessage
    git push origin $Tag
}

# Cleanup
Set-Location $MainRepoPath
Remove-Item -Path $TempDir -Recurse -Force

Write-Host "Template sync completed successfully!" -ForegroundColor Green
Write-Host "Synced $ChangedFiles files to template repository" -ForegroundColor Green

# Show what was synced
Write-Host "`nSummary of synced content:" -ForegroundColor Yellow
Write-Host "- Agent configurations (.claude/agents/)"
Write-Host "- Command definitions (.claude/commands/)"  
Write-Host "- Configuration files"
Write-Host "- Scripts (except sync-template.ps1)"
Write-Host "Excluded:" -ForegroundColor Yellow
Write-Host "- docs/ folder"
Write-Host "- Main repository README.md"
Write-Host "- Sync workflow and scripts"