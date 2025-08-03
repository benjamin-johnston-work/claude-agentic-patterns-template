---
description: Comprehensive project planning using specialized planning agents with manager-worker coordination
argument-hint: [project description] --scope=[small|medium|large|enterprise] --timeline=[months]
---

# Comprehensive Project Planning

Execute complete project planning using the specialized @planners/project-planner master coordinator with intelligent specialist coordination following official Claude Code patterns.

## Usage

This command triggers the @planners/project-planner master coordinator which will:
- Perform evidence-first project analysis with systematic confidence building
- Coordinate specialized planning agents using explicit Task tool invocation
- Integrate architecture, technology, and validation specialist outputs
- Create comprehensive project plan documentation in docs/projects/ folder
- Achieve 8/10+ confidence in project feasibility and approach

## Parameters

- **scope**: small/medium/large/enterprise (project complexity and resource requirements)
- **timeline**: Expected project duration in months for resource planning
- **--tech-stack**: Optional technology preferences (e.g., ".net-core,azure,react")
- **--architecture**: Optional architecture preferences (e.g., "microservices,ddd,onion")

## Project Planning Process

Please use the Task tool to launch @planners/project-planner with the following project planning request:

**Project Description**: $ARGUMENTS

**Planning Parameters**:
- Scope assessment: Use provided --scope parameter or auto-detect from project complexity
- Timeline framework: Use provided --timeline parameter or estimate from project scope
- Technology preferences: Use provided --tech-stack parameter or determine optimal stack
- Architecture preferences: Use provided --architecture parameter or select enterprise patterns

The @planners/project-planner will:

### Phase 1: Project Analysis and Requirements
- Evidence-first project requirements analysis (10% → 95% confidence building)
- Business objective extraction and success criteria definition
- Scope, constraint, and assumption documentation
- Stakeholder requirement analysis and validation

### Phase 2: Specialized Planning Coordination
The master coordinator will use explicit Task tool invocation for specialist coordination:

**Architecture Planning**:
```
Use the `architecture-planner` agent to design the enterprise architecture for:
- Project requirements: [extracted from analysis]
- Business context: [business objectives and constraints]
- Architecture preferences: [specified or determined preferences]
- Integration requirements: [system integration needs]
```

**Technology Stack Planning**:
```
Use the `tech-stack-planner` agent to select and plan the technology stack for:
- Architecture approach: [from architecture-planner results]
- Project requirements: [technical and performance requirements]
- Technology preferences: [specified or recommended technologies]
- Integration patterns: [technology integration requirements]
```

**Project Validation**:
```
Use the `project-validator` agent to validate the complete project plan:
- Architecture design: [comprehensive architecture from specialist]
- Technology selection: [complete technology stack from specialist]  
- Resource requirements: [estimated team and infrastructure needs]
- Risk assessment: [identified risks and mitigation strategies]
```

**Security Requirements**:
```
Use the `security-analyzer` agent to validate security requirements for:
- Project architecture: [security architecture implications]
- Technology stack: [technology security considerations]
- Compliance requirements: [enterprise security and compliance standards]
- Risk mitigation: [security risk assessment and mitigation]
```

### Phase 3: Plan Integration and Quality Assurance
- Assumption detection using @hypothesis-validator to prevent planning assumptions
- Cross-domain specialist output integration and conflict resolution
- Plan coherence validation and quality assessment
- Evidence-based confidence building to 8/10+ threshold

### Phase 4: Documentation Synthesis
```
Use the `documentor` agent to create comprehensive project plan documentation:
- Integrated project plan: [synthesis of all specialist planning outputs]
- Documentation location: docs/projects/project-{slug}.md
- Target audience: Technical and business stakeholders
- Content structure: Executive summary, architecture, technology, validation, implementation roadmap
```

## Expected Outcomes

- **8/10+ confidence** project plan with evidence-based feasibility validation
- **Comprehensive project plan** at `docs/projects/project-{slug}.md`
- **Enterprise architecture design** with C4 diagrams and DDD patterns
- **Complete technology stack** specification with Azure services and .NET technologies
- **Risk assessment and mitigation** strategies for identified project risks
- **Resource requirements** analysis with team and infrastructure specifications
- **Implementation roadmap** with phases, milestones, and timeline estimates

## Integration with Implementation Commands

Project planning results automatically provide complete context for implementation commands:
- `/development/features/implement-feature` can use project architecture and technology specifications
- `/development/technical/implement-tech-task` can leverage project technology stack and patterns
- `/development/technical/design-tech-task` can follow project architecture and design decisions

## Examples

### Enterprise E-commerce Platform
```bash
/project/plan-project "Design a platform that works like tetris on azure" --scope=medium --timeline=6 --tech-stack=".net-core,azure,react"
```

### Microservices Platform
```bash
/project/plan-project "Multi-tenant SaaS platform with microservices architecture" --scope=large --timeline=12 --architecture="microservices,ddd,event-driven"
```

### Simple Business Application
```bash
/project/plan-project "Employee management system with reporting dashboard" --scope=small --timeline=3 --tech-stack=".net-core,azure-sql,blazor"
```

## Manager-Worker Coordination Benefits

**Context Isolation**: Each specialist operates in isolated context window for deep domain focus
**Quality Through Specialization**: Architecture, technology, and validation specialists provide expert analysis
**Explicit Coordination**: Master coordinator uses clear Task tool invocation following official Claude Code patterns
**Human Oversight**: Strategic decision points for business alignment and approval
**Resource Efficiency**: Parallel specialist work with intelligent master coordination

This command follows official Claude Code documentation for manager-worker patterns with single responsibility agents, context isolation, and explicit sub-agent coordination.