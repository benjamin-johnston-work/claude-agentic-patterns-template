---
description: Intelligent bug fix implementation with adaptive validation and mandatory documentation updates
argument-hint: [bug reference or investigation context]
---

# Intelligent Bug Fix Implementation

Execute intelligent bug fix implementation using the specialized @bug-fixer agent with adaptive validation patterns and comprehensive documentation updates.

## Usage

This command triggers the @bug-fixer agent which will:
- Inherit complete investigation context from central bug document
- Determine optimal validation patterns based on risk and complexity
- Coordinate appropriate validators using intelligent resource optimization
- Implement test-first approach with comprehensive regression prevention
- Update central bug document with complete fix implementation details

## Context Inheritance

The @bug-fixer automatically inherits investigation context including:
- Validated root cause analysis with 95% confidence
- Solution approach approved by domain experts
- Specific fix locations and implementation guidance  
- Complexity, risk, and size assessments
- Documentation foundation from central bug document

## Fix Implementation Process

Please use the Task tool to launch @bug-fixer with the following fix request:

**Fix Target**: $ARGUMENTS

**Context Source**: The @bug-fixer will automatically locate and load the investigation context from the central bug document, typically at `docs/development/bugs/BUG-YYYY-MMDD-HHMMSS-{slug}.md`.

The @bug-fixer will:
1. **Load investigation context** from central bug document with validated root cause and solution approach
2. **Determine validation pattern** (comprehensive_parallel, focused_parallel, adaptive_focused, or minimal_targeted) based on inherited risk and complexity
3. **Coordinate implementation** with test-first approach and real-time validator feedback
4. **Execute adaptive validation** using Task tool to launch appropriate validators (@qa-validator, @performance-analyzer, @architecture-validator, @security-analyzer, plus investigation domain validators)
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