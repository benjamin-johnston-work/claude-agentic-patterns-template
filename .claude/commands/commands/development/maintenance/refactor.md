---
description: Code refactoring with enterprise architecture compliance
coordination-pattern: parallel
quality-thresholds: [complete behavior preservation, enterprise architecture compliance, test coverage maintenance]
evidence-requirements: [code quality improvement, documentation consistency, parallel validation approval]
complexity: medium
estimated-duration: 75
---

# Primary Goals
Execute comprehensive code refactoring with enterprise architecture compliance, improving code quality and maintainability while preserving all existing functionality and maintaining strict architectural integrity through parallel validation.

# Agent Coordination
**Parallel Process - Multi-Agent Coordination with Implementation**
- **Phase 1**: @code-reviewer + @architecture-validator execute parallel analysis and validation
- **Phase 2**: Implementation execution following parallel validation approval
- **Coordination Pattern**: Parallel agents provide validation, then sequential implementation
- **Quality Gates**: Both code review and architecture validation must approve before implementation proceeds

**Parallel Agent Responsibilities**:
- **@code-reviewer**: Analyzes code quality, patterns, and refactoring opportunities
- **@architecture-validator**: Validates enterprise architecture compliance and dependency integrity

# Success Criteria
- **Complete behavior preservation** with zero functionality changes or performance regression
- **Enterprise architecture compliance** maintaining Onion Architecture patterns and SOLID principles
- **Code quality improvement** with enhanced readability, maintainability, and reduced technical debt
- **Test coverage maintenance** with all existing tests passing and coverage preserved or improved
- **Documentation consistency** with updated inline and architectural documentation as needed

# Refactoring Process
Perform code refactoring on: $ARGUMENTS

The refactoring follows this comprehensive 10-phase approach with parallel validation:

## Phase 1: Pre-Refactoring Analysis (Parallel Validation)
**@code-reviewer Analysis**:
- Read and understand current code structure, patterns, and quality metrics
- Identify specific refactoring goals, scope boundaries, and improvement opportunities
- Analyze code smells, duplication patterns, and maintainability issues
- Review existing documentation and architectural decisions

**@architecture-validator Analysis**:
- Analyze current enterprise architecture compliance and dependency relationships
- Validate existing Onion Architecture layer separation (L1→L2→L3→L4)
- Review SOLID principle adherence and DDD pattern implementation
- Assess potential architectural impact areas and constraint violations

## Phase 2: Enterprise Architecture Validation (Parallel Review)
**@code-reviewer Validation**:
- Ensure refactoring maintains code quality standards and conventions
- Validate proposed changes improve readability and maintainability
- Review impact on existing tests and code coverage requirements
- Assess refactoring scope appropriateness and change boundaries

**@architecture-validator Validation**:
- Ensure refactoring maintains Onion Architecture patterns (L1→L2→L3→L4)
- Preserve SOLID principles throughout the refactoring:
  - **Single Responsibility**: Keep classes focused on one concern
  - **Open/Closed**: Maintain extensibility without modification
  - **Liskov Substitution**: Ensure derived classes remain substitutable
  - **Interface Segregation**: Keep interfaces focused and minimal
  - **Dependency Inversion**: Maintain abstractions over concretions
- Follow DDD patterns for domain modeling where applicable
- Preserve existing enterprise conventions and established patterns

## Phase 3: Refactoring Execution (Sequential Implementation)
**Method and Class Extraction**:
- **Extract Methods**: Break down large methods into smaller, focused functions
- **Extract Classes**: Separate concerns into appropriate classes with clear responsibilities
- **Extract Interfaces**: Create abstractions for better dependency management
- **Consolidate Duplicates**: Merge repeated code patterns into reusable components

**Code Quality Improvements**:
- **Rename Variables/Methods**: Use clear, descriptive names following conventions
- **Remove Duplication**: Consolidate repeated code patterns and logic
- **Simplify Conditionals**: Improve readability of complex conditional logic
- **Improve Error Handling**: Ensure consistent exception management and recovery

## Phase 4: Behavior Preservation Validation
- Ensure no functionality changes during refactoring execution
- Maintain all existing interfaces and public contracts without modification
- Preserve performance characteristics and resource consumption patterns
- Keep security measures, authentication, and authorization intact

## Phase 5: Code Quality Standards Implementation
- Follow consistent code style and formatting conventions throughout
- Ensure proper documentation and comments where needed for clarity
- Remove dead code, unused imports, and deprecated dependencies
- Optimize for readability, maintainability, and future extensibility

## Phase 6: Testing and Validation Execution
- Run existing test suite to ensure no regressions: `dotnet test` or `npm test`
- Add tests for newly extracted methods or classes if coverage gaps identified
- Verify code coverage hasn't decreased from refactoring changes
- Test edge cases and error scenarios with existing test patterns

## Phase 7: Architecture Compliance Verification
- Run architecture tests if available: `dotnet test tests/Architecture/`
- Verify dependency direction still flows correctly within layer boundaries
- Ensure no new coupling has been introduced between architectural layers
- Validate interface segregation is maintained and improved where possible

## Phase 8: Final Validation and Quality Assurance
- Run linters and formatters: `dotnet format` or `npm run lint`
- Build the project to ensure no compilation errors or warnings
- Perform smoke tests on affected functionality with realistic scenarios
- Review changes for code quality improvement and maintainability enhancement

## Phase 9: Documentation Update and Consistency
- Update inline documentation if significant changes were made to methods or classes
- Modify architecture documentation if patterns changed or new patterns introduced
- Update developer guides if new refactoring patterns were established
- Ensure code comments reflect current implementation and design decisions

## Phase 10: Commit Strategy and Change Management
- Create focused commits for each logical refactoring change
- Use descriptive commit messages following conventions: `refactor: extract payment validation logic`
- Consider creating multiple smaller commits for complex refactorings
- Document refactoring rationale and benefits in commit messages

**Quality Gates**:
- **Parallel validation approval**: Both @code-reviewer and @architecture-validator must approve before implementation
- **Test suite validation**: All existing tests must pass without modification
- **Architecture compliance**: Enterprise patterns must be preserved or improved
- **Performance preservation**: No performance regression allowed
- **Documentation consistency**: All documentation must reflect refactored state

The refactoring must improve code quality while maintaining strict architectural integrity and complete behavioral preservation.