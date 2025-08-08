# Feature 08: BitBucket Integration and Multi-Source Repository Support

## Feature Overview

**Feature ID**: F08  
**Feature Name**: BitBucket Integration and Multi-Source Repository Support  
**Phase**: Phase 4 (Weeks 13-16)  
**Bounded Context**: Repository Management Context  

### Business Value Proposition
Extend repository integration capabilities to support BitBucket in addition to GitHub, providing comprehensive version control system coverage for enterprise customers. This feature enables organizations using diverse VCS platforms to leverage the AI-powered documentation platform across their entire codebase portfolio.

### User Impact
- Organizations can connect repositories from multiple VCS platforms
- Teams using BitBucket gain access to AI-powered documentation features
- Unified interface for managing repositories regardless of source platform
- Enterprise customers can integrate their complete development ecosystem

### Success Criteria
- Successfully connect and analyze BitBucket repositories (Cloud and Server)
- Support for both public and private BitBucket repositories
- Unified repository management experience across GitHub and BitBucket
- Enterprise authentication integration (OAuth, SAML)
- 95% feature parity between GitHub and BitBucket integrations

### Dependencies
- F01: Repository Connection and Management (extends existing functionality)
- F02: Core Infrastructure and DevOps Pipeline (for Azure services)
- F03: File Parsing and Code Structure Indexing (reuses parsing logic)

## Technical Specification

### Domain Model Extensions
```csharp
// Extended Repository model for multi-source support
public class Repository
{
    // ... existing properties ...
    
    public RepositorySource Source { get; private set; }
    public SourceConnectionDetails ConnectionDetails { get; private set; }
    public RepositoryPermissions Permissions { get; private set; }
    
    // New methods for multi-source support
    public void UpdateFromBitBucket(BitBucketRepository bitbucketRepo) { /* ... */ }
    public void UpdateConnectionDetails(SourceConnectionDetails details) { /* ... */ }
}

public class SourceConnectionDetails
{
    public string ApiEndpoint { get; set; }
    public string RepositoryId { get; set; }
    public string ProjectKey { get; set; } // BitBucket specific
    public string WorkspaceId { get; set; } // BitBucket specific
    public AuthenticationDetails Authentication { get; set; }
    public DateTime LastSynchronized { get; set; }
    public SynchronizationStatus Status { get; set; }
}

public class AuthenticationDetails
{
    public AuthenticationType Type { get; set; }
    public string TokenReference { get; set; } // Key Vault reference
    public string Username { get; set; }
    public DateTime TokenExpiresAt { get; set; }
    public List<string> Scopes { get; set; }
    public bool RequiresRefresh { get; set; }
}

public class RepositoryPermissions
{
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanAdmin { get; set; }
    public List<string> RestrictedBranches { get; set; }
    public PermissionSource Source { get; set; }
}

public enum RepositorySource
{
    GitHub,
    BitBucketCloud,
    BitBucketServer,
    GitLab, // Future support
    AzureDevOps // Future support
}

public enum AuthenticationType
{
    PersonalAccessToken,
    OAuth2,
    AppPassword,
    SSHKey,
    SAML
}

public enum SynchronizationStatus
{
    Connected,
    Synchronizing,
    Synchronized,
    Error,
    PermissionDenied,
    TokenExpired
}
```

### API Specification

