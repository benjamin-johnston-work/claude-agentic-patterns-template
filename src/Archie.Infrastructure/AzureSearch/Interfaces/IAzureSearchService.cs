using Archie.Infrastructure.AzureSearch.Models;

namespace Archie.Infrastructure.AzureSearch.Interfaces;

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