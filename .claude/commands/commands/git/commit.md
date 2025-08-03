---
description: Simple git commit with smart conventional commit message generation
argument-hint: "[optional custom commit message] - If not provided, will analyze changes automatically"
allowed-tools: [Bash, Read]
coordination-pattern: direct
quality-thresholds: [conventional commit format compliance, accurate commit type reflection, proper change documentation]
evidence-requirements: [git status analysis, change analysis for commit type determination, clear actionable commit messages]
complexity: low
estimated-duration: 15
---

Create smart conventional commit: $ARGUMENTS

## Primary Goals

Analyze current git changes and create a properly formatted conventional commit with appropriate type, scope, and description based on the actual code changes.

## Success Criteria

**Validation Requirements:**
- Git status shows changes ready for commit (staged or unstaged)
- Generated commit message follows conventional commit format
- Commit type accurately reflects the nature of changes made
- Commit description is concise but descriptive
- Breaking changes are properly documented if present

**Operation Steps:**
1. **Status Analysis**: Check git status and staged changes
2. **Change Analysis**: Determine appropriate commit type (feat, fix, docs, style, refactor, perf, test, chore)
3. **Message Generation**: Create conventional commit message with scope if applicable
4. **Confirmation**: Present suggested message and get approval
5. **Execution**: Create commit and show result
6. **Next Steps**: Offer to push changes or create PR

**Output Standards:**
- Conventional commit format: `type(scope): description`
- Clear, actionable commit messages
- Proper handling of breaking changes
- Immediate feedback on commit success
- Guidance on next steps (push, PR creation)