#### GraphQL Schema Extensions
```graphql
extend type Repository {
  source: RepositorySource!
  connectionDetails: SourceConnectionDetails!
  permissions: RepositoryPermissions!
  
  # Source-specific operations
  synchronizeWithSource: SynchronizationResult!
  validateConnection: ConnectionValidationResult!
}

type SourceConnectionDetails {
  apiEndpoint: String!
  repositoryId: String!
  projectKey: String # BitBucket specific
  workspaceId: String # BitBucket specific
  authentication: AuthenticationDetails!
  lastSynchronized: DateTime!
  status: SynchronizationStatus!
}

type AuthenticationDetails {
  type: AuthenticationType!
  username: String
  tokenExpiresAt: DateTime
  scopes: [String!]!
  requiresRefresh: Boolean!
}

type RepositoryPermissions {
  canRead: Boolean!
  canWrite: Boolean!
  canAdmin: Boolean!
  restrictedBranches: [String!]!
  source: PermissionSource!
}

type SynchronizationResult {
  success: Boolean!
  lastSynchronized: DateTime!
  changesDetected: [RepositoryChange!]!
  errors: [String!]!
}

type ConnectionValidationResult {
  isValid: Boolean!
  permissions: RepositoryPermissions!
  issues: [ValidationIssue!]!
  recommendations: [String!]!
}

type RepositoryChange {
  type: ChangeType!
  description: String!
  affectedFiles: [String!]!
  timestamp: DateTime!
}

enum RepositorySource {
  GITHUB
  BITBUCKET_CLOUD
  BITBUCKET_SERVER
  GITLAB
  AZURE_DEVOPS
}

enum AuthenticationType {
  PERSONAL_ACCESS_TOKEN
  OAUTH2
  APP_PASSWORD
  SSH_KEY
  SAML
}

enum SynchronizationStatus {
  CONNECTED
  SYNCHRONIZING
  SYNCHRONIZED
  ERROR
  PERMISSION_DENIED
  TOKEN_EXPIRED
}

enum ChangeType {
  FILE_ADDED
  FILE_MODIFIED
  FILE_DELETED
  BRANCH_CREATED
  BRANCH_DELETED
  PERMISSION_CHANGED
}

# Extended input types
input AddRepositoryInput {
  url: String!
  source: RepositorySource!
  authentication: AuthenticationInput!
  connectionOptions: ConnectionOptionsInput
}

input AuthenticationInput {
  type: AuthenticationType!
  token: String # Will be stored securely in Key Vault
  username: String
  additionalCredentials: JSON
}

input ConnectionOptionsInput {
  defaultBranch: String
  syncFrequency: Int # minutes
  includePrivateBranches: Boolean
  customApiEndpoint: String # For BitBucket Server
}

# Extended mutations
extend type Mutation {
  addBitBucketRepository(input: AddBitBucketRepositoryInput!): Repository!
  refreshRepositoryConnection(repositoryId: ID!): SynchronizationResult!
  updateRepositoryCredentials(repositoryId: ID!, credentials: AuthenticationInput!): Repository!
  testRepositoryConnection(connectionDetails: TestConnectionInput!): ConnectionValidationResult!
}

input AddBitBucketRepositoryInput {
  workspaceId: String!
  repositorySlug: String!
  authentication: AuthenticationInput!
  serverEndpoint: String # For BitBucket Server
  projectKey: String # For BitBucket Server
}

input TestConnectionInput {
  source: RepositorySource!
  url: String!
  authentication: AuthenticationInput!
  serverEndpoint: String
}

# Extended queries
extend type Query {
  repositoriesBySource(source: RepositorySource!): [Repository!]!
  connectionStatus(repositoryId: ID!): ConnectionValidationResult!
  supportedRepositorySources: [RepositorySourceInfo!]!
}

type RepositorySourceInfo {
  source: RepositorySource!
  name: String!
  description: String!
  supportedAuthTypes: [AuthenticationType!]!
  capabilities: SourceCapabilities!
  isEnabled: Boolean!
}

type SourceCapabilities {
  supportsWebhooks: Boolean!
  supportsPrivateRepositories: Boolean!
  supportsOrganizationAccess: Boolean!
  supportsBranchPermissions: Boolean!
  maxRepositoriesPerUser: Int
}

# New subscriptions for multi-source support
extend type Subscription {
  repositoryConnectionStatusChanged(repositoryId: ID!): ConnectionStatusUpdate!
  sourceSystemStatusChanged: SourceSystemStatusUpdate!
}

type ConnectionStatusUpdate {
  repositoryId: ID!
  oldStatus: SynchronizationStatus!
  newStatus: SynchronizationStatus!
  timestamp: DateTime!
  details: String
}

type SourceSystemStatusUpdate {
  source: RepositorySource!
  isAvailable: Boolean!
  responseTime: Int # milliseconds
  timestamp: DateTime!
}
```

### Integration Points

