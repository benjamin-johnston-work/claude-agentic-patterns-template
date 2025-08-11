# Feature 02: AI-Powered Repository Search with Azure AI Search

## ⚠️ **ARCHITECTURAL DECISION NOTICE** ⚠️
**Status**: IMPLEMENTED - Azure AI Search Implementation  
**Date**: 2025-08-09  
**Decision**: Azure AI Search vector database for semantic code search  
**Reason**: AI-powered semantic search capabilities with vector embeddings  
**Data Residency**: Australia East region for compliance with Australian data sovereignty requirements  

## Feature Overview

**Feature ID**: F02  
**Feature Name**: AI-Powered Repository Search with Azure AI Search  
**Phase**: Phase 1 (Weeks 5-8) - **NEW IMPLEMENTATION**  
**Bounded Context**: Repository Search and Intelligence Context  

### Business Value Proposition
Enable AI-powered semantic search across repository code, documentation, and metadata using Azure AI Search vector database capabilities. This feature provides the core search infrastructure that transforms static repositories into intelligent, searchable knowledge bases, enabling DeepWiki-like functionality for documentation platforms.

### User Impact
- Developers can perform natural language searches across their entire codebase
- Semantic code discovery finds relevant functions/classes even without exact keyword matches
- AI-powered search understands intent and context, not just keywords
- Repository contents become searchable and discoverable at unprecedented granularity
- Teams can quickly locate relevant code patterns, documentation, and examples

### Success Criteria
- Successfully index 10+ test repositories with 10,000+ files each
- Semantic search returns relevant results with >80% accuracy for natural language queries
- Hybrid search (vector + text) completes within 500ms for typical queries
- Support incremental updates when repository content changes
- Azure AI Search index creation completes within 10 minutes for typical repositories
- Australian data residency compliance maintained (Australia East region)

### Dependencies
- **Prerequisite**: Feature 01 (Repository Connection) - GitHub API integration and repository domain model
- **Azure Services**: Azure AI Search (NEW), Azure OpenAI Service (EXISTS: ract-ai-foundry-dev), Azure Key Vault (NEW)
- **External APIs**: GitHub API (already implemented via Octokit.NET)
- **Resource Group**: ract-ai-dev-agents-rg (Australia East)
- **Subscription**: RACT-AI-Non-Production (c2edcde5-7ea8-4c97-808b-7993f422725d)

## Technical Specification

### Architecture Overview

#### Document-Per-File Model
Each repository file becomes a searchable document in Azure AI Search with:
- **Content**: Raw file content for text search
- **Vector Embeddings**: Generated using Azure OpenAI text-embedding-ada-002
- **Metadata**: Repository, branch, path, language, last modified date
- **Structure**: Code symbols, functions, classes extracted where applicable

#### Hybrid Search Strategy
- **Vector Search**: Semantic similarity using Azure OpenAI embeddings
- **Text Search**: Traditional keyword matching with BM25 scoring
- **Reciprocal Rank Fusion (RRF)**: Combines vector and text results for optimal relevance
- **Filtering**: Repository, language, file type, date range filters

### Domain Model Extensions

```csharp
// New entities for search functionality
public class SearchableDocument
{
    public string Id { get; private set; } // repo_id:file_path hash
    public Guid RepositoryId { get; private set; }
    public string FilePath { get; private set; }
    public string FileName { get; private set; }
    public string FileExtension { get; private set; }
    public string Language { get; private set; }
    public string Content { get; private set; }
    public float[] ContentVector { get; private set; } // Embedding vector
    public int LineCount { get; private set; }
    public long SizeInBytes { get; private set; }
    public DateTime LastModified { get; private set; }
    public string BranchName { get; private set; }
    public DocumentMetadata Metadata { get; private set; }
}

public class DocumentMetadata
{
    public string RepositoryName { get; set; }
    public string RepositoryOwner { get; set; }
    public string RepositoryUrl { get; set; }
    public List<string> CodeSymbols { get; set; } = new(); // Functions, classes, etc.
    public Dictionary<string, string> CustomFields { get; set; } = new();
}

public class SearchQuery
{
    public string Query { get; private set; }
    public SearchType SearchType { get; private set; } // Semantic, Keyword, Hybrid
    public List<SearchFilter> Filters { get; private set; } = new();
    public int Top { get; private set; } = 50;
    public int Skip { get; private set; } = 0;
}

public enum SearchType
{
    Semantic,      // Vector search only
    Keyword,       // Text search only  
    Hybrid         // Combined vector + text search
}

public class SearchFilter
{
    public string Field { get; set; }
    public string Operator { get; set; } // eq, ne, gt, lt, contains
    public object Value { get; set; }
}

public class SearchResult
{
    public string DocumentId { get; set; }
    public double Score { get; set; }
    public SearchableDocument Document { get; set; }
    public List<string> Highlights { get; set; } = new(); // Highlighted text snippets
}
```

