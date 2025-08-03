---
description: Simple pull request creation with comprehensive quality checklist
argument-hint: "[PR title] - Title for the pull request"
allowed-tools: [Bash, Read]
coordination-pattern: direct
quality-thresholds: [all project-specific quality checks pass, proper branch management, comprehensive PR template completion]
evidence-requirements: [branch validation, quality checks execution, comprehensive PR template with checklist, related issues linking]
complexity: low
estimated-duration: 20
---

Create pull request: $ARGUMENTS

## Primary Goals

Create a well-structured pull request with comprehensive quality checklist, automated quality validation, and proper branch management to ensure code review readiness.

## Success Criteria

**Validation Requirements:**
- Current branch is not main/master (feature branch required)
- All project-specific quality checks pass (build, test, lint)
- Branch is successfully pushed to remote repository
- PR created with comprehensive template and checklist
- Related issues properly linked

**Operation Steps:**
1. **Branch Validation**: Confirm working on feature branch, not main
2. **Quality Checks**: Run project-appropriate validation (dotnet/npm commands)
3. **Change Review**: Display commits and changes to be included
4. **Branch Push**: Push current branch with upstream tracking
5. **PR Creation**: Generate PR with comprehensive checklist template
6. **Setup**: Add labels, reviewers, and issue links

**Output Standards:**
- Comprehensive PR template with all quality checkboxes
- Automatic project type detection for appropriate quality commands
- Clear summary of changes and testing requirements
- Security and performance validation checklist
- Documentation update requirements
- Immediate PR URL and next steps guidance