#### BitBucket API Service
```csharp
public interface IBitBucketService : IRepositorySourceService
{
    Task<BitBucketRepository> GetRepositoryAsync(string workspaceId, string repositorySlug);
    Task<IEnumerable<BitBucketRepository>> GetWorkspaceRepositoriesAsync(string workspaceId);
    Task<IEnumerable<BitBucketBranch>> GetBranchesAsync(string workspaceId, string repositorySlug);
    Task<BitBucketUser> GetCurrentUserAsync();
    Task<bool> ValidatePermissionsAsync(string workspaceId, string repositorySlug);
    Task<BitBucketWebhook> CreateWebhookAsync(string workspaceId, string repositorySlug, string callbackUrl);
}

public class BitBucketCloudService : IBitBucketService
{
    private readonly HttpClient _httpClient;
    private readonly ISecretManager _secretManager;
    private readonly ILogger<BitBucketCloudService> _logger;
    private const string BitBucketApiBase = "https://api.bitbucket.org/2.0";

    public BitBucketCloudService(HttpClient httpClient, ISecretManager secretManager, ILogger<BitBucketCloudService> logger)
    {
        _httpClient = httpClient;
        _secretManager = secretManager;
        _logger = logger;
    }

    public async Task<Repository> CloneRepositoryAsync(string url, string accessToken = null)
    {
        try
        {
            var (workspaceId, repositorySlug) = ParseBitBucketUrl(url);
            
            // Configure authentication
            await ConfigureAuthenticationAsync(accessToken);
            
            // Get repository metadata
            var bitBucketRepo = await GetRepositoryAsync(workspaceId, repositorySlug);
            
            // Create local repository representation
            var repository = new Repository
            {
                Id = Guid.NewGuid(),
                Name = bitBucketRepo.Name,
                Url = bitBucketRepo.Links.Clone.FirstOrDefault(c => c.Name == "https")?.Href ?? url,
                Language = bitBucketRepo.Language ?? "Unknown",
                Description = bitBucketRepo.Description,
                Source = RepositorySource.BitBucketCloud,
                ConnectionDetails = new SourceConnectionDetails
                {
                    ApiEndpoint = BitBucketApiBase,
                    RepositoryId = bitBucketRepo.FullName,
                    WorkspaceId = workspaceId,
                    Authentication = new AuthenticationDetails
                    {
                        Type = AuthenticationType.PersonalAccessToken,
                        TokenReference = await StoreTokenSecurelyAsync(accessToken),
                        Scopes = new List<string> { "repository" }
                    }
                },
                CreatedAt = DateTime.UtcNow,
                Status = RepositoryStatus.Connecting
            };

            // Clone the actual repository
            await CloneRepositoryFilesAsync(repository);
            
            // Validate permissions
            var permissions = await ValidateAndGetPermissionsAsync(workspaceId, repositorySlug);
            repository.Permissions = permissions;
            
            repository.Status = RepositoryStatus.Connected;
            
            return repository;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clone BitBucket repository: {Url}", url);
            throw;
        }
    }

    public async Task<BitBucketRepository> GetRepositoryAsync(string workspaceId, string repositorySlug)
    {
        var response = await _httpClient.GetAsync($"{BitBucketApiBase}/repositories/{workspaceId}/{repositorySlug}");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BitBucketRepository>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public async Task<IEnumerable<BitBucketBranch>> GetBranchesAsync(string workspaceId, string repositorySlug)
    {
        var branches = new List<BitBucketBranch>();
        var nextPageUrl = $"{BitBucketApiBase}/repositories/{workspaceId}/{repositorySlug}/refs/branches";

        while (!string.IsNullOrEmpty(nextPageUrl))
        {
            var response = await _httpClient.GetAsync(nextPageUrl);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var pageResult = JsonSerializer.Deserialize<BitBucketPagedResult<BitBucketBranch>>(content);
            
            branches.AddRange(pageResult.Values);
            nextPageUrl = pageResult.Next;
        }

        return branches;
    }

    private async Task ConfigureAuthenticationAsync(string accessToken)
    {
        if (!string.IsNullOrEmpty(accessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }
        else
        {
            // Try to get stored token
            var storedToken = await _secretManager.GetSecretAsync("bitbucket-default-token");
            if (!string.IsNullOrEmpty(storedToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", storedToken);
            }
        }
    }

    private (string workspaceId, string repositorySlug) ParseBitBucketUrl(string url)
    {
        // Parse URLs like:
        // https://bitbucket.org/workspace/repository
        // git@bitbucket.org:workspace/repository.git
        
        var uri = new Uri(url);
        var pathParts = uri.AbsolutePath.Trim('/').Split('/');
        
        if (pathParts.Length < 2)
        {
            throw new ArgumentException($"Invalid BitBucket URL format: {url}");
        }
        
        var workspaceId = pathParts[0];
        var repositorySlug = pathParts[1].Replace(".git", "");
        
        return (workspaceId, repositorySlug);
    }

    private async Task<RepositoryPermissions> ValidateAndGetPermissionsAsync(string workspaceId, string repositorySlug)
    {
        try
        {
            // Check repository permissions
            var response = await _httpClient.GetAsync($"{BitBucketApiBase}/repositories/{workspaceId}/{repositorySlug}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return new RepositoryPermissions
                {
                    CanRead = false,
                    CanWrite = false,
                    CanAdmin = false,
                    Source = PermissionSource.API
                };
            }
            
            response.EnsureSuccessStatusCode();
            
            // Try to get user permissions on the repository
            var permissionsResponse = await _httpClient.GetAsync(
                $"{BitBucketApiBase}/repositories/{workspaceId}/{repositorySlug}/permissions-config");
            
            var hasWriteAccess = permissionsResponse.IsSuccessStatusCode;
            
            return new RepositoryPermissions
            {
                CanRead = true,
                CanWrite = hasWriteAccess,
                CanAdmin = hasWriteAccess, // Simplification for now
                RestrictedBranches = new List<string>(),
                Source = PermissionSource.API
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not validate permissions for {WorkspaceId}/{RepositorySlug}", workspaceId, repositorySlug);
            
            return new RepositoryPermissions
            {
                CanRead = true, // Assume read access if we got this far
                CanWrite = false,
                CanAdmin = false,
                Source = PermissionSource.Assumed
            };
        }
    }
}

public class BitBucketServerService : IBitBucketService
{
    private readonly HttpClient _httpClient;
    private readonly ISecretManager _secretManager;
    private readonly ILogger<BitBucketServerService> _logger;
    private readonly string _serverBaseUrl;

    public BitBucketServerService(HttpClient httpClient, ISecretManager secretManager, 
        ILogger<BitBucketServerService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _secretManager = secretManager;
        _logger = logger;
        _serverBaseUrl = configuration["BitBucketServer:BaseUrl"];
    }

    public async Task<Repository> CloneRepositoryAsync(string url, string accessToken = null)
    {
        // Implementation similar to BitBucketCloudService but using BitBucket Server APIs
        // Server API endpoints: /rest/api/1.0/projects/{projectKey}/repos/{repositorySlug}
        
        try
        {
            var (projectKey, repositorySlug) = ParseBitBucketServerUrl(url);
            
            await ConfigureAuthenticationAsync(accessToken);
            
            var serverRepo = await GetServerRepositoryAsync(projectKey, repositorySlug);
            
            var repository = new Repository
            {
                Id = Guid.NewGuid(),
                Name = serverRepo.Name,
                Url = serverRepo.Links.Clone.FirstOrDefault(c => c.Name == "http")?.Href ?? url,
                Language = "Unknown", // BitBucket Server doesn't provide language detection
                Description = serverRepo.Description,
                Source = RepositorySource.BitBucketServer,
                ConnectionDetails = new SourceConnectionDetails
                {
                    ApiEndpoint = $"{_serverBaseUrl}/rest/api/1.0",
                    RepositoryId = $"{projectKey}/{repositorySlug}",
                    ProjectKey = projectKey,
                    Authentication = new AuthenticationDetails
                    {
                        Type = AuthenticationType.PersonalAccessToken,
                        TokenReference = await StoreTokenSecurelyAsync(accessToken),
                        Username = serverRepo.Project.Owner?.Name
                    }
                },
                CreatedAt = DateTime.UtcNow,
                Status = RepositoryStatus.Connecting
            };

            await CloneRepositoryFilesAsync(repository);
            
            repository.Status = RepositoryStatus.Connected;
            return repository;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clone BitBucket Server repository: {Url}", url);
            throw;
        }
    }

    private (string projectKey, string repositorySlug) ParseBitBucketServerUrl(string url)
    {
        // Parse URLs like:
        // https://bitbucket.company.com/scm/PROJECT/repository.git
        // https://bitbucket.company.com/projects/PROJECT/repos/repository
        
        var uri = new Uri(url);
        var pathParts = uri.AbsolutePath.Trim('/').Split('/');
        
        string projectKey, repositorySlug;
        
        if (pathParts.Contains("scm"))
        {
            // SCM URL format
            var scmIndex = Array.IndexOf(pathParts, "scm");
            projectKey = pathParts[scmIndex + 1];
            repositorySlug = pathParts[scmIndex + 2].Replace(".git", "");
        }
        else if (pathParts.Contains("projects"))
        {
            // Projects URL format
            var projectsIndex = Array.IndexOf(pathParts, "projects");
            projectKey = pathParts[projectsIndex + 1];
            var reposIndex = Array.IndexOf(pathParts, "repos");
            repositorySlug = pathParts[reposIndex + 1];
        }
        else
        {
            throw new ArgumentException($"Invalid BitBucket Server URL format: {url}");
        }
        
        return (projectKey, repositorySlug);
    }
}
```

