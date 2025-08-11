# Feature 07: Rate Limiting and API Optimization

## Feature Overview

**Feature ID**: F07  
**Feature Name**: Rate Limiting and API Optimization  
**Phase**: Phase 1-4 (Critical Infrastructure - Iterative Implementation)  
**Backend + Frontend**: Complete infrastructure with monitoring UI components
**Implementation Strategy**: Early foundation with incremental enhancement across Phases 1-4

### Business Value Proposition
Implement comprehensive rate limiting and API optimization strategies that enable Archie to efficiently process large repositories while staying within service provider limits and controlling costs. This critical infrastructure feature ensures system reliability, cost predictability, and scalable operations as the platform grows through iterative development phases, providing immediate value while supporting future feature expansion.

### User Impact
- **Phase 1**: Users get real-time progress indicators and rate limit status during repository indexing
- **Phase 2**: Users experience reliable processing of large repositories with intelligent queue management
- **Phase 3**: Users see improved performance through optimized API usage and caching strategies
- **Phase 4**: Users benefit from real-time updates and reduced processing times through GitHub App integration
- **Cross-Phase**: Administrative users get comprehensive rate limit monitoring dashboards
- **Long-term**: Predictable costs and scalable operations supporting organizational growth

### Success Criteria
- Successfully process repositories with 50,000+ files without encountering API rate limits
- Achieve >99% API request success rate across all external service integrations
- Maintain repository analysis costs below $500/month for typical organizational usage
- Complete indexing of 10,000-file repositories within 30 minutes (Phase 2 target)
- Support 100+ concurrent users without performance degradation
- Reduce API costs by 60% through optimization strategies (Phase 4 target)

### Dependencies
- **Critical for**: All features requiring external API integration (GitHub, Azure OpenAI, Azure AI Search)
- **Azure Services**: Azure OpenAI Service, Azure AI Search, Azure Key Vault, Azure Redis Cache
- **External APIs**: GitHub API (REST and GraphQL), Azure OpenAI API
- **Infrastructure**: Multi-instance deployments for load distribution

## Technical Specification

### Architecture Overview

#### Multi-Phase Rate Limiting Strategy
The rate limiting implementation follows the architectural plan's phased approach:

**Phase 1 (Weeks 1-4): Basic Rate Limit Infrastructure**
- GitHub API rate limit awareness and monitoring
- Azure OpenAI rate limit handling with exponential backoff
- Multiple Azure OpenAI deployment instances for load distribution
- Basic queuing system for batch processing

**Phase 2 (Weeks 5-8): Intelligent Batch Processing**
- Content-aware request prioritization
- Smart retry mechanisms with jittered exponential backoff
- Embedding result caching to avoid OpenAI API regeneration
- Adaptive batch sizing based on current rate limit status

**Phase 3 (Weeks 9-12): GraphQL Optimization & Caching**
- GitHub GraphQL migration (60-80% reduction in API calls)
- Distributed caching with Redis
- Query optimization and result caching
- Smart query batching for multiple repositories

**Phase 4 (Weeks 13-16): GitHub App + Webhooks**
- Upgrade to GitHub App (15,000 requests/hour vs 5,000)
- Real-time updates via webhooks instead of polling
- Enhanced security with app-level permissions
- Intelligent incremental processing

### Domain Model Extensions

