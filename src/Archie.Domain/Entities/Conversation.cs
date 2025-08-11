using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Entities;

public class Conversation : BaseEntity, IAggregateRoot
{
    private readonly List<ConversationMessage> _messages = new();

    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public ConversationStatus Status { get; private set; }
    public IReadOnlyList<ConversationMessage> Messages => _messages.AsReadOnly();
    public ConversationContext Context { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public ConversationMetadata Metadata { get; private set; }

    protected Conversation() // EF Constructor
    {
        Title = string.Empty;
        Context = ConversationContext.Empty;
        Metadata = ConversationMetadata.Empty;
    }

    private Conversation(Guid userId, string title, ConversationContext context)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        UserId = userId;
        Title = title;
        Context = context;
        Status = ConversationStatus.Active;
        CreatedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        Metadata = ConversationMetadata.Create();
    }

    public static Conversation Create(Guid userId, string title, ConversationContext context)
    {
        return new Conversation(userId, title, context);
    }

    public void AddMessage(ConversationMessage message)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        if (Status != ConversationStatus.Active)
            throw new InvalidOperationException("Cannot add messages to inactive conversation");

        if (message.ConversationId != Id)
            throw new ArgumentException("Message does not belong to this conversation");

        _messages.Add(message);
        UpdateLastActivity();
        Metadata = Metadata.IncrementMessageCount();
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        Title = title;
        UpdateLastActivity();
    }

    public void UpdateContext(ConversationContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        UpdateLastActivity();
    }

    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status == ConversationStatus.Archived)
            return;

        Status = ConversationStatus.Archived;
        UpdateLastActivity();
    }

    public void Delete()
    {
        Status = ConversationStatus.Deleted;
        UpdateLastActivity();
    }

    public void Restore()
    {
        if (Status != ConversationStatus.Archived)
            throw new InvalidOperationException("Only archived conversations can be restored");

        Status = ConversationStatus.Active;
        UpdateLastActivity();
    }

    public bool IsActive() => Status == ConversationStatus.Active;

    public bool CanAddMessages() => Status == ConversationStatus.Active;

    public ConversationMessage? GetLastUserMessage()
    {
        return _messages
            .Where(m => m.Type == MessageType.UserQuery)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefault();
    }

    public ConversationMessage? GetLastAIMessage()
    {
        return _messages
            .Where(m => m.Type == MessageType.AIResponse)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefault();
    }

    public List<ConversationMessage> GetRecentMessages(int count = 10)
    {
        return _messages
            .OrderByDescending(m => m.Timestamp)
            .Take(count)
            .OrderBy(m => m.Timestamp)
            .ToList();
    }

    public int GetMessageCount() => _messages.Count;

    public TimeSpan GetConversationDuration()
    {
        if (!_messages.Any())
            return TimeSpan.Zero;

        var firstMessage = _messages.OrderBy(m => m.Timestamp).First();
        var lastMessage = _messages.OrderByDescending(m => m.Timestamp).First();
        
        return lastMessage.Timestamp - firstMessage.Timestamp;
    }

    public bool HasActiveContext() => Context.RepositoryIds.Any();
}

public enum ConversationStatus
{
    Active,
    Archived,
    Deleted
}