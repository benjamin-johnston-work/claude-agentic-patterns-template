using Archie.Application.DTOs;
using Archie.Application.UseCases;
using Archie.Domain.Entities;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;

namespace Archie.Api.GraphQL.Resolvers;

/// <summary>
/// GraphQL query resolver for conversation functionality
/// </summary>
[ExtendObjectType(typeof(Query))]
public class ConversationQueryResolver
{
    private readonly GetConversationUseCase _getConversationUseCase;
    private readonly ILogger<ConversationQueryResolver> _logger;

    public ConversationQueryResolver(
        GetConversationUseCase getConversationUseCase,
        ILogger<ConversationQueryResolver> logger)
    {
        _getConversationUseCase = getConversationUseCase ?? throw new ArgumentNullException(nameof(getConversationUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get conversations for the current user
    /// </summary>
    /// <param name="userId">User ID to get conversations for</param>
    /// <param name="status">Optional conversation status filter</param>
    /// <param name="limit">Maximum number of conversations to return</param>
    /// <param name="offset">Number of conversations to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of conversation summaries</returns>
    [GraphQLDescription("Get conversations for a user")]
    public async Task<List<ConversationSummaryDto>> GetConversations(
        Guid? userId = null,
        ConversationStatus? status = null,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, use a default user ID since authentication is not implemented
            // In a real implementation, this would come from the authenticated user context
            var effectiveUserId = userId ?? Guid.NewGuid(); // Default to new GUID to avoid errors

            _logger.LogInformation("Retrieving conversations for user: {UserId}, Status: {Status}", 
                effectiveUserId, status);

            var result = await _getConversationUseCase.GetUserConversationsAsync(
                effectiveUserId, status, limit, offset, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} conversations", result.Value!.Count);
                return result.Value!.ToList();
            }
            else
            {
                _logger.LogWarning("Failed to retrieve conversations: {Error}", result.Error);
                // Return empty list rather than throwing to avoid breaking GraphQL response
                return new List<ConversationSummaryDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations for user: {UserId}", userId);
            // Return empty list rather than throwing to avoid breaking GraphQL response
            return new List<ConversationSummaryDto>();
        }
    }

    /// <summary>
    /// Get a specific conversation by ID
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Conversation details or null if not found</returns>
    [GraphQLDescription("Get a specific conversation by ID")]
    public async Task<ConversationDto?> GetConversation(
        Guid conversationId,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, use a default user ID since authentication is not implemented
            var effectiveUserId = userId ?? Guid.NewGuid();

            _logger.LogInformation("Retrieving conversation: {ConversationId} for user: {UserId}", 
                conversationId, effectiveUserId);

            var result = await _getConversationUseCase.ExecuteAsync(
                conversationId, effectiveUserId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved conversation: {ConversationId}", conversationId);
                return result.Value;
            }
            else
            {
                _logger.LogWarning("Failed to retrieve conversation {ConversationId}: {Error}", 
                    conversationId, result.Error);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation: {ConversationId}", conversationId);
            return null;
        }
    }

    /// <summary>
    /// Search conversations by content
    /// </summary>
    /// <param name="searchTerm">Search term to match</param>
    /// <param name="userId">User ID to filter results</param>
    /// <param name="status">Optional conversation status filter</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Number of results to skip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching conversation summaries</returns>
    [GraphQLDescription("Search conversations by content")]
    public async Task<List<ConversationSummaryDto>> SearchConversations(
        string searchTerm,
        Guid? userId = null,
        ConversationStatus? status = null,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<ConversationSummaryDto>();
            }

            // For now, use a default user ID since authentication is not implemented
            var effectiveUserId = userId ?? Guid.NewGuid();

            _logger.LogInformation("Searching conversations for user: {UserId} with term: {SearchTerm}", 
                effectiveUserId, searchTerm);

            var searchInput = new ConversationSearchInput
            {
                SearchTerm = searchTerm,
                Status = status,
                Limit = limit,
                Offset = offset
            };

            var result = await _getConversationUseCase.SearchConversationsAsync(
                searchInput, effectiveUserId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully found {Count} conversations matching search", result.Value!.Count);
                return result.Value!.ToList();
            }
            else
            {
                _logger.LogWarning("Failed to search conversations: {Error}", result.Error);
                return new List<ConversationSummaryDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching conversations with term: {SearchTerm}", searchTerm);
            return new List<ConversationSummaryDto>();
        }
    }
}