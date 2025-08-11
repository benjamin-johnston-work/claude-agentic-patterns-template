using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Events;

public record QueryProcessingFailedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid ConversationId { get; }
    public string Query { get; }
    public QueryIntent? Intent { get; }
    public string ErrorMessage { get; }
    public string ErrorType { get; }
    public double ProcessingTimeSeconds { get; }

    public QueryProcessingFailedEvent(
        Guid conversationId,
        string query,
        QueryIntent? intent,
        string errorMessage,
        string errorType,
        double processingTimeSeconds)
    {
        ConversationId = conversationId;
        Query = query;
        Intent = intent;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
        ProcessingTimeSeconds = processingTimeSeconds;
    }
}