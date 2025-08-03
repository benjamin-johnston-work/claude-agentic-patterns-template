---
description: Intelligent bug investigation with adaptive validation and comprehensive documentation
argument-hint: [bug description] --complexity=[1-5] --risk=[low|medium|high|critical] --size=[small|medium|large|enterprise]
---

# Intelligent Bug Investigation

Execute comprehensive bug investigation using the specialized @bug-investigator agent with intelligent validator coordination and adaptive execution patterns.

## Usage

This command triggers the @bug-investigator agent which will:
- Automatically categorize the bug based on symptoms and context
- Select appropriate specialist validators using intelligent algorithms
- Execute optimal investigation patterns (parallel/sequential based on complexity/risk)
- Create comprehensive documentation as central source of truth
- Achieve 95% confidence in root cause identification

## Parameters

- **complexity**: 1-5 scale (1=simple config, 5=enterprise architecture)
- **risk**: low/medium/high/critical (based on environment impact)  
- **size**: small/medium/large/enterprise (based on investigation scope)

## Investigation Process

Please use the Task tool to launch @bug-investigator with the following investigation request:

**Bug Description**: $ARGUMENTS

**Investigation Parameters**:
- Complexity assessment: Use provided --complexity parameter or auto-detect from symptoms
- Risk assessment: Use provided --risk parameter or auto-detect from environment context
- Size assessment: Use provided --size parameter or auto-detect from scope indicators

The @bug-investigator will:
1. **Analyze and categorize** the bug using intelligent categorization algorithms
2. **Select optimal validators** from available specialists (@auth-flow-analyzer, @config-validator, @log-analyzer, @startup-dependency-analyzer, @security-analyzer, @performance-analyzer, @architecture-validator, @hypothesis-validator)
3. **Determine execution pattern** (parallel_comprehensive, parallel_focused, adaptive_sequential_parallel, or sequential_optimized)
4. **Coordinate investigation** using Task tool to launch appropriate validators
5. **Synthesize findings** with cross-domain evidence correlation and consensus building
6. **Create documentation** using @documentor for central source of truth bug document

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