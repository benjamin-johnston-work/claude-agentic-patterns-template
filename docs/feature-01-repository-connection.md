# Feature 01: Repository Connection and Management

## ✅ **IMPLEMENTATION STATUS** ✅
**Status**: IMPLEMENTED with Azure AI Search Integration  
**Date**: 2025-08-09  
**Architecture**: Azure AI Search vector database with Azure OpenAI embeddings  
**Related Feature**: [Feature 02: AI-Powered Repository Search](./feature-02-azure-ai-search-implementation.md)  

**What was implemented:**
- ✅ GitHub API integration (Octokit.NET)
- ✅ Repository domain model and GraphQL schema
- ✅ Repository connection and analysis workflow
- ✅ Azure AI Search integration for repository indexing
- ✅ Azure OpenAI embeddings for semantic search
- ✅ GitHubService with comprehensive repository operations

**Architecture Decision:**
- ✅ Azure AI Search vector database implementation
- ✅ Australian data residency compliance (Australia East region)
- ✅ Hybrid search capabilities (vector + text search)
- ✅ Azure Key Vault integration for secret management

## Feature Overview

**Feature ID**: F01  
**Feature Name**: Repository Connection and Management  
**Phase**: Phase 1 (Weeks 1-4) - **COMPLETED with Azure AI Search**  
**Bounded Context**: Repository Management Context  

### Business Value Proposition
Enable users to connect their Git repositories to the platform, providing the foundational capability for all subsequent AI-powered documentation features. This feature establishes the core data ingestion pipeline that makes the entire system valuable.

### User Impact
- Developers can easily connect their repositories to start leveraging AI-powered documentation
- Repository owners get immediate visibility into their codebase structure
- Teams can begin collaborative code exploration workflows

### Success Criteria
- Successfully connect and ingest 10+ test repositories from GitHub
- Complete repository metadata extraction within 2 minutes for typical repositories
- 95% success rate for repository connection attempts
- GraphQL API returns accurate repository structure data

### Dependencies
- None (foundational feature)

## Technical Specification

### Domain Model
```csharp
// Repository Aggregate Root
public class Repository
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Url { get; private set; }
    public string Language { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public RepositoryStatus Status { get; private set; }
    public List<Branch> Branches { get; private set; }
    public RepositoryStatistics Statistics { get; private set; }
}

public enum RepositoryStatus
{
    Connecting,
    Connected,
    Analyzing,
    Ready,
    Error,
    Disconnected
}
```

### API Specification

#### GraphQL Schema Changes
```graphql
type Repository {
  id: ID!
  name: String!
  url: String!
  language: String!
  description: String
  status: RepositoryStatus!
  branches: [Branch!]!
  statistics: RepositoryStatistics
  createdAt: DateTime!
  updatedAt: DateTime!
}

enum RepositoryStatus {
  CONNECTING
  CONNECTED
  ANALYZING
  READY
  ERROR
  DISCONNECTED
}

type Branch {
  name: String!
  isDefault: Boolean!
  lastCommit: Commit
  createdAt: DateTime!
}

type Commit {
  hash: String!
  message: String!
  author: String!
  timestamp: DateTime!
}

type RepositoryStatistics {
  fileCount: Int!
  lineCount: Int!
  languageBreakdown: [LanguageStats!]!
}

input AddRepositoryInput {
  url: String!
  accessToken: String
  branch: String
}

type Mutation {
  addRepository(input: AddRepositoryInput!): Repository!
  refreshRepository(id: ID!): Repository!
  removeRepository(id: ID!): Boolean!
}

type Query {
  repositories(filter: RepositoryFilter): [Repository!]!
  repository(id: ID!): Repository
}
```

### Data Storage Architecture

#### Azure AI Search Integration
Repository connection functionality is now integrated with Azure AI Search vector database. This provides:

- **Repository Metadata Storage**: Core repository information stored in domain entities
- **Searchable Document Index**: Individual files indexed as searchable documents with embeddings
- **Hybrid Search Capabilities**: Combined vector and text search for intelligent repository discovery
- **Australian Data Residency**: All data stored in Australia East region for compliance