```csharp
// Rate Limiting Infrastructure
public class RateLimitManager
{
    private readonly Dictionary<string, ServiceRateLimit> _serviceLimits = new();
    private readonly IRedisCache _cache;
    private readonly ILogger<RateLimitManager> _logger;

    public async Task<bool> CanMakeRequestAsync(string serviceName, string operation = "default")
    {
        var key = $"rate_limit:{serviceName}:{operation}";
        var limit = _serviceLimits[serviceName];
        
        var currentUsage = await _cache.GetAsync<int>(key);
        return currentUsage < limit.RequestsPerWindow * 0.9; // 90% threshold for safety
    }

    public async Task RecordRequestAsync(string serviceName, string operation = "default")
    {
        var key = $"rate_limit:{serviceName}:{operation}";
        await _cache.IncrementAsync(key);
        await _cache.ExpireAsync(key, TimeSpan.FromHours(1));
    }

    public async Task<RateLimitStatus> GetRateLimitStatusAsync(string serviceName)
    {
        var limit = _serviceLimits[serviceName];
        var usage = await GetCurrentUsageAsync(serviceName);
        
        return new RateLimitStatus
        {
            ServiceName = serviceName,
            RequestsRemaining = Math.Max(0, limit.RequestsPerWindow - usage),
            WindowResetTime = GetNextResetTime(limit.WindowType),
            UsagePercentage = (double)usage / limit.RequestsPerWindow,
            Status = DetermineStatus(usage, limit.RequestsPerWindow)
        };
    }
}

public class ServiceRateLimit
{
    public string ServiceName { get; set; }
    public int RequestsPerWindow { get; set; }
    public RateLimitWindow WindowType { get; set; }
    public TimeSpan BackoffDelay { get; set; }
    public int MaxRetryAttempts { get; set; }
}

public enum RateLimitWindow
{
    PerMinute,
    PerHour,
    PerDay
}

public class RateLimitStatus
{
    public string ServiceName { get; set; }
    public int RequestsRemaining { get; set; }
    public DateTime WindowResetTime { get; set; }
    public double UsagePercentage { get; set; }
    public RateLimitStatusLevel Status { get; set; }
}

public enum RateLimitStatusLevel
{
    Available,    // <70% usage
    Caution,      // 70-85% usage
    Throttled,    // 85-95% usage
    Limited       // >95% usage
}

// GitHub API Optimization
public class GitHubApiOptimizer
{
    private readonly IGitHubClient _restClient;
    private readonly IGraphQLClient _graphQLClient;
    private readonly IRateLimitManager _rateLimitManager;
    private readonly IRedisCache _cache;

    public async Task<RepositoryDataBundle> GetRepositoryDataBundleAsync(string owner, string repository)
    {
        // Phase 3: Single GraphQL query replacing multiple REST calls
        var cacheKey = $"repo_data:{owner}:{repository}";
        var cached = await _cache.GetAsync<RepositoryDataBundle>(cacheKey);
        if (cached != null) return cached;

        var query = @"
            query GetRepositoryData($owner: String!, $name: String!) {
                repository(owner: $owner, name: $name) {
                    id
                    name
                    description
                    url
                    defaultBranchRef {
                        name
                        target {
                            ... on Commit {
                                history(first: 10) {
                                    nodes {
                                        oid
                                        message
                                        author {
                                            name
                                            email
                                            date
                                        }
                                    }
                                }
                            }
                        }
                    }
                    refs(refPrefix: ""refs/heads/"", first: 20) {
                        nodes {
                            name
                            target {
                                ... on Commit {
                                    oid
                                    committedDate
                                }
                            }
                        }
                    }
                    object(expression: ""HEAD:"") {
                        ... on Tree {
                            entries {
                                name
                                type
                                object {
                                    ... on Tree {
                                        entries {
                                            name
                                            type
                                        }
                                    }
                                    ... on Blob {
                                        byteSize
                                    }
                                }
                            }
                        }
                    }
                    languages(first: 10) {
                        nodes {
                            name
                            color
                        }
                        totalSize
                    }
                }
                rateLimit {
                    remaining
                    resetAt
                }
            }";

        await _rateLimitManager.RecordRequestAsync("GitHub", "GraphQL");
        var result = await _graphQLClient.ExecuteAsync(query, new { owner, name = repository });
        
        var bundle = MapToRepositoryDataBundle(result);
        await _cache.SetAsync(cacheKey, bundle, TimeSpan.FromHours(6));
        
        return bundle;
    }
}

public class RepositoryDataBundle
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public string DefaultBranch { get; set; }
    public List<GitHubBranchInfo> Branches { get; set; } = new();
    public List<GitHubCommitInfo> RecentCommits { get; set; } = new();
    public GitHubTree FileTree { get; set; }
    public Dictionary<string, int> Languages { get; set; } = new();
    public RateLimitInfo RateLimit { get; set; }
}

// Azure OpenAI Optimization
public class AzureOpenAILoadBalancer
{
    private readonly List<AzureOpenAIInstance> _instances;
    private readonly IRateLimitManager _rateLimitManager;
    private readonly ILogger<AzureOpenAILoadBalancer> _logger;

    public async Task<string> GetNextAvailableInstanceAsync()
    {
        var availableInstances = new List<(AzureOpenAIInstance instance, RateLimitStatus status)>();
        
        foreach (var instance in _instances)
        {
            var status = await _rateLimitManager.GetRateLimitStatusAsync($"AzureOpenAI_{instance.Name}");
            if (status.Status != RateLimitStatusLevel.Limited)
            {
                availableInstances.Add((instance, status));
            }
        }
        
        if (!availableInstances.Any())
        {
            _logger.LogWarning("All Azure OpenAI instances are at rate limit");
            return _instances.First().Endpoint; // Return first as fallback
        }
        
        // Select instance with lowest usage percentage
        var selected = availableInstances
            .OrderBy(x => x.status.UsagePercentage)
            .First();
        
        return selected.instance.Endpoint;
    }

    public async Task<T> ExecuteWithLoadBalancingAsync<T>(Func<string, Task<T>> operation)
    {
        var maxAttempts = _instances.Count;
        var attempts = 0;
        
        while (attempts < maxAttempts)
        {
            try
            {
                var endpoint = await GetNextAvailableInstanceAsync();
                return await operation(endpoint);
            }
            catch (RateLimitExceededException ex)
            {
                attempts++;
                _logger.LogWarning("Rate limit exceeded on attempt {Attempt}, trying next instance", attempts);
                
                if (attempts < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempts))); // Exponential backoff
                }
                else
                {
                    throw new InvalidOperationException("All Azure OpenAI instances are rate limited", ex);
                }
            }
        }
        
        throw new InvalidOperationException("Failed to execute operation with load balancing");
    }
}

public class AzureOpenAIInstance
{
    public string Name { get; set; }
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
    public int TokensPerMinute { get; set; }
    public bool IsActive { get; set; }
}

// Intelligent Batch Processing (Phase 2)
public class IntelligentBatchProcessor
{
    private readonly IRateLimitManager _rateLimitManager;
    private readonly IRedisCache _cache;
    private readonly ILogger<IntelligentBatchProcessor> _logger;

    public async Task<List<T>> ProcessInBatchesAsync<T, TInput>(
        IEnumerable<TInput> items,
        Func<TInput, Task<T>> processor,
        string serviceName,
        int? customBatchSize = null)
    {
        var itemList = items.ToList();
        var batchSize = customBatchSize ?? await GetOptimalBatchSizeAsync(serviceName);
        var results = new List<T>();
        
        _logger.LogInformation("Processing {ItemCount} items in batches of {BatchSize}", 
            itemList.Count, batchSize);
        
        for (int i = 0; i < itemList.Count; i += batchSize)
        {
            var batch = itemList.Skip(i).Take(batchSize);
            var batchResults = new List<T>();
            
            foreach (var item in batch)
            {
                // Wait for rate limit availability
                await WaitForRateLimitAsync(serviceName);
                
                try
                {
                    var result = await processor(item);
                    batchResults.Add(result);
                    
                    await _rateLimitManager.RecordRequestAsync(serviceName);
                }
                catch (RateLimitExceededException ex)
                {
                    _logger.LogWarning("Rate limit exceeded, implementing backoff");
                    await ImplementIntelligentBackoffAsync(serviceName, ex);
                    
                    // Retry the item
                    var result = await processor(item);
                    batchResults.Add(result);
                }
            }
            
            results.AddRange(batchResults);
            
            // Inter-batch delay to prevent rate limit saturation
            if (i + batchSize < itemList.Count)
            {
                await Task.Delay(await GetInterBatchDelayAsync(serviceName));
            }
        }
        
        return results;
    }

    private async Task<int> GetOptimalBatchSizeAsync(string serviceName)
    {
        var status = await _rateLimitManager.GetRateLimitStatusAsync(serviceName);
        
        return status.Status switch
        {
            RateLimitStatusLevel.Available => 10,
            RateLimitStatusLevel.Caution => 5,
            RateLimitStatusLevel.Throttled => 2,
            RateLimitStatusLevel.Limited => 1,
            _ => 5
        };
    }

    private async Task<TimeSpan> GetInterBatchDelayAsync(string serviceName)
    {
        var status = await _rateLimitManager.GetRateLimitStatusAsync(serviceName);
        
        return status.Status switch
        {
            RateLimitStatusLevel.Available => TimeSpan.FromSeconds(1),
            RateLimitStatusLevel.Caution => TimeSpan.FromSeconds(3),
            RateLimitStatusLevel.Throttled => TimeSpan.FromSeconds(5),
            RateLimitStatusLevel.Limited => TimeSpan.FromSeconds(10),
            _ => TimeSpan.FromSeconds(3)
        };
    }

    private async Task WaitForRateLimitAsync(string serviceName)
    {
        while (!await _rateLimitManager.CanMakeRequestAsync(serviceName))
        {
            var status = await _rateLimitManager.GetRateLimitStatusAsync(serviceName);
            var waitTime = CalculateWaitTime(status);
            
            _logger.LogInformation("Rate limit threshold reached for {Service}, waiting {WaitTime}ms", 
                serviceName, waitTime.TotalMilliseconds);
            
            await Task.Delay(waitTime);
        }
    }

    private async Task ImplementIntelligentBackoffAsync(string serviceName, RateLimitExceededException ex)
    {
        // Jittered exponential backoff
        var baseDelay = TimeSpan.FromSeconds(5);
        var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 2000));
        var delay = baseDelay.Add(jitter);
        
        _logger.LogWarning("Implementing backoff for {Service}: {Delay}ms", serviceName, delay.TotalMilliseconds);
        await Task.Delay(delay);
    }

    private TimeSpan CalculateWaitTime(RateLimitStatus status)
    {
        var timeToReset = status.WindowResetTime - DateTime.UtcNow;
        var safetyMargin = TimeSpan.FromMinutes(2);
        
        return timeToReset.Add(safetyMargin);
    }
}

// Content Caching and Deduplication
public class ContentCacheManager
{
    private readonly IRedisCache _cache;
    private readonly ILogger<ContentCacheManager> _logger;

    public async Task<float[]?> GetCachedEmbeddingAsync(string content)
    {
        var hash = ComputeContentHash(content);
        var cacheKey = $"embedding:{hash}";
        
        return await _cache.GetAsync<float[]>(cacheKey);
    }

    public async Task CacheEmbeddingAsync(string content, float[] embedding)
    {
        var hash = ComputeContentHash(content);
        var cacheKey = $"embedding:{hash}";
        
        // Cache embeddings for 30 days
        await _cache.SetAsync(cacheKey, embedding, TimeSpan.FromDays(30));
    }

    public async Task<string?> GetCachedAnalysisAsync(string repositoryId, string analysisType)
    {
        var cacheKey = $"analysis:{repositoryId}:{analysisType}";
        return await _cache.GetStringAsync(cacheKey);
    }

    public async Task CacheAnalysisAsync(string repositoryId, string analysisType, string result)
    {
        var cacheKey = $"analysis:{repositoryId}:{analysisType}";
        
        // Cache analysis results for 24 hours
        await _cache.SetStringAsync(cacheKey, result, TimeSpan.FromHours(24));
    }

    private string ComputeContentHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}

// Priority-Based Processing Queue
public class ProcessingQueue
{
    private readonly Dictionary<ProcessingPriority, Queue<ProcessingTask>> _queues;
    private readonly SemaphoreSlim _processingLock;
    private readonly IRateLimitManager _rateLimitManager;

    public async Task EnqueueAsync(ProcessingTask task)
    {
        _queues[task.Priority].Enqueue(task);
        _ = Task.Run(async () => await ProcessNextTaskAsync());
    }

    private async Task ProcessNextTaskAsync()
    {
        await _processingLock.WaitAsync();
        
        try
        {
            var task = GetNextHighestPriorityTask();
            if (task != null)
            {
                await ProcessTaskWithRateLimitingAsync(task);
            }
        }
        finally
        {
            _processingLock.Release();
        }
    }

    private ProcessingTask? GetNextHighestPriorityTask()
    {
        foreach (var priority in Enum.GetValues<ProcessingPriority>().OrderByDescending(p => p))
        {
            if (_queues[priority].TryDequeue(out var task))
            {
                return task;
            }
        }
        return null;
    }

    private async Task ProcessTaskWithRateLimitingAsync(ProcessingTask task)
    {
        // Wait for rate limit availability before processing
        while (!await _rateLimitManager.CanMakeRequestAsync(task.ServiceName))
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        
        try
        {
            await task.ExecuteAsync();
            await _rateLimitManager.RecordRequestAsync(task.ServiceName);
        }
        catch (RateLimitExceededException)
        {
            // Re-queue with lower priority and delay
            task.Priority = ProcessingPriority.Low;
            await Task.Delay(TimeSpan.FromMinutes(1));
            await EnqueueAsync(task);
        }
    }
}

public class ProcessingTask
{
    public string Id { get; set; }
    public ProcessingPriority Priority { get; set; }
    public string ServiceName { get; set; }
    public Func<Task> ExecuteAsync { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ProcessingPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}
```