### API Specification Extensions

#### GraphQL Schema Changes
```graphql
# New search types
type SearchableDocument {
  id: ID!
  repositoryId: ID!
  filePath: String!
  fileName: String!
  fileExtension: String!
  language: String!
  content: String!
  lineCount: Int!
  sizeInBytes: Long!
  lastModified: DateTime!
  branchName: String!
  metadata: DocumentMetadata!
}

type DocumentMetadata {
  repositoryName: String!
  repositoryOwner: String!
  repositoryUrl: String!
  codeSymbols: [String!]!
  customFields: JSON
}

type SearchResult {
  documentId: String!
  score: Float!
  document: SearchableDocument!
  highlights: [String!]!
}

enum SearchType {
  SEMANTIC
  KEYWORD  
  HYBRID
}

input SearchFilter {
  field: String!
  operator: String! # eq, ne, gt, lt, contains
  value: JSON!
}

input SearchRepositoriesInput {
  query: String!
  searchType: SearchType = HYBRID
  filters: [SearchFilter!]
  top: Int = 50
  skip: Int = 0
}

# Extended repository type
extend type Repository {
  # Get searchable documents for this repository
  documents(
    branch: String
    filePattern: String
    language: String
  ): [SearchableDocument!]!
  
  # Search within this specific repository
  search(input: SearchRepositoriesInput!): [SearchResult!]!
  
  # Get indexing status
  indexStatus: IndexStatus!
}

type IndexStatus {
  status: IndexingStatus!
  documentsIndexed: Int!
  totalDocuments: Int!
  lastIndexed: DateTime
  estimatedCompletion: DateTime
}

enum IndexingStatus {
  NOT_STARTED
  IN_PROGRESS
  COMPLETED
  ERROR
  REFRESHING
}

# New mutations
extend type Mutation {
  # Trigger repository indexing/re-indexing
  indexRepository(repositoryId: ID!, force: Boolean = false): IndexStatus!
  
  # Remove repository from search index
  removeRepositoryFromIndex(repositoryId: ID!): Boolean!
}

# New queries
extend type Query {
  # Global search across all repositories
  searchRepositories(input: SearchRepositoriesInput!): [SearchResult!]!
  
  # Get document by ID
  getDocument(documentId: ID!): SearchableDocument
  
  # Get indexing status for repository
  getIndexStatus(repositoryId: ID!): IndexStatus
}
```

### Azure AI Search Index Schema

