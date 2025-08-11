using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Events;

public record QueryProcessedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid ConversationId { get; }
    public Guid MessageId { get; }
    public string Query { get; }
    public string Response { get; }
    public QueryIntent Intent { get; }
    public double ResponseTimeSeconds { get; }
    public double ConfidenceScore { get; }
    public int AttachmentCount { get; }

    public QueryProcessedEvent(
        Guid conversationId,
        Guid messageId,
        string query,
        string response,
        QueryIntent intent,
        double responseTimeSeconds,
        double confidenceScore,
        int attachmentCount)
    {
        ConversationId = conversationId;
        MessageId = messageId;
        Query = query;
        Response = response;
        Intent = intent;
        ResponseTimeSeconds = responseTimeSeconds;
        ConfidenceScore = confidenceScore;
        AttachmentCount = attachmentCount;
    }
}