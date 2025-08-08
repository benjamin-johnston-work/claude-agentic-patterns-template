# Feature 04: GraphQL API Foundation

## Feature Overview

**Feature ID**: F04  
**Feature Name**: GraphQL API Foundation  
**Phase**: Phase 1 (Weeks 1-4)  
**Bounded Context**: User Experience Context / API Gateway  

### Business Value Proposition
Provide a modern, flexible, and efficient API layer that enables front-end applications and integrations to access repository data and code structures. The GraphQL API serves as the primary interface for all client interactions, offering real-time capabilities and optimized data fetching.

### User Impact
- Frontend developers can efficiently query exactly the data they need
- Third-party integrations can easily consume repository information
- Real-time updates enable responsive user experiences
- API consumers benefit from strong typing and introspection capabilities

### Success Criteria
- Complete GraphQL schema covering all Phase 1 domain models
- API response times < 500ms for typical queries
- Support for real-time subscriptions via WebSockets
- Comprehensive query filtering and pagination
- 100% API test coverage for all resolvers

### Dependencies
- F01: Repository Connection and Management (provides repository data)
- F02: Core Infrastructure and DevOps Pipeline (provides Azure hosting)
- F03: File Parsing and Code Structure Indexing (provides code structure data)

## Technical Specification

### GraphQL Schema Architecture

