using System.ComponentModel.DataAnnotations;

namespace Archie.Infrastructure.Configuration;

public class ConversationStorageOptions
{
    public const string SectionName = "ConversationStorage";
    
    [Range(1, 365)]
    public int ConversationRetentionDays { get; set; } = 90;
    
    [Range(1, 1000)]
    public int MaxConversationsPerUser { get; set; } = 100;
    
    [Range(1, 500)]
    public int MaxMessagesPerConversation { get; set; } = 200;
    
    public bool EnableConversationBackup { get; set; } = true;
    public bool EnableMessageEncryption { get; set; } = true;
    public bool EnableAutoArchiving { get; set; } = true;
    public bool EnableCleanupJob { get; set; } = true;
    
    [Range(1, 168)] // 1 hour to 1 week in hours
    public int AutoArchiveAfterHours { get; set; } = 168; // 1 week
    
    [Range(1, 24)] // 1 to 24 hours
    public int CleanupJobIntervalHours { get; set; } = 24; // Daily
    
    [Range(1, 10000)]
    public int CleanupBatchSize { get; set; } = 100;
    
    public string IndexName { get; set; } = "conversations";
    public string MessageIndexName { get; set; } = "conversation-messages";
    
    // Azure Search specific settings
    public SearchIndexSettings SearchSettings { get; set; } = new();
}

public class SearchIndexSettings
{
    public int SearchResultsLimit { get; set; } = 50;
    public double MinimumSearchScore { get; set; } = 0.5;
    public bool EnableFuzzySearch { get; set; } = true;
    public bool EnableSemanticSearch { get; set; } = true;
    public int MaxSearchQueryLength { get; set; } = 1000;
    
    // Indexing configuration
    public bool IndexUserMessages { get; set; } = true;
    public bool IndexAIResponses { get; set; } = true;
    public bool IndexSystemMessages { get; set; } = false;
    public bool IndexAttachments { get; set; } = true;
    
    // Privacy and security settings
    public bool EnableContentMasking { get; set; } = true;
    public List<string> SensitiveFieldPatterns { get; set; } = new()
    {
        @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", // Email patterns
        @"\b(?:\d{4}[-.\s]?){3}\d{4}\b", // Credit card patterns
        @"\b\d{3}-\d{2}-\d{4}\b", // SSN patterns
        @"\bsk-[a-zA-Z0-9]{32,}\b", // API key patterns
        @"\b[A-Za-z0-9+/]{40,}={0,2}\b" // Base64 encoded secrets
    };
    
    public List<string> ExcludedFileTypes { get; set; } = new()
    {
        ".exe", ".dll", ".bin", ".zip", ".tar", ".gz",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".ico",
        ".mp3", ".mp4", ".avi", ".mov", ".wmv", ".wav"
    };
}