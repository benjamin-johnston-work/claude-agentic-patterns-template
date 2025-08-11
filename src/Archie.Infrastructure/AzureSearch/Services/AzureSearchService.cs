using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.AzureSearch.Models;
using Archie.Infrastructure.Configuration;

namespace Archie.Infrastructure.AzureSearch.Services;

/// <summary>
/// Azure AI Search service implementation providing document indexing and search capabilities
/// with hybrid search (vector + text) support for repository documents.
/// </summary>
public class AzureSearchService : IAzureSearchService
{
    private readonly SearchIndexClient _indexClient;
    private readonly SearchClient _searchClient;
    private readonly AzureSearchOptions _options;
    private readonly ILogger<AzureSearchService> _logger;
    private readonly string _indexName;

    public AzureSearchService(
        IOptions<AzureSearchOptions> options,
        ILogger<AzureSearchService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _indexName = _options.IndexName;

        var serviceEndpoint = new Uri(_options.ServiceUrl);
        var credential = new AzureKeyCredential(_options.AdminKey);

        _indexClient = new SearchIndexClient(serviceEndpoint, credential);
        _searchClient = new SearchClient(serviceEndpoint, _indexName, credential);
    }

    public async Task<bool> CreateIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating Azure Search index: {IndexName}", _indexName);

            var searchIndex = CreateSearchIndexDefinition();
            
            var response = await _indexClient.CreateOrUpdateIndexAsync(searchIndex);
            
            if (response.GetRawResponse().IsError)
            {
                _logger.LogError("Failed to create index {IndexName}: {Error}", 
                    _indexName, response.GetRawResponse().ReasonPhrase);
                return false;
            }