#### Multi-Source Repository Manager
```csharp
public interface IMultiSourceRepositoryManager
{
    Task<Repository> AddRepositoryAsync(string url, RepositorySource source, AuthenticationInput auth);
    Task<List<Repository>> GetRepositoriesBySourceAsync(RepositorySource source);
    Task<SynchronizationResult> SynchronizeRepositoryAsync(Guid repositoryId);
    Task<ConnectionValidationResult> ValidateConnectionAsync(Guid repositoryId);
    Task<bool> RefreshAuthenticationAsync(Guid repositoryId);
}

public class MultiSourceRepositoryManager : IMultiSourceRepositoryManager
{
    private readonly Dictionary<RepositorySource, IRepositorySourceService> _sourceServices;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<MultiSourceRepositoryManager> _logger;

    public MultiSourceRepositoryManager(
        IEnumerable<IRepositorySourceService> sourceServices,
        IRepositoryRepository repositoryRepository,
        ILogger<MultiSourceRepositoryManager> logger)
    {
        _sourceServices = sourceServices.ToDictionary(s => s.Source, s => s);
        _repositoryRepository = repositoryRepository;
        _logger = logger;
    }

    public async Task<Repository> AddRepositoryAsync(string url, RepositorySource source, AuthenticationInput auth)
    {
        if (!_sourceServices.TryGetValue(source, out var sourceService))
        {
            throw new NotSupportedException($"Repository source {source} is not supported");
        }

        try
        {
            var repository = await sourceService.CloneRepositoryAsync(url, auth.Token);
            
            // Store repository
            await _repositoryRepository.SaveAsync(repository);
            
            // Publish event
            await PublishRepositoryAddedEventAsync(repository);
            
            return repository;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add repository from {Source}: {Url}", source, url);
            throw;
        }
    }

    public async Task<SynchronizationResult> SynchronizeRepositoryAsync(Guid repositoryId)
    {
        var repository = await _repositoryRepository.GetByIdAsync(repositoryId);
        if (repository == null)
        {
            throw new NotFoundException($"Repository {repositoryId} not found");
        }

        if (!_sourceServices.TryGetValue(repository.Source, out var sourceService))
        {
            throw new NotSupportedException($"Repository source {repository.Source} is not supported");
        }

        try
        {
            repository.ConnectionDetails.Status = SynchronizationStatus.Synchronizing;
            await _repositoryRepository.UpdateAsync(repository);

            var result = await sourceService.SynchronizeAsync(repository);
            
            repository.ConnectionDetails.Status = result.Success 
                ? SynchronizationStatus.Synchronized 
                : SynchronizationStatus.Error;
            repository.ConnectionDetails.LastSynchronized = DateTime.UtcNow;
            
            await _repositoryRepository.UpdateAsync(repository);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to synchronize repository {RepositoryId}", repositoryId);
            
            repository.ConnectionDetails.Status = SynchronizationStatus.Error;
            await _repositoryRepository.UpdateAsync(repository);
            
            return new SynchronizationResult
            {
                Success = false,
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
```

