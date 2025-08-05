---
description: Implement technical solutions with intelligent coordination based on design complexity. Use after /design-tech-task achieves 8/10+ confidence. Adapts coordination pattern automatically.
coordination-pattern: intelligent-adaptive
coordination-modes: [simple-parallel, medium-validation, complex-hierarchical]
quality-thresholds: [90%+ design compliance, complete business functionality preservation, standard tooling implementation]
evidence-requirements: [enterprise pattern compliance, generated code management, testing validation with NO MOCKING policy, coordination effectiveness]
complexity: adaptive
estimated-duration: 60-180
---

# Primary Goals
Execute precise technical solution implementation following exact design specifications with 90%+ compliance, using standard tooling and boring technology to deliver business value while preserving existing functionality and maintaining architectural simplicity.

# Sequential Agent Coordination

I'll coordinate technical implementation specialists sequentially in the main context for: $ARGUMENTS

## Implementation Workflow

**Step 1: Technical Implementation**
> Use the tech-task-implementor agent to execute precise technical solution implementation following design specifications

**Step 2: Code Review**
> Use the code-reviewer agent to validate design compliance, code quality, and enterprise pattern adherence

**Step 3: Quality Validation**
> Use the qa-validator agent to ensure testing coverage and validate business functionality preservation

**Step 4: Security Analysis (if applicable)**
> Use the security-investigator agent to validate security patterns for security-related technical tasks

**Step 5: Technical Implementation Documentation**
> Use the tech-task-documentor agent to update technical documentation with implementation details and guides

Each specialist operates in isolation and returns results for integration into the complete technical solution.

### Complex Mode (Complexity 8+)
- **Primary Agent**: @tech-task-implementor as master coordinator
- **Pattern**: Hierarchical master-worker coordination
- **Domain Specialists**: @backend-developer, @frontend-developer, @database-specialist based on required domains
- **Use Case**: Platform-scale implementations requiring cross-domain expertise

## Design Analysis Logic
Command parses `docs/development/techtasks/{task-name}-design.md` to extract:
- **Complexity Score**: Determines base coordination mode
- **Required Domains**: Identifies needed domain specialists
- **Risk Factors**: Selects appropriate validators
- **Technology Stack**: Influences specialist selection
- **Security Triggers**: Mandatory security validation for sensitive components

### Mandatory Security Validation Triggers
**Always spawn @security-investigator regardless of complexity if any present**:
- **Azure Managed Identity** (system-assigned, user-assigned)
- **Authentication systems** (JWT, OAuth, SAML, etc.)
- **Authorization frameworks** (RBAC, claims-based, policy-based)
- **Credential management** (certificates, secrets, API keys)
- **External identity providers** (Azure AD, third-party SSO)
- **Security boundaries** (cross-tenant, cross-domain access)
- **Privileged operations** (admin APIs, elevated permissions)

**Prerequisites**: Must follow /design-tech-task command (Phase 1) with 8/10+ design confidence
**Quality Gates**: 90%+ design compliance required with deviation tracking and business functionality preservation

**Context Integration**: Inherits complete design context including implementation blueprint, tooling specifications, and complexity justifications

# Success Criteria
- **90%+ design compliance** with exact adherence to specifications and minimal justified deviations
- **Complete business functionality preservation** without disruption to existing workflows
- **Standard tooling implementation** using exact tools and parameters specified in design
- **Enterprise pattern compliance** following existing architectural conventions and coding standards
- **Generated code management** with proper source control integration as specified
- **Testing validation** with NO MOCKING policy using real service dependencies

# Technical Implementation Process

## Intelligent Coordination Logic

### Design Complexity Analysis
First, analyze design document to determine coordination strategy:

```bash
# Extract complexity metrics from design document
DESIGN_FILE="docs/development/techtasks/${TASK_NAME}-design.md"
COMPLEXITY_SCORE=$(grep "complexity_score:" $DESIGN_FILE | sed 's/.*: *//')
REQUIRED_DOMAINS=$(grep "required_domains:" $DESIGN_FILE | sed 's/.*: *//')
RISK_FACTORS=$(grep "risk_factors:" $DESIGN_FILE | sed 's/.*: *//')
TECHNOLOGY_STACK=$(grep "technology_stack:" $DESIGN_FILE | sed 's/.*: *//')

# Check for mandatory security triggers
SECURITY_MANDATORY=false
if grep -q -E "(managed.identity|authentication|authorization|azure.ad|oauth|jwt|saml|rbac|credential|certificate|secret|api.key|privileged)" $DESIGN_FILE; then
    SECURITY_MANDATORY=true
fi
```

### Coordination Strategy Selection

**If Complexity < 6 (Simple Mode)**:
Use @tech-task-implementor with dependency-free parallel subagents approach.
- **Security Override**: Always add @security-investigator if SECURITY_MANDATORY=true

**If Complexity 6-7 (Medium Mode)**:
Use @tech-task-implementor as coordinator with parallel validators:
- Always spawn @code-reviewer for architecture compliance validation
- Always spawn @security-investigator if SECURITY_MANDATORY=true OR security risk factors present
- Synthesize findings and resolve conflicts before implementation

**If Complexity 8+ (Complex Mode)**:
Use @tech-task-implementor as master coordinator with domain specialists:
- Spawn @backend-developer if backend domain required
- Spawn @frontend-developer if frontend domain required  
- Spawn @database-specialist if database domain required
- **Security Override**: Always add @security-investigator if SECURITY_MANDATORY=true OR security present in any domain
- Coordinate cross-domain integration and handoffs