### API Specification Extensions

#### GraphQL Schema Changes
```graphql
# Rate limiting and performance monitoring types
type RateLimitStatus {
  serviceName: String!
  requestsRemaining: Int!
  windowResetTime: DateTime!
  usagePercentage: Float!
  status: RateLimitStatusLevel!
}

enum RateLimitStatusLevel {
  AVAILABLE
  CAUTION
  THROTTLED
  LIMITED
}

type ServicePerformanceMetrics {
  serviceName: String!
  averageResponseTime: Float!
  successRate: Float!
  requestCount: Int!
  errorCount: Int!
  rateLimitHits: Int!
  lastUpdated: DateTime!
}

type ProcessingQueueStatus {
  queueName: String!
  pendingTasks: Int!
  processingTasks: Int!
  completedTasks: Int!
  failedTasks: Int!
  averageProcessingTime: Float!
  estimatedWaitTime: Float
}

type CacheStatistics {
  cacheType: String!
  hitRate: Float!
  missRate: Float!
  totalRequests: Int!
  evictionCount: Int!
  memoryUsage: Float!
  itemCount: Int!
}

type ApiOptimizationReport {
  reportDate: DateTime!
  totalRequests: Int!
  costSavings: Float!
  performanceGains: Float!
  cacheEffectiveness: Float!
  recommendations: [OptimizationRecommendation!]!
}

type OptimizationRecommendation {
  type: RecommendationType!
  priority: Priority!
  description: String!
  estimatedImpact: String!
  implementationEffort: ImplementationEffort!
}

enum RecommendationType {
  CACHING_IMPROVEMENT
  BATCH_SIZE_OPTIMIZATION
  API_MIGRATION
  RATE_LIMIT_ADJUSTMENT
  COST_REDUCTION
}

enum Priority {
  LOW
  MEDIUM
  HIGH
  CRITICAL
}

enum ImplementationEffort {
  MINIMAL
  MODERATE
  SIGNIFICANT
  MAJOR
}

# Extended queries for monitoring and optimization
extend type Query {
  # Rate limiting queries
  rateLimitStatus(serviceName: String!): RateLimitStatus!
  allRateLimitStatuses: [RateLimitStatus!]!
  
  # Performance monitoring
  servicePerformanceMetrics(serviceName: String!, timeRange: TimeRange!): ServicePerformanceMetrics!
  processingQueueStatus(queueName: String): ProcessingQueueStatus!
  allProcessingQueues: [ProcessingQueueStatus!]!
  
  # Cache monitoring
  cacheStatistics(cacheType: String): CacheStatistics!
  allCacheStatistics: [CacheStatistics!]!
  
  # Optimization reporting
  apiOptimizationReport(timeRange: TimeRange!): ApiOptimizationReport!
  costAnalysis(timeRange: TimeRange!): CostAnalysis!
}

input TimeRange {
  startDate: DateTime!
  endDate: DateTime!
}

type CostAnalysis {
  timeRange: TimeRange!
  totalCost: Float!
  costByService: [ServiceCost!]!
  projectedMonthlyCost: Float!
  costTrends: [CostTrend!]!
  savingsOpportunities: [SavingsOpportunity!]!
}

type ServiceCost {
  serviceName: String!
  cost: Float!
  requestCount: Int!
  costPerRequest: Float!
}

type CostTrend {
  date: DateTime!
  cost: Float!
  requestCount: Int!
}

type SavingsOpportunity {
  type: String!
  description: String!
  estimatedSavings: Float!
  implementationComplexity: ImplementationEffort!
}

# Extended mutations for optimization management
extend type Mutation {
  # Rate limiting management
  updateRateLimitThreshold(serviceName: String!, threshold: Float!): Boolean!
  resetRateLimitCounter(serviceName: String!): Boolean!
  
  # Processing queue management
  prioritizeTask(taskId: String!, priority: ProcessingPriority!): Boolean!
  clearProcessingQueue(queueName: String!): Int!
  retryFailedTasks(queueName: String!): Int!
  
  # Cache management
  clearCache(cacheType: String!): Boolean!
  preloadCache(cacheType: String!, items: [String!]!): Boolean!
  
  # Optimization controls
  enableOptimization(optimizationType: String!): Boolean!
  disableOptimization(optimizationType: String!): Boolean!
  updateBatchSize(serviceName: String!, batchSize: Int!): Boolean!
}

enum ProcessingPriority {
  LOW
  NORMAL
  HIGH
  CRITICAL
}

# Real-time subscriptions for monitoring
extend type Subscription {
  rateLimitUpdates(serviceName: String): RateLimitStatus!
  processingQueueUpdates(queueName: String): ProcessingQueueStatus!
  servicePerformanceUpdates(serviceName: String): ServicePerformanceMetrics!
  costAlerts(threshold: Float!): CostAlert!
}

type CostAlert {
  serviceName: String!
  currentCost: Float!
  threshold: Float!
  projectedMonthlyCost: Float!
  alertLevel: AlertLevel!
  timestamp: DateTime!
}

enum AlertLevel {
  INFO
  WARNING
  CRITICAL
}
```

