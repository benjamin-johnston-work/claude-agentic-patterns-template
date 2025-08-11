using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.Events;
using Archie.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class StartConversationUseCase
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<StartConversationUseCase> _logger;

    public StartConversationUseCase(
        IConversationRepository conversationRepository,
        IRepositoryRepository repositoryRepository,
        IEventPublisher eventPublisher,
        ILogger<StartConversationUseCase> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ConversationDto>> ExecuteAsync(
        StartConversationInput input, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting conversation for user: {UserId} with repositories: {RepositoryIds}", 
                userId, string.Join(", ", input.RepositoryIds));

            // Validate repository access
            var repositoryNames = new List<string>();
            foreach (var repositoryId in input.RepositoryIds)
            {
                var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
                if (repository == null)
                {
                    _logger.LogWarning("Repository not found: {RepositoryId}", repositoryId);
                    return Result<ConversationDto>.Failure($"Repository not found: {repositoryId}");
                }

                if (!repository.IsReady())
                {
                    _logger.LogWarning("Repository not ready: {RepositoryId}, Status: {Status}", 
                        repositoryId, repository.Status);
                    return Result<ConversationDto>.Failure($"Repository {repository.Name} is not ready for conversations");
                }

                repositoryNames.Add(repository.Name);
            }

            // Create conversation preferences
            var preferences = input.Preferences != null 
                ? ConversationPreferences.Create(
                    input.Preferences.ResponseStyle,
                    input.Preferences.IncludeCodeExamples,
                    input.Preferences.IncludeReferences,
                    input.Preferences.MaxResponseLength,
                    input.Preferences.PreferredLanguages)
                : ConversationPreferences.Default;

            // Create conversation context
            var context = ConversationContext.Create(
                input.RepositoryIds,
                repositoryNames,
                input.Domain,
                input.TechnicalTags,
                input.SessionData,
                preferences);

            // Generate title if not provided
            var title = input.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                title = repositoryNames.Count == 1 
                    ? $"Conversation about {repositoryNames[0]}"
                    : $"Multi-repository conversation ({repositoryNames.Count} repos)";

                if (!string.IsNullOrWhiteSpace(input.Domain))
                {
                    title += $" - {input.Domain}";
                }
            }

            // Create conversation
            var conversation = Conversation.Create(userId, title, context);

            // Save conversation
            var saveResult = await _conversationRepository.SaveAsync(conversation, cancellationToken);
            if (saveResult.IsFailure)
            {
                _logger.LogError("Failed to save conversation: {Error}", saveResult.Error);
                return Result<ConversationDto>.Failure($"Failed to save conversation: {saveResult.Error}");
            }

            var savedConversation = saveResult.Value!;

            // Publish domain event
            var conversationStartedEvent = new ConversationStartedEvent(
                savedConversation.Id, 
                userId, 
                savedConversation.Title, 
                savedConversation.Context);
            
            await _eventPublisher.PublishAsync(conversationStartedEvent, cancellationToken);

            _logger.LogInformation("Successfully started conversation: {ConversationId}", savedConversation.Id);

            return Result<ConversationDto>.Success(MapToDto(savedConversation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting conversation for user: {UserId}", userId);
            return Result<ConversationDto>.Failure($"Failed to start conversation: {ex.Message}");
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
            TopicFrequency = metadata.TopicFrequency.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
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