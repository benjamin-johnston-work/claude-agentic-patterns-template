---
description: Comprehensive implementation-ready research with codebase analysis and technology validation
argument-hint: [feature name or planning reference]
coordination-pattern: sequential
quality-thresholds: [9-10/10 implementation confidence, proven pattern validation]
evidence-requirements: [specific file paths and working code examples, technology compatibility validation, step-by-step implementation blueprint, external integration patterns]
complexity: high
estimated-duration: 90
---

# Primary Goals
Generate comprehensive implementation-ready research context that enables single-pass development success through detailed codebase analysis, technology stack validation, and proven pattern identification with enterprise architecture compliance.

# Agent Coordination
**Sequential Process - Phase 2 of 3-Phase Feature Development**
- **Phase 2**: @feature-researcher executes complete research workflow from codebase analysis to implementation blueprint
- **Coordination Pattern**: Single-agent sequential execution with confidence gating
- **Prerequisites**: Must follow /plan-feature command (Phase 1) with 7-10/10 planning confidence
- **Handoff Requirements**: Must achieve 9-10/10 implementation confidence before proceeding to Phase 3
- **Quality Gate**: Research must provide specific file paths, working code examples, and proven integration patterns

**Context Integration**: Inherits complete planning context including architecture decisions, bounded contexts, and implementation roadmap
**Next Phase Integration**: Results directly feed into /implement-feature command (Phase 3) for development execution

# Success Criteria
- **9-10/10 implementation confidence** achieved through comprehensive codebase analysis and proven pattern validation
- **Specific file paths and working code examples** from actual codebase analysis with integration points identified
- **Technology compatibility validation** with version-specific compatibility matrices and gotcha identification
- **Step-by-step implementation blueprint** with detailed error handling strategies and rollback procedures
- **External integration patterns** with authentication flows, security considerations, and data protection compliance
- **Testing strategy specification** with concrete test scenarios and validation approaches

# Feature Research Process
Please use @feature-researcher to research: $ARGUMENTS

The feature research follows this comprehensive 8-phase approach:

## Phase 1: Planning Context Loading
- Import complete feature plan and architectural decisions from @feature-planner (Phase 1)
- Load bounded context definitions, domain relationships, and integration requirements
- Review implementation roadmap, risk assessments, and success criteria
- Establish research scope based on planning confidence and complexity assessment

## Phase 2: Codebase Deep Analysis
- Document specific file paths, existing patterns, and architectural implementations
- Analyze current Onion Architecture layer implementations (L1→L2→L3→L4)
- Map existing domain models, repositories, services, and integration patterns
- Identify reusable components, utilities, and established enterprise conventions

## Phase 3: Technology Stack Validation
- Verify technology compatibility with current framework versions and dependencies
- Create compatibility matrices for all external libraries and services
- Validate package versions, security updates, and long-term support status
- Document upgrade paths and migration requirements for incompatible components

## Phase 4: External Service Analysis
- Research API patterns, authentication flows, and integration requirements
- Document data flow requirements, request/response formats, and error handling
- Analyze rate limiting, throttling, and reliability patterns
- Establish monitoring, logging, and observability requirements

## Phase 5: Implementation Examples and Proven Patterns
- Find concrete working code samples from similar implementations
- Document proven integration patterns from existing codebase
- Collect working, tested code examples with performance characteristics
- Establish coding standards and enterprise pattern compliance examples

## Phase 6: Gotcha and Pitfall Identification
- Document known issues with version-specific solutions and workarounds
- Identify common integration pitfalls and proven resolution strategies
- Catalog performance bottlenecks and optimization approaches
- Establish debugging strategies and diagnostic approaches

## Phase 7: Testing Strategy Design
- Plan unit testing approach with NO MOCKING policy enforcement
- Design integration tests using real service dependencies
- Create end-to-end test scenarios covering critical business workflows
- Establish performance testing and load validation requirements

## Phase 8: Implementation Blueprint Creation
- Create detailed step-by-step development guide with sequence specifications
- Document error handling strategies and exception management patterns
- Establish rollback procedures and feature flag implementation approaches
- Design monitoring and alerting requirements for production deployment

