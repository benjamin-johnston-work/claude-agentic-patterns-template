using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using Archie.Api.GraphQL.Types;
using Archie.Domain.Entities;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.AzureSearch.Models;

namespace Archie.Api.GraphQL.Resolvers;

/// <summary>
/// Extensions to the Repository type for search functionality
/// </summary>
[ExtendObjectType(typeof(Repository))]
public class RepositorySearchExtensions
{
    private readonly IAzureSearchService _searchService;
    private readonly IRepositoryIndexingService _indexingService;
    private readonly ILogger<RepositorySearchExtensions> _logger;

    public RepositorySearchExtensions(
        IAzureSearchService searchService,
        IRepositoryIndexingService indexingService,
        ILogger<RepositorySearchExtensions> logger)
    {
        _searchService = searchService;
        _indexingService = indexingService;
        _logger = logger;
    }

    /// <summary>
    /// Get searchable documents for this repository
    /// </summary>
    /// <param name="parent">The repository</param>
    /// <param name="branch">Optional branch filter</param>
    /// <param name="filePattern">Optional file pattern filter</param>
    /// <param name="language">Optional language filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of searchable documents</returns>
    [GraphQLDescription("Get searchable documents for this repository")]
    public async Task<List<SearchableDocument>> GetDocuments(
        [Parent] Repository parent,
        string? branch = null,
        string? filePattern = null,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting documents for repository {RepositoryId} with filters: branch={Branch}, pattern={FilePattern}, language={Language}",
                parent.Id, branch, filePattern, language);

            // Build search query with filters
            var searchQuery = SearchQuery.Create("*", SearchType.Keyword)
                .WithPaging(1000, 0); // Get up to 1000 documents

            var filters = new List<SearchFilter>
            {
                SearchFilter.Equal("repository_id", parent.Id.ToString())
            };

            if (!string.IsNullOrEmpty(branch))
                filters.Add(SearchFilter.Equal("branch_name", branch));

            if (!string.IsNullOrEmpty(filePattern))
                filters.Add(SearchFilter.Contains("file_name", filePattern));

            if (!string.IsNullOrEmpty(language))
                filters.Add(SearchFilter.Equal("language", language));

            searchQuery.WithFilters(filters.ToArray());

            var results = await _searchService.SearchAsync(searchQuery, cancellationToken);
            
            var documents = results.Results.Select(r => r.Document).ToList();
            
            _logger.LogDebug("Found {DocumentCount} documents for repository {RepositoryId}", 
                documents.Count, parent.Id);
            
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for repository {RepositoryId}", parent.Id);
            return new List<SearchableDocument>();
        }
    }

    /// <summary>
    /// Search within this specific repository
    /// </summary>
    /// <param name="parent">The repository</param>
    /// <param name="input">Search parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results</returns>
    [GraphQLDescription("Search within this specific repository")]
    public async Task<SearchResults> Search(
        [Parent] Repository parent,
        SearchRepositoriesInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching repository {RepositoryId}: {Query} (Type: {SearchType})", 
                parent.Id, input.Query, input.SearchType);

            var searchQuery = CreateSearchQuery(input);
            var results = await _searchService.SearchRepositoryAsync(parent.Id, searchQuery, cancellationToken);

            _logger.LogInformation("Repository search completed for {RepositoryId}. Found {ResultCount} results in {Duration}ms",
                parent.Id, results.TotalCount, results.SearchDuration.TotalMilliseconds);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching repository {RepositoryId}: {Query}", parent.Id, input.Query);
            
            // Return empty results rather than throwing
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
    /// Get indexing status for this repository
    /// </summary>
    /// <param name="parent">The repository</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Indexing status</returns>
    [GraphQLDescription("Get indexing status for this repository")]
    public async Task<IndexStatus> GetIndexStatus(
        [Parent] Repository parent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting index status for repository {RepositoryId}", parent.Id);
            
            var status = await _indexingService.GetIndexingStatusAsync(parent.Id, cancellationToken);
            
            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index status for repository {RepositoryId}", parent.Id);
            
            return new IndexStatus
            {
                Status = IndexingStatus.ERROR,
                ErrorMessage = "Error retrieving status",
                DocumentsIndexed = 0,
                TotalDocuments = 0
            };
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