### Security Requirements
- Secure storage of multiple authentication tokens in Azure Key Vault
- Support for enterprise authentication protocols (SAML, OAuth)
- Token refresh mechanisms for long-lived connections
- Audit logging for all multi-source repository operations
- Fine-grained permission validation for different VCS platforms

### Performance Requirements
- Repository connection establishment < 10 seconds for both GitHub and BitBucket
- Synchronization operations complete < 5 minutes for typical repositories
- Support concurrent operations across different VCS platforms
- Authentication token validation < 2 seconds
- Unified query performance regardless of repository source

## Implementation Guidance

### Recommended Development Approach
1. **Source Abstraction**: Implement common interface for all repository sources
2. **BitBucket Integration**: Add BitBucket Cloud and Server support
3. **Authentication Framework**: Build flexible authentication system
4. **Migration Support**: Provide tools for existing repository migration
5. **Testing Framework**: Comprehensive testing across all supported platforms

### Key Architectural Decisions
- Use factory pattern for repository source services
- Implement unified authentication credential management
- Store source-specific metadata in flexible JSON structures
- Use common repository model with source-specific extensions
- Implement comprehensive error handling for different API responses

### Technical Risks and Mitigation
1. **Risk**: API differences between VCS platforms causing inconsistencies
   - **Mitigation**: Comprehensive abstraction layer and thorough testing
   - **Fallback**: Platform-specific handlers for edge cases