### Configuration Extensions

#### Rate Limiting Configuration
```csharp
public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    
    public Dictionary<string, ServiceRateLimitConfig> Services { get; set; } = new()
    {
        {
            "GitHub",
            new ServiceRateLimitConfig
            {
                RequestsPerHour = 5000, // Personal token limit
                BackoffDelaySeconds = 60,
                MaxRetryAttempts = 3,
                SafetyThresholdPercentage = 90
            }
        },
        {
            "GitHubApp",
            new ServiceRateLimitConfig
            {
                RequestsPerHour = 15000, // GitHub App limit
                BackoffDelaySeconds = 30,
                MaxRetryAttempts = 5,
                SafetyThresholdPercentage = 85
            }
        },
        {
            "AzureOpenAI",
            new ServiceRateLimitConfig
            {
                RequestsPerMinute = 240, // Variable based on deployment
                BackoffDelaySeconds = 10,
                MaxRetryAttempts = 5,
                SafetyThresholdPercentage = 80
            }
        }
    };
    
    public CachingOptions Caching { get; set; } = new();
    public BatchProcessingOptions BatchProcessing { get; set; } = new();
    public LoadBalancingOptions LoadBalancing { get; set; } = new();
}

public class ServiceRateLimitConfig
{
    public int RequestsPerHour { get; set; }
    public int RequestsPerMinute { get; set; }
    public int BackoffDelaySeconds { get; set; }
    public int MaxRetryAttempts { get; set; }
    public double SafetyThresholdPercentage { get; set; }
}

public class CachingOptions
{
    [Required]
    public string RedisConnectionString { get; set; } = string.Empty; // From Azure Key Vault
    
    [Range(1, 365)]
    public int EmbeddingCacheDays { get; set; } = 30;
    
    [Range(1, 72)]
    public int AnalysisResultCacheHours { get; set; } = 24;
    
    [Range(1, 24)]
    public int RepositoryDataCacheHours { get; set; } = 6;
    
    public bool EnableDistributedCaching { get; set; } = true;
    public bool EnableCompressionForLargeObjects { get; set; } = true;
    
    [Range(1024, 10485760)] // 1KB to 10MB
    public int MaxCacheObjectSize { get; set; } = 1048576; // 1MB
}

public class BatchProcessingOptions
{
    [Range(1, 100)]
    public int DefaultBatchSize { get; set; } = 10;
    
    [Range(1, 50)]
    public int MaxConcurrentBatches { get; set; } = 5;
    
    [Range(500, 30000)]
    public int InterBatchDelayMs { get; set; } = 2000;
    
    public Dictionary<string, int> ServiceBatchSizes { get; set; } = new()
    {
        { "GitHub", 20 },
        { "AzureOpenAI", 8 },
        { "AzureSearch", 100 }
    };
    
    public bool EnableAdaptiveBatching { get; set; } = true;
    public bool EnablePriorityQueuing { get; set; } = true;
}

public class LoadBalancingOptions
{
    public List<AzureOpenAIInstanceConfig> AzureOpenAIInstances { get; set; } = new();
    
    public LoadBalancingStrategy Strategy { get; set; } = LoadBalancingStrategy.LeastUsage;
    
    [Range(1, 60)]
    public int HealthCheckIntervalMinutes { get; set; } = 5;
    
    public bool EnableAutoFailover { get; set; } = true;
    public bool EnableInstanceRecovery { get; set; } = true;
}

public class AzureOpenAIInstanceConfig
{
    public string Name { get; set; }
    public string Endpoint { get; set; }
    public string ApiKeySecretName { get; set; } // Azure Key Vault secret name
    public int TokensPerMinute { get; set; }
    public bool IsActive { get; set; } = true;
    public string Region { get; set; } = "australiaeast";
}

public enum LoadBalancingStrategy
{
    RoundRobin,
    LeastUsage,
    Random,
    WeightedRoundRobin
}
```

