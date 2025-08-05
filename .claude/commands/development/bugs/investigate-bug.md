---
description: Intelligent bug investigation with adaptive validation and comprehensive documentation
argument-hint: [bug description] --complexity=[1-5] --risk=[low|medium|high|critical] --size=[small|medium|large|enterprise]
---

# Sequential Bug Investigation

I'll coordinate bug investigation specialists sequentially in the main context for: $ARGUMENTS

## Investigation Workflow

**Step 1: Bug Investigation**
> Use the bug-investigator agent to execute comprehensive investigation including categorization, symptoms analysis, and initial root cause identification

**Step 2: Hypothesis Validation**
> Use the hypothesis-validator agent to systematically validate investigation hypotheses and ensure evidence-based analysis

**Step 3: Security Analysis (if applicable)**
> Use the security-investigator agent to assess security implications and identify potential security-related root causes

**Step 4: Performance Analysis (if applicable)**
> Use the performance-investigator agent to identify performance-related factors and bottlenecks

**Step 5: Architecture Review (if applicable)**
> Use the architecture-validator agent to validate architectural factors and system boundary issues

**Step 6: Bug Investigation Documentation**
> Use the bug-documentor agent to create comprehensive bug investigation documentation as central source of truth

Each specialist operates in isolation and returns results for integration into the complete investigation.

## Investigation Goals
- Achieve 95% confidence in root cause identification
- Create comprehensive documentation as central source of truth
- Establish validated solution approach ready for implementation

**Investigation Context**:

**Investigation Parameters**:
- Complexity assessment: Use provided --complexity parameter or auto-detect from symptoms
- Risk assessment: Use provided --risk parameter or auto-detect from environment context
- Size assessment: Use provided --size parameter or auto-detect from scope indicators

Please use @bug-investigator to investigate: $ARGUMENTS

## Expected Outcomes

- **95% confidence** root cause identification with evidence from multiple domains
- **Central bug document** at `docs/development/bugs/BUG-YYYY-MMDD-HHMMSS-{slug}.md`
- **Cross-domain consensus** on root cause and solution approach
- **Investigation methodology** documented for future learning
- **Fix guidance** with specific locations and implementation strategy

## Integration

Investigation results automatically provide complete context for the fix-bug command including validated root cause, solution approach, and comprehensive documentation foundation.

## Examples

Critical production authentication issue:
```bash
/development/bugs/investigate-bug "Azure AD authentication failing with 500 errors affecting all users" --complexity=4 --risk=critical --size=large
```

Medium complexity staging issue:
```bash  
/development/bugs/investigate-bug "User features not displaying after login in staging environment" --complexity=3 --risk=medium --size=medium
```

Simple development configuration issue:
```bash
/development/bugs/investigate-bug "Application fails to start due to connection string error" --complexity=1 --risk=low --size=small
```