```json
{
  "name": "archie-repository-documents-v1",
  "defaultScoringProfile": "hybrid-scoring",
  "fields": [
    {
      "name": "document_id",
      "type": "Edm.String",
      "key": true,
      "searchable": false,
      "filterable": true,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "repository_id",
      "type": "Edm.String",
      "searchable": false,
      "filterable": true,
      "sortable": false,
      "facetable": true
    },
    {
      "name": "file_path",
      "type": "Edm.String",
      "searchable": true,
      "filterable": true,
      "sortable": true,
      "facetable": false
    },
    {
      "name": "file_name",
      "type": "Edm.String",
      "searchable": true,
      "filterable": true,
      "sortable": true,
      "facetable": false
    },
    {
      "name": "file_extension",
      "type": "Edm.String",
      "searchable": false,
      "filterable": true,
      "sortable": false,
      "facetable": true
    },
    {
      "name": "language",
      "type": "Edm.String",
      "searchable": false,
      "filterable": true,
      "sortable": false,
      "facetable": true
    },
    {
      "name": "content",
      "type": "Edm.String",
      "searchable": true,
      "filterable": false,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "content_vector",
      "type": "Collection(Edm.Single)",
      "dimensions": 1536,
      "vectorSearchProfile": "vector-profile",
      "searchable": true,
      "filterable": false,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "line_count",
      "type": "Edm.Int32",
      "searchable": false,
      "filterable": true,
      "sortable": true,
      "facetable": true
    },
    {
      "name": "size_bytes",
      "type": "Edm.Int64",
      "searchable": false,
      "filterable": true,
      "sortable": true,
      "facetable": false
    },
    {
      "name": "last_modified",
      "type": "Edm.DateTimeOffset",
      "searchable": false,
      "filterable": true,
      "sortable": true,
      "facetable": false
    },
    {
      "name": "branch_name",
      "type": "Edm.String",
      "searchable": false,
      "filterable": true,
      "sortable": false,
      "facetable": true
    },
    {
      "name": "repository_name",
      "type": "Edm.String",
      "searchable": true,
      "filterable": true,
      "sortable": false,
      "facetable": true
    },
    {
      "name": "repository_owner",
      "type": "Edm.String",
      "searchable": true,
      "filterable": true,
      "sortable": false,
      "facetable": true
    },
    {
      "name": "repository_url",
      "type": "Edm.String",
      "searchable": false,
      "filterable": false,
      "sortable": false,
      "facetable": false
    },
    {
      "name": "code_symbols",
      "type": "Collection(Edm.String)",
      "searchable": true,
      "filterable": true,
      "sortable": false,
      "facetable": true
    }
  ],
  "vectorSearch": {
    "algorithms": [
      {
        "name": "hnsw-algorithm",
        "kind": "hnsw",
        "hnswParameters": {
          "m": 4,
          "efConstruction": 400,
          "efSearch": 500,
          "metric": "cosine"
        }
      }
    ],
    "profiles": [
      {
        "name": "vector-profile",
        "algorithm": "hnsw-algorithm"
      }
    ]
  },
  "scoringProfiles": [
    {
      "name": "hybrid-scoring",
      "text": {
        "weights": {
          "content": 1.0,
          "file_name": 2.0,
          "file_path": 1.5,
          "repository_name": 1.2,
          "code_symbols": 1.8
        }
      },
      "functions": [
        {
          "type": "freshness",
          "fieldName": "last_modified",
          "boost": 1.1,
          "interpolation": "linear",
          "freshness": {
            "boostingDuration": "P30D"
          }
        }
      ]
    }
  ]
}
```

### Integration Points

#### Azure AI Search Service Interface
```csharp
public interface IAzureSearchService
{
    // Index management
    Task<bool> CreateIndexAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteIndexAsync(CancellationToken cancellationToken = default);
    Task<IndexStatus> GetIndexStatusAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    
    // Document operations
    Task<bool> IndexDocumentAsync(SearchableDocument document, CancellationToken cancellationToken = default);
    Task<bool> IndexDocumentsAsync(IEnumerable<SearchableDocument> documents, CancellationToken cancellationToken = default);
    Task<bool> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);
    Task<bool> DeleteRepositoryDocumentsAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    
    // Search operations
    Task<SearchResults> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
    Task<SearchResults> SearchRepositoryAsync(Guid repositoryId, SearchQuery query, CancellationToken cancellationToken = default);
    Task<SearchableDocument?> GetDocumentAsync(string documentId, CancellationToken cancellationToken = default);
}

public class SearchResults
{
    public long TotalCount { get; set; }
    public List<SearchResult> Results { get; set; } = new();
    public Dictionary<string, List<FacetResult>> Facets { get; set; } = new();
    public TimeSpan SearchDuration { get; set; }
}
```

#### Azure OpenAI Embedding Service Interface
```csharp
public interface IAzureOpenAIEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<List<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
    Task<bool> ValidateServiceAsync(CancellationToken cancellationToken = default);
}
```

#### Repository Indexing Service Interface
```csharp
public interface IRepositoryIndexingService
{
    Task<IndexStatus> IndexRepositoryAsync(Guid repositoryId, bool forceReindex = false, CancellationToken cancellationToken = default);
    Task<IndexStatus> RefreshRepositoryIndexAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    Task<bool> RemoveRepositoryFromIndexAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    Task<IndexStatus> GetIndexingStatusAsync(Guid repositoryId, CancellationToken cancellationToken = default);
}
```

### Configuration Extensions

