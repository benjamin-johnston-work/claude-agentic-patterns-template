---
description: Simple intelligent merge conflict resolution with context-aware analysis
argument-hint: "[optional context] - Additional context about the conflicts being resolved"
allowed-tools: [Bash, Read, Edit]
coordination-pattern: direct
quality-thresholds: [context-aware resolution preserving functionality, post-resolution validation passes, no semantic conflicts]
evidence-requirements: [conflict analysis and intent understanding, type-specific resolution strategies, comprehensive post-resolution quality validation]
complexity: medium
estimated-duration: 30
---

Resolve merge conflicts intelligently: $ARGUMENTS

## Primary Goals

Intelligently resolve merge conflicts by understanding the context of both sides, preserving functionality from all branches, and ensuring the resolution maintains code quality and business logic integrity.

## Success Criteria

**Validation Requirements:**
- All conflicted files identified and analyzed for conflict type
- Both sides of each conflict understood (feature intent, business logic)
- Resolution strategy appropriate for file type (source, tests, config, docs)
- Post-resolution validation passes (build, test, lint)
- No semantic conflicts introduced (code merges but breaks functionality)

**Operation Steps:**
1. **Conflict Analysis**: Identify conflicted files and understand both branch contexts
2. **Intent Understanding**: Determine what each side was trying to achieve
3. **Type-Specific Resolution**: Apply appropriate resolution strategy by file type
4. **Quality Validation**: Run project-appropriate quality checks after resolution
5. **Documentation**: Create detailed commit message explaining resolution decisions
6. **Verification**: Ensure no semantic conflicts or functionality breaks

**Output Standards:**
- Context-aware resolution preserving functionality from both sides
- File-type specific resolution strategies (source, test, config, docs, lock files)
- Comprehensive post-resolution quality validation
- Detailed resolution summary with decisions documented
- Quality assurance recommendations for additional testing
- Clean commit message documenting resolution approach