# Claude Code Configuration

## Project Overview

This project is configured for development with Claude Code CLI.

## Local Development Dependencies

### Required Software
- .NET 9.0 SDK or later
- Git for Windows 2.30+
- Node.js 20.x LTS
- PowerShell 5.1+ or PowerShell Core 7.0+

### Quick Setup
Run the automated setup script from the project root:
```powershell
# Run as Administrator
.\scripts\Setup-ArchieDevEnvironment.ps1
```

### Manual Setup (if automated setup fails)
1. Install .NET 9.0 SDK from https://dotnet.microsoft.com/download
4. Install Git for Windows
5. Run `dotnet restore` to restore NuGet packages

### Health Check
Validate your environment setup:
```powershell
.\scripts\Test-ArchieEnvironment.ps1
```

## Azure Environment Configuration

### Azure Subscription Details
- **Subscription**: RACT-AI-Non-Production
- **Subscription ID**: `c2edcde5-7ea8-4c97-808b-7993f422725d`
- **Tenant ID**: `ceb29e10-60d1-4f6b-86b1-b7d497b5b66e`
- **Tenant Domain**: `ract.com.au`
- **Primary User**: `b.johnston@ract.com.au`

### Resource Groups
- **Development**: `ract-ai-dev-agents-rg` (Australia East)

### Azure Resources (Development Environment)
- **Azure OpenAI**: `ract-ai-foundry-dev` (Australia East)
- **Azure AI Search**: `ract-archie-search-dev` (Australia East)
- **Key Vault**: `ract-archie-kv-dev` (Australia East)
- **Storage Account**: `ractknowledgemvp` (Australia East)

### Common Azure CLI Commands
```bash
# Login and set subscription
az login
az account set --subscription "c2edcde5-7ea8-4c97-808b-7993f422725d"

# Resource group operations
az group list --location australiaeast
az resource list --resource-group ract-ai-dev-agents-rg --output table

# Azure AI Search operations
az search service show --name ract-archie-search-dev --resource-group ract-ai-dev-agents-rg
az search admin-key show --service-name ract-archie-search-dev --resource-group ract-ai-dev-agents-rg

# Azure OpenAI operations
az cognitiveservices account show --name ract-ai-foundry-dev --resource-group ract-ai-dev-agents-rg
az cognitiveservices account keys list --name ract-ai-foundry-dev --resource-group ract-ai-dev-agents-rg

# Key Vault operations
az keyvault show --name ract-archie-kv-dev --resource-group ract-ai-dev-agents-rg
az keyvault secret list --vault-name ract-archie-kv-dev
```

## Common Commands
- `'powershell "Stop-Process-Force"'` - Stop Process

## Enhanced Build Commands

### Development Workflow
- `dotnet build` - Build entire solution
- `dotnet test` - Run all tests  
- `dotnet run --project src\Archie.Api` - Start API server
- `.\scripts\Start-ArchieDevEnvironment.ps1` - Start full development environment
- `.\scripts\Stop-ArchieDevEnvironment.ps1` - Stop development environment
- `.\scripts\Test-ArchieEnvironment.ps1` - Environment health check