#### Azure AI Search Configuration (ACTUAL DEPLOYMENT)
```csharp
public class AzureSearchOptions
{
    public const string SectionName = "AzureSearch";
    
    [Required]
    public string ServiceName { get; set; } = "ract-archie-search-dev";
    
    [Required]
    public string ServiceUrl { get; set; } = "https://ract-archie-search-dev.search.windows.net";
    
    [Required]
    public string AdminKey { get; set; } = string.Empty; // From Azure Key Vault
    
    public string QueryKey { get; set; } = string.Empty; // From Azure Key Vault
    
    [Required]
    public string IndexName { get; set; } = "archie-repository-documents-v1";
    
    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 120;
    
    [Range(1, 1000)]
    public int MaxBatchSize { get; set; } = 100;
    
    [Range(1, 10)]
    public int RetryAttempts { get; set; } = 3;
    
    public bool EnableDetailedLogging { get; set; } = false;
}

public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";
    
    [Required]
    public string Endpoint { get; set; } = "https://ract-ai-foundry-dev.openai.azure.com/";
    
    [Required]
    public string ApiKey { get; set; } = string.Empty; // From Azure Key Vault
    
    [Required]
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-ada-002";
    
    public string ApiVersion { get; set; } = "2024-02-01";
    
    [Range(1, 16)]
    public int MaxBatchSize { get; set; } = 8;
    
    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 60;
    
    [Range(1, 10)]
    public int RetryAttempts { get; set; } = 3;
    
    public bool EnableRateLimitProtection { get; set; } = true;
}

public class IndexingOptions  
{
    public const string SectionName = "Indexing";
    
    [Range(1, 100)]
    public int MaxConcurrentIndexingOperations { get; set; } = 5;
    
    [Range(1000, 100000)]
    public int MaxFileContentLength { get; set; } = 32768; // 32KB limit for embedding
    
    public List<string> IndexableFileExtensions { get; set; } = new()
    {
        ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".go", ".rs",
        ".php", ".rb", ".swift", ".kt", ".scala", ".html", ".css", ".sql",
        ".md", ".txt", ".json", ".xml", ".yml", ".yaml", ".sh", ".ps1"
    };
    
    public List<string> IgnoredDirectories { get; set; } = new()
    {
        ".git", ".vs", ".vscode", "node_modules", "bin", "obj", "packages", ".nuget", "target", "build"
    };
    
    [Range(3600, 86400)]
    public int IndexRefreshIntervalSeconds { get; set; } = 21600; // 6 hours
    
    public bool ExtractCodeSymbols { get; set; } = true;
    
    public bool EnableIncrementalIndexing { get; set; } = true;
}
```

### Data Processing Pipeline

#### File Content Processing
```csharp
public class FileContentProcessor
{
    private readonly IAzureOpenAIEmbeddingService _embeddingService;
    private readonly ICodeSymbolExtractor _symbolExtractor;
    private readonly ILogger<FileContentProcessor> _logger;

    public async Task<SearchableDocument> ProcessFileAsync(
        Repository repository, 
        string filePath, 
        string content,
        string branchName,
        CancellationToken cancellationToken = default)
    {
        // 1. Content preprocessing
        var processedContent = PreprocessContent(content, filePath);
        
        // 2. Generate embeddings
        var embedding = await _embeddingService.GenerateEmbeddingAsync(
            processedContent, cancellationToken);
            
        // 3. Extract code symbols (functions, classes, etc.)
        var codeSymbols = await _symbolExtractor.ExtractSymbolsAsync(
            content, GetLanguageFromPath(filePath), cancellationToken);
            
        // 4. Create searchable document
        return CreateSearchableDocument(repository, filePath, content, 
            embedding, codeSymbols, branchName);
    }
}

public interface ICodeSymbolExtractor
{
    Task<List<string>> ExtractSymbolsAsync(string content, string language, CancellationToken cancellationToken = default);
}
```

