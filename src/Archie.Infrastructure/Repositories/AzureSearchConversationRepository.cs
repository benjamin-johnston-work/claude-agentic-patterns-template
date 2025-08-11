using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Archie.Application.Common;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Infrastructure.Configuration;

namespace Archie.Infrastructure.Repositories;

/// <summary>
/// Azure Search implementation of conversation repository
/// </summary>
public class AzureSearchConversationRepository : IConversationRepository
{
    private readonly ConversationStorageOptions _options;
    private readonly ILogger<AzureSearchConversationRepository> _logger;

    // For now, use in-memory storage - would be replaced with actual Azure Search implementation
    private static readonly Dictionary<Guid, Conversation> _conversations = new();
    private static readonly Dictionary<Guid, List<Guid>> _userConversations = new();

    public AzureSearchConversationRepository(
        IOptions<ConversationStorageOptions> options,
        ILogger<AzureSearchConversationRepository> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Conversation>> SaveAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving conversation: {ConversationId}", conversation.Id);

            // In a real implementation, this would serialize the conversation and store it in Azure Search
            _conversations[conversation.Id] = conversation;

            // Track user conversations
            if (!_userConversations.ContainsKey(conversation.UserId))
            {
                _userConversations[conversation.UserId] = new List<Guid>();
            }

            if (!_userConversations[conversation.UserId].Contains(conversation.Id))
            {
                _userConversations[conversation.UserId].Add(conversation.Id);
            }

            _logger.LogInformation("Successfully saved conversation: {ConversationId}", conversation.Id);
            return Result<Conversation>.Success(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving conversation: {ConversationId}", conversation.Id);
            return Result<Conversation>.Failure($"Failed to save conversation: {ex.Message}");
        }
    }

    public async Task<Result<Conversation?>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving conversation: {ConversationId}", id);

            if (_conversations.TryGetValue(id, out var conversation))
            {
                _logger.LogDebug("Found conversation: {ConversationId}", id);
                return Result<Conversation?>.Success(conversation);
            }

            _logger.LogDebug("Conversation not found: {ConversationId}", id);
            return Result<Conversation?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation: {ConversationId}", id);
            return Result<Conversation?>.Failure($"Failed to retrieve conversation: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<Conversation>>> GetByUserIdAsync(
        Guid userId,
        ConversationStatus? status = null,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving conversations for user: {UserId}, Status: {Status}, Limit: {Limit}, Offset: {Offset}",
                userId, status, limit, offset);

            var conversations = new List<Conversation>();

            if (_userConversations.TryGetValue(userId, out var userConversationIds))
            {
                foreach (var conversationId in userConversationIds)
                {
                    if (_conversations.TryGetValue(conversationId, out var conversation))
                    {
                        if (status == null || conversation.Status == status.Value)
                        {
                            conversations.Add(conversation);
                        }
                    }
                }
            }

            // Sort by last activity (most recent first) and apply pagination
            var result = conversations
                .OrderByDescending(c => c.LastActivityAt)
                .Skip(offset)
                .Take(limit)
                .ToList();