### Performance Requirements

#### Rate Limiting Targets by Phase

**Phase 1 Targets:**
- Complete indexing of 1,000-file repository without API failures
- Handle GitHub and Azure OpenAI rate limits gracefully with automatic retry
- Achieve >95% API request success rate
- Process up to 10 concurrent repository analysis operations

**Phase 2 Targets:**
- Successfully process repositories with 10,000+ files within 30 minutes
- Achieve 80% reduction in redundant API calls through intelligent caching
- Support 25+ concurrent users without performance degradation
- Maintain API costs below $200/month for typical usage

**Phase 3 Targets:**
- Achieve 75% reduction in GitHub API calls through GraphQL optimization
- Support 50+ concurrent users with <5% performance degradation
- Cache hit rate >90% for repository metadata and embeddings
- Complete 10,000-file repository analysis within 20 minutes

**Phase 4 Targets:**
- Support 100+ concurrent users without significant performance degradation
- Achieve 60% overall reduction in API usage costs
- Process 50,000+ file repositories within 45 minutes
- Real-time updates within 30 seconds of repository changes

### Implementation Roadmap

#### Phase 1: Basic Rate Limit Infrastructure (Weeks 1-4)
1. **Rate Limit Management**
   - Implement RateLimitManager with Redis-based tracking
   - Create rate limit monitoring and alerting
   - Add exponential backoff with jitter for failed requests
   - Set up multiple Azure OpenAI deployment instances