2. **Risk**: Authentication token management complexity
   - **Mitigation**: Centralized credential management with automatic refresh
   - **Fallback**: Manual token refresh workflows

3. **Risk**: Performance variations between different APIs
   - **Mitigation**: Platform-specific optimization and caching strategies
   - **Fallback**: Configurable timeout and retry policies

## Testing Strategy

### Unit Testing Requirements (80% coverage minimum)
- **Multi-Source Repository Manager**
  - Repository addition from different sources
  - Authentication handling across platforms
  - Error handling for platform-specific failures
  - Permission validation logic

- **BitBucket Services**
  - API integration accuracy
  - URL parsing for different formats
  - Authentication configuration
  - Repository metadata extraction

- **Authentication Framework**
  - Token storage and retrieval
  - Refresh mechanism functionality
  - Permission validation
  - Security compliance

### Integration Testing Requirements (30% coverage minimum)
- **Cross-Platform Repository Management**
  - Repository operations across GitHub and BitBucket
  - Unified query functionality
  - Performance consistency
  - Error handling scenarios

- **BitBucket API Integration**
  - Cloud and Server API functionality
  - Authentication flows
  - Repository synchronization
  - Webhook management

- **Enterprise Authentication**
  - SAML and OAuth flows
  - Token refresh mechanisms
  - Permission inheritance
  - Audit trail validation

### Test Data Requirements
- Sample repositories from BitBucket Cloud and Server
- Various authentication scenarios
- Performance benchmarking datasets
- Error condition test cases

## Quality Assurance

### Code Review Checkpoints
- [ ] Multi-source abstraction is properly implemented
- [ ] BitBucket API integration follows best practices
- [ ] Authentication handling is secure and comprehensive
- [ ] Error handling covers platform-specific scenarios
- [ ] Performance is consistent across platforms
- [ ] Security measures protect credentials
- [ ] Monitoring covers all supported platforms
- [ ] Documentation includes platform-specific guidance

### Definition of Done Checklist
- [ ] BitBucket Cloud integration works for public and private repositories
- [ ] BitBucket Server integration supports enterprise deployments
- [ ] Authentication works across all supported platforms
- [ ] Performance meets requirements for all repository sources
- [ ] Integration tests pass for all platform combinations
- [ ] Security review completed for multi-platform access
- [ ] Documentation updated for multi-source support
- [ ] Migration tools are available for existing users

### Monitoring and Observability
- **Custom Metrics**
  - Repository connection success rates by source
  - Authentication success/failure rates
  - Synchronization performance by platform
  - API response times for different sources

- **Alerts**
  - Platform-specific API failures
  - Authentication token expiration warnings
  - Synchronization failures by source
  - Performance degradation on specific platforms

- **Dashboards**
  - Multi-source repository health overview
  - Platform-specific performance metrics
  - Authentication status and token health
  - Cross-platform usage analytics

### Documentation Requirements
- Multi-source repository setup guide
- BitBucket integration documentation
- Enterprise authentication configuration guide
- Migration documentation for existing repositories
- Platform-specific troubleshooting guides