## Implementation Execution

### For All Modes: Use @tech-task-implementor to implement: $ARGUMENTS

The technical implementation follows coordination-specific approach:

### Simple Mode Implementation (Complexity < 6)
**Current 8-Phase Approach**:
1. **Design Context Loading**: Import complete design context and validate 8/10+ confidence threshold
2. **Standard Tooling Implementation**: Execute exact tooling with precise parameters (dotnet-svcutil, OpenAPI generators)
3. **Configuration Implementation**: Follow existing enterprise patterns and dependency injection
4. **Generated Code Management**: Check generated code into source control per design blueprint
5. **Business Functionality Preservation**: Validate existing processes remain intact without regression
6. **Testing Implementation**: Use NO MOCKING policy with real service dependencies
7. **Enterprise Pattern Integration**: Maintain Onion Architecture and SOLID principles
8. **Compliance Validation**: Achieve 90%+ design compliance with deviation tracking

### Medium Mode Implementation (Complexity 6-7)
**Parallel Validation Approach**:
1. **Design Analysis & Agent Spawning**: @tech-task-implementor analyzes risk factors and spawns appropriate validators
2. **Parallel Validation**: Run @code-reviewer and @security-investigator (if security risks present) simultaneously
3. **Finding Synthesis**: @tech-task-implementor correlates validation findings and resolves conflicts
4. **Validated Implementation**: Execute implementation incorporating validator recommendations
5. **Cross-Domain Integration**: Ensure consistency across validation domains
6. **Comprehensive Testing**: Validate implementation meets all validator requirements
7. **Quality Assurance**: Document validation findings and implementation adjustments
8. **Compliance Confirmation**: Achieve 90%+ compliance with enhanced validation coverage

### Complex Mode Implementation (Complexity 8+)
**Hierarchical Master-Worker Approach**:
1. **Master Coordination Setup**: @tech-task-implementor acts as master, analyzes required domains
2. **Domain Specialist Spawning**: Deploy @backend-developer, @frontend-developer, @database-specialist based on requirements
3. **Work Decomposition**: Master decomposes implementation across domain boundaries
4. **Parallel Domain Implementation**: Domain specialists work in parallel on their areas
5. **Cross-Domain Handoffs**: Master coordinates integration points and data flow between domains
6. **Integration Synthesis**: Master synthesizes all domain implementations into cohesive solution
7. **End-to-End Validation**: Test complete system integration across all domains
8. **Master Compliance Validation**: Ensure 90%+ compliance across all domain implementations

**Implementation Requirements**:
- **Code generation** using exact tooling parameters from design (e.g., dotnet-svcutil, scaffolding tools)
- **Configuration implementation** following existing enterprise patterns and conventions
- **Testing with NO MOCKING policy** using real services for comprehensive validation
- **Source control integration** with generated code checked in as specified in design

**Quality Standards**:
- **90%+ design compliance** with systematic deviation tracking and clear justification
- **Business functionality preservation** without disruption to existing workflows or user experience
- **Integration following enterprise patterns** as designed with architectural consistency
- **Testing approach alignment** with design specifications using real service validation
- **Performance maintenance** ensuring solution meets or exceeds baseline requirements

**Complexity-Adaptive Documentation Requirements**:

### Simple Mode Documentation
- **Basic implementation notes** with tooling execution results
- **Configuration changes** documented with before/after comparisons
- **Generated code management** with source control integration notes
- **Compliance metrics** showing 90%+ design adherence

### Medium Mode Documentation
- **Parallel validation synthesis** with findings correlation across validators
- **Risk mitigation documentation** showing how validator recommendations were addressed
- **Cross-domain consistency validation** with conflict resolution details
- **Enhanced compliance reporting** with validator-specific metrics

### Complex Mode Documentation
- **Master coordination report** detailing domain decomposition and specialist assignments
- **Cross-domain integration documentation** with handoff protocols and data flow diagrams
- **Domain-specific implementation reports** from each specialist agent
- **Comprehensive system integration testing** with end-to-end validation results
- **Master compliance validation** aggregating compliance across all domain implementations

**Compliance Validation Checkpoints**:
- **Exact tooling usage**: Implementation uses precisely the tools specified in design
- **Method adherence**: Approach follows exact methodology documented in design blueprint
- **Complexity containment**: No additional complexity beyond design specifications
- **Pattern consistency**: Uses existing enterprise patterns exactly as specified in design
- **Structure compliance**: Generates files and structures exactly as designed
- **Coordination effectiveness**: Appropriate coordination pattern selected and executed successfully

## Usage Examples

### Simple Technical Task (Complexity < 6)
```bash
/implement-tech-task "static-wsdl-integration"
# Uses current 8-phase approach with parallel subagents
# Result: Standard tooling execution with basic documentation
```

### Medium Complexity Task (Complexity 6-7)
```bash
/implement-tech-task "multi-service-authentication-integration"
# Spawns @code-reviewer and @security-investigator in parallel
# Result: Validated implementation with enhanced security compliance
```

### Complex Platform Task (Complexity 8+)
```bash
/implement-tech-task "ai-documentation-platform-core-system"
# Master coordinates @backend-developer, @frontend-developer, @database-specialist
# Result: Full-stack implementation with cross-domain integration
```

The implementation must maintain simplicity principles and boring technology choices while delivering the designed technical capability with measurable business value through intelligent coordination scaling.