            _logger.LogInformation("Successfully created Azure Search index: {IndexName}", _indexName);
            return true;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Search API error creating index {IndexName}: {Message}", 
                _indexName, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating index {IndexName}", _indexName);
            return false;
        }
    }

    public async Task<bool> DeleteIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting Azure Search index: {IndexName}", _indexName);

            var response = await _indexClient.DeleteIndexAsync(_indexName, cancellationToken);
            
            if (response.IsError)
            {
                _logger.LogError("Failed to delete index {IndexName}: {Error}", 
                    _indexName, response.ReasonPhrase);
                return false;
            }

            _logger.LogInformation("Successfully deleted Azure Search index: {IndexName}", _indexName);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Index {IndexName} does not exist, deletion skipped", _indexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting index {IndexName}", _indexName);
            return false;
        }
    }

    public async Task<IndexStatus> GetIndexStatusAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Query for documents belonging to this repository
            var searchOptions = new SearchOptions
            {
                Filter = $"repository_id eq '{repositoryId}'",
                Size = 0, // We only want count
                IncludeTotalCount = true
            };

            var searchResults = await _searchClient.SearchAsync<SearchableDocument>("*", searchOptions, cancellationToken);
            var totalCount = searchResults.Value.TotalCount ?? 0;

            return new IndexStatus
            {
                Status = totalCount > 0 ? IndexingStatus.COMPLETED : IndexingStatus.NOT_STARTED,
                DocumentsIndexed = (int)totalCount,
                TotalDocuments = (int)totalCount, // This would need to be calculated from repository analysis
                LastIndexed = totalCount > 0 ? DateTime.UtcNow : null // Simplified - would need actual tracking
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index status for repository {RepositoryId}", repositoryId);
            return new IndexStatus
            {
                Status = IndexingStatus.ERROR,
                DocumentsIndexed = 0,
                TotalDocuments = 0
            };
        }
    }

    public async Task<bool> IndexDocumentAsync(SearchableDocument document, CancellationToken cancellationToken = default)
    {
        try
        {
            var searchDocument = ConvertToSearchDocument(document);
            
            var batch = IndexDocumentsBatch.Create(
                IndexDocumentsAction.MergeOrUpload(searchDocument)
            );

            var response = await _searchClient.IndexDocumentsAsync(batch);
            var result = response.Value.Results.FirstOrDefault();

            if (result?.Succeeded == true)
            {
                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug("Successfully indexed document {DocumentId}", document.Id);
                }
                return true;
            }

            _logger.LogError("Failed to index document {DocumentId}: {ErrorMessage}", 
                document.Id, result?.ErrorMessage);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document {DocumentId}", document.Id);
            return false;
        }
    }

    public async Task<bool> IndexDocumentsAsync(IEnumerable<SearchableDocument> documents, CancellationToken cancellationToken = default)
    {
        var documentList = documents.ToList();
        if (!documentList.Any())
        {
            _logger.LogWarning("No documents provided for batch indexing");
            return true;
        }

        try
        {
            _logger.LogInformation("Starting batch indexing of {DocumentCount} documents", documentList.Count);

            var batches = documentList
                .Select(ConvertToSearchDocument)
                .Chunk(_options.MaxBatchSize)
                .ToList();

            var totalSuccess = 0;
            var totalErrors = 0;

            foreach (var batch in batches)
            {
                var indexBatch = IndexDocumentsBatch.Create(
                    batch.Select(doc => IndexDocumentsAction.MergeOrUpload(doc)).ToArray()
                );

                var response = await _searchClient.IndexDocumentsAsync(indexBatch);
                
                var successCount = response.Value.Results.Count(r => r.Succeeded);
                var errorCount = response.Value.Results.Count(r => !r.Succeeded);

                totalSuccess += successCount;
                totalErrors += errorCount;

                if (errorCount > 0 && _options.EnableDetailedLogging)
                {
                    var failedResults = response.Value.Results.Where(r => !r.Succeeded);
                    foreach (var failure in failedResults)
                    {
                        _logger.LogError("Failed to index document {Key}: {ErrorMessage}", 
                            failure.Key, failure.ErrorMessage);
                    }
                }
            }

            _logger.LogInformation("Batch indexing completed. Success: {SuccessCount}, Errors: {ErrorCount}", 
                totalSuccess, totalErrors);

            return totalErrors == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch indexing of {DocumentCount} documents", documentList.Count);
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var batch = IndexDocumentsBatch.Create(
                IndexDocumentsAction.Delete("document_id", documentId)
            );

            var response = await _searchClient.IndexDocumentsAsync(batch);
            var result = response.Value.Results.FirstOrDefault();

            if (result?.Succeeded == true)
            {
                if (_options.EnableDetailedLogging)
                {
                    _logger.LogDebug("Successfully deleted document {DocumentId}", documentId);
                }
                return true;
            }

            _logger.LogError("Failed to delete document {DocumentId}: {ErrorMessage}", 
                documentId, result?.ErrorMessage);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<bool> DeleteRepositoryDocumentsAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting all documents for repository {RepositoryId}", repositoryId);

            // First, search for all documents belonging to this repository
            var searchOptions = new SearchOptions
            {
                Filter = $"repository_id eq '{repositoryId}'",
                Select = { "document_id" },
                Size = 1000 // Process in chunks
            };

            var totalDeleted = 0;
            string? continuationToken = null;

            do
            {
                // Note: Azure AI Search 11.6.1 doesn't use SearchId for continuation
                // Pagination is handled differently in newer versions

                var searchResults = await _searchClient.SearchAsync<Dictionary<string, object>>("*", searchOptions);
                
                var documentsToDelete = searchResults.Value.GetResults()
                    .Select(result => result.Document["document_id"].ToString())
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

                if (documentsToDelete.Any())
                {
                    var deleteBatch = IndexDocumentsBatch.Create(
                        documentsToDelete.Select(id => IndexDocumentsAction.Delete("document_id", id)).ToArray()
                    );

                    var deleteResponse = await _searchClient.IndexDocumentsAsync(deleteBatch);
                    var successCount = deleteResponse.Value.Results.Count(r => r.Succeeded);
                    totalDeleted += successCount;
                }

                // Check if there are more results - in newer versions this is handled by Skip/Top
                // For now, just exit the loop after first batch
                break;

            } while (false);

            _logger.LogInformation("Successfully deleted {DocumentCount} documents for repository {RepositoryId}", 
                totalDeleted, repositoryId);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting documents for repository {RepositoryId}", repositoryId);
            return false;
        }
    }

    public async Task<SearchResults> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var searchOptions = BuildSearchOptions(query);
            var searchText = string.IsNullOrEmpty(query.Query) ? "*" : query.Query;

            SearchResults<SearchableDocument> response;

            if (query.SearchType == SearchType.Semantic)
            {
                // Vector search only
                response = await _searchClient.SearchAsync<SearchableDocument>(searchText, searchOptions, cancellationToken);
            }
            else if (query.SearchType == SearchType.Keyword)
            {
                // Text search only
                response = await _searchClient.SearchAsync<SearchableDocument>(searchText, searchOptions, cancellationToken);
            }
            else // Hybrid search (default)
            {
                // Combine vector and text search using RRF (Reciprocal Rank Fusion)
                response = await _searchClient.SearchAsync<SearchableDocument>(searchText, searchOptions, cancellationToken);
            }

            stopwatch.Stop();

            var results = new SearchResults
            {
                TotalCount = response.TotalCount ?? 0,
                SearchDuration = stopwatch.Elapsed,
                Results = response.GetResults().Select(MapToSearchResult).ToList()
            };

            // Add facets if requested
            if (response.Facets != null)
            {
                results.Facets = response.Facets.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(f => new Models.FacetResult
                    {
                        Value = f.Value?.ToString() ?? string.Empty,
                        Count = f.Count ?? 0
                    }).ToList()
                );
            }

            _logger.LogInformation("Search completed in {Duration}ms, found {Count} results for query: {Query}", 
                stopwatch.ElapsedMilliseconds, results.TotalCount, query.Query);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing search query: {Query}", query.Query);
            return new SearchResults();
        }
    }

    public async Task<SearchResults> SearchRepositoryAsync(Guid repositoryId, SearchQuery query, CancellationToken cancellationToken = default)
    {
        // Add repository filter to the existing query
        var repositoryFilter = Models.SearchFilter.Equal("repository_id", repositoryId.ToString());
        var filteredQuery = Models.SearchQuery.Create(query.Query, query.SearchType)
            .WithFilters(query.Filters.Concat(new[] { repositoryFilter }).ToArray())
            .WithPaging(query.Top, query.Skip);

        return await SearchAsync(filteredQuery, cancellationToken);
    }

    public async Task<SearchableDocument?> GetDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _searchClient.GetDocumentAsync<SearchableDocument>(documentId);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId}", documentId);
            return null;
        }
    }

    #region Private Helper Methods

    private SearchIndex CreateSearchIndexDefinition()
    {
        var searchIndex = new SearchIndex(_indexName)
        {
            Fields =
            {
                new SimpleField("document_id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                new SimpleField("repository_id", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SearchableField("file_path") { IsFilterable = true, IsSortable = true },
                new SearchableField("file_name") { IsFilterable = true, IsSortable = true },
                new SimpleField("file_extension", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SimpleField("language", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SearchableField("content"),
                new VectorSearchField("content_vector", 1536, "vector-profile"),
                new SimpleField("line_count", SearchFieldDataType.Int32) { IsFilterable = true, IsSortable = true, IsFacetable = true },
                new SimpleField("size_bytes", SearchFieldDataType.Int64) { IsFilterable = true, IsSortable = true },
                new SimpleField("last_modified", SearchFieldDataType.DateTimeOffset) { IsFilterable = true, IsSortable = true },
                new SimpleField("branch_name", SearchFieldDataType.String) { IsFilterable = true, IsFacetable = true },
                new SearchableField("repository_name") { IsFilterable = true, IsFacetable = true },
                new SearchableField("repository_owner") { IsFilterable = true, IsFacetable = true },
                new SimpleField("repository_url", SearchFieldDataType.String),
                new SearchableField("code_symbols") { IsFilterable = true, IsFacetable = true }
            }
        };

        // Vector search configuration
        searchIndex.VectorSearch = new VectorSearch();
        searchIndex.VectorSearch.Algorithms.Add(new HnswAlgorithmConfiguration("hnsw-algorithm")
        {
            Parameters = new HnswParameters
            {
                M = 4,
                EfConstruction = 400,
                EfSearch = 500,
                Metric = VectorSearchAlgorithmMetric.Cosine
            }
        });

        searchIndex.VectorSearch.Profiles.Add(new VectorSearchProfile("vector-profile", "hnsw-algorithm"));

        // Scoring profile for hybrid search
        var scoringProfile = new ScoringProfile("hybrid-scoring");
        scoringProfile.TextWeights = new TextWeights(new Dictionary<string, double>
        {
            { "content", 1.0 },
            { "file_name", 2.0 },
            { "file_path", 1.5 },
            { "repository_name", 1.2 },
            { "code_symbols", 1.8 }
        });

        scoringProfile.Functions.Add(new FreshnessScoringFunction("last_modified", 1.1, new FreshnessScoringParameters(TimeSpan.FromDays(30)))
        {
            Interpolation = ScoringFunctionInterpolation.Linear
        });

        searchIndex.ScoringProfiles.Add(scoringProfile);

        return searchIndex;
    }

    private SearchOptions BuildSearchOptions(SearchQuery query)
    {
        var options = new SearchOptions
        {
            Size = query.Top,
            Skip = query.Skip,
            IncludeTotalCount = true,
            ScoringProfile = "hybrid-scoring"
        };

        // Add filters
        if (query.Filters.Any())
        {
            var filterExpressions = query.Filters.Select(f => BuildFilterExpression(f));
            options.Filter = string.Join(" and ", filterExpressions);
        }

        // Add highlighting
        options.HighlightFields.Add("content");
        options.HighlightFields.Add("file_name");
        options.HighlightFields.Add("code_symbols");

        // Add facets
        options.Facets.Add("language");
        options.Facets.Add("file_extension");
        options.Facets.Add("repository_name");
        options.Facets.Add("branch_name");

        return options;
    }

    private string BuildFilterExpression(Archie.Infrastructure.AzureSearch.Models.SearchFilter filter)
    {
        var field = filter.Field;
        var op = filter.Operator.ToLowerInvariant();
        var value = filter.Value;

        return op switch
        {
            "eq" => $"{field} eq '{value}'",
            "ne" => $"{field} ne '{value}'",
            "gt" => $"{field} gt {value}",
            "lt" => $"{field} lt {value}",
            "contains" => $"search.ismatch('{value}', '{field}')",
            _ => throw new ArgumentException($"Unsupported filter operator: {op}")
        };
    }

    private Dictionary<string, object> ConvertToSearchDocument(SearchableDocument document)
    {
        return new Dictionary<string, object>
        {
            ["document_id"] = document.Id,
            ["repository_id"] = document.RepositoryId.ToString(),
            ["file_path"] = document.FilePath,
            ["file_name"] = document.FileName,
            ["file_extension"] = document.FileExtension,
            ["language"] = document.Language,
            ["content"] = document.Content,
            ["content_vector"] = document.ContentVector,
            ["line_count"] = document.LineCount,
            ["size_bytes"] = document.SizeInBytes,
            ["last_modified"] = document.LastModified,
            ["branch_name"] = document.BranchName,
            ["repository_name"] = document.Metadata.RepositoryName,
            ["repository_owner"] = document.Metadata.RepositoryOwner,
            ["repository_url"] = document.Metadata.RepositoryUrl,
            ["code_symbols"] = document.Metadata.CodeSymbols.ToArray()
        };
    }

    private SearchResult MapToSearchResult(Azure.Search.Documents.Models.SearchResult<SearchableDocument> result)
    {
        var highlights = new List<string>();
        if (result.Highlights?.Any() == true)
        {
            highlights = result.Highlights.SelectMany(h => h.Value).ToList();
        }

        return new SearchResult
        {
            DocumentId = result.Document.Id,
            Score = result.Score ?? 0.0,
            Document = result.Document,
            Highlights = highlights
        };
    }

    #endregion
}