#### Batch Indexing Workflow
```csharp
public class RepositoryIndexingWorkflow
{
    public async Task<IndexStatus> ExecuteAsync(Repository repository, bool forceReindex)
    {
        var status = new IndexStatus { Status = IndexingStatus.IN_PROGRESS };
        
        try
        {
            // 1. Get repository files
            var files = await _gitHubService.GetRepositoryTreeAsync(
                repository.Owner, repository.Name, repository.DefaultBranch, true);
                
            // 2. Filter indexable files  
            var indexableFiles = FilterIndexableFiles(files);
            
            // 3. Process files in batches
            var documents = new List<SearchableDocument>();
            await foreach (var batch in ProcessFilesInBatches(repository, indexableFiles))
            {
                documents.AddRange(batch);
                
                // Update progress
                status.DocumentsIndexed = documents.Count;
                await _statusService.UpdateStatusAsync(repository.Id, status);
            }
            
            // 4. Upload to Azure AI Search
            await _searchService.IndexDocumentsAsync(documents);
            
            status.Status = IndexingStatus.COMPLETED;
            status.LastIndexed = DateTime.UtcNow;
            
            return status;
        }
        catch (Exception ex)
        {
            status.Status = IndexingStatus.ERROR;
            _logger.LogError(ex, "Failed to index repository {RepositoryId}", repository.Id);
            throw;
        }
    }
}
```

### Security & Compliance Requirements

#### Australian Data Residency Implementation
- **Azure Region**: Australia East (australiaeast) for all resources
- **Data Storage**: All search indexes and embeddings stored in Australia East
- **Processing Location**: All AI/ML processing performed within Australian regions
- **Cross-Border Data Transfer**: None - all data remains within Australian boundaries
- **Compliance**: ACSC Essential 8, Australian Privacy Principles (APP) compliance

#### Azure Key Vault Integration
```csharp
public class AzureKeyVaultSecretProvider : ISecretProvider
{
    private readonly SecretClient _secretClient;
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        var secret = await _secretClient.GetSecretAsync(secretName);
        return secret.Value.Value;
    }
}

// Key Vault secrets to be stored:
// - azure-search-admin-key
// - azure-search-query-key  
// - azure-openai-api-key
// - github-default-access-token (if applicable)
```

#### Access Control and Authentication
- **API Keys**: Stored in Azure Key Vault, rotated every 90 days
- **RBAC**: Azure AI Search uses built-in roles (Search Service Contributor, Search Index Data Contributor)
- **Network Security**: Private endpoints for Azure AI Search and Azure OpenAI
- **Data Encryption**: At rest (256-bit AES) and in transit (TLS 1.2+)

### Performance Requirements

#### Search Performance Targets
- **Query Response Time**: <500ms for hybrid queries (95th percentile)
- **Vector Search Latency**: <200ms for semantic similarity queries
- **Index Refresh Time**: <10 minutes for repositories up to 10,000 files
- **Concurrent Queries**: Support 100+ concurrent search requests
- **Throughput**: Process 1000+ documents per minute during indexing

#### Scalability Considerations
- **Azure AI Search SKU**: Standard S1 minimum (15GB storage, 36 search units)
- **Index Partitioning**: Horizontal scaling for repositories >100,000 files
- **Embedding Generation**: Batch processing with rate limit awareness
- **Caching**: In-memory cache for frequently accessed documents and search results

### Implementation Roadmap

#### Phase 1: Foundation (Weeks 1-2)
1. **Azure Resource Provisioning**
   - Set up Azure AI Search service in Australia East
   - Configure Azure OpenAI Service with text-embedding-ada-002 model
   - Set up Azure Key Vault for secret management
   - Configure private networking and security policies

2. **Core Infrastructure Development**  
   - Implement `IAzureSearchService` with basic CRUD operations
   - Implement `IAzureOpenAIEmbeddingService` for text vectorization
   - Create configuration classes and dependency injection setup
   - Set up logging and telemetry for Azure services

#### Phase 2: Document Processing (Weeks 3-4)
1. **Content Processing Pipeline**
   - Implement `FileContentProcessor` for file analysis
   - Create `ICodeSymbolExtractor` for extracting functions/classes
   - Build batch processing capabilities for large repositories
   - Add content filtering and preprocessing logic

2. **Integration with Existing Repository Service**
   - Extend `GitRepositoryService` to work with Azure AI Search
   - Modify repository domain model to support search metadata
   - Update use cases to trigger indexing operations
   - Create GraphQL resolvers for search functionality

#### Phase 3: Search Implementation (Weeks 5-6) 
1. **Search Service Development**
   - Implement hybrid search capabilities (vector + text)
   - Add query parsing and filter construction
   - Create result ranking and highlighting features
   - Build semantic search for natural language queries

2. **API Integration**
   - Extend GraphQL schema with search types and operations
   - Implement search resolvers with error handling
   - Add pagination and sorting capabilities
   - Create search result caching mechanisms