2. **Basic Optimization**
   - Implement request queuing system with priority levels
   - Add basic caching for frequently requested data
   - Create circuit breaker patterns for external service protection
   - Implement health checks and failover mechanisms

#### Phase 2: Intelligent Batch Processing (Weeks 5-8)
1. **Advanced Batch Processing**
   - Implement IntelligentBatchProcessor with adaptive sizing
   - Create content-aware prioritization algorithms
   - Add embedding caching with content deduplication
   - Build comprehensive retry mechanisms with intelligent backoff

2. **Performance Monitoring**
   - Create detailed performance metrics collection
   - Implement cost tracking and budget alerting
   - Add optimization recommendation engine
   - Build performance dashboards and reporting

#### Phase 3: GraphQL Optimization & Caching (Weeks 9-12)
1. **GitHub GraphQL Migration**
   - Implement GitHubApiOptimizer with GraphQL client
   - Migrate high-frequency operations to GraphQL
   - Create query optimization and batching strategies
   - Add comprehensive result caching with Redis

2. **Distributed Caching**
   - Implement ContentCacheManager with Redis clustering
   - Create cache warming and preloading strategies
   - Add cache invalidation and consistency mechanisms
   - Build cache analytics and optimization tools

#### Phase 4: GitHub App + Webhooks (Weeks 13-16)
1. **GitHub App Integration**
   - Upgrade from personal access tokens to GitHub App
   - Implement app-level authentication and permissions
   - Create webhook endpoints for real-time updates
   - Add incremental processing based on webhook events

2. **Advanced Optimization**
   - Implement predictive rate limit management
   - Create ML-based optimization recommendations
   - Add automated cost optimization strategies
   - Build comprehensive performance analytics

### Technical Risks and Mitigation Strategies

#### Risk 1: GitHub API Rate Limit Complexity
**Risk**: Different rate limits for different GitHub API endpoints and authentication methods
**Impact**: High - Critical feature functionality depends on GitHub API access
**Mitigation**:
- Implement per-endpoint rate limit tracking and management
- Create fallback mechanisms with different authentication methods
- Use GraphQL to reduce API call volume by 60-80%
- Implement intelligent request prioritization and queuing
- **Fallback**: Manual rate limit management with user-configurable delays

#### Risk 2: Azure OpenAI Cost Escalation
**Risk**: Embedding generation costs may spiral out of control for large repositories
**Impact**: High - Budget overruns and potential service suspension
**Mitigation**:
- Implement aggressive caching with 30-day retention for embeddings
- Use content deduplication to avoid regenerating identical embeddings
- Create cost monitoring with automatic alerts and spending limits
- Implement tiered processing with cost-aware optimization
- **Fallback**: Repository size limits with manual approval for large repositories

