---
description: Implement features using research context following enterprise architecture. Use after /research-feature achieves 9/10+ confidence. Triggers @feature-implementor agent.
coordination-pattern: sequential
quality-thresholds: [complete feature delivery, zero breaking changes, 80%+ unit test coverage]
evidence-requirements: [enterprise architecture compliance, comprehensive test coverage, production readiness, performance validation]
complexity: high
estimated-duration: 180
---

# Primary Goals
Execute comprehensive feature implementation as master coordinator using research context, following enterprise architecture patterns with layer-by-layer development to deliver business value while preserving all existing functionality and maintaining architectural integrity.

# Sequential Agent Coordination

I'll coordinate feature implementation specialists sequentially in the main context for: $ARGUMENTS

## Implementation Workflow

**Step 1: Feature Implementation**
> Use the feature-implementor agent to execute comprehensive layer-by-layer implementation following enterprise architecture patterns

**Step 2: Code Review**
> Use the code-reviewer agent to validate enterprise architecture compliance, SOLID principles, and code quality standards

**Step 3: Quality Assurance**
> Use the qa-validator agent to ensure comprehensive test coverage and validate business functionality preservation

**Step 4: Performance Validation**
> Use the performance-investigator agent to validate performance baselines and identify optimization opportunities

**Step 5: Security Review**
> Use the security-investigator agent to validate security patterns and identify potential vulnerabilities

**Step 6: Feature Implementation Documentation**
> Use the feature-documentor agent to update feature documentation with implementation details and create deployment guides

Each specialist operates in isolation and returns results for integration into the complete feature implementation.

# Success Criteria
- **Complete feature delivery** with all business requirements satisfied
- **Zero breaking changes** with all existing functionality preserved
- **Enterprise architecture compliance** with proper layer separation
- **80%+ test coverage** using NO MOCKING policy
- **Production readiness** with monitoring and error handling
- **Performance validation** meeting baseline requirements

The feature implementation follows this master coordinator approach with 8-phase execution:

## Phase 1: Research Context Integration
- Load comprehensive research context from @feature-researcher (Phase 2)
- Import implementation blueprint, codebase analysis, and proven patterns
- Review technology compatibility matrices and integration requirements
- Establish development sequence based on Onion Architecture layers

## Phase 2: Layer-by-Layer Implementation (L4→L3→L2→L1)
**Domain Layer (L4) - Business Logic Foundation**:
- Implement business entities and domain logic with zero infrastructure dependencies
- Create domain services, value objects, and aggregate roots
- Establish business rules and invariant validation
- Ensure pure domain modeling without external concerns

**Infrastructure Layer (L3) - External Integration**:
- Implement repository patterns and data access abstractions
- Create external service integrations and API clients
- Establish configuration management and dependency injection
- Implement cross-cutting concerns like logging and caching

**Application Layer (L2) - Use Case Orchestration**:
- Create application services orchestrating domain operations
- Implement use case workflows and business process coordination
- Handle transaction management and cross-cutting concerns
- Establish validation and authorization patterns

**API Layer (L1) - External Interface**:
- Create thin controllers delegating to application services
- Implement API endpoints with proper request/response handling
- Establish authentication, authorization, and input validation
- Create API documentation and contract specifications

## Phase 3: SOLID Principles and DDD Pattern Implementation
- **Single Responsibility**: Ensure each class has one clear purpose and responsibility
- **Open/Closed**: Design for extension without modification through abstractions
- **Liskov Substitution**: Maintain substitutability of derived classes
- **Interface Segregation**: Create focused, minimal interfaces avoiding god objects
- **Dependency Inversion**: Depend on abstractions rather than concretions

## Phase 4: API Contract Preservation
- Validate all existing API endpoints remain functional and unmodified
- Ensure backward compatibility with existing client integrations
- Test existing business processes and user workflows for disruption
- Maintain data contracts and service interface specifications

## Phase 5: Comprehensive Testing Implementation
**Unit Testing (80%+ Coverage with NO MOCKING)**:
- Test domain logic using real objects and actual dependencies
- Validate business rules and invariant enforcement
- Test edge cases and error conditions with concrete implementations
- Ensure test reliability through deterministic real object testing

**Integration Testing**:
- Test critical business processes with real service dependencies
- Validate external service integrations and data flow
- Test cross-layer interactions and dependency injection
- Ensure database operations and transaction management work correctly

**End-to-End Testing**:
- Test essential user workflows focusing on revenue-generating paths
- Validate complete business processes from user input to system output
- Test authentication, authorization, and security workflows
- Ensure performance requirements are met under realistic load

## Phase 6: Enterprise Architecture Compliance Validation
- Validate Onion Architecture layer separation and dependency direction
- Ensure SOLID principles are maintained throughout implementation
- Verify DDD patterns and domain model integrity
- Test architectural constraints and boundaries

## Phase 7: Production Readiness Implementation
- Implement comprehensive logging and monitoring capabilities
- Create error handling and graceful degradation strategies
- Establish health checks and observability endpoints
- Implement feature flags and rollback mechanisms

