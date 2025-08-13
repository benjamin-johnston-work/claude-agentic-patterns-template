using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Archie.Api.GraphQL.Resolvers;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.AzureSearch.Models;

namespace Archie.Api.GraphQL.Resolvers;

/// <summary>
/// GraphQL mutation resolver for search operations
/// </summary>
[ExtendObjectType(typeof(Mutation))]
public class SearchMutationResolver
{
    private readonly IRepositoryIndexingService _indexingService;
    private readonly IAzureSearchService _azureSearchService;
    private readonly ILogger<SearchMutationResolver> _logger;

    public SearchMutationResolver(
        IRepositoryIndexingService indexingService,
        IAzureSearchService azureSearchService,
        ILogger<SearchMutationResolver> logger)
    {
        _indexingService = indexingService;
        _azureSearchService = azureSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Trigger repository indexing or re-indexing
    /// </summary>
    /// <param name="repositoryId">Repository ID to index</param>
    /// <param name="force">Whether to force full re-indexing</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Indexing status</returns>
    [GraphQLDescription("Trigger repository indexing or re-indexing")]
    public async Task<IndexStatus> IndexRepository(
        Guid repositoryId,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogError("DEBUG: IndexRepository called with repositoryId: {RepositoryId} (Type: {Type}, IsEmpty: {IsEmpty})", 
                repositoryId, repositoryId.GetType().Name, repositoryId == Guid.Empty);
            _logger.LogInformation("Starting repository indexing for {RepositoryId} (force: {Force})", 
                repositoryId, force);

            // Create a separate cancellation token for the background operation
            // This prevents GraphQL timeout from cancelling the indexing operation
            using var backgroundCts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
            var backgroundToken = backgroundCts.Token;

            // Start indexing in the background - don't await here
            var indexingTask = _indexingService.IndexRepositoryAsync(repositoryId, force, backgroundToken);

            // Return initial status immediately - don't wait for completion
            var initialStatus = new IndexStatus
            {
                Status = IndexingStatus.IN_PROGRESS,
                DocumentsIndexed = 0,
                TotalDocuments = 0,
                EstimatedCompletion = DateTime.UtcNow.AddMinutes(5) // Initial estimate
            };

            _logger.LogInformation("Repository indexing initiated for {RepositoryId}. Initial status: {Status}", 
                repositoryId, initialStatus.Status);

            // Fire and forget - let indexing continue in background with comprehensive error logging
            Console.WriteLine($"[CONSOLE TEST] BACKGROUND INDEXING: About to start Task.Run for repository {repositoryId}");
            _logger.LogInformation("BACKGROUND INDEXING: About to start Task.Run for repository {RepositoryId}", repositoryId);
            
            // Also write to a debug file to verify execution
            try
            {
                var debugLogPath = @"C:\Dev\Archie\background-task-debug.log";
                var debugMessage = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] About to start Task.Run for repository {repositoryId}\n";
                File.AppendAllText(debugLogPath, debugMessage);
            }
            catch { /* Ignore file write errors */ }
            
            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine($"[CONSOLE TEST] BACKGROUND INDEXING: Inside Task.Run for repository {repositoryId}");
                    _logger.LogInformation("BACKGROUND INDEXING: Inside Task.Run for repository {RepositoryId}", repositoryId);
                    
                    // Debug file write inside Task.Run
                    try
                    {
                        var debugLogPath = @"C:\Dev\Archie\background-task-debug.log";
                        var debugMessage = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Inside Task.Run for repository {repositoryId}\n";
                        File.AppendAllText(debugLogPath, debugMessage);
                    }
                    catch { /* Ignore file write errors */ }
                    
                    // Await the indexing task
                    var finalStatus = await indexingTask;
                    
                    Console.WriteLine($"[CONSOLE TEST] BACKGROUND INDEXING: Completed for {repositoryId}. Status: {finalStatus.Status}");
                    _logger.LogInformation("BACKGROUND INDEXING: Completed for {RepositoryId}. Final status: {Status}, Documents: {DocumentsIndexed}/{TotalDocuments}", 
                        repositoryId, finalStatus.Status, finalStatus.DocumentsIndexed, finalStatus.TotalDocuments);
                        
                    if (finalStatus.Status == IndexingStatus.ERROR)
                    {
                        _logger.LogError("BACKGROUND INDEXING: Error occurred - {ErrorMessage}", finalStatus.ErrorMessage);
                    }
                }
                catch (OperationCanceledException cancelEx)
                {
                    Console.WriteLine($"[CONSOLE TEST] BACKGROUND INDEXING: Cancelled for {repositoryId} - {cancelEx.Message}");
                    _logger.LogWarning("BACKGROUND INDEXING: Cancelled/Timeout for {RepositoryId} - {Message}", repositoryId, cancelEx.Message);
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"[CONSOLE TEST] BACKGROUND INDEXING: HTTP error for {repositoryId}");
                    _logger.LogError(httpEx, "BACKGROUND INDEXING: HTTP error for {RepositoryId} - likely GitHub API or Azure OpenAI issue", repositoryId);
                }
                catch (UnauthorizedAccessException authEx)
                {
                    Console.WriteLine($"[CONSOLE TEST] BACKGROUND INDEXING: Auth error for {repositoryId}");
                    _logger.LogError(authEx, "BACKGROUND INDEXING: Authentication error for {RepositoryId} - check API keys", repositoryId);
                }
                catch (Exception backgroundEx)
                {
                    Console.WriteLine($"[CONSOLE TEST] BACKGROUND INDEXING: Exception for {repositoryId} - {backgroundEx.GetType().Name}: {backgroundEx.Message}");
                    _logger.LogError(backgroundEx, "BACKGROUND INDEXING: Unexpected failure for {RepositoryId} - {ExceptionType}: {Message}", 
                        repositoryId, backgroundEx.GetType().Name, backgroundEx.Message);
                    
                    if (backgroundEx.InnerException != null)
                    {
                        _logger.LogError("BACKGROUND INDEXING: Inner exception - {InnerType}: {InnerMessage}", 
                            backgroundEx.InnerException.GetType().Name, backgroundEx.InnerException.Message);
                    }
                }
            }, CancellationToken.None);

            return initialStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting repository indexing for {RepositoryId}", repositoryId);
            
            return new IndexStatus
            {
                Status = IndexingStatus.ERROR,
                ErrorMessage = ex.Message,
                DocumentsIndexed = 0,
                TotalDocuments = 0
            };
        }
    }

    /// <summary>
    /// Remove repository from search index
    /// </summary>
    /// <param name="repositoryId">Repository ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    [GraphQLDescription("Remove repository from search index")]
    public async Task<bool> RemoveRepositoryFromIndex(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Removing repository {RepositoryId} from search index", repositoryId);

            var success = await _indexingService.RemoveRepositoryFromIndexAsync(repositoryId, cancellationToken);

            _logger.LogInformation("Repository {RepositoryId} removal {Result}", 
                repositoryId, success ? "succeeded" : "failed");

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing repository {RepositoryId} from index", repositoryId);
            return false;
        }
    }

    /// <summary>
    /// Refresh repository index (incremental update)
    /// </summary>
    /// <param name="repositoryId">Repository ID to refresh</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Indexing status</returns>
    [GraphQLDescription("Refresh repository index with incremental updates")]
    public async Task<IndexStatus> RefreshRepositoryIndex(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting repository index refresh for {RepositoryId}", repositoryId);

            var status = await _indexingService.RefreshRepositoryIndexAsync(repositoryId, cancellationToken);

            _logger.LogInformation("Repository index refresh initiated for {RepositoryId}. Status: {Status}", 
                repositoryId, status.Status);

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing repository index for {RepositoryId}", repositoryId);
            
            return new IndexStatus
            {
                Status = IndexingStatus.ERROR,
                ErrorMessage = ex.Message,
                DocumentsIndexed = 0,
                TotalDocuments = 0
            };
        }
    }

    /// <summary>
    /// Recreate the Azure Search index with updated schema
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    [GraphQLDescription("Recreate Azure Search index with updated schema")]
    public async Task<bool> RecreateSearchIndex(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting Azure Search index recreation");

            // Delete existing index
            await _azureSearchService.DeleteIndexAsync(cancellationToken);
            
            // Create new index with updated schema
            var success = await _azureSearchService.CreateIndexAsync(cancellationToken);

            _logger.LogInformation("Azure Search index recreation {Result}", 
                success ? "succeeded" : "failed");

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recreating Azure Search index");
            return false;
        }
    }
}