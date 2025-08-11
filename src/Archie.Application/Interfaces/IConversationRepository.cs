using Archie.Domain.Entities;
using Archie.Application.Common;

namespace Archie.Application.Interfaces;

/// <summary>
/// Repository interface for conversation persistence and retrieval
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Save a conversation to the data store
    /// </summary>
    /// <param name="conversation">Conversation entity to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Saved conversation with updated timestamps</returns>
    Task<Result<Conversation>> SaveAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve a conversation by its unique identifier
    /// </summary>
    /// <param name="id">Conversation identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation if found, null otherwise</returns>
    Task<Result<Conversation?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve all conversations for a specific user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="limit">Maximum number of conversations to return</param>
    /// <param name="offset">Number of conversations to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user conversations</returns>
    Task<Result<IReadOnlyList<Conversation>>> GetByUserIdAsync(
        Guid userId, 
        ConversationStatus? status = null, 
        int limit = 20, 
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve conversations associated with specific repositories
    /// </summary>
    /// <param name="repositoryIds">List of repository identifiers</param>
    /// <param name="userId">Optional user identifier to filter by</param>
    /// <param name="limit">Maximum number of conversations to return</param>
    /// <param name="offset">Number of conversations to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of repository-related conversations</returns>
    Task<Result<IReadOnlyList<Conversation>>> GetByRepositoryIdsAsync(
        IReadOnlyList<Guid> repositoryIds, 
        Guid? userId = null,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a conversation by its identifier
    /// </summary>
    /// <param name="id">Conversation identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false if not found</returns>
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search conversations by content or title
    /// </summary>
    /// <param name="searchTerm">Search term to match against conversation titles and messages</param>
    /// <param name="userId">User identifier to filter results</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="limit">Maximum number of results to return</param>
    /// <param name="offset">Number of results to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching conversations</returns>
    Task<Result<IReadOnlyList<Conversation>>> SearchAsync(
        string searchTerm,
        Guid userId,
        ConversationStatus? status = null,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get conversations that need cleanup (archived or deleted beyond retention period)
    /// </summary>
    /// <param name="retentionDays">Number of days to retain conversations</param>
    /// <param name="limit">Maximum number of conversations to return for cleanup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of conversations eligible for cleanup</returns>
    Task<Result<IReadOnlyList<Conversation>>> GetConversationsForCleanupAsync(
        int retentionDays,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get conversation statistics for analytics
    /// </summary>
    /// <param name="userId">Optional user identifier to filter statistics</param>
    /// <param name="fromDate">Optional start date for statistics</param>
    /// <param name="toDate">Optional end date for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation analytics data</returns>
    Task<Result<ConversationStatistics>> GetStatisticsAsync(
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk archive conversations by criteria
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="olderThanDays">Archive conversations older than specified days</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of conversations archived</returns>
    Task<Result<int>> BulkArchiveAsync(
        Guid userId,
        int olderThanDays,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics model for conversation analytics
/// </summary>
public class ConversationStatistics
{
    public int TotalConversations { get; set; }
    public int ActiveConversations { get; set; }
    public int ArchivedConversations { get; set; }
    public int DeletedConversations { get; set; }
    public int TotalMessages { get; set; }
    public int TotalUserMessages { get; set; }
    public int TotalAIMessages { get; set; }
    public double AverageMessagesPerConversation { get; set; }
    public double AverageResponseTimeSeconds { get; set; }
    public TimeSpan AverageConversationDuration { get; set; }
    public Dictionary<string, int> TopDomains { get; set; } = new();
    public Dictionary<string, int> TopIntentTypes { get; set; } = new();
    public Dictionary<Guid, int> ConversationsByRepository { get; set; } = new();
    public DateTime? LastActivityAt { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}