#### Phase 4: Indexing Automation (Weeks 7-8)
1. **Repository Indexing Service**
   - Implement `IRepositoryIndexingService` with background processing
   - Create incremental indexing for repository updates
   - Add monitoring and status tracking for indexing operations
   - Build retry mechanisms and error recovery

2. **Integration Testing & Performance Optimization**
   - End-to-end testing with real repositories
   - Performance tuning for search queries and indexing
   - Load testing with concurrent users and large datasets
   - Documentation and deployment preparation

### Technical Risks and Mitigation Strategies

#### Risk 1: Azure OpenAI Service Regional Availability
**Risk**: text-embedding-ada-002 model may not be available in Australia East region
**Impact**: High - Core embedding functionality unavailable
**Mitigation**: 
- **Primary**: Use text-embedding-3-large model (latest, more widely available)
- **Secondary**: Deploy Azure OpenAI in nearest available region with data residency approval
- **Fallback**: Implement local embeddings using sentence-transformers or similar OSS models

#### Risk 2: Azure AI Search Cost Overrun
**Risk**: Vector storage and query costs may exceed budget for large repositories
**Impact**: Medium - Potential project budget issues
**Mitigation**:
- Implement content size limits and intelligent filtering
- Use tiered indexing (index most important files first)
- Monitor usage with Azure Cost Management alerts
- Implement query result caching to reduce search API calls

#### Risk 3: Embedding Generation Rate Limits
**Risk**: Azure OpenAI rate limits may slow repository indexing
**Impact**: Medium - Extended indexing times
**Mitigation**:
- Implement intelligent batching with backoff strategies
- Use multiple Azure OpenAI deployments for load distribution
- Cache embeddings to avoid regeneration for unchanged files
- Prioritize indexing of recently modified files

#### Risk 4: Large Repository Indexing Performance
**Risk**: Repositories with 50,000+ files may exceed reasonable indexing times
**Impact**: Medium - User experience degradation
**Mitigation**:
- Implement progressive indexing (index in phases)
- Add file prioritization based on recent activity
- Use Azure Functions for distributed indexing workloads
- Implement smart filtering to exclude binary and generated files

#### Risk 5: Search Result Relevance Quality
**Risk**: Vector search may return semantically similar but contextually irrelevant results
**Impact**: High - Core feature effectiveness compromised
**Mitigation**:
- Implement hybrid search combining vector and keyword approaches
- Use semantic reranking for improved result quality
- Add user feedback mechanisms for relevance tuning
- Implement query understanding and intent detection

### Deployment Architecture

#### Azure Resource Topology
```yaml
# Australia East Region Deployment
Resource Group: rg-archie-prod-australiaeast
  
Azure AI Search:
  - Service: srch-archie-prod-australiaeast
  - SKU: Standard S1 (scalable to S2/S3)
  - Replicas: 2 (high availability)
  - Partitions: 1 (can scale to 12)
  
Azure OpenAI Service:
  - Service: oai-archie-prod-australiaeast  
  - Model Deployments:
    - text-embedding-ada-002 (or text-embedding-3-large)
    - Capacity: 10 TPM (scalable based on usage)
    
Azure Key Vault:
  - Vault: kv-archie-prod-australiaeast
  - Access Policies: Managed Identity integration
  - Network: Private endpoint access only
  
Azure Application Insights:
  - Component: appi-archie-prod-australiaeast
  - Integration: Full telemetry and performance monitoring
```

