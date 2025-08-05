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

# Sequential Agent Coordination

I'll coordinate feature planning specialists sequentially in the main context for: $ARGUMENTS

## Planning Workflow

**Step 1: Feature Planning**
> Use the feature-planner agent to execute comprehensive feature planning from size assessment to architecture design

**Step 2: Architecture Validation** 
> Use the architecture-validator agent to validate architectural decisions, DDD context mapping, and enterprise pattern consistency

**Step 3: Security Review**
> Use the security-planner agent to review security considerations, authentication patterns, and data protection compliance

**Step 4: Feature Documentation**
> Use the feature-documentor agent to create implementation-ready feature specification documentation

Each specialist operates in isolation and returns results for integration into the final feature plan.

# Success Criteria
- **7-10/10 planning confidence** achieved through comprehensive analysis
- **Size-appropriate architecture** with complexity matching current application scale  
- **Evidence-based complexity justification** for all architectural decisions
- **Clear implementation roadmap** with defined Onion Architecture layers
- **DDD context mapping** with bounded context identification
- **Security validation** ensuring enterprise compliance

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

The feature-planner agent will handle the comprehensive 8-phase planning process outlined above, then I'll coordinate the validation specialists in sequence to create the final validated feature plan.

## Integration Process

After each specialist completes their work, I'll integrate their outputs into a comprehensive feature plan document following the established structure:

### Documentation Output
- **Location**: `docs/development/features/business/{feature-name}/01-{feature-name}-plan-FEAT-{timestamp}.md`
- **Structure**: Complete feature plan with all specialist validations integrated
- **Format**: Enterprise documentation standards with evidence-based decisions

This ensures feature plans are architecturally sound, secure, and properly documented before proceeding to the research phase.