using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

/// <summary>
/// Service for managing conversation context and retrieving relevant information
/// </summary>
public interface IConversationContextService
{
    /// <summary>
    /// Retrieve relevant context from repository content using hybrid search
    /// </summary>
    /// <param name="query">User query to search for relevant context</param>
    /// <param name="context">Conversation context with repository information</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant search results for context building</returns>
    Task<IReadOnlyList<SearchResult>> RetrieveRelevantContextAsync(
        string query,
        ConversationContext context,
        int maxResults = 10,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Build a comprehensive context prompt for AI processing
    /// </summary>
    /// <param name="query">Original user query</param>
    /// <param name="searchResults">Relevant search results from repository content</param>
    /// <param name="messageHistory">Previous conversation messages</param>
    /// <param name="context">Conversation context and preferences</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Well-structured context prompt for AI processing</returns>
    Task<string> BuildContextPromptAsync(
        string query,
        IReadOnlyList<SearchResult> searchResults,
        IReadOnlyList<ConversationMessage> messageHistory,
        ConversationContext context,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enrich conversation context with additional information based on the query
    /// </summary>
    /// <param name="context">Current conversation context</param>
    /// <param name="query">User query for context enrichment</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced conversation context with additional metadata</returns>
    Task<ConversationContext> EnrichContextAsync(
        ConversationContext context,
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve code references related to the query from indexed repositories
    /// </summary>
    /// <param name="query">Query to search for code references</param>
    /// <param name="repositoryIds">Repository IDs to search within</param>
    /// <param name="maxResults">Maximum number of code references to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant code references with file paths and line numbers</returns>
    Task<IReadOnlyList<CodeReference>> RetrieveCodeReferencesAsync(
        string query,
        IReadOnlyList<Guid> repositoryIds,
        int maxResults = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve documentation sections relevant to the query
    /// </summary>
    /// <param name="query">Query to search for documentation</param>
    /// <param name="repositoryIds">Repository IDs to search within</param>
    /// <param name="maxResults">Maximum number of documentation sections to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant documentation sections</returns>
    Task<IReadOnlyList<DocumentationSection>> RetrieveDocumentationAsync(
        string query,
        IReadOnlyList<Guid> repositoryIds,
        int maxResults = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Build cross-repository context when query spans multiple repositories
    /// </summary>
    /// <param name="query">User query</param>
    /// <param name="repositoryIds">List of repository IDs to search across</param>
    /// <param name="maxResultsPerRepository">Maximum results per repository</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cross-repository context with comparative analysis</returns>
    Task<CrossRepositoryContext> BuildCrossRepositoryContextAsync(
        string query,
        IReadOnlyList<Guid> repositoryIds,
        int maxResultsPerRepository = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze conversation context to suggest query improvements
    /// </summary>
    /// <param name="query">Original query</param>
    /// <param name="context">Current conversation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Suggested query refinements for better results</returns>
    Task<IReadOnlyList<string>> SuggestQueryRefinementsAsync(
        string query,
        ConversationContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Search result model for context retrieval
/// </summary>
public class SearchResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Language { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public Guid RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime IndexedAt { get; set; }
}

/// <summary>
/// Cross-repository context model for multi-repository queries
/// </summary>
public class CrossRepositoryContext
{
    public IReadOnlyList<Guid> RepositoryIds { get; set; } = Array.Empty<Guid>();
    public IReadOnlyList<SearchResult> Results { get; set; } = Array.Empty<SearchResult>();
    public Dictionary<Guid, IReadOnlyList<SearchResult>> ResultsByRepository { get; set; } = new();
    public IReadOnlyList<string> CommonPatterns { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Differences { get; set; } = Array.Empty<string>();
    public Dictionary<string, object> ComparisonMetadata { get; set; } = new();
}