---
description: Enterprise architecture feature planning with size-appropriate complexity assessment
argument-hint: [feature description or user story]
coordination-pattern: sequential
quality-thresholds: [7-10/10 planning confidence, evidence-based complexity justification]
evidence-requirements: [size-appropriate architecture, DDD context mapping, clear implementation roadmap, risk mitigation strategy]
complexity: high
estimated-duration: 120
---

# Primary Goals
Execute comprehensive feature planning that matches architectural complexity to application size reality, establishing clear implementation roadmap and bounded context definitions while avoiding over-engineering through evidence-based design decisions.

# Agent Coordination
**Sequential Process - Phase 1 of 3-Phase Feature Development**
- **Phase 1**: @feature-planner executes complete planning workflow from size assessment to architecture design
- **Coordination Pattern**: Single-agent sequential execution with confidence gating
- **Handoff Requirements**: Must achieve 7-10/10 planning confidence before proceeding to Phase 2
- **Quality Gate**: All architectural decisions must be justified with evidence-based complexity assessment

**Next Phase Integration**: Results directly feed into /research-feature command (Phase 2) for implementation-ready research

# Success Criteria
- **7-10/10 planning confidence** achieved through comprehensive analysis before proceeding to research phase
- **Size-appropriate architecture** with complexity matching current application scale and realistic growth projections
- **Evidence-based complexity justification** for all architectural decisions with cost-benefit analysis
- **Clear implementation roadmap** with defined Onion Architecture layers, development sequence, and integration points
- **DDD context mapping** with bounded context identification, domain relationships, and integration patterns
- **Risk mitigation strategy** with identified challenges and concrete mitigation approaches

# Feature Planning Process
Please use @feature-planner to plan: $ARGUMENTS

The feature planning follows this enterprise-grade 8-phase approach:

## Phase 1: Application Size Assessment
- Evaluate current codebase scale, complexity, and team capacity constraints
- Analyze existing system boundaries and scalability requirements
- Document current architecture patterns and technical debt considerations
- Assess realistic growth projections vs. theoretical future requirements

## Phase 2: Architecture Pattern Detection
- Identify existing Onion Architecture layers (L1→L2→L3→L4) and established patterns
- Document current dependency injection patterns and abstraction layers
- Map existing domain models, repositories, and service implementations
- Analyze established enterprise conventions and coding standards

## Phase 3: Complexity Matching and Justification
- Ensure architectural decisions match application reality and team capabilities
- Justify complexity choices with evidence-based cost-benefit analysis
- Avoid over-engineering by validating necessity of each architectural component
- Document decision rationale with specific requirements driving complexity needs

## Phase 4: DDD Context Mapping
- Define bounded contexts with clear domain boundaries and responsibilities
- Map domain relationships, integration patterns, and shared kernel areas
- Identify aggregate roots, entities, and value objects within each context
- Establish context integration strategies and anti-corruption layer needs

## Phase 5: C4 Architecture Diagrams
- Create system context diagrams showing external dependencies and integrations
- Design container diagrams illustrating high-level system components
- Develop component diagrams detailing internal module relationships
- Document code-level patterns for complex architectural areas

## Phase 6: Implementation Roadmap
- Define layer-by-layer development approach following Onion Architecture sequence
- Establish development milestones with clear deliverables and acceptance criteria
- Plan integration points and dependency resolution strategies
- Create testing strategy aligned with architectural layers and business requirements

## Phase 7: Risk Assessment and Mitigation
- Identify potential technical challenges, integration complexities, and scalability concerns
- Document external dependencies, third-party service risks, and performance considerations
- Establish mitigation strategies with concrete fallback approaches
- Plan monitoring and observability requirements for production validation

## Phase 8: Success Metrics and Acceptance Criteria
- Establish measurable outcomes for feature success and business value delivery
- Define technical acceptance criteria including performance, security, and maintainability requirements
- Create user acceptance criteria with clear workflow validation points
- Document rollback strategies and feature flag considerations

**Key Principles**:
- **Size first, architecture second** - avoid over-engineering through evidence-based complexity assessment
- **Prefer proven, simple solutions** over cutting-edge complexity without clear business justification
- **Design for change** without over-building for imagined future requirements
- **All architectural decisions must justify their cost** with specific business or technical requirements
- **Evidence-based validation** of complexity appropriateness and implementation feasibility

**Confidence Gates**: Must achieve 7-10/10 planning confidence with evidence-based justification before transitioning to /research-feature command (Phase 2).

# Multi-Phase Validation and Documentation

After @feature-planner completes the planning workflow and achieves 7-10/10 confidence, validate the plan through specialized reviewers before documentation synthesis:

## Phase 1: Architecture Validation
Use @architecture-validator to validate architectural decisions:
- **Onion Architecture compliance** with proper layer separation and dependency direction
- **DDD context mapping accuracy** with appropriate bounded contexts and domain relationships
- **Enterprise pattern consistency** with existing codebase architecture
- **Complexity justification** ensuring architectural decisions match application size and needs
- **Integration pattern validation** for external service dependencies and system boundaries

## Phase 2: Security Review
Use @security-analyzer to review security considerations:
- **Authentication and authorization patterns** appropriate for the feature requirements
- **Data protection compliance** with privacy requirements and data handling standards
- **External integration security** including API security and third-party service risks
- **Input validation and sanitization** requirements for user-facing components
- **Security architecture alignment** with enterprise security policies

## Phase 3: Feature Plan Documentation
After validation phases complete, synthesize all planning and validation context into structured documentation:

### Feature Plan Generation
Create comprehensive planning documentation in: `docs/development/features/business/{feature-name}/01-{feature-name}-plan-FEAT-{timestamp}.md`

The feature plan should synthesize all planning and validation context:

### Plan Structure
- **Feature ID and Metadata**: Unique identifier with timestamp and feature classification
- **Feature Overview**: Clear description of feature purpose and business value
- **Application Size Assessment**: Current codebase analysis and complexity matching
- **Architecture Design**: Onion Architecture layers and established patterns integration
- **DDD Context Mapping**: Bounded contexts, domain relationships, and integration patterns
- **Security Considerations**: Authentication, authorization, and data protection requirements
- **Implementation Roadmap**: Layer-by-layer development sequence with milestones
- **Risk Assessment**: Technical challenges and mitigation strategies
- **Validation Results**: Summary of architecture and security validation findings
- **Success Metrics**: Measurable outcomes and acceptance criteria

### File Naming Convention
Use format: `01-{feature-name}-plan-FEAT-{timestamp}.md` where:
- {feature-name} is a URL-safe version of the feature name
- {timestamp} is YYYY-MMDD-HHMMSS format for the planning completion time

### Folder Structure
- **Business features**: `docs/development/features/business/{feature-name}/`
- **Technical features**: `docs/development/features/technical/{feature-name}/`

## Phase 4: Documentation Quality Validation
Finally, use @documentor to validate documentation compliance:
- **Structure compliance** with feature planning documentation standards
- **Content completeness** ensuring all planning phases and validation results are documented
- **Evidence quality** confirming architectural and security decisions are well-supported
- **Formatting consistency** with established documentation patterns
- **Implementation readiness** ensuring plan enables successful research phase

### Documentation Requirements
- All architectural decisions must be validated by @architecture-validator
- Security considerations must be reviewed by @security-analyzer  
- All validation findings must be incorporated into the final plan
- Plan must enable /research-feature command execution with complete validated context
- Documentation must pass @documentor validation before planning is considered complete

This multi-phase validation ensures feature plans are architecturally sound, secure, and properly documented before proceeding to the research phase.