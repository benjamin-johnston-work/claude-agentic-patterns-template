---
description: Fast parallel MVP-to-enterprise project planning using simultaneous specialist agents for rapid prototyping and testing
argument-hint: [project description] --tech-stack=[preferences]
---

# Fast Parallel MVP-to-Enterprise Project Planning

I'll coordinate specialist agents in parallel for rapid project planning, then synthesize results into complete MVP evolution documentation for: $ARGUMENTS

## Planning Philosophy

**Smart Evolution Planning**:
- Plan database schema and architecture to avoid refactoring as you scale. You may not implement this in MVP but should be planned upfront.
- Design API contracts that can evolve backward-compatibly
- Design API contracts that can evolve backward-compatibly
- Choose technology stack that grows from MVP to enterprise without migrations

**MVP-First Implementation**:
- Build only features needed for validation in first iteration
- Each evolution phase adds business value incrementally
- Architecture supports growth but starts with minimal complexity
- For example do not include a database or persistant cache unless these are needed for the MVP. 

## Planning Approach

**Planning Scope**: Complete evolution path from MVP to enterprise
**Implementation Scope**: Start with MVP validation features only

Agents should plan the complete journey but specify:
- What to build in MVP (minimal features for validation)
- What to build in each iteration (incremental value)  
- How architecture supports this growth without refactoring

## Fast Parallel Planning Workflow

**Step 1: Parallel Specialist Agent Execution (Max 4 concurrent)**

⚠️ **Resource Management**: This spawns 4 agents simultaneously. Ensure system resources available.

Launch 4 specialist agents in parallel with shared context:

> Use the architecture-planner agent to design the complete MVP-to-enterprise evolution path including database-first design, clean architecture from day one, C4 evolution diagrams showing MVP → Transitional → End State progression, and enterprise patterns that avoid refactoring throughout the journey.

> Use the tech-stack-planner agent to select technologies that support the entire evolution journey from 2-4 week MVP delivery through enterprise scale without requiring technology migrations or architectural refactoring.

> Use the project-validator agent to validate the feasibility of delivering working software in 2-4 weeks AND the complete evolution path to enterprise scale, ensuring realistic timelines and AI implementation compatibility for the entire journey.

> Use the architecture-validator agent to validate that enterprise standards (Onion Architecture, DDD, Clean Architecture) are maintained throughout all evolution phases from MVP to enterprise scale with proper security and compliance integration.

**Step 2: Complete Documentation Synthesis**

> Use the project-documentor agent to synthesize all parallel planning results into comprehensive MVP evolution documentation, extracting and integrating context from all specialist agents to create the complete project documentation structure with actual files, C4 evolution diagrams, database-first design documentation, feature prioritization, and implementation roadmaps.

## Expected Deliverables

**Complete MVP-to-Enterprise Plan:**
- 2-4 week MVP delivery plan with working software scope
- Database-first design preventing migrations throughout evolution
- Clean architecture from day one avoiding refactoring needs
- Complete evolution roadmap to enterprise-ready end state
- Technology stack supporting entire journey without migrations
- Enterprise compliance validation throughout all phases
- Comprehensive project documentation with actual files created