**Key Benefits:**
- Semantic search across repository content using AI embeddings
- Natural language querying capabilities ("Find authentication functions")
- Scalable vector storage for large repository collections
- Integration with Azure OpenAI for embeddings generation

**Implementation Details:**
See [Feature 02: AI-Powered Repository Search](./feature-02-azure-ai-search-implementation.md) for comprehensive Azure AI Search schema, indexing strategies, and search capabilities.

### Integration Points

#### GitHub Integration (Implemented)
```csharp
public interface IGitRepositoryService
{
    Task<Repository> ConnectRepositoryAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetBranchesAsync(string repositoryUrl, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Commit>> GetCommitHistoryAsync(string repositoryUrl, string branch, int limit = 100, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<RepositoryStatistics> AnalyzeRepositoryStructureAsync(string repositoryUrl, string branch = "main", string? accessToken = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateRepositoryAccessAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default);
}

// Implemented via GitHubService using Octokit.NET
public interface IGitHubService
{
    Task<GitHubRepositoryInfo> GetRepositoryAsync(string owner, string repository, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<GitHubBranchInfo>> GetBranchesAsync(string owner, string repository, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<GitHubTree> GetRepositoryTreeWithMetadataAsync(string owner, string repository, string branch = "main", bool recursive = true, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<GitHubCommitInfo>> GetCommitHistoryAsync(string owner, string repository, string branch, int limit = 100, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<string> GetFileContentAsync(string owner, string repository, string filePath, string branch = "main", string? accessToken = null, CancellationToken cancellationToken = default);
    (string Owner, string Repository) ParseRepositoryUrl(string url);
}
```

