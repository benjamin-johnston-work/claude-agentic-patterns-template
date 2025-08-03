---
description: Simple new branch creation from develop with proper naming conventions
argument-hint: "[branch description] - What you're working on (will be converted to proper branch name)"
allowed-tools: [Bash]
coordination-pattern: direct
quality-thresholds: [proper branch naming convention, clean starting point from latest develop, ready-to-work environment]
evidence-requirements: [base branch setup, branch creation with appropriate prefix, clean starting point confirmation]
complexity: low
estimated-duration: 10
---

Create new branch for: $ARGUMENTS

## Primary Goals

Create a properly named feature branch from the latest develop branch, ensuring clean starting point and following established branching conventions.

## Success Criteria

**Validation Requirements:**
- Successfully switch to develop branch (handling uncommitted changes if needed)
- Latest changes pulled from remote develop branch without conflicts
- New branch created with appropriate prefix and sanitized name
- Branch name follows convention: `[prefix]/[sanitized-description]`
- Ready to begin development work

**Operation Steps:**
1. **Base Branch Setup**: Switch to develop and handle any uncommitted changes
2. **Update Base**: Pull latest changes from origin/develop
3. **Branch Creation**: Create new branch with appropriate prefix (feature/, bugfix/, hotfix/, tech/)
4. **Name Sanitization**: Convert description to lowercase, hyphens, no special characters
5. **Confirmation**: Display current branch and recent commits
6. **Guidance**: Provide reminders about commit practices and PR workflow

**Output Standards:**
- Proper branch naming: `feature/user-authentication`, `bugfix/payment-error`, etc.
- Clean starting point from latest develop
- Clear confirmation of branch creation success
- Development workflow reminders and best practices
- Ready-to-work environment confirmation