#### Core Schema Structure
```graphql
# Scalars and Enums
scalar DateTime
scalar Guid

enum RepositoryStatus {
  CONNECTING
  CONNECTED  
  ANALYZING
  READY
  ERROR
  DISCONNECTED
}

enum AccessModifier {
  PUBLIC
  PRIVATE
  PROTECTED
  INTERNAL
  PACKAGE
  EXPORT
}

# Core Types
type Repository {
  id: ID!
  name: String!
  url: String!
  language: String!
  description: String
  status: RepositoryStatus!
  branches: [Branch!]!
  files(filter: FileFilter, pagination: PaginationInput): FileConnection!
  classes(filter: ClassFilter, pagination: PaginationInput): ClassConnection!
  methods(filter: MethodFilter, pagination: PaginationInput): MethodConnection!
  statistics: RepositoryStatistics!
  createdAt: DateTime!
  updatedAt: DateTime!
  lastAnalyzed: DateTime
}

type Branch {
  name: String!
  isDefault: Boolean!
  lastCommit: Commit
  createdAt: DateTime!
  repository: Repository!
}

type Commit {
  hash: String!
  message: String!
  author: String!
  timestamp: DateTime!
  repository: Repository!
}

type CodeFile {
  id: ID!
  path: String!
  name: String!
  extension: String!
  language: String!
  size: Int!
  lineCount: Int!
  lastModified: DateTime!
  contentHash: String!
  classes: [CodeClass!]!
  functions: [CodeFunction!]!
  imports: [ImportStatement!]!
  complexity: CodeComplexity!
  repository: Repository!
}

type CodeClass {
  id: ID!
  name: String!
  namespace: String
  fullName: String!
  accessLevel: AccessModifier!
  isAbstract: Boolean!
  isInterface: Boolean!
  baseClass: String
  implementedInterfaces: [String!]!
  methods: [CodeMethod!]!
  properties: [CodeProperty!]!
  location: SourceLocation!
  file: CodeFile!
  repository: Repository!
}

type CodeMethod {
  id: ID!
  name: String!
  returnType: String!
  parameters: [Parameter!]!
  accessLevel: AccessModifier!
  isStatic: Boolean!
  isAsync: Boolean!
  location: SourceLocation!
  complexity: CodeComplexity!
  methodCalls: [MethodCall!]!
  class: CodeClass
  file: CodeFile!
  repository: Repository!
}

# Connection types for pagination
type RepositoryConnection {
  edges: [RepositoryEdge!]!
  pageInfo: PageInfo!
  totalCount: Int!
}

type RepositoryEdge {
  node: Repository!
  cursor: String!
}

type FileConnection {
  edges: [FileEdge!]!
  pageInfo: PageInfo!
  totalCount: Int!
}

type ClassConnection {
  edges: [ClassEdge!]!
  pageInfo: PageInfo!
  totalCount: Int!
}

type PageInfo {
  hasNextPage: Boolean!
  hasPreviousPage: Boolean!
  startCursor: String
  endCursor: String
}

# Input types
input RepositoryFilter {
  status: RepositoryStatus
  language: String
  nameContains: String
  createdAfter: DateTime
  createdBefore: DateTime
}

input FileFilter {
  language: String
  extension: String
  pathContains: String
  minSize: Int
  maxSize: Int
  complexityThreshold: Int
}

input ClassFilter {
  namespace: String
  nameContains: String
  isInterface: Boolean
  accessLevel: AccessModifier
  hasBaseClass: Boolean
}

input MethodFilter {
  nameContains: String
  returnType: String
  accessLevel: AccessModifier
  minComplexity: Int
  maxComplexity: Int
  isAsync: Boolean
}

input PaginationInput {
  first: Int
  after: String
  last: Int
  before: String
}

input AddRepositoryInput {
  url: String!
  accessToken: String
  branch: String
}

# Queries
type Query {
  # Repository queries
  repositories(
    filter: RepositoryFilter
    pagination: PaginationInput
  ): RepositoryConnection!
  
  repository(id: ID!): Repository
  
  searchRepositories(
    query: String!
    pagination: PaginationInput
  ): RepositoryConnection!
  
  # Code structure queries
  file(id: ID!): CodeFile
  class(id: ID!): CodeClass  
  method(id: ID!): CodeMethod
  
  # Search capabilities
  searchClasses(
    repositoryId: ID!
    query: String!
    pagination: PaginationInput
  ): ClassConnection!
  
  searchMethods(
    repositoryId: ID!
    query: String!
    pagination: PaginationInput
  ): MethodConnection!
}

# Mutations
type Mutation {
  # Repository management
  addRepository(input: AddRepositoryInput!): Repository!
  refreshRepository(id: ID!): Repository!
  removeRepository(id: ID!): Boolean!
  
  # Repository analysis
  requestAnalysis(repositoryId: ID!): AnalysisJob!
  cancelAnalysis(repositoryId: ID!): Boolean!
}

# Subscriptions for real-time updates
type Subscription {
  # Repository updates
  repositoryUpdated(repositoryId: ID): Repository!
  repositoryStatusChanged(repositoryId: ID!): RepositoryStatusUpdate!
  
  # Analysis updates  
  analysisProgress(repositoryId: ID!): AnalysisProgressUpdate!
  analysisCompleted(repositoryId: ID!): AnalysisCompletedUpdate!
}

type RepositoryStatusUpdate {
  repositoryId: ID!
  oldStatus: RepositoryStatus!
  newStatus: RepositoryStatus!
  timestamp: DateTime!
}

type AnalysisProgressUpdate {
  repositoryId: ID!
  progress: Float! # 0.0 to 1.0
  currentStep: String!
  estimatedTimeRemaining: Int # seconds
  timestamp: DateTime!
}

type AnalysisCompletedUpdate {
  repositoryId: ID!
  success: Boolean!
  statistics: RepositoryStatistics
  errors: [String!]!
  completedAt: DateTime!
}
```

### Resolver Implementation Architecture

