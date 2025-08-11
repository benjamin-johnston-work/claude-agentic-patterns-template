using Archie.Domain.Common;

namespace Archie.Domain.Events;

public record ConversationArchivedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid ConversationId { get; }
    public Guid UserId { get; }
    public string Title { get; }
    public int MessageCount { get; }
    public TimeSpan ConversationDuration { get; }

    public ConversationArchivedEvent(
        Guid conversationId, 
        Guid userId, 
        string title, 
        int messageCount,
        TimeSpan conversationDuration)
    {
        ConversationId = conversationId;
        UserId = userId;
        Title = title;
        MessageCount = messageCount;
        ConversationDuration = conversationDuration;
    }
}