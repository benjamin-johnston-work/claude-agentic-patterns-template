namespace Archie.Domain.ValueObjects;

public record ConversationMetadata
{
    public int MessageCount { get; }
    public int UserMessageCount { get; }
    public int AIMessageCount { get; }
    public int SystemMessageCount { get; }
    public int TotalAttachmentCount { get; }
    public int TotalWordCount { get; }
    public double AverageResponseTimeSeconds { get; }
    public DateTime? LastUserActivity { get; }
    public DateTime? LastAIActivity { get; }
    public IReadOnlyDictionary<string, int> TopicFrequency { get; }
    public IReadOnlyDictionary<string, object> AdditionalMetadata { get; }

    public ConversationMetadata(
        int messageCount,
        int userMessageCount,
        int aiMessageCount,
        int systemMessageCount,
        int totalAttachmentCount,
        int totalWordCount,
        double averageResponseTimeSeconds,
        DateTime? lastUserActivity,
        DateTime? lastAIActivity,
        IReadOnlyDictionary<string, int> topicFrequency,
        IReadOnlyDictionary<string, object> additionalMetadata)
    {
        if (messageCount < 0)
            throw new ArgumentException("Message count cannot be negative", nameof(messageCount));
        if (userMessageCount < 0)
            throw new ArgumentException("User message count cannot be negative", nameof(userMessageCount));
        if (aiMessageCount < 0)
            throw new ArgumentException("AI message count cannot be negative", nameof(aiMessageCount));
        if (systemMessageCount < 0)
            throw new ArgumentException("System message count cannot be negative", nameof(systemMessageCount));
        if (totalAttachmentCount < 0)
            throw new ArgumentException("Total attachment count cannot be negative", nameof(totalAttachmentCount));
        if (totalWordCount < 0)
            throw new ArgumentException("Total word count cannot be negative", nameof(totalWordCount));
        if (averageResponseTimeSeconds < 0)
            throw new ArgumentException("Average response time cannot be negative", nameof(averageResponseTimeSeconds));

        MessageCount = messageCount;
        UserMessageCount = userMessageCount;
        AIMessageCount = aiMessageCount;
        SystemMessageCount = systemMessageCount;
        TotalAttachmentCount = totalAttachmentCount;
        TotalWordCount = totalWordCount;
        AverageResponseTimeSeconds = averageResponseTimeSeconds;
        LastUserActivity = lastUserActivity;
        LastAIActivity = lastAIActivity;
        TopicFrequency = topicFrequency ?? new Dictionary<string, int>();
        AdditionalMetadata = additionalMetadata ?? new Dictionary<string, object>();
    }

    public static ConversationMetadata Empty => new(
        0, 0, 0, 0, 0, 0, 0.0, null, null,
        new Dictionary<string, int>(),
        new Dictionary<string, object>());

    public static ConversationMetadata Create(
        int messageCount = 0,
        int userMessageCount = 0,
        int aiMessageCount = 0,
        int systemMessageCount = 0,
        int totalAttachmentCount = 0,
        int totalWordCount = 0,
        double averageResponseTimeSeconds = 0.0,
        DateTime? lastUserActivity = null,
        DateTime? lastAIActivity = null,
        Dictionary<string, int>? topicFrequency = null,
        Dictionary<string, object>? additionalMetadata = null)
    {
        return new ConversationMetadata(
            messageCount,
            userMessageCount,
            aiMessageCount,
            systemMessageCount,
            totalAttachmentCount,
            totalWordCount,
            averageResponseTimeSeconds,
            lastUserActivity,
            lastAIActivity,
            topicFrequency ?? new Dictionary<string, int>(),
            additionalMetadata ?? new Dictionary<string, object>());
    }

    public ConversationMetadata IncrementMessageCount()
    {
        return new ConversationMetadata(
            MessageCount + 1,
            UserMessageCount,
            AIMessageCount,
            SystemMessageCount,
            TotalAttachmentCount,
            TotalWordCount,
            AverageResponseTimeSeconds,
            LastUserActivity,
            LastAIActivity,
            TopicFrequency,
            AdditionalMetadata);
    }

    public ConversationMetadata IncrementUserMessageCount(DateTime? activityTime = null)
    {
        return new ConversationMetadata(
            MessageCount + 1,
            UserMessageCount + 1,
            AIMessageCount,
            SystemMessageCount,
            TotalAttachmentCount,
            TotalWordCount,
            AverageResponseTimeSeconds,
            activityTime ?? DateTime.UtcNow,
            LastAIActivity,
            TopicFrequency,
            AdditionalMetadata);
    }

