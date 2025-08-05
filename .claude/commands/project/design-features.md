---
description: Design implementation-ready specifications for all features in project catalog
argument-hint: [project-directory] --features=[specific-features] --mode=[sequential|batch]
---

# Project Feature Design

I'll coordinate feature design specialists to create implementation-ready specifications for all features in the project catalog: $ARGUMENTS

## Feature Design Workflow

I'll execute the following process for each feature in the project catalog:

**For Each Feature in Project Catalog:**
1. **Feature Planning**: Use the feature-planner agent to execute comprehensive feature planning from requirements to architecture design
2. **Architecture Validation**: Use the architecture-validator agent to validate feature architecture, DDD context mapping, and enterprise compliance 
3. **Feature Documentation**: Use the feature-documentor agent to create implementation-ready feature specification

Each feature gets its own complete planning workflow and implementation-ready documentation.

## Process

**Step 1: Project Context Loading**
- Load project overview document from project directory
- Extract feature catalog with business and technical features
- Identify feature dependencies and implementation sequence
- Prepare feature planning context for each feature

**Step 2: Sequential Feature Design**
For each feature identified in the project catalog:

> Use the feature-planner agent to plan [FEATURE_NAME] based on project context, architecture decisions, and feature requirements

> Use the architecture-validator agent to validate [FEATURE_NAME] architecture integration, DDD context mapping, and enterprise pattern compliance

> Use the feature-documentor agent to create implementation-ready specification for [FEATURE_NAME] including technical requirements, architecture integration, and development guidance

**Step 3: Feature Design Integration**
- Validate feature dependencies and integration requirements
- Ensure consistent architecture approach across all features  
- Create feature implementation roadmap with dependencies
- Prepare features for implementation phase workflows

## Expected Output

**Feature Specifications Created:**
- `docs/projects/PROJECT-NAME/features/FEAT-YYYY-MMDD-{feature-1}.md`
- `docs/projects/PROJECT-NAME/features/FEAT-YYYY-MMDD-{feature-2}.md`
- `docs/projects/PROJECT-NAME/features/FEAT-YYYY-MMDD-{feature-N}.md`

**Each Feature Specification Contains:**
- Complete business and technical requirements
- Architecture integration with Onion Architecture layers
- DDD bounded context definitions and domain relationships
- Implementation sequence and development guidance
- Testing strategy and quality gate requirements
- Integration requirements and dependency specifications

## Success Criteria

- All features from project catalog have implementation-ready specifications
- Feature architecture validated for enterprise compliance and consistency
- Feature dependencies and integration requirements clearly documented
- Each feature specification enables direct implementation workflow execution
- Features ready for `/development/features/implement-feature` commands

This creates a complete bridge from high-level project planning to detailed feature implementation, ensuring every feature has the detailed specifications needed for successful development.