#### Risk 3: Cache Consistency and Performance
**Risk**: Distributed caching may introduce consistency issues or performance bottlenecks
**Impact**: Medium - Incorrect data or degraded performance
**Mitigation**:
- Implement cache invalidation strategies with event-driven updates
- Use cache versioning and consistency checks
- Monitor cache hit rates and performance metrics
- Create cache warming strategies for critical data
- **Fallback**: Reduce cache TTL and increase cache redundancy

#### Risk 4: Load Balancing Complexity
**Risk**: Managing multiple Azure OpenAI instances may introduce complexity and failure points
**Impact**: Medium - Potential service unavailability or suboptimal performance
**Mitigation**:
- Implement health checks and automatic failover mechanisms
- Create circuit breakers for failing instances
- Monitor instance performance and automatically route traffic
- Use configuration-driven instance management
- **Fallback**: Single instance operation with manual failover

#### Risk 5: Performance Monitoring Overhead
**Risk**: Comprehensive monitoring may introduce significant performance overhead
**Impact**: Low - Monitoring systems may impact primary functionality
**Mitigation**:
- Implement sampling-based metrics collection
- Use asynchronous logging and metrics processing
- Create configurable monitoring levels based on environment
- Optimize metric storage and aggregation strategies
- **Fallback**: Reduced monitoring granularity with essential metrics only

### Security & Compliance Requirements

#### API Security and Access Control
- **API Key Management**: Secure storage of all service API keys in Azure Key Vault
- **Rate Limit Bypass Prevention**: Implement controls to prevent rate limit bypass attempts
- **Access Logging**: Log all external API requests for audit and analysis
- **Key Rotation**: Automated rotation of API keys and access tokens
- **Least Privilege**: Use minimal required permissions for all external service integrations

#### Australian Data Residency Implementation
- **Azure Region**: All caching and processing resources in Australia East
- **API Processing**: Route all external API calls through Australian infrastructure
- **Data Storage**: Cache data and analytics stored in Australian regions
- **Compliance Monitoring**: Track and report on data residency compliance
- **Cross-Border Restrictions**: Prevent data transit outside Australian boundaries

## Testing Strategy

### Unit Testing Requirements (80% coverage minimum)

#### Rate Limiting Tests
```csharp
[TestFixture]
public class RateLimitManagerTests
{
    [Test]
    public async Task CanMakeRequestAsync_UnderThreshold_ReturnsTrue()
    {
        // Test rate limit availability detection
    }
    
    [Test]
    public async Task RecordRequestAsync_ValidRequest_IncrementsCounter()
    {
        // Test request tracking and counting
    }
    
    [Test]
    public async Task GetRateLimitStatusAsync_NearLimit_ReturnsThrottledStatus()
    {
        // Test status level determination
    }
}
```

#### Batch Processing Tests
```csharp
[TestFixture]
public class IntelligentBatchProcessorTests
{
    [Test]
    public async Task ProcessInBatchesAsync_LargeDataset_ProcessesInOptimalBatches()
    {
        // Test adaptive batch size calculation
    }
    
    [Test]
    public async Task ProcessInBatchesAsync_RateLimitHit_ImplementsBackoff()
    {
        // Test rate limit handling and backoff
    }
    
    [Test]
    public async Task GetOptimalBatchSizeAsync_HighUsage_ReturnsReducedSize()
    {
        // Test dynamic batch size optimization
    }
}
```

#### Caching Tests
```csharp
[TestFixture]
public class ContentCacheManagerTests
{
    [Test]
    public async Task GetCachedEmbeddingAsync_CachedContent_ReturnsEmbedding()
    {
        // Test embedding cache retrieval
    }
    
    [Test]
    public async Task CacheEmbeddingAsync_ValidEmbedding_StoresWithCorrectTTL()
    {
        // Test embedding cache storage
    }
    
    [Test]
    public async Task ComputeContentHash_SameContent_ReturnsSameHash()
    {
        // Test content deduplication hashing
    }
}
```

### Integration Testing Requirements (40% coverage minimum)

#### End-to-End Rate Limiting Tests
- **Large Repository Processing**: Test complete workflow with 10,000+ files
- **Concurrent User Simulation**: Multiple users triggering analysis simultaneously
- **API Limit Scenarios**: Simulated rate limit conditions and recovery
- **Cost Tracking**: Validate cost calculations and budget alerting
- **Performance Under Load**: System behavior during peak usage periods

#### External Service Integration Tests
- **GitHub API Integration**: Rate limiting with REST and GraphQL APIs
- **Azure OpenAI Integration**: Load balancing across multiple instances
- **Azure AI Search Integration**: Batch indexing with rate limit awareness
- **Redis Caching Integration**: Distributed cache performance and consistency

#### Optimization Feature Tests
- **Cache Effectiveness**: Hit rates and performance improvements
- **GraphQL Migration**: API call reduction and response time improvements
- **Batch Processing**: Optimal sizing and throughput optimization
- **Load Balancing**: Automatic failover and recovery testing

### Performance Testing Requirements

#### Rate Limiting Performance Benchmarks
- **API Success Rate**: >99% success rate under normal load
- **Response Time**: <100ms overhead for rate limiting checks
- **Throughput**: Support 10,000+ API requests per hour
- **Recovery Time**: <60 seconds recovery from rate limit conditions

