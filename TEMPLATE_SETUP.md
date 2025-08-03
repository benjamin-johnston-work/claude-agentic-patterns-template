# Template Repository Setup Guide

This guide explains how to set up and maintain the template repository for sharing Claude Agentic Patterns.

## Overview

The template repository allows users to reuse this Claude Code setup without the documentation files, providing a clean starting point for their own projects.

## Setup Process

### 1. Create Template Repository

1. Create a new GitHub repository named `claude-agentic-patterns-template`
2. Mark it as a "Template repository" in GitHub settings
3. Initialize with a basic README

### 2. Configure GitHub Secrets

For the auto-sync workflow to work, you need to set up a GitHub token:

1. Go to GitHub Settings → Developer settings → Personal access tokens
2. Create a token with `repo` permissions
3. In your main repository, go to Settings → Secrets and variables → Actions
4. Add a secret named `TEMPLATE_SYNC_TOKEN` with your token value

### 3. Update Repository URLs

Edit these files to replace placeholder URLs with your actual repository URLs:

- `.github/workflows/sync-template.yml` (line 14 and throughout)
- `scripts/sync-template.ps1` (line 8)

Replace `your-username` with your actual GitHub username.

## Automatic Sync

The GitHub Actions workflow (`.github/workflows/sync-template.yml`) automatically syncs changes:

**Triggers:**
- Push to master branch
- Excludes changes to `docs/` folder
- Manual trigger via GitHub Actions UI

**What gets synced:**
- All `.claude/` configuration files
- Scripts (except sync scripts)
- Root configuration files
- Custom template README

**What gets excluded:**
- `docs/` folder
- Main repository README.md
- Sync workflow and scripts

## Manual Sync

Use the PowerShell script for manual syncing:

```powershell
# Run from repository root
.\scripts\sync-template.ps1
```

The script will:
1. Clone the template repository
2. Sync all relevant files
3. Create a template-specific README
4. Commit and push changes
5. Create release tags for major updates

## Template Usage

Users can use the template by:

1. Going to the template repository
2. Clicking "Use this template"
3. Creating their own repository
4. Customizing agents and commands for their project

## Maintenance

### Regular Updates
- The auto-sync workflow runs on every push (excluding docs)
- Manual sync can be triggered when needed
- Release tags are created for major updates (5+ file changes)

### Monitoring
- Check GitHub Actions for sync failures
- Monitor template repository for issues
- Update documentation as needed

## File Structure

The template includes:
```
.claude/
├── agents/          # Agent configurations
├── commands/        # Slash commands
└── *.md            # Agent documentation

scripts/            # Utility scripts (excluding sync scripts)
.gitignore         # Git ignore file
README.md          # Template-specific README
```

## Troubleshooting

### Sync Failures
- Check GitHub token permissions
- Verify repository URLs are correct
- Ensure template repository exists and is accessible

### Missing Files
- Review exclude patterns in sync scripts
- Check that source files exist in main repository
- Verify rsync/robocopy commands are working

### Permission Issues
- Ensure GitHub token has sufficient permissions
- Check repository access settings
- Verify template repository allows pushes

## Benefits

- **Clean Starting Point**: Users get only the Claude Code setup
- **Always Current**: Auto-sync keeps template up-to-date
- **Easy Distribution**: GitHub template makes sharing simple
- **Customizable**: Users can modify for their specific needs