            _logger.LogDebug("Retrieved {Count} conversations for user: {UserId}", result.Count, userId);
            return Result<IReadOnlyList<Conversation>>.Success(result.AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations for user: {UserId}", userId);
            return Result<IReadOnlyList<Conversation>>.Failure($"Failed to retrieve user conversations: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<Conversation>>> GetByRepositoryIdsAsync(
        IReadOnlyList<Guid> repositoryIds,
        Guid? userId = null,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving conversations for repositories: {RepositoryIds}, UserId: {UserId}",
                string.Join(",", repositoryIds), userId);

            var conversations = _conversations.Values
                .Where(c => repositoryIds.Any(repoId => c.Context.RepositoryIds.Contains(repoId)))
                .Where(c => !userId.HasValue || c.UserId == userId.Value)
                .OrderByDescending(c => c.LastActivityAt)
                .Skip(offset)
                .Take(limit)
                .ToList();

            _logger.LogDebug("Retrieved {Count} repository conversations", conversations.Count);
            return Result<IReadOnlyList<Conversation>>.Success(conversations.AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving repository conversations");
            return Result<IReadOnlyList<Conversation>>.Failure($"Failed to retrieve repository conversations: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting conversation: {ConversationId}", id);

            if (_conversations.TryGetValue(id, out var conversation))
            {
                // Mark as deleted instead of actually removing (soft delete)
                conversation.Delete();
                _logger.LogInformation("Successfully marked conversation as deleted: {ConversationId}", id);
                return Result<bool>.Success(true);
            }

            _logger.LogWarning("Conversation not found for deletion: {ConversationId}", id);
            return Result<bool>.Success(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation: {ConversationId}", id);
            return Result<bool>.Failure($"Failed to delete conversation: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<Conversation>>> SearchAsync(
        string searchTerm,
        Guid userId,
        ConversationStatus? status = null,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Searching conversations for user: {UserId}, Term: {SearchTerm}", userId, searchTerm);

            var conversations = _conversations.Values
                .Where(c => c.UserId == userId)
                .Where(c => status == null || c.Status == status.Value)
                .Where(c => 
                    c.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    c.Messages.Any(m => m.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(c => c.LastActivityAt)
                .Skip(offset)
                .Take(limit)
                .ToList();

            _logger.LogDebug("Found {Count} conversations matching search term: {SearchTerm}", conversations.Count, searchTerm);
            return Result<IReadOnlyList<Conversation>>.Success(conversations.AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching conversations for user: {UserId}", userId);
            return Result<IReadOnlyList<Conversation>>.Failure($"Failed to search conversations: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<Conversation>>> GetConversationsForCleanupAsync(
        int retentionDays,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Retrieving conversations for cleanup older than {RetentionDays} days", retentionDays);

            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

            var conversations = _conversations.Values
                .Where(c => c.Status == ConversationStatus.Deleted && c.LastActivityAt < cutoffDate)
                .OrderBy(c => c.LastActivityAt)
                .Take(limit)
                .ToList();

            _logger.LogDebug("Found {Count} conversations eligible for cleanup", conversations.Count);
            return Result<IReadOnlyList<Conversation>>.Success(conversations.AsReadOnly());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations for cleanup");
            return Result<IReadOnlyList<Conversation>>.Failure($"Failed to retrieve conversations for cleanup: {ex.Message}");
        }
    }

    public async Task<Result<ConversationStatistics>> GetStatisticsAsync(
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating conversation statistics for user: {UserId}", userId);

            var conversations = _conversations.Values
                .Where(c => !userId.HasValue || c.UserId == userId.Value)
                .Where(c => !fromDate.HasValue || c.CreatedAt >= fromDate.Value)
                .Where(c => !toDate.HasValue || c.CreatedAt <= toDate.Value)
                .ToList();

            var statistics = new ConversationStatistics
            {
                TotalConversations = conversations.Count,
                ActiveConversations = conversations.Count(c => c.Status == ConversationStatus.Active),
                ArchivedConversations = conversations.Count(c => c.Status == ConversationStatus.Archived),
                DeletedConversations = conversations.Count(c => c.Status == ConversationStatus.Deleted),
                TotalMessages = conversations.Sum(c => c.GetMessageCount()),
                TotalUserMessages = conversations.Sum(c => c.Messages.Count(m => m.Type == MessageType.UserQuery)),
                TotalAIMessages = conversations.Sum(c => c.Messages.Count(m => m.Type == MessageType.AIResponse)),
                AverageMessagesPerConversation = conversations.Any() ? conversations.Average(c => c.GetMessageCount()) : 0,
                AverageResponseTimeSeconds = conversations
                    .SelectMany(c => c.Messages)
                    .Where(m => m.Metadata.HasResponseTime())
                    .Average(m => m.Metadata.ResponseTimeSeconds ?? 0),
                AverageConversationDuration = TimeSpan.FromTicks(conversations.Any() 
                    ? (long)conversations.Average(c => c.GetConversationDuration().Ticks) 
                    : 0),
                LastActivityAt = conversations.Any() ? conversations.Max(c => c.LastActivityAt) : null,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogDebug("Generated statistics for {ConversationCount} conversations", conversations.Count);
            return Result<ConversationStatistics>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating conversation statistics");
            return Result<ConversationStatistics>.Failure($"Failed to generate statistics: {ex.Message}");
        }
    }

    public async Task<Result<int>> BulkArchiveAsync(
        Guid userId,
        int olderThanDays,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Bulk archiving conversations for user: {UserId} older than {Days} days", userId, olderThanDays);

            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            var archivedCount = 0;

            if (_userConversations.TryGetValue(userId, out var userConversationIds))
            {
                foreach (var conversationId in userConversationIds)
                {
                    if (_conversations.TryGetValue(conversationId, out var conversation))
                    {
                        if (conversation.Status == ConversationStatus.Active && 
                            conversation.LastActivityAt < cutoffDate)
                        {
                            conversation.Archive();
                            archivedCount++;
                        }
                    }
                }
            }

            _logger.LogInformation("Bulk archived {Count} conversations for user: {UserId}", archivedCount, userId);
            return Result<int>.Success(archivedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk archiving conversations for user: {UserId}", userId);
            return Result<int>.Failure($"Failed to bulk archive conversations: {ex.Message}");
        }
    }
}