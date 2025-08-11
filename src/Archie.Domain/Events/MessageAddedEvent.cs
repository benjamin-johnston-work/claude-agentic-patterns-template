using Archie.Domain.Common;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Events;

public record MessageAddedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid ConversationId { get; }
    public Guid MessageId { get; }
    public MessageType MessageType { get; }
    public string Content { get; }
    public int WordCount { get; }
    public int AttachmentCount { get; }
    public Guid? ParentMessageId { get; }

    public MessageAddedEvent(
        Guid conversationId,
        Guid messageId,
        MessageType messageType,
        string content,
        int wordCount,
        int attachmentCount,
        Guid? parentMessageId = null)
    {
        ConversationId = conversationId;
        MessageId = messageId;
        MessageType = messageType;
        Content = content;
        WordCount = wordCount;
        AttachmentCount = attachmentCount;
        ParentMessageId = parentMessageId;
    }
}