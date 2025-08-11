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
    private readonly ILogger<SearchMutationResolver> _logger;

    public SearchMutationResolver(
        IRepositoryIndexingService indexingService,
        ILogger<SearchMutationResolver> logger)
    {
        _indexingService = indexingService;
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
            _logger.LogInformation("Starting repository indexing for {RepositoryId} (force: {Force})", 
                repositoryId, force);

            var status = await _indexingService.IndexRepositoryAsync(repositoryId, force, cancellationToken);

            _logger.LogInformation("Repository indexing initiated for {RepositoryId}. Status: {Status}", 
                repositoryId, status.Status);

            return status;
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
}