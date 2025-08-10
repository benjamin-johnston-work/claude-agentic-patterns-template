using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Archie.Api.GraphQL.Types;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.AzureSearch.Models;

namespace Archie.Api.GraphQL.Resolvers;

/// <summary>
/// GraphQL query resolver for search functionality
/// </summary>
[ExtendObjectType(typeof(Query))]
public class SearchQueryResolver
{
    private readonly IAzureSearchService _searchService;
    private readonly IRepositoryIndexingService _indexingService;
    private readonly ILogger<SearchQueryResolver> _logger;

    public SearchQueryResolver(
        IAzureSearchService searchService,
        IRepositoryIndexingService indexingService,
        ILogger<SearchQueryResolver> logger)
    {
        _searchService = searchService;
        _indexingService = indexingService;
        _logger = logger;
    }

    /// <summary>
    /// Search across all repositories
    /// </summary>
    /// <param name="input">Search parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    [GraphQLDescription("Search across all indexed repositories")]
    public async Task<SearchResults> SearchRepositories(
        SearchRepositoriesInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing global repository search: {Query} (Type: {SearchType})", 
                input.Query, input.SearchType);

            var searchQuery = CreateSearchQuery(input);
            var results = await _searchService.SearchAsync(searchQuery, cancellationToken);

            _logger.LogInformation("Search completed. Found {ResultCount} results in {Duration}ms",
                results.TotalCount, results.SearchDuration.TotalMilliseconds);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing global repository search: {Query}", input.Query);
            
            // Return empty results rather than throwing to avoid breaking GraphQL response
            return new SearchResults
            {
                TotalCount = 0,
                Results = new List<SearchResult>(),
                SearchDuration = TimeSpan.Zero,
                Facets = new Dictionary<string, List<FacetResult>>()
            };
        }
    }

    /// <summary>
    /// Get a specific document by ID
    /// </summary>
    /// <param name="documentId">Document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document or null if not found</returns>
    [GraphQLDescription("Get a document by its ID")]
    public async Task<SearchableDocument?> GetDocument(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting document: {DocumentId}", documentId);
            
            var document = await _searchService.GetDocumentAsync(documentId, cancellationToken);
            
            if (document == null)
            {
                _logger.LogWarning("Document not found: {DocumentId}", documentId);
            }
            
            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document: {DocumentId}", documentId);
            return null;
        }
    }

    /// <summary>
    /// Get indexing status for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Indexing status</returns>
    [GraphQLDescription("Get indexing status for a repository")]
    public async Task<IndexStatus> GetIndexStatus(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting index status for repository: {RepositoryId}", repositoryId);
            
            var status = await _indexingService.GetIndexingStatusAsync(repositoryId, cancellationToken);
            
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index status for repository: {RepositoryId}", repositoryId);
            
            return new IndexStatus
            {
                Status = IndexingStatus.ERROR,
                ErrorMessage = "Error retrieving status"
            };
        }
    }

    /// <summary>
    /// Get search suggestions based on query input
    /// </summary>
    /// <param name="query">Search query for suggestions</param>
    /// <param name="limit">Maximum number of suggestions to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of search suggestions</returns>
    [GraphQLDescription("Get search suggestions for a query")]
    public async Task<List<string>> GetSearchSuggestions(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting search suggestions for query: {Query}", query);

            // For now, return empty suggestions - can be enhanced later with actual suggestion logic
            // This prevents GraphQL errors while maintaining schema compatibility
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return new List<string>();
            }

            // TODO: Implement actual search suggestion logic using Azure Search service
            // This could include:
            // - Popular search terms
            // - Repository names matching the query
            // - Code symbols/classes matching the query
            // - Recent searches by user
            
            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions for query: {Query}", query);
            return new List<string>();
        }
    }

    #region Private Helper Methods

    private static SearchQuery CreateSearchQuery(SearchRepositoriesInput input)
    {
        var searchQuery = SearchQuery.Create(input.Query, input.SearchType)
            .WithPaging(input.Top, input.Skip);

        if (input.Filters?.Any() == true)
        {
            var filters = input.Filters.Select(f => new SearchFilter
            {
                Field = f.Field,
                Operator = f.Operator,
                Value = f.Value
            }).ToArray();

            searchQuery.WithFilters(filters);
        }

        return searchQuery;
    }

    #endregion
}