#### Repository Resolvers
```csharp
[ExtendObjectType(typeof(Query))]
public class RepositoryQueries
{
    public async Task<IConnection<Repository>> GetRepositories(
        RepositoryFilter? filter,
        PaginationInput? pagination,
        [Service] IRepositoryService repositoryService,
        CancellationToken cancellationToken)
    {
        var repositories = await repositoryService.GetRepositoriesAsync(filter, cancellationToken);
        
        return await repositories.ApplyPaginationAsync(pagination, cancellationToken);
    }
    
    public async Task<Repository?> GetRepository(
        Guid id,
        [Service] IRepositoryService repositoryService,
        CancellationToken cancellationToken)
    {
        return await repositoryService.GetRepositoryByIdAsync(id, cancellationToken);
    }
    
    public async Task<IConnection<Repository>> SearchRepositories(
        string query,
        PaginationInput? pagination,
        [Service] IRepositorySearchService searchService,
        CancellationToken cancellationToken)
    {
        var results = await searchService.SearchAsync(query, cancellationToken);
        
        return await results.ApplyPaginationAsync(pagination, cancellationToken);
    }
}

[ExtendObjectType(typeof(Repository))]
public class RepositoryResolvers
{
    public async Task<IConnection<CodeFile>> GetFiles(
        [Parent] Repository repository,
        FileFilter? filter,
        PaginationInput? pagination,
        [Service] ICodeStructureService codeService,
        CancellationToken cancellationToken)
    {
        var files = await codeService.GetFilesAsync(repository.Id, filter, cancellationToken);
        
        return await files.ApplyPaginationAsync(pagination, cancellationToken);
    }
    
    public async Task<IConnection<CodeClass>> GetClasses(
        [Parent] Repository repository,
        ClassFilter? filter,
        PaginationInput? pagination,
        [Service] ICodeStructureService codeService,
        CancellationToken cancellationToken)
    {
        var classes = await codeService.GetClassesAsync(repository.Id, filter, cancellationToken);
        
        return await classes.ApplyPaginationAsync(pagination, cancellationToken);
    }
    
    public async Task<RepositoryStatistics> GetStatistics(
        [Parent] Repository repository,
        [Service] IRepositoryStatisticsService statisticsService,
        CancellationToken cancellationToken)
    {
        return await statisticsService.GetStatisticsAsync(repository.Id, cancellationToken);
    }
}
```

#### Mutation Resolvers
```csharp
[ExtendObjectType(typeof(Mutation))]
public class RepositoryMutations
{
    public async Task<Repository> AddRepository(
        AddRepositoryInput input,
        [Service] IRepositoryService repositoryService,
        [Service] IPublisher publisher,
        CancellationToken cancellationToken)
    {
        var repository = await repositoryService.AddRepositoryAsync(
            input.Url, 
            input.AccessToken, 
            input.Branch, 
            cancellationToken);
        
        // Publish event for analysis
        await publisher.PublishAsync(new RepositoryAddedEvent
        {
            RepositoryId = repository.Id,
            Url = repository.Url,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        
        return repository;
    }
    
    public async Task<Repository> RefreshRepository(
        Guid id,
        [Service] IRepositoryService repositoryService,
        [Service] IPublisher publisher,
        CancellationToken cancellationToken)
    {
        var repository = await repositoryService.RefreshRepositoryAsync(id, cancellationToken);
        
        await publisher.PublishAsync(new RepositoryRefreshRequestedEvent
        {
            RepositoryId = id,
            Timestamp = DateTime.UtcNow
        }, cancellationToken);
        
        return repository;
    }
    
    public async Task<AnalysisJob> RequestAnalysis(
        Guid repositoryId,
        [Service] IAnalysisService analysisService,
        CancellationToken cancellationToken)
    {
        return await analysisService.StartAnalysisAsync(repositoryId, cancellationToken);
    }
}
```

#### Subscription Resolvers
```csharp
[ExtendObjectType(typeof(Subscription))]
public class RepositorySubscriptions
{
    [Subscribe]
    public Repository RepositoryUpdated(
        [EventMessage] Repository repository) => repository;
    
    [Subscribe]
    public RepositoryStatusUpdate RepositoryStatusChanged(
        [EventMessage] RepositoryStatusUpdate update) => update;
    
    [Subscribe]
    public AnalysisProgressUpdate AnalysisProgress(
        Guid repositoryId,
        [EventMessage] AnalysisProgressUpdate update)
    {
        return update.RepositoryId == repositoryId ? update : null;
    }
}
```

### Data Loader Implementation