    public ConversationMetadata IncrementAIMessageCount(DateTime? activityTime = null)
    {
        return new ConversationMetadata(
            MessageCount + 1,
            UserMessageCount,
            AIMessageCount + 1,
            SystemMessageCount,
            TotalAttachmentCount,
            TotalWordCount,
            AverageResponseTimeSeconds,
            LastUserActivity,
            activityTime ?? DateTime.UtcNow,
            TopicFrequency,
            AdditionalMetadata);
    }

    public ConversationMetadata IncrementSystemMessageCount()
    {
        return new ConversationMetadata(
            MessageCount + 1,
            UserMessageCount,
            AIMessageCount,
            SystemMessageCount + 1,
            TotalAttachmentCount,
            TotalWordCount,
            AverageResponseTimeSeconds,
            LastUserActivity,
            LastAIActivity,
            TopicFrequency,
            AdditionalMetadata);
    }

    public ConversationMetadata AddAttachmentCount(int count)
    {
        return new ConversationMetadata(
            MessageCount,
            UserMessageCount,
            AIMessageCount,
            SystemMessageCount,
            TotalAttachmentCount + Math.Max(0, count),
            TotalWordCount,
            AverageResponseTimeSeconds,
            LastUserActivity,
            LastAIActivity,
            TopicFrequency,
            AdditionalMetadata);
    }

    public ConversationMetadata AddWordCount(int count)
    {
        return new ConversationMetadata(
            MessageCount,
            UserMessageCount,
            AIMessageCount,
            SystemMessageCount,
            TotalAttachmentCount,
            TotalWordCount + Math.Max(0, count),
            AverageResponseTimeSeconds,
            LastUserActivity,
            LastAIActivity,
            TopicFrequency,
            AdditionalMetadata);
    }

    public ConversationMetadata UpdateAverageResponseTime(double newResponseTime)
    {
        if (AIMessageCount == 0)
            return this;

        var updatedAverage = ((AverageResponseTimeSeconds * (AIMessageCount - 1)) + newResponseTime) / AIMessageCount;

        return new ConversationMetadata(
            MessageCount,
            UserMessageCount,
            AIMessageCount,
            SystemMessageCount,
            TotalAttachmentCount,
            TotalWordCount,
            updatedAverage,
            LastUserActivity,
            LastAIActivity,
            TopicFrequency,
            AdditionalMetadata);
    }

    public ConversationMetadata AddTopicFrequency(string topic, int frequency = 1)
    {
        if (string.IsNullOrWhiteSpace(topic) || frequency <= 0)
            return this;

        var newTopicFrequency = new Dictionary<string, int>(TopicFrequency);
        newTopicFrequency[topic] = newTopicFrequency.GetValueOrDefault(topic, 0) + frequency;

        return new ConversationMetadata(
            MessageCount,
            UserMessageCount,
            AIMessageCount,
            SystemMessageCount,
            TotalAttachmentCount,
            TotalWordCount,
            AverageResponseTimeSeconds,
            LastUserActivity,
            LastAIActivity,
            newTopicFrequency,
            AdditionalMetadata);
    }

    public ConversationMetadata AddMetadata(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return this;

        var newMetadata = new Dictionary<string, object>(AdditionalMetadata)
        {
            [key] = value
        };

        return new ConversationMetadata(
            MessageCount,
            UserMessageCount,
            AIMessageCount,
            SystemMessageCount,
            TotalAttachmentCount,
            TotalWordCount,
            AverageResponseTimeSeconds,
            LastUserActivity,
            LastAIActivity,
            TopicFrequency,
            newMetadata);
    }

    public bool HasActivity() => MessageCount > 0;

    public bool HasUserActivity() => UserMessageCount > 0 && LastUserActivity.HasValue;

    public bool HasAIActivity() => AIMessageCount > 0 && LastAIActivity.HasValue;

    public double GetUserEngagementRatio()
    {
        if (MessageCount == 0) return 0.0;
        return (double)UserMessageCount / MessageCount;
    }

    public double GetAverageWordsPerMessage()
    {
        if (MessageCount == 0) return 0.0;
        return (double)TotalWordCount / MessageCount;
    }

    public TimeSpan? GetTimeSinceLastUserActivity()
    {
        if (!LastUserActivity.HasValue) return null;
        return DateTime.UtcNow - LastUserActivity.Value;
    }

    public TimeSpan? GetTimeSinceLastAIActivity()
    {
        if (!LastAIActivity.HasValue) return null;
        return DateTime.UtcNow - LastAIActivity.Value;
    }

    public IReadOnlyList<string> GetTopTopics(int count = 5)
    {
        return TopicFrequency
            .OrderByDescending(kvp => kvp.Value)
            .Take(count)
            .Select(kvp => kvp.Key)
            .ToList()
            .AsReadOnly();
    }

    public T? GetMetadata<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !AdditionalMetadata.ContainsKey(key))
            return default;

        try
        {
            return (T)AdditionalMetadata[key];
        }
        catch
        {
            return default;
        }
    }
}