#### Development Environment Configuration (ACTUAL DEPLOYMENT)
```json
{
  "AzureSearch": {
    "ServiceName": "ract-archie-search-dev",
    "ServiceUrl": "https://ract-archie-search-dev.search.windows.net",
    "IndexName": "archie-repository-documents-v1",
    "AdminKey": "@Microsoft.KeyVault(SecretUri=https://ract-archie-kv-dev.vault.azure.net/secrets/AzureSearch-ApiKey/)",
    "RequestTimeoutSeconds": 120,
    "MaxBatchSize": 100,
    "RetryAttempts": 3
  },
  "AzureOpenAI": {
    "Endpoint": "https://ract-ai-foundry-dev.openai.azure.com/",
    "ApiKey": "@Microsoft.KeyVault(SecretUri=https://ract-archie-kv-dev.vault.azure.net/secrets/AzureOpenAI-ApiKey/)",
    "EmbeddingDeploymentName": "text-embedding-ada-002",
    "ApiVersion": "2024-02-01",
    "MaxBatchSize": 8,
    "RequestTimeoutSeconds": 60,
    "RetryAttempts": 3,
    "EnableRateLimitProtection": true
  },
  "GitHub": {
    "DefaultAccessToken": "@Microsoft.KeyVault(SecretUri=https://ract-archie-kv-dev.vault.azure.net/secrets/GitHub-PersonalAccessToken/)"
  },
  "KeyVault": {
    "VaultUri": "https://ract-archie-kv-dev.vault.azure.net/"
  },
  "Indexing": {
    "MaxConcurrentIndexingOperations": 5,
    "MaxFileContentLength": 32768,
    "IndexRefreshIntervalSeconds": 21600,
    "ExtractCodeSymbols": true,
    "EnableIncrementalIndexing": true
  }
}
```

## Testing Strategy

### Unit Testing Requirements (80% coverage minimum)

#### Azure AI Search Integration Tests
```csharp
[TestClass]
public class AzureSearchServiceTests
{
    [TestMethod]
    public async Task IndexDocumentAsync_ValidDocument_ReturnsSuccess()
    {
        // Test document indexing with proper embedding vectors
    }
    
    [TestMethod] 
    public async Task SearchAsync_HybridQuery_ReturnsRelevantResults()
    {
        // Test hybrid search functionality with vector + text search
    }
    
    [TestMethod]
    public async Task SearchAsync_WithFilters_AppliesCorrectFilters()
    {
        // Test search filtering by repository, language, date range
    }
}
```

#### Embedding Service Tests
```csharp
[TestClass]
public class AzureOpenAIEmbeddingServiceTests
{
    [TestMethod]
    public async Task GenerateEmbeddingAsync_ValidText_Returns1536DimensionVector()
    {
        // Test embedding generation with correct dimensionality
    }
    
    [TestMethod]
    public async Task GenerateEmbeddingsAsync_BatchRequest_HandlesRateLimits()
    {
        // Test batch processing with rate limit handling
    }
}
```

#### Repository Indexing Tests
```csharp
[TestClass]
public class RepositoryIndexingServiceTests
{
    [TestMethod]
    public async Task IndexRepositoryAsync_NewRepository_IndexesAllFiles()
    {
        // Test complete repository indexing workflow
    }
    
    [TestMethod]
    public async Task RefreshRepositoryIndexAsync_IncrementalUpdate_OnlyUpdatesChangedFiles()
    {
        // Test incremental indexing functionality
    }
}
```

### Integration Testing Requirements (40% coverage minimum)

#### End-to-End Search Workflow Tests
- **Repository Indexing to Search**: Complete flow from repository connection to searchable documents
- **Hybrid Search Accuracy**: Validate semantic + keyword search result quality
- **Filter Functionality**: Test complex filtering scenarios
- **Performance Under Load**: Concurrent search and indexing operations

#### Azure Services Integration Tests  
- **Azure AI Search**: Index creation, document operations, search queries
- **Azure OpenAI Service**: Embedding generation, rate limiting, error handling
- **Azure Key Vault**: Secret retrieval, authentication integration
- **Australian Data Residency**: Verify all operations remain within Australia East

#### GraphQL API Integration Tests
- **Search Queries**: Test all search-related GraphQL operations
- **Repository Extensions**: Verify repository search capabilities
- **Error Handling**: Test error scenarios and proper GraphQL error responses
- **Authentication**: Validate search access controls

### Performance Testing Requirements

#### Search Performance Benchmarks
- **Query Latency**: <500ms for hybrid queries (95th percentile)
- **Concurrent Users**: 100+ concurrent search requests
- **Large Result Sets**: Handle queries returning 1000+ results efficiently  
- **Complex Filters**: Performance with multiple simultaneous filters

#### Indexing Performance Benchmarks
- **Repository Size**: Test with repositories up to 50,000 files
- **Concurrent Indexing**: Multiple repositories indexing simultaneously
- **Incremental Updates**: Performance of incremental vs full reindexing
- **Memory Usage**: Monitor memory consumption during large indexing operations

### Test Data Requirements