## Phase 8: Business Functionality Preservation Validation
- Execute comprehensive testing of existing business functionality
- Validate critical user workflows remain intact and performant
- Test integration points and external service dependencies
- Confirm regulatory compliance and security requirements

**Quality Standards**:
- **80%+ unit test coverage** with NO MOCKING policy enforcement using real object testing
- **Integration tests** for critical business processes with real service dependencies
- **E2E tests** for essential user workflows focusing on revenue-generating paths
- **Enterprise architecture compliance** validation throughout all implementation phases
- **Performance validation** ensuring new functionality meets baseline requirements
- **Security compliance** with authentication, authorization, and data protection standards

The implementation must maintain existing business functionality while adding new capabilities through careful coordination and comprehensive validation.

# Multi-Phase Implementation Validation and Documentation

After @feature-implementor completes the implementation workflow and meets all quality standards, validate the implementation through specialized reviewers before documentation synthesis:

## Phase 1: Architecture Implementation Validation
Use @architecture-validator to validate architectural implementation:
- **Onion Architecture compliance** confirming proper layer separation and dependency direction
- **SOLID principles implementation** ensuring all principles are maintained throughout the codebase
- **DDD pattern compliance** validating domain model integrity and bounded context implementation
- **Enterprise pattern consistency** confirming implementation follows established architectural patterns
- **Integration pattern validation** ensuring external service integrations follow enterprise standards

## Phase 2: Security Implementation Review
Use @security-investigator to review security implementation:
- **Authentication and authorization** validating proper implementation of security patterns
- **Data protection compliance** ensuring privacy requirements and data handling are correctly implemented
- **Input validation and sanitization** confirming all user inputs are properly secured
- **API security implementation** validating endpoint security and access controls
- **Security testing coverage** ensuring security test scenarios adequately cover implementation

## Phase 3: Performance Implementation Analysis
Use @performance-investigator to analyze performance implementation:
- **Performance optimization** validating that optimization strategies were properly implemented
- **Load handling capability** confirming implementation can handle projected usage patterns
- **Resource utilization** ensuring efficient use of system resources
- **Monitoring and observability** validating adequate performance monitoring is implemented
- **Scalability validation** confirming implementation supports planned growth

## Phase 4: Quality Assurance Review
Use @qa-validator to review overall implementation quality:
- **Test coverage validation** confirming 80%+ unit test coverage with NO MOCKING policy
- **Integration test completeness** ensuring critical business processes are adequately tested
- **End-to-end test coverage** validating essential user workflows are properly tested
- **Business functionality preservation** confirming existing functionality remains intact
- **Production readiness** ensuring implementation meets deployment standards

## Phase 5: Implementation Documentation
After validation phases complete, synthesize all implementation and validation context into structured documentation:

### Implementation Document Generation
Create comprehensive implementation documentation in: `docs/development/features/business/{feature-name}/03-{feature-name}-implementation-FEAT-{timestamp}.md`

The implementation document should synthesize all implementation and validation context:

### Implementation Structure
- **Implementation ID and Metadata**: Unique identifier linking to research and planning phases
- **Context Integration**: Summary of planning and research decisions that guided implementation
- **Architecture Implementation**: Details of Onion Architecture layer implementation and patterns used
- **Security Implementation**: Authentication, authorization, and data protection implementation details
- **Performance Implementation**: Optimization strategies and monitoring implementation
- **Testing Implementation**: Comprehensive testing strategy execution and coverage results
- **Quality Validation Results**: Summary of architecture, security, performance, and QA validation findings
- **Production Deployment**: Deployment procedures, monitoring setup, and rollback capabilities
- **Business Impact**: Delivered business value and user workflow improvements
- **Lessons Learned**: Implementation insights and recommendations for future features

### File Naming Convention
Use format: `03-{feature-name}-implementation-FEAT-{timestamp}.md` where:
- {feature-name} matches the planning and research phase naming
- {timestamp} is YYYY-MMDD-HHMMSS format for the implementation completion time

## Phase 6: Documentation Quality Validation
Finally, use @documentor to validate documentation compliance:
- **Structure compliance** with feature implementation documentation standards
- **Content completeness** ensuring all implementation phases and validation results are documented
- **Evidence quality** validating that all implementation decisions are well-supported
- **Formatting consistency** with established documentation patterns
- **Knowledge transfer readiness** ensuring documentation enables future maintenance and enhancements

### Documentation Requirements
- All architectural implementation must be validated by @architecture-validator
- Security implementation must be reviewed by @security-investigator
- Performance implementation must be analyzed by @performance-investigator
- Overall quality must be validated by @qa-validator
- All validation findings must be incorporated into the final implementation document
- Documentation must pass @documentor validation before implementation is considered complete
- Implementation must be ready for production deployment with comprehensive documentation

This multi-phase validation ensures feature implementations are architecturally sound, secure, performant, and properly documented for long-term maintenance and team knowledge transfer.