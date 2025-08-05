---
description: Intelligent bug fix implementation with adaptive validation and mandatory documentation updates
argument-hint: [bug reference or investigation context]
---

# Sequential Bug Fix Implementation

I'll coordinate bug fix specialists sequentially in the main context for: $ARGUMENTS

## Fix Implementation Workflow

**Step 1: Bug Fix Implementation**
> Use the bug-fixer agent to implement the validated fix using investigation context from the central bug document

**Step 2: Code Review**
> Use the code-reviewer agent to validate fix quality, ensure no regressions, and verify enterprise pattern compliance

**Step 3: Quality Assurance**
> Use the qa-validator agent to ensure comprehensive test coverage including regression tests and edge case validation

**Step 4: Performance Validation (if applicable)**
> Use the performance-investigator agent to validate performance impact and ensure no performance regressions

**Step 5: Security Review (if applicable)**
> Use the security-investigator agent to validate security implications of the fix

**Step 6: Bug Fix Documentation Update**
> Use the bug-documentor agent to update bug documentation with fix details and validation results

Each specialist operates in isolation and returns results for integration into the complete bug fix.

## Context Integration

The bug-fixer agent will automatically inherit investigation context from the central bug document including:
- Validated root cause analysis with 95% confidence
- Solution approach approved by domain experts
- Specific fix locations and implementation guidance
- Complexity, risk, and size assessments
5. **Apply quality gates** with risk-appropriate validation requirements
6. **Update documentation** using @documentor to add comprehensive fix implementation details to central bug document

## Adaptive Validation Patterns

**Critical Risk (Production Outages)**:
- Comprehensive parallel validation with all validators
- All quality gates must pass before deployment
- Duration: 45-60 minutes with maximum validation coverage

**High Risk (Production Non-Critical)**:
- Focused parallel validation with core validators
- Core quality gates must pass
- Duration: 30-45 minutes with targeted validation

**Medium Risk (Staging/Internal)**:
- Adaptive validation with expansion based on fix complexity
- Primary domain validation required
- Duration: 20-35 minutes with intelligent resource usage

**Low Risk (Development)**:
- Minimal targeted validation with single domain focus
- Basic effectiveness validation required
- Duration: 15-25 minutes with efficient resource usage

## Expected Outcomes

- **Root cause resolution** with fix completely addressing validated root cause
- **Quality gate compliance** with all risk-appropriate validation requirements met
- **Performance preservation** with no performance regression introduced
- **Documentation completeness** with central bug document updated with comprehensive fix details
- **Validation evidence** with all validation results documented with concrete evidence

## Integration Benefits

- **Prevents multiple failed attempts** through evidence-based implementation
- **Optimizes resource usage** with risk-appropriate validation intensity
- **Maintains central source of truth** with complete investigation + fix lifecycle documentation
- **Enables knowledge transfer** with comprehensive documentation for future learning

## Examples

Fix critical production authentication issue:
```bash
/development/bugs/fix-bug "BUG-20241231-103000-azure-auth-failure"
# Inherits: complexity=4, risk=critical, triggers comprehensive parallel validation
```

Fix medium staging configuration issue:
```bash
/development/bugs/fix-bug "BUG-20241231-140000-staging-config-mismatch"  
# Inherits: complexity=2, risk=medium, triggers adaptive focused validation
```

Fix simple development issue:
```bash
/development/bugs/fix-bug "BUG-20241231-160000-dev-connection-error"
# Inherits: complexity=1, risk=low, triggers minimal targeted validation
```

The @bug-fixer ensures validation effort matches issue characteristics while maintaining comprehensive documentation as the central source of truth for the complete investigation and resolution lifecycle.