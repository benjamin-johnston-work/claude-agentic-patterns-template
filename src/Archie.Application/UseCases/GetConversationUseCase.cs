using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class GetConversationUseCase
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<GetConversationUseCase> _logger;

    public GetConversationUseCase(
        IConversationRepository conversationRepository,
        ILogger<GetConversationUseCase> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ConversationDto?>> ExecuteAsync(
        Guid conversationId, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving conversation: {ConversationId} for user: {UserId}", 
                conversationId, userId);

            var result = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);
            if (result.IsFailure)
            {
                _logger.LogError("Failed to retrieve conversation: {Error}", result.Error);
                return Result<ConversationDto?>.Failure($"Failed to retrieve conversation: {result.Error}");
            }

            var conversation = result.Value;
            if (conversation == null)
            {
                _logger.LogInformation("Conversation not found: {ConversationId}", conversationId);
                return Result<ConversationDto?>.Success(null);
            }

            // Verify user ownership
            if (conversation.UserId != userId)
            {
                _logger.LogWarning("Unauthorized access to conversation: {ConversationId} by user: {UserId}", 
                    conversationId, userId);
                return Result<ConversationDto?>.Failure("Unauthorized access to conversation");
            }

            var conversationDto = MapToDto(conversation);

            _logger.LogInformation("Successfully retrieved conversation: {ConversationId}", conversationId);

            return Result<ConversationDto?>.Success(conversationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation: {ConversationId}", conversationId);
            return Result<ConversationDto?>.Failure($"Failed to retrieve conversation: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<ConversationSummaryDto>>> GetUserConversationsAsync(
        Guid userId,
        ConversationStatus? status = null,
        List<Guid>? repositoryIds = null,
        int limit = 20,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving conversations for user: {UserId}, Status: {Status}, RepositoryIds: {RepositoryIds}, Limit: {Limit}, Offset: {Offset}", 
                userId, status, repositoryIds?.Count ?? 0, limit, offset);

            Result<IReadOnlyList<Conversation>> result;
            
            if (repositoryIds?.Any() == true)
            {
                // Filter by repository IDs and user
                result = await _conversationRepository.GetByRepositoryIdsAsync(repositoryIds, userId, limit, offset, cancellationToken);
            }
            else
            {
                // Get all conversations for user
                result = await _conversationRepository.GetByUserIdAsync(userId, status, limit, offset, cancellationToken);
            }
            if (result.IsFailure)
            {
                _logger.LogError("Failed to retrieve user conversations: {Error}", result.Error);
                return Result<IReadOnlyList<ConversationSummaryDto>>.Failure($"Failed to retrieve conversations: {result.Error}");
            }

            var conversations = result.Value!;
            var summaryDtos = conversations.Select(MapToSummaryDto).ToList().AsReadOnly();

            _logger.LogInformation("Successfully retrieved {Count} conversations for user: {UserId}", 
                summaryDtos.Count, userId);

            return Result<IReadOnlyList<ConversationSummaryDto>>.Success(summaryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations for user: {UserId}", userId);
            return Result<IReadOnlyList<ConversationSummaryDto>>.Failure($"Failed to retrieve conversations: {ex.Message}");
        }
    }

    public async Task<Result<IReadOnlyList<ConversationSummaryDto>>> SearchConversationsAsync(
        ConversationSearchInput input,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching conversations for user: {UserId} with term: {SearchTerm}", 
                userId, input.SearchTerm);

            var result = await _conversationRepository.SearchAsync(
                input.SearchTerm, 
                userId, 
                input.Status, 
                input.Limit, 
                input.Offset, 
                cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to search conversations: {Error}", result.Error);
                return Result<IReadOnlyList<ConversationSummaryDto>>.Failure($"Failed to search conversations: {result.Error}");
            }

            var conversations = result.Value!;
            var summaryDtos = conversations.Select(MapToSummaryDto).ToList().AsReadOnly();

            _logger.LogInformation("Successfully found {Count} conversations matching search: {SearchTerm}", 
                summaryDtos.Count, input.SearchTerm);

            return Result<IReadOnlyList<ConversationSummaryDto>>.Success(summaryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching conversations for user: {UserId}", userId);
            return Result<IReadOnlyList<ConversationSummaryDto>>.Failure($"Failed to search conversations: {ex.Message}");
        }
    }

    private static ConversationDto MapToDto(Conversation conversation)
    {
        return new ConversationDto
        {
            Id = conversation.Id,
            UserId = conversation.UserId,
            Title = conversation.Title,
            Status = conversation.Status,
            Messages = conversation.Messages.Select(MapMessageToDto).ToList(),
            Context = MapContextToDto(conversation.Context),
            CreatedAt = conversation.CreatedAt,
            LastActivityAt = conversation.LastActivityAt,
            Metadata = MapMetadataToDto(conversation.Metadata)
        };
    }

    private static ConversationSummaryDto MapToSummaryDto(Conversation conversation)
    {
        return new ConversationSummaryDto
        {
            Id = conversation.Id,
            Title = conversation.Title,
            Status = conversation.Status,
            MessageCount = conversation.GetMessageCount(),
            CreatedAt = conversation.CreatedAt,
            LastActivityAt = conversation.LastActivityAt,
            LastMessageAt = conversation.LastActivityAt, // Use LastActivityAt as proxy for last message
            RepositoryId = conversation.Context.RepositoryIds.FirstOrDefault(),
            RepositoryName = conversation.Context.RepositoryNames.FirstOrDefault(),
            RepositoryNames = conversation.Context.RepositoryNames.ToList(),
            Domain = conversation.Context.Domain,
            Duration = conversation.GetConversationDuration(),
            Metadata = MapMetadataToDto(conversation.Metadata)
        };
    }

    private static ConversationMessageDto MapMessageToDto(ConversationMessage message)
    {
        return new ConversationMessageDto
        {
            Id = message.Id,
            Type = message.Type,
            Content = message.Content,
            Timestamp = message.Timestamp,
            Attachments = message.Attachments.Select(MapAttachmentToDto).ToList(),
            Metadata = MapMessageMetadataToDto(message.Metadata),
            ParentMessageId = message.ParentMessageId
        };
    }

    private static MessageAttachmentDto MapAttachmentToDto(MessageAttachment attachment)
    {
        return new MessageAttachmentDto
        {
            Id = attachment.Id,
            Type = attachment.Type,
            Content = attachment.Content,
            Title = attachment.Title,
            Properties = attachment.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    private static ConversationContextDto MapContextToDto(ConversationContext context)
    {
        return new ConversationContextDto
        {
            RepositoryIds = context.RepositoryIds.ToList(),
            RepositoryNames = context.RepositoryNames.ToList(),
            Domain = context.Domain,
            TechnicalTags = context.TechnicalTags.ToList(),
            SessionData = context.SessionData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Preferences = MapPreferencesToDto(context.Preferences)
        };
    }

    private static ConversationPreferencesDto MapPreferencesToDto(ConversationPreferences preferences)
    {
        return new ConversationPreferencesDto
        {
            ResponseStyle = preferences.ResponseStyle,
            IncludeCodeExamples = preferences.IncludeCodeExamples,
            IncludeReferences = preferences.IncludeReferences,
            MaxResponseLength = preferences.MaxResponseLength,
            PreferredLanguages = preferences.PreferredLanguages.ToList()
        };
    }

    private static ConversationMetadataDto MapMetadataToDto(ConversationMetadata metadata)
    {
        return new ConversationMetadataDto
        {
            MessageCount = metadata.MessageCount,
            UserMessageCount = metadata.UserMessageCount,
            AIMessageCount = metadata.AIMessageCount,
            SystemMessageCount = metadata.SystemMessageCount,
            TotalAttachmentCount = metadata.TotalAttachmentCount,
            TotalWordCount = metadata.TotalWordCount,
            AverageResponseTimeSeconds = metadata.AverageResponseTimeSeconds,
            LastUserActivity = metadata.LastUserActivity,
            LastAIActivity = metadata.LastAIActivity,
            TopicFrequency = metadata.TopicFrequency.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            Tags = new List<string>(), // TODO: Extract from conversation content/context
            Summary = string.Empty, // TODO: Generate conversation summary
            Participants = new List<string> { "User", "AI" } // Default participants
        };
    }

    private static MessageMetadataDto MapMessageMetadataToDto(MessageMetadata metadata)
    {
        return new MessageMetadataDto
        {
            MessageType = metadata.MessageType,
            WordCount = metadata.WordCount,
            AttachmentCount = metadata.AttachmentCount,
            ResponseTimeSeconds = metadata.ResponseTimeSeconds,
            ConfidenceScore = metadata.ConfidenceScore,
            IsEdited = metadata.IsEdited,
            EditedAt = metadata.EditedAt,
            Topics = metadata.Topics.ToList(),
            ExtractedEntities = metadata.ExtractedEntities.ToList()
        };
    }
}