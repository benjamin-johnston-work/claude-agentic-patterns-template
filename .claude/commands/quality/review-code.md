---
description: Comprehensive code review with parallel specialist analysis and coordinated reporting
argument-hint: "[file/directory path] - Code to review"
allowed-tools: [Task]
coordination-pattern: parallel
quality-thresholds: [95% confidence from all analysts, security vulnerability identification, performance bottleneck detection]
evidence-requirements: [specific file:line references, concrete implementation examples, prioritized recommendation list, executive summary]
complexity: medium
estimated-duration: 45
---

Review the code: $ARGUMENTS

## Primary Goals

Deliver a comprehensive code quality assessment through coordinated parallel analysis by specialized agents, providing actionable insights for security, performance, architecture, and documentation improvements.

## Agent Coordination

**PARALLEL EXECUTION** - Execute simultaneously for comprehensive coverage:

1. **security-investigator agent**: Security vulnerability assessment
   - Identify security anti-patterns and vulnerabilities
   - Validate input sanitization and authentication flows
   - Check for hardcoded secrets and insecure configurations
   - Assess API security and data exposure risks

2. **performance-investigator agent**: Performance optimization analysis  
   - Identify performance bottlenecks and anti-patterns
   - Analyze resource usage and memory management
   - Validate database query efficiency and caching strategies
   - Assess algorithmic complexity and optimization opportunities

3. **architecture-validator agent**: Architecture and design pattern validation
   - Validate adherence to established patterns (SOLID, DDD, Clean Architecture)
   - Assess separation of concerns and dependency management
   - Evaluate code organization and modular design
   - Check enterprise pattern implementation when applicable

**SEQUENTIAL EXECUTION** - After parallel analysis completion:

4. **feature-documentor agent**: Documentation quality and completeness
   - Consolidate findings from all analysts
   - Generate comprehensive review report with prioritized recommendations
   - Ensure all issues have specific file:line references
   - Provide concrete fix suggestions with implementation examples

## Success Criteria

**Confidence Gates:**
- All four agents must complete their analysis with 95% confidence
- Security analysis must identify any critical vulnerabilities (severity >= 7.0)  
- Performance analysis must flag any issues causing >100ms delays
- Architecture validation must confirm pattern adherence or document deviations
- Documentation must provide actionable fixes for all identified issues

**Validation Requirements:**
- Repository technology stack automatically detected and validated
- Analysis adapted to actual codebase patterns and conventions
- All findings prioritized by business impact and technical risk
- Integration safety verified for external dependencies
- Legacy code assessment included with modernization recommendations

**Output Standards:**
- Specific file:line references for all issues
- Concrete implementation examples for fixes
- Prioritized recommendation list with effort estimates
- Executive summary with key findings and risk assessment

# Code Review Documentation

After all parallel analysis and feature-documentor agent coordination complete, synthesize findings into a structured code review document:

## Review Report Generation
Create comprehensive code review documentation in: `docs/development/reviews/REVIEW-YYYY-MMDD-HHMMSS-{scope}.md`

The review report should synthesize all parallel analysis findings:

### Review Structure
- **Review ID and Metadata**: Unique identifier with timestamp and scope classification
- **Review Scope**: Files, directories, or components reviewed with change context
- **Executive Summary**: High-level findings and risk assessment from all analyzers
- **Security Analysis Results**: Vulnerabilities, risks, and mitigation strategies
- **Performance Analysis Results**: Bottlenecks, optimization opportunities, and resource usage
- **Architecture Analysis Results**: Pattern compliance, design quality, and structural recommendations
- **Critical Issues**: Highest priority findings requiring immediate attention
- **Recommendations**: Prioritized action items with effort estimates and implementation guidance
- **Risk Assessment**: Business and technical risk classification

### File Naming Convention
Use format: `REVIEW-YYYY-MMDD-HHMMSS-{scope}.md` where:
- YYYY-MMDD-HHMMSS is the review completion timestamp
- {scope} describes the review scope (e.g., "payment-module", "api-security", "full-codebase")

## Final Documentation Validation
After creating the review report, use the feature-documentor agent to validate final documentation quality:

Please use the feature-documentor agent to validate the generated code review report for:
- **Structure compliance** with code review documentation standards
- **Content completeness** ensuring all analysis findings are properly documented
- **Actionability** confirming all recommendations have specific implementation guidance
- **Formatting consistency** with established documentation patterns
- **Risk communication** ensuring business and technical risks are clearly communicated

### Documentation Requirements
- All findings must have specific file:line references
- Security vulnerabilities must be clearly classified by severity
- Performance issues must include impact assessment and optimization strategies
- Architecture recommendations must reference established patterns
- All recommendations must include effort estimates and implementation examples
- Review must enable informed decision-making on code quality improvements
- Documentation must pass feature-documentor agent validation before review is considered complete

This comprehensive approach ensures code reviews provide actionable insights while maintaining consistent documentation standards.