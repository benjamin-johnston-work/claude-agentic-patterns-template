using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Entities;

public class ConversationMessage : BaseEntity
{
    private readonly List<MessageAttachment> _attachments = new();

    public Guid ConversationId { get; private set; }
    public MessageType Type { get; private set; }
    public string Content { get; private set; }
    public DateTime Timestamp { get; private set; }
    public IReadOnlyList<MessageAttachment> Attachments => _attachments.AsReadOnly();
    public MessageMetadata Metadata { get; private set; }
    public Guid? ParentMessageId { get; private set; } // For threaded conversations

    protected ConversationMessage() // EF Constructor
    {
        Content = string.Empty;
        Metadata = MessageMetadata.Empty;
    }

    private ConversationMessage(Guid conversationId, MessageType type, string content, Guid? parentMessageId = null)
    {
        if (conversationId == Guid.Empty)
            throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));

        ConversationId = conversationId;
        Type = type;
        Content = content;
        Timestamp = DateTime.UtcNow;
        ParentMessageId = parentMessageId;
        Metadata = MessageMetadata.Create(type);
    }

    public static ConversationMessage CreateUserQuery(Guid conversationId, string content, Guid? parentMessageId = null)
    {
        return new ConversationMessage(conversationId, MessageType.UserQuery, content, parentMessageId);
    }

    public static ConversationMessage CreateAIResponse(Guid conversationId, string content, List<CodeReference>? references = null, Guid? parentMessageId = null)
    {
        var message = new ConversationMessage(conversationId, MessageType.AIResponse, content, parentMessageId);
        
        if (references?.Any() == true)
        {
            foreach (var reference in references)
            {
                var attachment = MessageAttachment.CreateCodeReference(
                    reference.FilePath,
                    reference.CodeSnippet,
                    reference.StartLine ?? 1);
                message.AddAttachment(attachment);
            }
        }

        return message;
    }

    public static ConversationMessage CreateSystemMessage(Guid conversationId, string content, Guid? parentMessageId = null)
    {
        return new ConversationMessage(conversationId, MessageType.SystemMessage, content, parentMessageId);
    }

    public static ConversationMessage CreateCodeReference(Guid conversationId, string filePath, string code, int lineNumber, Guid? parentMessageId = null)
    {
        var message = new ConversationMessage(conversationId, MessageType.CodeReference, code, parentMessageId);
        var attachment = MessageAttachment.CreateCodeReference(filePath, code, lineNumber);
        message.AddAttachment(attachment);
        
        return message;
    }

    public static ConversationMessage CreateSearchResult(Guid conversationId, string query, List<object> results, Guid? parentMessageId = null)
    {
        var content = $"Search results for: {query}";
        var message = new ConversationMessage(conversationId, MessageType.SearchResult, content, parentMessageId);
        var attachment = MessageAttachment.CreateSearchResult(query, results);
        message.AddAttachment(attachment);
        
        return message;
    }

    public void AddAttachment(MessageAttachment attachment)
    {
        if (attachment == null)
            throw new ArgumentNullException(nameof(attachment));

        if (attachment.MessageId != Id)
            throw new ArgumentException("Attachment does not belong to this message");

        _attachments.Add(attachment);
        Metadata = Metadata.IncrementAttachmentCount();
    }

    public void RemoveAttachment(Guid attachmentId)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId);
        if (attachment == null)
            return;

        _attachments.Remove(attachment);
        Metadata = Metadata.DecrementAttachmentCount();
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));

        Content = content;
        Metadata = Metadata.MarkAsEdited();
    }

    public bool IsUserMessage() => Type == MessageType.UserQuery;
    
    public bool IsAIResponse() => Type == MessageType.AIResponse;
    
    public bool IsSystemMessage() => Type == MessageType.SystemMessage;
    
    public bool HasAttachments() => _attachments.Any();
    
    public bool IsThreadReply() => ParentMessageId.HasValue;

    public List<MessageAttachment> GetCodeReferences()
    {
        return _attachments
            .Where(a => a.Type == AttachmentType.CodeReference)
            .ToList();
    }

    public List<MessageAttachment> GetDocumentationReferences()
    {
        return _attachments
            .Where(a => a.Type == AttachmentType.DocumentationReference)
            .ToList();
    }

    public int GetWordCount()
    {
        return Content?.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length ?? 0;
    }

    public TimeSpan GetAge()
    {
        return DateTime.UtcNow - Timestamp;
    }
}

public enum MessageType
{
    UserQuery,
    AIResponse,
    SystemMessage,
    CodeReference,
    SearchResult
}