#### Efficient Data Loading
```csharp
public class RepositoryDataLoaders
{
    [DataLoader]
    public static async Task<IReadOnlyDictionary<Guid, Repository>> GetRepositoryBatchAsync(
        IReadOnlyList<Guid> ids,
        IRepositoryService repositoryService,
        CancellationToken cancellationToken)
    {
        var repositories = await repositoryService.GetRepositoriesByIdsAsync(ids, cancellationToken);
        return repositories.ToDictionary(r => r.Id, r => r);
    }
    
    [DataLoader]
    public static async Task<ILookup<Guid, CodeFile>> GetFilesByRepositoryBatchAsync(
        IReadOnlyList<Guid> repositoryIds,
        ICodeStructureService codeService,
        CancellationToken cancellationToken)
    {
        var files = await codeService.GetFilesByRepositoryIdsAsync(repositoryIds, cancellationToken);
        return files.ToLookup(f => f.RepositoryId);
    }
    
    [DataLoader]
    public static async Task<ILookup<Guid, CodeClass>> GetClassesByRepositoryBatchAsync(
        IReadOnlyList<Guid> repositoryIds,
        ICodeStructureService codeService,
        CancellationToken cancellationToken)
    {
        var classes = await codeService.GetClassesByRepositoryIdsAsync(repositoryIds, cancellationToken);
        return classes.ToLookup(c => c.RepositoryId);
    }
}
```

### Authentication and Authorization

#### JWT Token Validation
```csharp
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var token = ExtractTokenFromRequest(context.Request);
        
        if (!string.IsNullOrEmpty(token))
        {
            var principal = await ValidateTokenAsync(token);
            if (principal != null)
            {
                context.User = principal;
            }
        }
        
        await _next(context);
    }
    
    private async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();
            
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return null;
        }
    }
}

[Authorize]
[ExtendObjectType(typeof(Mutation))]
public class SecuredRepositoryMutations
{
    public async Task<Repository> AddRepository(
        AddRepositoryInput input,
        ClaimsPrincipal user,
        [Service] IRepositoryService repositoryService)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return await repositoryService.AddRepositoryAsync(
            input.Url, 
            input.AccessToken, 
            input.Branch,
            userId);
    }
}
```

### Error Handling and Validation

#### Custom Error Types
```csharp
public class RepositoryNotFoundError : Exception, IError
{
    public RepositoryNotFoundError(Guid repositoryId) 
        : base($"Repository with ID {repositoryId} was not found.")
    {
        RepositoryId = repositoryId;
    }
    
    public Guid RepositoryId { get; }
    
    public IError WithPath(Path path) => this;
    public IError WithLocations(IReadOnlyList<Location> locations) => this;
    public IError RemoveLocations() => this;
    public IError RemovePath() => this;
    public IError WithExtensions(IReadOnlyDictionary<string, object?> extensions) => this;
    public IError RemoveExtensions() => this;
}

public class InputValidationMiddleware
{
    private readonly FieldDelegate _next;
    
    public async Task InvokeAsync(IMiddlewareContext context)
    {
        // Validate input arguments
        var inputValidationResult = ValidateInputs(context.Arguments);
        
        if (!inputValidationResult.IsValid)
        {
            foreach (var error in inputValidationResult.Errors)
            {
                context.ReportError(ErrorBuilder.New()
                    .SetMessage(error.ErrorMessage)
                    .SetCode("INPUT_VALIDATION_ERROR")
                    .SetPath(context.Path)
                    .Build());
            }
            
            return;
        }
        
        await _next(context);
    }
}
```

### Performance Requirements
- GraphQL query response time < 500ms for typical operations
- Support for concurrent requests (100+ simultaneous connections)
- Real-time subscription latency < 100ms
- DataLoader batching efficiency >90% for N+1 scenarios
- Memory usage < 2GB under normal load

## Implementation Guidance

### Recommended Development Approach
1. **Schema Design**: Start with core domain types and relationships
2. **Resolver Implementation**: Build query resolvers with proper data loading
3. **Mutation Layer**: Add mutations with event publishing integration
4. **Subscription System**: Implement real-time updates via SignalR
5. **Security Integration**: Add authentication and authorization
6. **Performance Optimization**: Implement DataLoaders and caching