#### Mock Repository Scenarios
- **Small Repository**: <100 files, mixed languages (C#, JavaScript, Python)
- **Medium Repository**: 1,000-5,000 files, enterprise codebase structure
- **Large Repository**: 10,000+ files, open source project complexity
- **Multi-language Repository**: 10+ programming languages represented

#### Search Query Test Cases
- **Natural Language**: "Find functions that handle user authentication"
- **Technical Terms**: "Redis cache implementation", "JWT token validation"  
- **Code Patterns**: "async methods that return Task<Result>", "error handling"
- **Mixed Queries**: Combine semantic search with specific file filters

## Quality Assurance

### Code Review Checkpoints
- [ ] Azure AI Search integration follows Microsoft best practices
- [ ] Embedding generation handles rate limits and retries properly
- [ ] Search queries are optimized for performance and relevance
- [ ] Australian data residency requirements are enforced
- [ ] Error handling covers all Azure service failure scenarios
- [ ] Vector search algorithms are configured optimally (HNSW parameters)
- [ ] Security configurations follow principle of least privilege
- [ ] Logging and telemetry provide adequate observability
- [ ] Cost management controls are implemented and monitored

### Definition of Done Checklist
- [ ] All unit tests pass with >80% coverage
- [ ] Integration tests pass with >40% coverage  
- [ ] Azure AI Search index created successfully in Australia East
- [ ] Azure OpenAI Service generates embeddings for code content
- [ ] Hybrid search returns relevant results for natural language queries
- [ ] Repository indexing completes within performance targets
- [ ] GraphQL API extensions implemented and documented
- [ ] Australian data residency compliance verified and documented
- [ ] Performance benchmarks met under load testing
- [ ] Security review completed and approved
- [ ] Azure cost monitoring and alerting configured
- [ ] Deployment pipeline functional for Australia East region

### Monitoring and Observability

#### Custom Metrics
- **Search Performance**:
  - Query response time percentiles (50th, 95th, 99th)
  - Search result relevance scores
  - Query success/failure rates
  - Filter usage patterns

- **Indexing Performance**:
  - Documents indexed per minute
  - Repository indexing completion time
  - Embedding generation throughput
  - Incremental vs full indexing ratios

- **Azure Service Health**:
  - Azure AI Search availability and performance
  - Azure OpenAI Service rate limit utilization
  - Vector index size and storage usage
  - Cross-region latency (Australia East focus)

#### Alerts Configuration
- **Performance Alerts**:
  - Search query response time >1000ms (P95)
  - Repository indexing time >30 minutes for typical repositories
  - Azure AI Search service availability <99.9%
  - Azure OpenAI Service rate limit errors >5%

- **Cost Management Alerts**: 
  - Monthly Azure AI Search costs exceed budget threshold
  - Azure OpenAI token usage above projected limits
  - Unexpected cost spikes in vector storage or queries

#### Dashboards
- **Search Analytics Dashboard**:
  - Real-time search query volume and performance
  - Most searched terms and repositories
  - User search behavior patterns
  - Search result click-through rates

- **Repository Intelligence Dashboard**:
  - Repository indexing status and health
  - Content analysis insights (languages, file types)
  - Embedding quality metrics
  - Index growth trends over time

- **Infrastructure Health Dashboard**:
  - Azure services performance and availability
  - Australian data residency compliance status  
  - Security and access control metrics
  - Cost tracking and optimization opportunities

### Documentation Requirements
- **API Documentation**: GraphQL schema documentation for search functionality
- **Developer Guide**: Integration guide for adding semantic search capabilities  
- **Operations Manual**: Azure AI Search administration and troubleshooting
- **Compliance Guide**: Australian data residency implementation details
- **Architecture Decision Records**: Key technical decisions and rationales

---

## Conclusion

This feature transforms Archie from a basic repository connection tool into an intelligent, AI-powered documentation platform. By leveraging Azure AI Search vector database capabilities with Azure OpenAI embeddings, we enable semantic code discovery that understands developer intent beyond simple keyword matching.

The hybrid search approach combines the precision of traditional text search with the contextual understanding of vector similarity, delivering a search experience comparable to advanced AI coding assistants while maintaining strict Australian data residency compliance.

This foundation enables future features like intelligent documentation generation, code pattern discovery, and AI-powered code recommendations, positioning Archie as a comprehensive solution for repository intelligence and developer productivity.