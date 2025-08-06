---
description: Design technical infrastructure with boring technology preference. Use this to trigger @tech-task-designer agent.
coordination-pattern: sequential
quality-thresholds: [8/10+ design confidence, complexity justification, standard tooling validation]
evidence-requirements: [business value proposition, step-by-step implementation blueprint, risk assessment, boring technology validation]
complexity: medium
estimated-duration: 75
---

# Primary Goals
Execute comprehensive technical task design focused on simple, standard solutions using boring technology and proven patterns to solve specific business problems while avoiding over-engineering and unnecessary complexity.

# Sequential Agent Coordination

I'll coordinate technical design specialists sequentially in the main context for: $ARGUMENTS

## Design Workflow

**Step 1: Technical Design**
> Use the tech-task-designer agent to execute comprehensive technical design including problem analysis, boring technology validation, and implementation blueprint creation for: $ARGUMENTS

**Step 2: Architecture Validation with Problem-Size Assessment**  
> Use the architecture-validator agent to validate design decisions, complexity justification against problem size, and ensure architectural patterns serve actual requirements rather than theoretical best practices

**Step 3: Security Review Appropriate to Context**
> Use the security-planner agent to review security implications with threat model appropriate to the deployment context and problem scope

**Step 4: Technical Design Documentation**
> Use the tech-task-documentor agent to create implementation-ready technical design documentation

Each specialist operates in isolation and returns results for integration into the complete technical design.

# Success Criteria
- **8/10+ design confidence** achieved through comprehensive analysis
- **Complexity justification** with all decisions backed by requirements
- **Boring technology validation** confirming standard tooling usage
- **Implementation blueprint** with detailed sequence and specifications
- **Business value proposition** with measurable outcomes
- **Risk assessment** with fallback strategies
Please use @tech-task-designer to design: $ARGUMENTS

The technical design follows this comprehensive 8-phase approach:

## Phase 1: Problem Analysis and Current System Assessment
- Document specific business problem and technical constraints requiring solution
- Analyze current system architecture, dependencies, and integration points
- Identify performance requirements, scalability needs, and resource constraints
- Establish scope boundaries and clarify what problems are NOT being solved

## Phase 2: Simplicity Assessment and Boring Technology Validation
- Evaluate if existing standard tooling can solve the problem without custom development
- Research proven, boring technology solutions with established track records
- Validate Microsoft ecosystem solutions and standard .NET tooling capabilities
- Assess team expertise and maintenance requirements for proposed solutions

## Phase 3: Business Context Integration and Value Proposition
- Connect technical solution to specific business outcomes and measurable value
- Document cost-benefit analysis including development, maintenance, and operational costs
- Establish success metrics and key performance indicators for solution effectiveness
- Align technical approach with business priorities and strategic objectives

## Phase 4: Standard Pattern Research and Tooling Analysis
- Research existing tooling solutions within Microsoft ecosystem (.NET, Azure, etc.)
- Document standard implementation patterns and proven architectural approaches
- Evaluate existing enterprise patterns and established coding conventions
- Identify reusable components and library solutions before custom development

## Phase 5: Azure-First Implementation Strategy (When Applicable)
- Prioritize Azure-native solutions and platform-as-a-service offerings
- Research managed services and serverless options to minimize operational overhead
- Document integration patterns with existing Azure infrastructure
- Evaluate cost implications and scaling characteristics of Azure services

## Phase 6: Development-Time vs Runtime Complexity Assessment
- Default to development-time code generation over build-time complexity
- Choose checked-in generated code over runtime generation when appropriate
- Evaluate tooling like dotnet-svcutil, OpenAPI generators, and standard scaffolding
- Minimize runtime complexity through pre-compilation and code generation

## Phase 7: Design Validation and Complexity Justification
**Complexity Validation Questions**:
- Is complexity justified by actual requirements (not theoretical future needs)?
- Can standard Microsoft tooling solve this problem without custom implementation?
- Are we solving actual business problems or theoretical engineering challenges?
- Does solution use boring, proven technology with established support?
- Is the learning curve reasonable for team capabilities and timeline constraints?

## Phase 8: Implementation Blueprint Creation
- Create detailed step-by-step implementation guide with tooling specifications
- Document exact commands, configuration files, and generated code management
- Establish testing strategy and validation approaches
- Design monitoring, logging, and operational support requirements

**Key Design Principles**:
- **Prefer boring technology** and proven patterns over custom complexity and cutting-edge solutions
- **Use existing .NET tooling** in standard ways rather than complex integrations or custom frameworks
- **Default to development-time code generation** over build-time complexity and runtime generation
- **Choose checked-in generated code** over runtime generation when appropriate for maintainability
- **Research simple solutions** using existing tooling before considering custom implementations
- **Solve actual problems** rather than theoretical engineering challenges or future requirements

**Design Validation Requirements**:
- **Complexity justification**: Every architectural decision must be backed by specific requirements
- **Standard tooling validation**: Confirm existing tools cannot solve the problem before custom development
- **Problem-solution alignment**: Ensure we're solving actual business problems not theoretical ones
- **Boring technology preference**: Solution must use proven, well-supported technology
- **Implementation feasibility**: Target 8/10+ design confidence with executable implementation blueprint

**Confidence Gates**: Must achieve 8/10+ design confidence with complexity justification and standard tooling validation before transitioning to /implement-tech-task command (Phase 2).

After design completion with required confidence level, proceed with implementation using the /implement-tech-task command.