using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Events;

public record ConversationStartedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid ConversationId { get; }
    public Guid UserId { get; }
    public string Title { get; }
    public ConversationContext Context { get; }

    public ConversationStartedEvent(Guid conversationId, Guid userId, string title, ConversationContext context)
    {
        ConversationId = conversationId;
        UserId = userId;
        Title = title;
        Context = context;
    }
}