### Key Architectural Decisions
- Use HotChocolate for comprehensive GraphQL support
- Implement cursor-based pagination for large result sets
- Use SignalR for WebSocket-based subscriptions
- Apply DataLoader pattern to solve N+1 query problems
- Implement comprehensive input validation middleware
- Use Azure Application Insights for API monitoring

### Technical Risks and Mitigation
1. **Risk**: N+1 query problems with complex object graphs
   - **Mitigation**: Implement comprehensive DataLoader strategy
   - **Fallback**: Query result caching for frequently accessed data

2. **Risk**: Large query result sets causing memory issues
   - **Mitigation**: Enforce pagination limits and query depth restrictions
   - **Fallback**: Implement query complexity analysis and throttling

3. **Risk**: WebSocket connection management at scale
   - **Mitigation**: Use Azure SignalR Service for connection management
   - **Fallback**: Implement connection pooling and cleanup strategies

### Deployment Considerations
- Deploy to Azure App Service with auto-scaling enabled
- Configure Azure Application Gateway for load balancing
- Set up Azure SignalR Service for subscription scalability
- Implement comprehensive health checks and monitoring

## Testing Strategy

### Unit Testing Requirements (80% coverage minimum)
- **GraphQL Resolvers**
  - Query resolver accuracy and error handling
  - Mutation resolver business logic validation
  - Subscription resolver event handling
  - DataLoader batching efficiency

- **Schema Validation**
  - Input type validation rules
  - Output type completeness
  - Schema introspection functionality
  - Error type handling

- **Authentication/Authorization**
  - JWT token validation logic
  - Authorization policy enforcement
  - User context propagation
  - Security middleware functionality

### Integration Testing Requirements (30% coverage minimum)
- **End-to-End GraphQL Operations**
  - Complete query execution workflows
  - Mutation operations with side effects
  - Subscription lifecycle management
  - Error scenarios and edge cases

- **Authentication Integration**
  - JWT token-based authentication
  - User authorization across operations
  - Security policy enforcement
  - Session management

- **Performance Testing**
  - Concurrent query handling
  - Large result set pagination
  - DataLoader performance validation
  - Memory usage under load

### Test Data Requirements
- Mock repositories with various structures and sizes
- User authentication test scenarios
- Performance test datasets with known metrics
- Error condition test cases

## Quality Assurance

### Code Review Checkpoints
- [ ] GraphQL schema follows best practices and naming conventions
- [ ] Resolvers implement proper error handling and validation
- [ ] DataLoaders are implemented for all parent-child relationships
- [ ] Authentication and authorization are properly integrated
- [ ] Input validation covers all edge cases
- [ ] Performance optimizations are implemented
- [ ] Real-time subscriptions work correctly
- [ ] Monitoring and logging are comprehensive

### Definition of Done Checklist
- [ ] Complete GraphQL schema covers all Phase 1 requirements
- [ ] All query and mutation resolvers are implemented
- [ ] Real-time subscriptions are functional
- [ ] Authentication and authorization work correctly
- [ ] DataLoaders prevent N+1 query problems
- [ ] Input validation and error handling are comprehensive
- [ ] Performance meets specified requirements
- [ ] Integration tests pass for all scenarios
- [ ] API documentation is complete
- [ ] Security review is completed

### Monitoring and Observability
- **Custom Metrics**
  - GraphQL query execution times
  - Resolver performance by type
  - DataLoader hit/miss ratios
  - Subscription connection counts
  - Authentication success/failure rates

- **Alerts**
  - Query response time >500ms
  - High error rates (>5%)
  - Authentication failures spike
  - WebSocket connection issues
  - Memory usage >80%

- **Dashboards**
  - GraphQL API performance overview
  - Query complexity and frequency analysis
  - User activity and authentication metrics
  - Real-time subscription health

### Documentation Requirements
- Complete GraphQL schema documentation
- API usage examples and best practices
- Authentication and authorization guide
- Performance optimization guidelines
- Troubleshooting common issues