### Frontend Development
- `cd src/frontend && npm install` - Install frontend dependencies
- `npm run dev` - Start Next.js development server (http://localhost:3000)
- `npm run build` - Build production frontend
- `npm run test` - Run Jest unit tests
- `npx playwright test` - Run end-to-end tests

### Testing & Validation
- **API Testing**: Use GraphQL playground at `/graphql` when API running
- **Documentation Testing**: Test with diverse repositories to validate accuracy
- **Content Analysis Validation**: Check logs for proper content-based analysis

### Troubleshooting
- Check Azure OpenAI connectivity for documentation generation
- Verify Azure Search connection for repository storage
- Monitor ContentSummarizationService for file processing errors

## Architecture & Tech Stack

### Primary Technologies
- **Backend**: .NET 9.0 Web API with GraphQL (HotChocolate)
- **Frontend**: Next.js 14 with TypeScript, Tailwind CSS, shadcn/ui
- **Architecture**: Microsoft GraphRAG with Azure AI Search vector indexing
- **Data Storage**: Azure AI Search with semantic search capabilities
- **AI Services**: Azure OpenAI GPT-4.1, text-embedding-ada-002
- **Authentication**: NextAuth.js with Azure AD integration
- **Testing**: NUnit 4.1.0, Moq, Jest (frontend), Playwright (E2E)
- **Infrastructure**: Azure Cloud Services (Search, OpenAI, Key Vault)

### Architecture Pattern
- **Clean Architecture**: Domain, Application, Infrastructure, API layers with strict dependency inversion
- **Microsoft GraphRAG**: Knowledge graph construction with semantic search and AI-powered insights
- **Domain-Driven Design**: Rich value objects, aggregates, domain events, and use cases
- **Repository Pattern**: Data access abstraction via Azure Search with vector embeddings
- **CQRS**: Command/Query separation with GraphQL mutations/queries
- **Event-Driven Architecture**: Domain events for cross-cutting concerns
- **Microservices-Ready**: Structured for future service decomposition

## Core Features & Implementation Status

### ✅ Completed Features (F01-F14)
- **F01: Repository Connection & Management**: GitHub integration, validation, indexing pipeline
- **F02: Azure AI Search Implementation**: Vector search, semantic indexing, document storage
- **F03: AI-Powered Documentation Generation**: Content-based analysis, contextual documentation
- **F04: Conversational Query Interface**: Natural language repository queries with context
- **F05: Semantic Kernel Code Analysis**: Advanced code understanding and relationship mapping
- **F06: Event-Driven Architecture**: Domain events, messaging, cross-service communication  
- **F07: Rate Limiting & API Optimization**: Request throttling, performance monitoring
- **F08: GitHub Webhooks & Real-time Updates**: Live repository synchronization
- **F09: Azure DevOps CI/CD Infrastructure**: Automated deployment pipelines
- **F10: Authentication & Security**: Azure AD integration, role-based access control
- **F11: Performance Monitoring & Observability**: Application insights, telemetry
- **F12: GraphRAG Knowledge Construction**: Entity extraction, relationship mapping
- **F13: GraphRAG Visual Discovery Interface**: Interactive knowledge graph exploration
- **F14: Enterprise GraphRAG Analytics**: Compliance reporting, usage analytics
  
### 🔧 Key Services
- **Core Analysis Services**:
  - `ContentSummarizationService`: File content analysis for AI context
  - `RepositoryAnalysisService`: Content-driven repository understanding
  - `AIDocumentationGeneratorService`: Context-aware documentation generation
- **Azure Integration Services**:
  - `AzureSearchService`: Vector search and semantic indexing
  - `AzureOpenAIEmbeddingService`: Text embedding generation
  - `CodeSymbolExtractor`: Programming language symbol extraction
- **Repository Services**:
  - `GitHubService`: GitHub API integration and webhook handling
  - `GitRepositoryService`: Git operations and repository management
  - `RepositoryIndexingService`: Automated repository content indexing
- **Conversation Services**:
  - `ConversationalAIService`: Natural language query processing
  - `ConversationContextService`: Context management for conversations

## Code Style & Best Practices

- **Modular Design**: Files under 500 lines
- **Environment Safety**: Never hardcode secrets
- **Test-First**: Write tests before implementation using NUnit
- **Clean Architecture**: Separate concerns
- **Documentation**: Keep updated

## Critical Patterns & Antipatterns

### **Clean Architecture Patterns**

#### **✅ Pattern: Infrastructure Data Transfer Objects (DTOs)**
- **DO**: Create separate DTOs for external systems (Azure Search, APIs)
- **DO**: Flatten complex domain objects (List<string> → comma-separated string)
- **DO**: Use mappers to convert between domain and infrastructure types
- **DON'T**: Send complex domain objects directly to external services
- **DON'T**: Assume external systems can handle your domain object structure

#### **✅ Pattern: Type System Validation**
- **DO**: Verify external system type requirements explicitly (DateTimeOffset vs DateTime)
- **DO**: Test type compatibility before implementing full workflows
- **DON'T**: Assume compatible types without verification
- **DON'T**: Ignore type mismatch warnings in external API documentation

### **Azure Integration Patterns**

#### **✅ Pattern: Schema-First Azure Search Integration**
- **DO**: Define Azure Search index schema with primitive types only
- **DO**: Create DTOs that match the exact schema field types
- **DO**: Test single document indexing before batch operations
- **DON'T**: Index complex objects with nested collections directly
- **DON'T**: Use DateTime when Azure Search expects DateTimeOffset

#### **✅ Pattern: Capacity Planning for Azure OpenAI**
- **DO**: Calculate concurrent request needs before deployment
- **DO**: Provision adequate units (10 units = 10 req/10sec minimum for concurrent operations)
- **DO**: Monitor and upgrade capacity proactively
- **DON'T**: Use default/minimum capacity (S0 = 1 req/10sec) for production workloads
- **DON'T**: Assume rate limits won't impact your workflow

### **Background Processing Patterns**

#### **✅ Pattern: Comprehensive Error Categorization**
- **DO**: Handle specific exception types with targeted responses
- **DO**: Implement separate catch blocks for HTTP, Auth, Timeout, and General exceptions
- **DO**: Log specific error types with actionable context
- **DON'T**: Use generic catch-all exception handling
- **DON'T**: Let background tasks fail silently without error categorization

#### **✅ Pattern: Workflow Step Visibility**
- **DO**: Log progress at each major workflow step with step numbers
- **DO**: Include counts, timing, and success/failure metrics
- **DO**: Update status tracking throughout long-running operations
- **DON'T**: Run long operations without intermediate status updates
- **DON'T**: Hide workflow progress from debugging visibility

### **Development Environment Patterns**

#### **✅ Pattern: Debug-Friendly Development Scripts**
- **DO**: Use visible console windows during development
- **DO**: Enable detailed logging in development environment
- **DO**: Provide real-time feedback for long-running operations
- **DON'T**: Use `-WindowStyle Hidden` or equivalent in development scripts
- **DON'T**: Suppress console output during active development

#### **✅ Pattern: Local Configuration Safety**
- **DO**: Use `appsettings.Local.json` (gitignored) for development secrets
- **DO**: Validate all required secrets are present at startup
- **DO**: Use Azure Key Vault for production deployments
- **DON'T**: Hardcode API keys or connection strings anywhere in code
- **DON'T**: Commit real API keys to version control

### **Dependency Injection Patterns**

#### **✅ Pattern: Complete Dependency Chain Registration**
- **DO**: Register all dependencies required by your services
- **DO**: Verify DI registration completeness before deployment
- **DO**: Use DI container validation in development builds
- **DON'T**: Register services without registering their dependencies
- **DON'T**: Assume dependencies will be auto-registered

### **GraphQL Schema Patterns**

#### **✅ Pattern: Explicit Type Configuration (HotChocolate)**
- **DO**: Use `ObjectType<T>` with explicit field configuration instead of direct domain model exposure
- **DO**: Configure field types explicitly with `Type<NonNullType<StringType>>()` for required fields
- **DO**: Use `InputObjectType<T>` for complex input validation and documentation
- **DON'T**: Expose domain models directly without GraphQL type wrappers
- **DON'T**: Rely solely on convention-based type inference for production schemas

#### **✅ Pattern: GraphQL Naming Conventions**
- **DO**: Use PascalCase for Types, Interfaces, Unions (`UserProfile`, `RepositoryType`)
- **DO**: Use camelCase for fields and arguments (`firstName`, `createdAt`, `repositoryId`)
- **DO**: Suffix input types with "Input" (`AddRepositoryInput`, `QueryInput`)
- **DO**: Suffix enum types with "Type" or "Enum" for clarity (`RepositoryStatusType`)
- **DON'T**: Use acronyms, abbreviations, or unclear naming in schema
- **DON'T**: Mix naming conventions within the same schema

#### **✅ Pattern: Resolver Organization and Separation**
- **DO**: Separate query and mutation resolvers into distinct classes
- **DO**: Use `[ExtendObjectType(typeof(Query))]` and `[ExtendObjectType(typeof(Mutation))]`
- **DO**: Group related operations in the same resolver (all repository operations together)
- **DO**: Inject dependencies via constructor for testability
- **DON'T**: Mix queries and mutations in the same resolver class
- **DON'T**: Create monolithic resolvers with unrelated operations

#### **✅ Pattern: Async Resolver Implementation**
- **DO**: Use async/await pattern for all database and external API operations
- **DO**: Accept and forward `CancellationToken` parameters to enable operation cancellation
- **DO**: Return `Task<T>` from resolver methods for proper async handling
- **DON'T**: Use blocking synchronous calls in resolver methods
- **DON'T**: Ignore cancellation tokens in long-running operations

#### **✅ Pattern: Field-Level Resolvers and Computed Fields**
- **DO**: Use field-level resolvers for computed or expensive operations
- **DO**: Access parent context with `context.Parent<T>()` in field resolvers
- **DO**: Lazy-load related data using field resolvers instead of eager loading
- **DON'T**: Compute expensive operations in object properties
- **DON'T**: Load unnecessary data when fields aren't requested

#### **✅ Pattern: Error Handling and Resilience**
- **DO**: Use try-catch blocks in resolvers with graceful degradation
- **DO**: Return null for optional data when operations fail
- **DO**: Log specific errors with context for debugging
- **DO**: Provide meaningful default values for computed fields during failures
- **DON'T**: Let unhandled exceptions crash GraphQL operations
- **DON'T**: Return empty objects when null is more appropriate

#### **✅ Pattern: Input Validation and Documentation**
- **DO**: Use `NonNullType<T>` for required fields and arguments
- **DO**: Provide `DefaultValue()` for optional parameters with sensible defaults
- **DO**: Add `Description()` to fields, arguments, and types for schema documentation
- **DO**: Validate complex inputs using dedicated input types
- **DON'T**: Leave input types undocumented
- **DON'T**: Use primitive types for complex input validation

#### **✅ Pattern: Background Operations with Immediate Response**
- **DO**: Return immediate status for long-running operations (indexing, analysis)
- **DO**: Use separate cancellation tokens for GraphQL vs background operations
- **DO**: Provide status tracking queries for monitoring background operations
- **DON'T**: Block GraphQL requests waiting for long-running operations
- **DON'T**: Let GraphQL timeouts cancel critical background processes

#### **✅ Pattern: Service Integration and Dependency Injection**
- **DO**: Access services via `context.Service<T>()` in field resolvers
- **DO**: Inject service dependencies in resolver constructors
- **DO**: Use proper scoping for service lifetimes (singleton, scoped, transient)
- **DON'T**: Create service instances directly in resolvers
- **DON'T**: Share mutable state between resolver instances

### **UI/UX Design Patterns**

#### **✅ Pattern: Professional Documentation Navigation**
- **DO**: Use typography hierarchy (font weights, sizes) over visual icons/emojis
- **DO**: Implement clean, minimal headers with subtle styling ("Contents" vs "Table of Contents")
- **DO**: Apply consistent color schemes (blue accents for active states, grays for hierarchy)
- **DO**: Use subtle hover states and clear selection indicators (ChevronRight icons)
- **DON'T**: Overload interface with badges, metadata counts, or content previews
- **DON'T**: Mix emojis with professional icon systems (creates visual discord)

#### **✅ Pattern: Enterprise-Grade Visual Hierarchy**
- **DO**: Establish semantic color coding (Overview=semibold, Getting Started=blue medium, API=gray normal)
- **DO**: Use consistent spacing patterns (px-3 py-2 for navigation items)
- **DO**: Apply professional interaction patterns (rounded-md transition-colors)
- **DO**: Implement minimal footer patterns (only show filtering info when relevant)
- **DON'T**: Use emoji-based categorization in professional interfaces
- **DON'T**: Create competing visual elements that distract from core navigation

#### **✅ Pattern: Search and Filtering UX**
- **DO**: Use subtle highlighting for search terms (bg-blue-100 with font-medium)
- **DO**: Show filtering context only when active ("Filtered by X")
- **DO**: Maintain clean visual feedback without overwhelming the interface
- **DON'T**: Use bright, distracting highlight colors (yellow backgrounds)
- **DON'T**: Show unnecessary metadata when not filtering

#### **✅ Pattern: Component State Management**
- **DO**: Use React.useMemo for expensive computations and filtering operations
- **DO**: Implement proper array handling with spread operators [...array].sort() for read-only data
- **DO**: Apply consistent loading and error states across components
- **DON'T**: Mutate GraphQL result arrays directly (causes read-only property errors)
- **DON'T**: Skip memoization for complex filtering or sorting operations

### **Testing & Validation Patterns**

#### **✅ Pattern: Integration Point Validation**
- **DO**: Test each external integration independently before end-to-end testing
- **DO**: Validate API connectivity, authentication, and basic operations first
- **DO**: Use health checks for critical external dependencies
- **DON'T**: Test complex workflows without validating individual components
- **DON'T**: Skip integration testing of external services

#### **✅ Pattern: Hypothesis-Driven Debugging**
- **DO**: Create specific, testable hypotheses when facing complex failures
- **DO**: Test one hypothesis at a time with targeted changes
- **DO**: Document root causes and solutions for future reference
- **DON'T**: Make multiple changes simultaneously during debugging
- **DON'T**: Assume issues without systematic validation

### **Data Mapping Patterns**

#### **✅ Pattern: External System Compatibility Layer**
- **DO**: Create compatibility layers for external system data requirements
- **DO**: Transform collections to external system formats (comma-separated strings)
- **DO**: Handle null/empty collections gracefully in transformations
- **DON'T**: Send internal collection types to external systems expecting primitives
- **DON'T**: Assume external systems will handle your internal data structures

### **Monitoring & Observability Patterns**

#### **✅ Pattern: Operational Visibility**
- **DO**: Include detailed success/error counts in batch operations
- **DO**: Log performance metrics (duration, throughput, error rates)
- **DO**: Provide clear status updates for long-running background operations
- **DON'T**: Process data in batches without progress reporting
- **DON'T**: Hide operational metrics from monitoring systems

## Git Commit Conventions

**All commits must follow conventional commit format with proper prefixing:**

### Commit Types
- **Feature**: New feature implementation (`Feature: Add repository indexing pipeline`)
- **Task**: Development tasks, refactoring, improvements (`Task: Refactor AzureSearchService for better error handling`)
- **Bug**: Bug fixes (`Bug: Fix null reference in DocumentationGenerator`)  
- **Hotfix**: Critical production fixes (`Hotfix: Resolve Azure Search connection timeout`)
- **Security**: Security-related changes (`Security: Remove hardcoded API keys from configuration`)
- **Docs**: Documentation updates (`Docs: Update API integration guide`)
- **Test**: Test additions/modifications (`Test: Add unit tests for ConversationService`)
- **CI/CD**: Build, deployment, pipeline changes (`CI/CD: Update Azure deployment pipeline`)

### Format Examples
```
Feature: Implement F05 semantic kernel code analysis foundation
Task: Optimize Azure Search query performance for large repositories  
Bug: Fix repository validation failing on private repositories
Security: Implement local secrets configuration pattern
```

### Commit Message Structure
```
Type: Brief description (50 chars max)

Optional detailed explanation of what and why.
Include breaking changes, migration notes.

🤖 Generated with [Claude Code](https://claude.ai/code)
Co-Authored-By: Claude <noreply@anthropic.com>
```

## Security Practices

### Local Secrets Management
**NEVER commit secrets to the repository**. Follow this pattern:

1. **Development Environment**:
   - Real API keys stored in `appsettings.Local.json` (not tracked by git)
   - Placeholders in `appsettings.Development.json` (tracked by git)
   - Application configured to load local secrets via `Program.cs`

2. **Required Local Secrets File** (`src/Archie.Api/appsettings.Local.json`):
```json
{
    "GitHub": {
        "DefaultAccessToken": "your-actual-github-token"
    },
    "AzureSearch": {
        "AdminKey": "your-actual-azure-search-key",
        "QueryKey": "your-actual-azure-search-query-key"
    },
    "AzureOpenAI": {
        "ApiKey": "your-actual-azure-openai-key"
    }
}
```

3. **Production Deployment**: 
   - Use Azure Key Vault for secret storage
   - Environment variables for CI/CD pipelines
   - Managed identities where possible

4. **Git History Safety**:
   - `.gitignore` excludes `appsettings.Local.json`
   - Git history cleaned of any committed secrets
   - GitHub secret scanning protection enabled

## Testing Framework & Standards

### **Core Testing Principles**
- **Test Behavior, Not Implementation**: Focus on what the code does, not how it does it
- **Domain-Aware Testing**: Tests must respect domain rules and state transitions
- **Deterministic Testing**: No random data or time dependencies
- **Testing Pyramid**: Unit tests (70%), Integration tests (20%), E2E tests (10%)
- **Right Tool for Right Level**: Mocks for unit tests, real services for integration tests

### **Test Architecture by Level**

## **Unit Testing Patterns** 🔬
*Test single units in isolation with mocked dependencies*

#### **✅ Unit Test Structure Pattern**
```csharp
[TestFixture] 
public class UserServiceTests
{
    private Mock<IUserRepository> _mockRepository;
    private Mock<ILogger<UserService>> _mockLogger;
    private UserService _service;
    
    [SetUp]
    public void SetUp()
    {
        // Arrange: Mock external dependencies
        _mockRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _service = new UserService(_mockRepository.Object, _mockLogger.Object);
    }
    
    [Test]
    public void GetUserAsync_ValidId_ReturnsUser()
    {
        // Arrange: Set up test data and mock behavior
        var userId = TestDataFactory.ValidUserId;
        var expectedUser = TestDataFactory.CreateUser();
        _mockRepository.Setup(x => x.GetByIdAsync(userId))
                      .ReturnsAsync(expectedUser);
        
        // Act: Execute the method under test
        var result = await _service.GetUserAsync(userId);
        
        // Assert: Verify behavior and mock interactions
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(userId));
        _mockRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }
}
```

## **Integration Testing Patterns** 🔗
*Test component integration with real dependencies*

#### **✅ Integration Test with Testcontainers**
```csharp
[TestFixture]
public class RepositoryServiceIntegrationTests : IAsyncLifetime
{
    private DockerContainer _azureSearchContainer;
    private DockerContainer _azuriteContainer;
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    public async Task InitializeAsync()
    {
        // Start real Azure service emulators
        _azuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .Build();
        
        await _azuriteContainer.StartAsync();
        
        // Configure application with real connection strings
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(config =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["AzureStorage:ConnectionString"] = _azuriteContainer.GetConnectionString()
                    });
                });
            });
        
        _client = _factory.CreateClient();
    }
    
    [Test]
    public async Task AddRepository_ValidData_CreatesAndIndexes()
    {
        // Arrange: Use real GraphQL mutation
        var mutation = @"
            mutation AddRepository($input: AddRepositoryInput!) {
                addRepository(input: $input) { id name status }
            }";
        
        // Act: Execute against real services
        var response = await _client.PostGraphQLAsync(mutation, new { input = TestData.ValidRepository });
        
        // Assert: Verify integration behavior
        var data = await response.Content.ReadFromJsonAsync<GraphQLResponse>();
        Assert.That(data.Data.addRepository.status, Is.EqualTo("Connected"));
    }
    
    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
        await _factory.DisposeAsync();
    }
}
```

## **End-to-End Testing Patterns** 🎭
*Test complete user workflows with Playwright*

#### **✅ E2E Test Structure**
```csharp
[Test]
public async Task CompleteDocumentationWorkflow_Success()
{
    // Arrange: Real application running
    await page.goto('/repositories/add');
    
    // Act: Complete user workflow
    await page.fill('input[name="name"]', 'test-repo');
    await page.fill('input[name="url"]', 'https://github.com/microsoft/TypeScript');
    await page.click('button[type="submit"]');
    
    // Wait for indexing
    await expect(page.locator('[data-testid="status"]')).toContainText('Indexed');
    
    // Generate documentation
    await page.click('button:has-text("Generate Documentation")');
    await expect(page.locator('[data-testid="doc-status"]')).toContainText('Generated');
    
    // Assert: Verify complete workflow
    await page.click('button:has-text("View Documentation")');
    await expect(page.locator('h1')).toContainText('test-repo');
}
```

### **Azure Services Testing with Testcontainers** 🛠️

#### **✅ Azure Storage (Azurite) Testing**
```csharp
public class AzureStorageIntegrationTests : IAsyncLifetime
{
    private AzuriteContainer _azuriteContainer;
    
    public async Task InitializeAsync()
    {
        _azuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .WithPortBinding(10000, 10000) // Blob service
            .Build();
        
        await _azuriteContainer.StartAsync();
    }
    
    [Test]
    public async Task StoreDocument_ValidFile_UploadsSuccessfully()
    {
        // Arrange: Real Azure Storage client
        var connectionString = _azuriteContainer.GetConnectionString();
        var blobClient = new BlobServiceClient(connectionString);
        
        // Act: Perform real storage operations
        var containerClient = await blobClient.CreateBlobContainerAsync("test-docs");
        var blobClient = containerClient.GetBlobClient("test.txt");
        await blobClient.UploadAsync(BinaryData.FromString("test content"));
        
        // Assert: Verify with real service
        var exists = await blobClient.ExistsAsync();
        Assert.That(exists.Value, Is.True);
    }
}
```

#### **✅ Azure AI Search Testing**
```csharp
public class SearchIntegrationTests : IAsyncLifetime
{
    private DockerContainer _searchContainer;
    private SearchClient _searchClient;
    
    public async Task InitializeAsync()
    {
        // Use Elasticsearch container as Azure Search equivalent for testing
        _searchContainer = new ElasticsearchBuilder()
            .WithImage("docker.elastic.co/elasticsearch/elasticsearch:8.11.0")
            .WithEnvironment("discovery.type", "single-node")
            .WithPortBinding(9200, true)
            .Build();
            
        await _searchContainer.StartAsync();
        
        var endpoint = _searchContainer.GetConnectionString();
        _searchClient = new SearchClient(new Uri(endpoint), "test-index", new AzureKeyCredential("test"));
    }
    
    [Test]
    public async Task IndexDocument_ValidDocument_SearchableAfterIndexing()
    {
        // Arrange: Real search document
        var document = new SearchableDocument 
        { 
            Id = "test-1", 
            Title = "Integration Test Document",
            Content = "This is test content for searching"
        };
        
        // Act: Index with real search service
        await _searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(new[] { document }));
        await Task.Delay(1000); // Wait for indexing
        
        // Assert: Search returns document
        var results = await _searchClient.SearchAsync<SearchableDocument>("integration");
        Assert.That(results.Value.GetResults().Count(), Is.GreaterThan(0));
    }
}
```

### **Domain Entity Testing (Unit Level)** 🏗️
*Test domain logic in isolation*

#### **✅ Domain State Transitions**
```csharp
[Test]
public void UpdateStatus_ValidTransition_UpdatesStatusAndTimestamp()
{
    // Arrange: Create entity in valid initial state
    var documentation = Documentation.Create("Test Doc", repositoryId);
    documentation.UpdateStatus(DocumentationStatus.GeneratingContent); // Follow domain rules
    var beforeTimestamp = documentation.ModifiedAt;
    
    // Act: Perform valid state transition
    documentation.MarkAsCompleted();
    
    // Assert: Verify state and side effects
    Assert.That(documentation.Status, Is.EqualTo(DocumentationStatus.Completed));
    Assert.That(documentation.ModifiedAt, Is.GreaterThan(beforeTimestamp));
}
```

#### **✅ Constructor Validation (Unit)**
```csharp
[Test]
public void Constructor_NullRepository_ThrowsArgumentNullException()
{
    // Act & Assert: Test actual thrown exception type
    var exception = Assert.Throws<ArgumentNullException>(
        () => new UserService(null, validLogger));
    
    Assert.That(exception.ParamName, Is.EqualTo("repository"));
}
```

### **Test Data Management**

#### **✅ Test Data Factory Pattern**
```csharp
public static class TestDataFactory
{
    public static Repository CreateValidRepository(string name = "test-repo")
        => new Repository(name, "https://github.com/test/repo", "owner", "csharp", "description");
    
    public static Mock<ILogger<T>> CreateMockLogger<T>()
        => new Mock<ILogger<T>>();
    
    // Deterministic GUIDs for testing
    public static Guid TestRepositoryId => new Guid("12345678-1234-1234-1234-123456789012");
}
```

### **Critical Test Antipatterns by Level**

## **❌ Unit Test Antipatterns**

#### **NEVER: Test Against Real External Systems**
```csharp
// WRONG: Real GitHub API calls in unit tests
var service = new GitHubService(realOptions);
var result = await service.GetRepositoryAsync("owner", "repo"); // Fails with auth!

// CORRECT: Mock external dependencies in unit tests
_mockGitHubClient.Setup(x => x.Repository.Get("owner", "repo"))
                 .ReturnsAsync(MockRepository);
```

#### **NEVER: Violate Domain State Transitions**
```csharp
// WRONG: Skips required intermediate states
var doc = Documentation.Create("title", repositoryId);
doc.MarkAsCompleted(); // InvalidOperationException!

// CORRECT: Follow domain workflow
var doc = Documentation.Create("title", repositoryId);
doc.UpdateStatus(DocumentationStatus.GeneratingContent);
doc.MarkAsCompleted();
```

#### **NEVER: Compare Random GUIDs**
```csharp
// WRONG: Random GUIDs will never match
var session1 = CodeAnalysisSession.Create(Guid.NewGuid());
var session2 = CodeAnalysisSession.Create(Guid.NewGuid());
Assert.That(session1.Id, Is.EqualTo(session2.Id)); // Always fails!

// CORRECT: Use deterministic test data
var repositoryId = TestDataFactory.TestRepositoryId;
var session = CodeAnalysisSession.Create(repositoryId);
Assert.That(session.RepositoryId, Is.EqualTo(repositoryId));
```

## **❌ Integration Test Antipatterns**

#### **NEVER: Mock the Application Itself**
```csharp
// WRONG: Mocking application services in integration tests
_mockUserService.Setup(x => x.CreateUser(It.IsAny<User>()))
                .ReturnsAsync(mockUser);

// CORRECT: Use real application with containerized dependencies
var response = await _client.PostAsync("/api/users", userJson);
// Tests real UserService with real database via testcontainers
```

#### **NEVER: Use Shared Test State**
```csharp
// WRONG: Shared database state between tests
public static readonly TestDatabase SharedDb = new TestDatabase();

// CORRECT: Isolated containers per test class
public async Task InitializeAsync()
{
    _dbContainer = new PostgreSqlBuilder().Build();
    await _dbContainer.StartAsync(); // Fresh database per test run
}
```

## **❌ E2E Test Antipatterns**

#### **NEVER: Test Implementation Details**
```csharp
// WRONG: Testing internal API calls
await expect(page).toHaveRequestCount('/api/internal/cache', 1);

// CORRECT: Test user-visible behavior
await expect(page.locator('[data-testid="user-profile"]')).toBeVisible();
```

### **Test Coverage Requirements by Level**

#### **Unit Tests (70% of test suite)**
- **Framework**: NUnit 4.1.0 with constraint-based assertions
- **Coverage**: >80% code coverage for business logic
- **Mocking**: Moq framework for external dependencies only
- **Focus**: Domain entities, services, use cases in isolation

#### **Integration Tests (20% of test suite)**
- **Framework**: NUnit + Testcontainers for .NET
- **Coverage**: >30% integration point coverage
- **Dependencies**: Real services via containers (Azurite, Elasticsearch, PostgreSQL)
- **Focus**: Component interaction, database operations, GraphQL API

#### **E2E Tests (10% of test suite)**
- **Framework**: Playwright for complete user workflows
- **Coverage**: Critical user journeys and happy paths
- **Environment**: Real application with real or production-like services
- **Focus**: User-visible functionality and business workflows

### **Test Quality Gates by Level**

#### **Unit Test Quality Standards**
- ✅ All external dependencies mocked (no real API calls)
- ✅ Domain state transitions respect business rules
- ✅ Constructor tests verify actual thrown exception types
- ✅ Deterministic test data (no random GUIDs, dates, or values)
- ✅ Proper mock verification (Times.Once, Times.Never, etc.)
- ✅ Tests run in <1ms each (fast feedback)

#### **Integration Test Quality Standards**
- ✅ Use real dependencies via testcontainers
- ✅ Each test class gets isolated container instances
- ✅ Dynamic port assignment to avoid conflicts
- ✅ Proper container lifecycle management (IAsyncLifetime)
- ✅ Test actual data persistence and retrieval
- ✅ Verify cross-component interactions work correctly

#### **E2E Test Quality Standards**  
- ✅ Test complete user workflows (repository → index → document → view)
- ✅ Use real application URLs and page interactions
- ✅ Wait for actual UI state changes, not arbitrary timeouts
- ✅ Test against user-visible behavior, not implementation
- ✅ Include error scenarios and edge cases
- ✅ Cross-browser compatibility validation

## File Organization Rules

**Organize files in appropriate subdirectories:**
- `/src` - Source code files
- `/tests` - Test files
- `/docs` - Documentation and markdown files
- `/config` - Configuration files
- `/scripts` - Utility scripts
- `/examples` - Example code

## Review Process Guidelines

Before submitting any code changes:
1. **Build & Test**: Run `dotnet build` and `dotnet test` - ALL tests must pass
2. **Environment Validation**: Run `.\scripts\Test-ArchieEnvironment.ps1`
3. **Architecture Compliance**: ✅/❌ Clean Architecture principles followed
4. **Test Quality Validation**: ✅/❌ All new tests follow level-appropriate patterns:
   - ✅ Unit tests: Mock external dependencies, deterministic data
   - ✅ Integration tests: Real services via testcontainers, isolated containers
   - ✅ E2E tests: Complete workflows, user-visible behavior
   - ✅ Domain state transitions respected at all levels
   - ✅ Proper exception type expectations
5. **Testing Coverage**: ✅/❌ Unit tests written for new functionality (>80% coverage)
6. **Documentation**: ✅/❌ Updates maintain accuracy with implementation  
7. **No Hardcoded Secrets**: ✅/❌ All sensitive data uses configuration

### **Test Quality Review Checklist**
Before approving any PR with test changes:

**Unit Test Review:**
- [ ] External dependencies mocked (no real API calls)
- [ ] Domain business rules respected (valid state transitions)
- [ ] Deterministic test data (no random values)
- [ ] Exception tests verify actual thrown types
- [ ] Tests run fast (<1ms each)

**Integration Test Review:**
- [ ] Real services used via testcontainers
- [ ] Containers properly isolated per test class
- [ ] Dynamic port assignment to avoid conflicts
- [ ] Proper container lifecycle management (IAsyncLifetime)
- [ ] Tests verify actual cross-component behavior

**E2E Test Review:**
- [ ] Complete user workflows tested
- [ ] User-visible behavior verified, not implementation details
- [ ] Proper wait conditions for UI state changes
- [ ] No arbitrary timeouts or sleep statements
- [ ] Cross-browser compatibility where required

**All Test Types:**
- [ ] Test names clearly describe scenario and expected outcome
- [ ] No flaky tests that depend on timing or external state
- [ ] Proper cleanup and resource disposal

## AI Development Guidelines

### Documentation Generation (Feature 03)
- **Content Analysis**: Always analyze actual file content, never assume from filenames
- **Project Purpose**: Extract from README analysis, not metadata guessing  
- **Domain Detection**: Use keyword analysis for business domains (Game, Web API, Library, etc.)
- **Component Mapping**: Build relationships between code components for "How It Works" sections

### Prompt Engineering
- Include actual project purpose in AI prompts
- Use code examples from content analysis
- Apply domain-specific instructions (game vs API vs library documentation)

## ⚠️ Do Not Modify

### Critical Implementation Files
- `ContentSummarizationService.cs` - Contains language-specific content analysis patterns
- `AIDocumentationGeneratorService.BuildSectionPrompt()` - Enhanced AI context generation
- `RepositoryAnalysisContext` - Enhanced with content analysis properties
- Domain value objects: `ProjectPurpose`, `ComponentRelationshipMap`, `ContentSummary`

### Configuration Safety
- **NEVER hardcode secrets**: Use `appsettings.Local.json` pattern for development
- **Azure Key Vault**: Production secrets stored securely in Azure Key Vault
- **Environment Variables**: CI/CD pipelines use environment-based configuration
- **Managed Identities**: Prefer Azure managed identities over API keys where possible
- **Git History**: All secrets removed from git history using filter-branch
- **Subscription Details**: Maintain Azure subscription accuracy for resource access

## Important Instructions
- Do what has been asked; nothing more, nothing less
- NEVER create files unless they're absolutely necessary for achieving your goal
- ALWAYS prefer editing an existing file to creating a new one
- NEVER proactively create documentation files (*.md) or README files
- Never save working files, text/mds and tests to the root folder
- Always test with Test-ArchieEnvironment.ps1 before saying a feature is complete
- You always need to use scripts to start and stop application from the scripts folder. You should always use the stop script before the start
- **Follow git commit conventions**: Use proper prefixes (Feature/Task/Bug/Hotfix/Security/etc.)
- **Never commit secrets**: Always use `appsettings.Local.json` pattern for development keys
- **Frontend development**: Run `npm run dev` for Next.js, ensure both API and frontend are running for full functionality