#### Optimization Effectiveness Benchmarks
- **Cache Hit Rate**: >90% for frequently accessed data
- **API Call Reduction**: 60% reduction in total API calls (Phase 3)
- **Cost Reduction**: 50% reduction in API costs (Phase 4)
- **Processing Speed**: 10,000-file repository analysis within 20 minutes

### Test Data Requirements

#### Rate Limiting Test Scenarios
- **Repository Sizes**: Small (100 files), Medium (1,000 files), Large (10,000 files), Enterprise (50,000 files)
- **Concurrent Users**: 1, 10, 50, 100 simultaneous users
- **API Load Patterns**: Burst loads, sustained high loads, mixed workload patterns
- **Error Scenarios**: Network failures, service outages, timeout conditions

#### Optimization Test Cases
- **Cache Scenarios**: Cold cache, warm cache, cache invalidation, cache overflow
- **Batch Processing**: Various data sizes and types, mixed priority workloads
- **Load Balancing**: Instance failures, performance differences, network partitions
- **Cost Analysis**: Different usage patterns and repository characteristics

## Quality Assurance

### Code Review Checkpoints
- [ ] Rate limiting implementation handles all external service constraints properly
- [ ] Batch processing algorithms optimize for performance and cost efficiency
- [ ] Caching strategies provide appropriate TTL and invalidation mechanisms
- [ ] Load balancing implements proper health checks and failover logic
- [ ] Cost tracking and alerting provide accurate budget monitoring
- [ ] Performance monitoring captures all relevant metrics and trends
- [ ] Error handling covers all rate limiting and optimization failure scenarios
- [ ] Security controls protect API keys and prevent unauthorized access
- [ ] Australian data residency requirements are properly enforced
- [ ] Configuration allows for operational tuning and optimization

### Definition of Done Checklist
- [ ] All unit tests pass with >80% coverage
- [ ] Integration tests pass with >40% coverage
- [ ] Rate limiting prevents API failures for large repository processing
- [ ] Intelligent batch processing optimizes performance and cost
- [ ] Caching reduces redundant API calls by target percentages
- [ ] Load balancing distributes load across multiple service instances
- [ ] Cost tracking and alerting work correctly for budget management
- [ ] Performance monitoring provides comprehensive optimization insights
- [ ] GraphQL migration reduces GitHub API calls significantly (Phase 3)
- [ ] GitHub App integration increases rate limits and enables webhooks (Phase 4)
- [ ] Security review completed and approved
- [ ] Australian data residency compliance verified
- [ ] Documentation and operational procedures complete

### Monitoring and Observability

#### Custom Metrics
- **Rate Limiting Performance**:
  - API requests per service per hour/minute
  - Rate limit threshold utilization percentages
  - Rate limit violations and recovery times
  - Request success rates by service and endpoint

- **Optimization Effectiveness**:
  - Cache hit rates by cache type and content category
  - API call reduction percentages compared to baseline
  - Cost per repository analysis and trending
  - Batch processing efficiency and throughput metrics

- **System Performance**:
  - Repository analysis completion times by size
  - Concurrent user capacity and performance degradation
  - Load balancing distribution and instance health
  - Error rates and recovery times for optimization features

#### Alerts Configuration
- **Critical Alerts**:
  - Any service API success rate <95%
  - Rate limit utilization >90% for any service
  - Repository analysis failure rate >10%
  - Monthly cost exceeding budget threshold by >20%

- **Warning Alerts**:
  - Cache hit rate <80% for any cache category
  - Batch processing delays >50% above optimal
  - Load balancer instance health degradation
  - API cost trending toward budget limits

#### Dashboards
- **Rate Limiting Dashboard**:
  - Real-time rate limit status for all services
  - Historical rate limit utilization and patterns
  - Request success rates and error distributions
  - Cost per API call and service usage trends

- **Optimization Performance Dashboard**:
  - Cache performance and hit rate analytics
  - API call reduction achievements and trends
  - Batch processing efficiency and throughput
  - Load balancing distribution and instance health

- **Cost Management Dashboard**:
  - Real-time and projected API costs by service
  - Cost per repository analysis and user activity
  - Budget utilization and savings achieved through optimization
  - Cost optimization recommendations and impact analysis

### Documentation Requirements
- **Operations Manual**: Rate limiting configuration and troubleshooting procedures
- **Optimization Guide**: Best practices for API usage and cost management
- **Performance Tuning**: Configuration options for different deployment scenarios
- **Cost Management**: Budget planning and cost optimization strategies
- **Architecture Decisions**: Key technical decisions and optimization trade-offs

---

## Conclusion

This feature provides the critical infrastructure that enables Archie to scale from a prototype to an enterprise-grade platform. By implementing comprehensive rate limiting, intelligent optimization, and cost management strategies, the system can reliably process large repositories while maintaining predictable costs and performance.

The phased implementation approach ensures that basic functionality is protected immediately while advanced optimizations are built incrementally. This foundation enables all other features to operate efficiently and cost-effectively, providing the scalability and reliability required for organizational deployment.