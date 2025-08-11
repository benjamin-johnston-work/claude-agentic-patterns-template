using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class ProcessQueryUseCase
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IConversationalAIService _aiService;
    private readonly IConversationContextService _contextService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ProcessQueryUseCase> _logger;

    public ProcessQueryUseCase(
        IConversationRepository conversationRepository,
        IConversationalAIService aiService,
        IConversationContextService contextService,
        IEventPublisher eventPublisher,
        ILogger<ProcessQueryUseCase> logger)
    {
        _conversationRepository = conversationRepository ?? throw new ArgumentNullException(nameof(conversationRepository));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _contextService = contextService ?? throw new ArgumentNullException(nameof(contextService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<QueryResponseDto>> ExecuteAsync(
        QueryInput input, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Processing query for conversation: {ConversationId}, User: {UserId}", 
                input.ConversationId, userId);

            // Retrieve conversation
            var conversationResult = await _conversationRepository.GetByIdAsync(input.ConversationId, cancellationToken);
            if (conversationResult.IsFailure)
            {
                _logger.LogError("Failed to retrieve conversation: {Error}", conversationResult.Error);
                return Result<QueryResponseDto>.Failure($"Failed to retrieve conversation: {conversationResult.Error}");
            }

            var conversation = conversationResult.Value;
            if (conversation == null)
            {
                _logger.LogWarning("Conversation not found: {ConversationId}", input.ConversationId);
                return Result<QueryResponseDto>.Failure("Conversation not found");
            }

            // Verify user ownership
            if (conversation.UserId != userId)
            {
                _logger.LogWarning("Unauthorized access to conversation: {ConversationId} by user: {UserId}", 
                    input.ConversationId, userId);
                return Result<QueryResponseDto>.Failure("Unauthorized access to conversation");
            }

            // Check if conversation is active
            if (!conversation.CanAddMessages())
            {
                _logger.LogWarning("Cannot add messages to inactive conversation: {ConversationId}, Status: {Status}", 
                    input.ConversationId, conversation.Status);
                return Result<QueryResponseDto>.Failure("Cannot add messages to inactive conversation");
            }

            try
            {
                // Phase 1: Analyze query intent
                _logger.LogDebug("Analyzing query intent");
                var intent = await _aiService.AnalyzeQueryIntentAsync(
                    input.Query, 
                    conversation.Context, 
                    cancellationToken);

                // Phase 2: Retrieve relevant context if enabled
                var contextResults = new List<SearchResult>();
                if (input.IncludeContext && conversation.HasActiveContext())
                {
                    _logger.LogDebug("Retrieving relevant context");
                    contextResults = (await _contextService.RetrieveRelevantContextAsync(
                        input.Query, 
                        conversation.Context, 
                        input.MaxContextItems, 
                        cancellationToken)).ToList();
                }

                // Phase 3: Get recent message history for context
                var messageHistory = conversation.GetRecentMessages(10);

                // Phase 4: Process query with AI service
                _logger.LogDebug("Processing query with AI service");
                var aiResponse = await _aiService.ProcessQueryAsync(
                    input.Query,
                    conversation.Context,
                    messageHistory,
                    cancellationToken);

                // Phase 5: Generate follow-up questions
                _logger.LogDebug("Generating follow-up questions");
                var followUps = await _aiService.GenerateFollowUpQuestionsAsync(
                    input.Query,
                    aiResponse.Response,
                    conversation.Context,
                    3,
                    cancellationToken);

                // Phase 6: Create and add messages to conversation
                var userMessage = ConversationMessage.CreateUserQuery(
                    conversation.Id, 
                    input.Query, 
                    input.ParentMessageId);
                
                // Calculate response time
                var responseTime = (DateTime.UtcNow - startTime).TotalSeconds;

                var aiMessage = ConversationMessage.CreateAIResponse(
                    conversation.Id,
                    aiResponse.Response,
                    null, // Code references handled via attachments
                    userMessage.Id);

                // Update message metadata
                var userMessageMetadata = userMessage.Metadata
                    .WithWordCount(input.Query.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

                var aiMessageMetadata = aiMessage.Metadata
                    .WithResponseTime(responseTime)
                    .WithConfidenceScore(aiResponse.Confidence)
                    .WithWordCount(aiResponse.Response.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

                // Add messages to conversation
                conversation.AddMessage(userMessage);
                conversation.AddMessage(aiMessage);

                // Save updated conversation
                var saveResult = await _conversationRepository.SaveAsync(conversation, cancellationToken);
                if (saveResult.IsFailure)
                {
                    _logger.LogError("Failed to save conversation after query processing: {Error}", saveResult.Error);
                    return Result<QueryResponseDto>.Failure($"Failed to save conversation: {saveResult.Error}");
                }

                // Publish success event
                var queryProcessedEvent = new QueryProcessedEvent(
                    conversation.Id,
                    aiMessage.Id,
                    input.Query,
                    aiResponse.Response,
                    intent,
                    responseTime,
                    aiResponse.Confidence,
                    aiResponse.Attachments.Count);

                await _eventPublisher.PublishAsync(queryProcessedEvent, cancellationToken);

                // Build response DTO
                var response = new QueryResponseDto
                {
                    Id = aiMessage.Id,
                    Query = input.Query,
                    Response = aiResponse.Response,
                    Confidence = aiResponse.Confidence,
                    ResponseTimeSeconds = responseTime,
                    Attachments = aiResponse.Attachments.Select(MapAttachmentToDto).ToList(),
                    SuggestedFollowUps = followUps.ToList(),
                    RelatedQueries = aiResponse.RelatedQueries.ToList(),
                    Intent = MapIntentToDto(intent)
                };

                _logger.LogInformation("Successfully processed query for conversation: {ConversationId}, Response time: {ResponseTime}s", 
                    input.ConversationId, responseTime);

                return Result<QueryResponseDto>.Success(response);
            }
            catch (Exception processingEx)
            {
                var processingTime = (DateTime.UtcNow - startTime).TotalSeconds;

                // Publish failure event
                var intent = await _aiService.AnalyzeQueryIntentAsync(input.Query, conversation.Context, cancellationToken)
                    .ConfigureAwait(false);

                var queryFailedEvent = new QueryProcessingFailedEvent(
                    conversation.Id,
                    input.Query,
                    intent,
                    processingEx.Message,
                    processingEx.GetType().Name,
                    processingTime);

                await _eventPublisher.PublishAsync(queryFailedEvent, cancellationToken);

                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query for conversation: {ConversationId}", input.ConversationId);
            return Result<QueryResponseDto>.Failure($"Failed to process query: {ex.Message}");
        }
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

    private static QueryIntentDto MapIntentToDto(Domain.ValueObjects.QueryIntent intent)
    {
        return new QueryIntentDto
        {
            Type = intent.Type,
            Domain = intent.Domain,
            Entities = intent.Entities.ToList(),
            Confidence = intent.Confidence,
            Parameters = intent.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }
}