**Research Deliverables**:
- **Specific file paths and code snippets** from comprehensive codebase analysis
- **URLs to relevant documentation** with highlighted critical sections and implementation notes
- **Concrete implementation examples** with working, tested code and performance benchmarks
- **Version-specific compatibility matrices** with issue identification and proven solutions
- **Comprehensive error handling strategies** and edge case management approaches
- **Testing approach specification** matching application complexity with concrete validation scenarios

**Confidence Gates**: Must achieve 9-10/10 implementation confidence with proven pattern validation before transitioning to /implement-feature command (Phase 3).

# Multi-Phase Validation and Documentation

After @feature-researcher completes the research workflow and achieves 9-10/10 implementation confidence, validate the research through specialized reviewers before documentation synthesis:

## Phase 1: Security Research Validation
Use @security-investigator to validate security research findings:
- **External integration security** reviewing API security patterns and authentication flows
- **Data protection implementation** validating privacy requirements and data handling approaches
- **Security testing strategy** ensuring security test scenarios are comprehensive
- **Vulnerability mitigation** confirming identified security risks have appropriate solutions
- **Compliance validation** ensuring research aligns with enterprise security policies

## Phase 2: Performance Analysis
Use @performance-investigator to review performance considerations:
- **Performance bottleneck identification** validating optimization approaches and benchmarks  
- **Load testing strategy** ensuring performance test scenarios cover realistic usage patterns
- **Scalability analysis** confirming architecture can handle projected growth
- **Resource utilization** validating infrastructure requirements and cost implications
- **Performance monitoring** ensuring adequate observability for production deployment

## Phase 3: Architecture Research Validation
Use @architecture-validator to validate architectural research decisions:
- **Pattern implementation accuracy** confirming Onion Architecture compliance in research
- **Integration pattern validation** ensuring external service integration follows enterprise patterns
- **DDD implementation alignment** validating domain model and bounded context implementations
- **Technology stack appropriateness** confirming technology choices align with enterprise standards
- **Implementation blueprint accuracy** ensuring step-by-step guidance follows established patterns

## Phase 4: Research Documentation
After validation phases complete, synthesize all research and validation context into structured documentation:

### Research Document Generation
Create comprehensive research documentation in: `docs/development/features/business/{feature-name}/02-{feature-name}-research-FEAT-{timestamp}.md`

The research document should synthesize all research and validation context:

### Research Structure
- **Research ID and Metadata**: Unique identifier linking to planning phase
- **Planning Context Integration**: Summary of architectural decisions from Phase 1
- **Codebase Analysis Results**: Specific file paths, patterns, and integration points
- **Technology Validation**: Compatibility matrices and version requirements
- **Security Implementation**: Authentication, authorization, and data protection patterns
- **Performance Considerations**: Optimization strategies and monitoring requirements
- **Implementation Blueprint**: Step-by-step development guide with concrete examples
- **Testing Strategy**: Comprehensive test approach with specific scenarios
- **Risk Mitigation**: Identified gotchas and proven resolution strategies
- **Validation Results**: Summary of security, performance, and architecture validation findings

### File Naming Convention
Use format: `02-{feature-name}-research-FEAT-{timestamp}.md` where:
- {feature-name} matches the planning phase naming
- {timestamp} is YYYY-MMDD-HHMMSS format for the research completion time

## Phase 5: Documentation Quality Validation
Finally, use the feature-documentor agent to validate documentation compliance:
- **Structure compliance** with feature research documentation standards
- **Content completeness** ensuring all research phases and validation results are documented
- **Implementation readiness** confirming research enables single-pass development success
- **Evidence quality** validating that all claims are supported with concrete examples
- **Formatting consistency** with established documentation patterns

### Documentation Requirements
- All security considerations must be validated by security-investigator agent
- Performance analysis must be reviewed by performance-investigator agent
- Architectural research must be validated by architecture-validator agent
- All validation findings must be incorporated into the final research document
- Research must enable /implement-feature command execution with complete validated context
- Documentation must pass feature-documentor agent validation before research is considered complete

This multi-phase validation ensures research findings are comprehensive, accurate, and properly validated before proceeding to the implementation phase.