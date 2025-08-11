using Archie.Domain.Common;

namespace Archie.Domain.Events;

public record ConversationDeletedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid ConversationId { get; }
    public Guid UserId { get; }
    public string Title { get; }
    public string Reason { get; }

    public ConversationDeletedEvent(Guid conversationId, Guid userId, string title, string reason = "User requested")
    {
        ConversationId = conversationId;
        UserId = userId;
        Title = title;
        Reason = reason;
    }
}