#### Azure Service Bus Events
```csharp
// Repository Events
public class RepositoryAddedEvent : IEvent
{
    public Guid RepositoryId { get; set; }
    public string Url { get; set; }
    public DateTime Timestamp { get; set; }
}

public class RepositoryAnalysisStartedEvent : IEvent
{
    public Guid RepositoryId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class RepositoryAnalysisCompletedEvent : IEvent
{
    public Guid RepositoryId { get; set; }
    public RepositoryStatistics Statistics { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Security Requirements
- Secure handling of GitHub personal access tokens using Azure Key Vault
- Token encryption at rest and in transit
- Repository access validation before analysis
- Rate limiting for repository connection attempts (5 per minute per user)
- Input validation for repository URLs (whitelist allowed domains)

### Performance Requirements
- Repository connection initiation < 5 seconds
- Complete repository analysis < 5 minutes for repositories up to 10,000 files
- Support concurrent processing of up to 10 repositories
- GraphQL queries return repository data < 500ms

## Implementation Guidance

### Implementation Approach (Completed)
1. **✅ Infrastructure**: Azure resources set up (AI Search, Key Vault, OpenAI Service)
2. **✅ Domain Model**: Repository aggregate implemented with proper encapsulation
3. **✅ Integration Layer**: GitHub service integration with comprehensive error handling
4. **✅ API Layer**: GraphQL resolvers implemented with validation
5. **✅ Search Integration**: Azure AI Search indexing and search capabilities
6. **✅ Event Handling**: Event-driven communication via Azure Service Bus

### ✅ Key Architectural Decisions (Azure AI Search architecture)
- ✅ Use Octokit.NET for GitHub API integration (no local cloning required)
- ✅ Implement repository analysis via GitHub API for security and performance
- ✅ Store searchable content in Azure AI Search with vector embeddings
- ✅ Use Azure Service Bus for reliable event communication
- ✅ Implement Azure OpenAI embeddings for semantic search capabilities
- ✅ Australian data residency compliance via Australia East region deployment
- ✅ Hybrid search combining vector similarity with traditional text search

### Technical Risks and Mitigation
1. **Risk**: Large repository analysis timeouts
   - **Mitigation**: Implement streaming analysis with progress tracking via GitHub API
   - **Fallback**: Progressive analysis with file tree pagination for initial indexing

2. **Risk**: GitHub API rate limiting
   - **Mitigation**: Implement exponential backoff and request queuing
   - **Fallback**: Cache GitHub metadata locally with periodic refresh

3. **Risk**: Azure AI Search performance with large file counts
   - **Mitigation**: Batch document indexing and use streaming updates
   - **Fallback**: Implement progressive indexing in phases

### Deployment Considerations (Implemented)
- ✅ Azure AI Search deployed in Australia East region (Standard S1 tier)
- ✅ Azure OpenAI Service integration for embeddings generation
- ✅ Azure Key Vault configured for secret management
- ✅ Service Bus configured with dead letter queues for failed messages
- ✅ Application Insights metrics for repository processing and search performance
- ✅ Australian data residency compliance verified
- ✅ Private networking and security policies implemented

## Testing Strategy

### Unit Testing Requirements (80% coverage minimum)
- **Repository Domain Model**
  - Repository creation and validation
  - Status transitions and business rules
  - Branch and commit management
  - Statistics calculation accuracy

- **Git Service Integration**
  - Repository connection and analysis success/failure scenarios
  - Branch detection and analysis
  - File structure parsing
  - Error handling for invalid repositories

- **GraphQL Resolvers**
  - Query result accuracy
  - Input validation
  - Error response formatting
  - Authentication integration

- **Event Handling**
  - Event serialization/deserialization
  - Message publishing and consumption
  - Dead letter queue handling

### Integration Testing Requirements (30% coverage minimum)
- **End-to-End Repository Connection**
  - Complete flow from URL input to ready status
  - GitHub authentication validation
  - Repository data persistence verification
  - Event flow validation

- **GraphQL API Integration**
  - Repository queries with filters
  - Mutation operations
  - Error handling scenarios
  - Performance under load

- **Azure Services Integration**
  - Service Bus message handling
  - Key Vault secret retrieval
  - Azure AI Search connectivity and operations
  - Azure OpenAI embeddings generation
  - Application Insights telemetry

- **GitHub API Integration**
  - Public and private repository access
  - Rate limiting handling
  - Authentication token validation
  - Repository metadata accuracy

### Test Data Requirements
- Mock repositories with various languages (C#, JavaScript, Python, Go)
- Repositories of different sizes (small: <100 files, medium: 1000 files, large: 10000 files)
- Private and public repository test cases
- Invalid repository URLs and access token scenarios

### Performance Testing
- Load test with 10 concurrent repository connections
- Stress test Azure AI Search with 10,000+ document index
- Memory usage validation for large repository processing
- GraphQL query and search performance benchmarking
- Vector search latency and relevance quality testing

## Quality Assurance

### Code Review Checkpoints
- [ ] Domain model follows DDD principles with proper encapsulation
- [ ] Git operations are properly isolated and error-handled
- [ ] GraphQL schema follows naming conventions
- [ ] Event handling implements proper retry mechanisms
- [ ] Security considerations are properly implemented
- [ ] Azure AI Search queries are optimized with proper vector configuration
- [ ] Azure resource configurations follow best practices
- [ ] Logging and telemetry are comprehensive

### Definition of Done Checklist
- [ ] All unit tests pass with >80% coverage
- [ ] Integration tests pass with >30% coverage
- [ ] GitHub repository connection works for public repositories
- [ ] Private repository connection works with valid tokens
- [ ] Repository data is correctly indexed in Azure AI Search with embeddings
- [ ] GraphQL API returns accurate repository information
- [ ] Events are published to Service Bus on state changes
- [ ] Azure deployment pipeline is functional
- [ ] Performance meets specified requirements
- [ ] Security review completed and approved
- [ ] Documentation updated

### Monitoring and Observability
- **Custom Metrics**
  - Repository connection success rate
  - Average repository analysis time
  - File processing throughput
  - GitHub API quota utilization

- **Alerts**
  - Repository connection failure rate >5%
  - Analysis time >10 minutes
  - Service Bus message processing failures
  - Azure AI Search connection and indexing issues
  - Azure OpenAI embeddings generation failures

- **Dashboards**
  - Repository processing and indexing pipeline health
  - GitHub integration status
  - Azure AI Search performance and vector index metrics
  - Azure OpenAI embeddings usage and performance
  - User search activity and adoption patterns

### Documentation Requirements
- API documentation for GraphQL schema
- Developer guide for adding new Git providers
- Operations runbook for common issues
- Architecture decision records for key choices