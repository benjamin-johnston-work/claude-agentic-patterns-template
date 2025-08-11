using System.ComponentModel.DataAnnotations;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.DTOs;

// Input DTOs for conversation operations
public record StartConversationInput
{
    [Required(ErrorMessage = "At least one repository ID is required")]
    [MinLength(1, ErrorMessage = "At least one repository must be specified")]
    public List<Guid> RepositoryIds { get; init; } = new();

    public string? Title { get; init; }

    public string? Domain { get; init; }

    public ConversationPreferencesInput? Preferences { get; init; }

    public List<string> TechnicalTags { get; init; } = new();

    public Dictionary<string, object> SessionData { get; init; } = new();
}

public record ConversationPreferencesInput
{
    public ResponseStyle ResponseStyle { get; init; } = ResponseStyle.Balanced;

    public bool IncludeCodeExamples { get; init; } = true;

    public bool IncludeReferences { get; init; } = true;

    [Range(100, 5000, ErrorMessage = "Max response length must be between 100 and 5000 characters")]
    public int MaxResponseLength { get; init; } = 2000;

    public List<string> PreferredLanguages { get; init; } = new();
}

public record QueryInput
{
    [Required(ErrorMessage = "Conversation ID is required")]
    public Guid ConversationId { get; init; }

    [Required(ErrorMessage = "Query is required")]
    [MinLength(3, ErrorMessage = "Query must be at least 3 characters long")]
    [MaxLength(2000, ErrorMessage = "Query cannot exceed 2000 characters")]
    public string Query { get; init; } = string.Empty;

    public bool IncludeContext { get; init; } = true;

    [Range(1, 20, ErrorMessage = "Max context items must be between 1 and 20")]
    public int MaxContextItems { get; init; } = 10;

    public Guid? ParentMessageId { get; init; }
}

public record UpdateConversationInput
{
    [Required(ErrorMessage = "Conversation ID is required")]
    public Guid ConversationId { get; init; }

    public string? Title { get; init; }

    public string? Domain { get; init; }

    public ConversationPreferencesInput? Preferences { get; init; }

    public List<string> TechnicalTags { get; init; } = new();
}

public record ConversationSearchInput
{
    [Required(ErrorMessage = "Search term is required")]
    [MinLength(2, ErrorMessage = "Search term must be at least 2 characters")]
    public string SearchTerm { get; init; } = string.Empty;

    public ConversationStatus? Status { get; init; }

    public List<Guid> RepositoryIds { get; init; } = new();

    [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
    public int Limit { get; init; } = 20;

    [Range(0, int.MaxValue, ErrorMessage = "Offset cannot be negative")]
    public int Offset { get; init; } = 0;
}

// Output DTOs for conversation data
public record ConversationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public ConversationStatus Status { get; init; }
    public List<ConversationMessageDto> Messages { get; init; } = new();
    public ConversationContextDto Context { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime LastActivityAt { get; init; }
    public ConversationMetadataDto Metadata { get; init; } = new();
}

public record ConversationSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public ConversationStatus Status { get; init; }
    public int MessageCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastActivityAt { get; init; }
    public DateTime LastMessageAt { get; init; }
    public Guid? RepositoryId { get; init; }
    public string? RepositoryName { get; init; }
    public List<string> RepositoryNames { get; init; } = new();
    public string Domain { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public ConversationMetadataDto Metadata { get; init; } = new();
}

public record ConversationMessageDto
{
    public Guid Id { get; init; }
    public MessageType Type { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public List<MessageAttachmentDto> Attachments { get; init; } = new();
    public MessageMetadataDto Metadata { get; init; } = new();
    public Guid? ParentMessageId { get; init; }
}

public record MessageAttachmentDto
{
    public Guid Id { get; init; }
    public AttachmentType Type { get; init; }
    public string Content { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public Dictionary<string, object> Properties { get; init; } = new();
}

public record ConversationContextDto
{
    public List<Guid> RepositoryIds { get; init; } = new();
    public List<string> RepositoryNames { get; init; } = new();
    public string Domain { get; init; } = string.Empty;
    public List<string> TechnicalTags { get; init; } = new();
    public Dictionary<string, object> SessionData { get; init; } = new();
    public ConversationPreferencesDto Preferences { get; init; } = new();
}

public record ConversationPreferencesDto
{
    public ResponseStyle ResponseStyle { get; init; }
    public bool IncludeCodeExamples { get; init; }
    public bool IncludeReferences { get; init; }
    public int MaxResponseLength { get; init; }
    public List<string> PreferredLanguages { get; init; } = new();
}

public record ConversationMetadataDto
{
    public int MessageCount { get; init; }
    public int UserMessageCount { get; init; }
    public int AIMessageCount { get; init; }
    public int SystemMessageCount { get; init; }
    public int TotalAttachmentCount { get; init; }
    public int TotalWordCount { get; init; }
    public double AverageResponseTimeSeconds { get; init; }
    public DateTime? LastUserActivity { get; init; }
    public DateTime? LastAIActivity { get; init; }
    public Dictionary<string, int> TopicFrequency { get; init; } = new();
    public List<string> Tags { get; init; } = new();
    public string Summary { get; init; } = string.Empty;
    public List<string> Participants { get; init; } = new();
}

public record MessageMetadataDto
{
    public MessageType MessageType { get; init; }
    public int WordCount { get; init; }
    public int AttachmentCount { get; init; }
    public double? ResponseTimeSeconds { get; init; }
    public double? ConfidenceScore { get; init; }
    public bool IsEdited { get; init; }
    public DateTime? EditedAt { get; init; }
    public List<string> Topics { get; init; } = new();
    public List<string> ExtractedEntities { get; init; } = new();
}

public record QueryResponseDto
{
    public Guid Id { get; init; }
    public string Query { get; init; } = string.Empty;
    public string Response { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public double ResponseTimeSeconds { get; init; }
    public List<MessageAttachmentDto> Attachments { get; init; } = new();
    public List<string> SuggestedFollowUps { get; init; } = new();
    public List<string> RelatedQueries { get; init; } = new();
    public QueryIntentDto? Intent { get; init; }
}

public record QueryIntentDto
{
    public IntentType Type { get; init; }
    public string Domain { get; init; } = string.Empty;
    public List<string> Entities { get; init; } = new();
    public double Confidence { get; init; }
    public Dictionary<string, object> Parameters { get; init; } = new();
}

public record ConversationStatisticsDto
{
    public int TotalConversations { get; init; }
    public int ActiveConversations { get; init; }
    public int ArchivedConversations { get; init; }
    public int DeletedConversations { get; init; }
    public int TotalMessages { get; init; }
    public int TotalUserMessages { get; init; }
    public int TotalAIMessages { get; init; }
    public double AverageMessagesPerConversation { get; init; }
    public double AverageResponseTimeSeconds { get; init; }
    public TimeSpan AverageConversationDuration { get; init; }
    public Dictionary<string, int> TopDomains { get; init; } = new();
    public Dictionary<string, int> TopIntentTypes { get; init; } = new();
    public Dictionary<Guid, int> ConversationsByRepository { get; init; } = new();
    public DateTime? LastActivityAt { get; init; }
    public DateTime GeneratedAt { get; init; }
}