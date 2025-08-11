using Archie.Domain.Entities;

namespace Archie.Domain.ValueObjects;

public record MessageMetadata
{
    public MessageType MessageType { get; }
    public int WordCount { get; }
    public int AttachmentCount { get; }
    public double? ResponseTimeSeconds { get; }
    public double? ConfidenceScore { get; }
    public bool IsEdited { get; }
    public DateTime? EditedAt { get; }
    public IReadOnlyList<string> Topics { get; }
    public IReadOnlyList<string> ExtractedEntities { get; }
    public IReadOnlyDictionary<string, object> AdditionalProperties { get; }

    public MessageMetadata(
        MessageType messageType,
        int wordCount,
        int attachmentCount,
        double? responseTimeSeconds,
        double? confidenceScore,
        bool isEdited,
        DateTime? editedAt,
        IReadOnlyList<string> topics,
        IReadOnlyList<string> extractedEntities,
        IReadOnlyDictionary<string, object> additionalProperties)
    {
        if (wordCount < 0)
            throw new ArgumentException("Word count cannot be negative", nameof(wordCount));
        if (attachmentCount < 0)
            throw new ArgumentException("Attachment count cannot be negative", nameof(attachmentCount));
        if (responseTimeSeconds.HasValue && responseTimeSeconds.Value < 0)
            throw new ArgumentException("Response time cannot be negative", nameof(responseTimeSeconds));
        if (confidenceScore.HasValue && (confidenceScore.Value < 0 || confidenceScore.Value > 1))
            throw new ArgumentException("Confidence score must be between 0 and 1", nameof(confidenceScore));

        MessageType = messageType;
        WordCount = wordCount;
        AttachmentCount = attachmentCount;
        ResponseTimeSeconds = responseTimeSeconds;
        ConfidenceScore = confidenceScore;
        IsEdited = isEdited;
        EditedAt = editedAt;
        Topics = topics ?? throw new ArgumentNullException(nameof(topics));
        ExtractedEntities = extractedEntities ?? throw new ArgumentNullException(nameof(extractedEntities));
        AdditionalProperties = additionalProperties ?? new Dictionary<string, object>();
    }

    public static MessageMetadata Empty => new(
        MessageType.SystemMessage,
        0, 0, null, null, false, null,
        Array.Empty<string>(),
        Array.Empty<string>(),
        new Dictionary<string, object>());

    public static MessageMetadata Create(
        MessageType messageType,
        int wordCount = 0,
        int attachmentCount = 0,
        double? responseTimeSeconds = null,
        double? confidenceScore = null,
        bool isEdited = false,
        DateTime? editedAt = null,
        List<string>? topics = null,
        List<string>? extractedEntities = null,
        Dictionary<string, object>? additionalProperties = null)
    {
        return new MessageMetadata(
            messageType,
            wordCount,
            attachmentCount,
            responseTimeSeconds,
            confidenceScore,
            isEdited,
            editedAt,
            topics ?? new List<string>(),
            extractedEntities ?? new List<string>(),
            additionalProperties ?? new Dictionary<string, object>());
    }

    public MessageMetadata IncrementAttachmentCount()
    {
        return new MessageMetadata(
            MessageType,
            WordCount,
            AttachmentCount + 1,
            ResponseTimeSeconds,
            ConfidenceScore,
            IsEdited,
            EditedAt,
            Topics,
            ExtractedEntities,
            AdditionalProperties);
    }

    public MessageMetadata DecrementAttachmentCount()
    {
        return new MessageMetadata(
            MessageType,
            WordCount,
            Math.Max(0, AttachmentCount - 1),
            ResponseTimeSeconds,
            ConfidenceScore,
            IsEdited,
            EditedAt,
            Topics,
            ExtractedEntities,
            AdditionalProperties);
    }

    public MessageMetadata WithWordCount(int wordCount)
    {
        return new MessageMetadata(
            MessageType,
            Math.Max(0, wordCount),
            AttachmentCount,
            ResponseTimeSeconds,
            ConfidenceScore,
            IsEdited,
            EditedAt,
            Topics,
            ExtractedEntities,
            AdditionalProperties);
    }

    public MessageMetadata WithResponseTime(double responseTimeSeconds)
    {
        return new MessageMetadata(
            MessageType,
            WordCount,
            AttachmentCount,
            Math.Max(0, responseTimeSeconds),
            ConfidenceScore,
            IsEdited,
            EditedAt,
            Topics,
            ExtractedEntities,
            AdditionalProperties);
    }

    public MessageMetadata WithConfidenceScore(double confidenceScore)
    {
        var validConfidence = Math.Max(0, Math.Min(1, confidenceScore));

        return new MessageMetadata(
            MessageType,
            WordCount,
            AttachmentCount,
            ResponseTimeSeconds,
            validConfidence,
            IsEdited,
            EditedAt,
            Topics,
            ExtractedEntities,
            AdditionalProperties);
    }

    public MessageMetadata MarkAsEdited(DateTime? editedAt = null)
    {
        return new MessageMetadata(
            MessageType,
            WordCount,
            AttachmentCount,
            ResponseTimeSeconds,
            ConfidenceScore,
            true,
            editedAt ?? DateTime.UtcNow,
            Topics,
            ExtractedEntities,
            AdditionalProperties);
    }

    public MessageMetadata WithTopics(List<string> topics)
    {
        return new MessageMetadata(
            MessageType,
            WordCount,
            AttachmentCount,
            ResponseTimeSeconds,
            ConfidenceScore,
            IsEdited,
            EditedAt,
            topics ?? new List<string>(),
            ExtractedEntities,
            AdditionalProperties);
    }

    public MessageMetadata AddTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
            return this;

        if (Topics.Contains(topic, StringComparer.OrdinalIgnoreCase))
            return this;

        var newTopics = new List<string>(Topics) { topic };
        return WithTopics(newTopics);
    }

    public MessageMetadata WithExtractedEntities(List<string> entities)
    {
        return new MessageMetadata(
            MessageType,
            WordCount,
            AttachmentCount,
            ResponseTimeSeconds,
            ConfidenceScore,
            IsEdited,
            EditedAt,
            Topics,
            entities ?? new List<string>(),
            AdditionalProperties);
    }

    public MessageMetadata AddExtractedEntity(string entity)
    {
        if (string.IsNullOrWhiteSpace(entity))
            return this;

        if (ExtractedEntities.Contains(entity, StringComparer.OrdinalIgnoreCase))
            return this;

        var newEntities = new List<string>(ExtractedEntities) { entity };
        return WithExtractedEntities(newEntities);
    }

    public MessageMetadata WithAdditionalProperties(Dictionary<string, object> properties)
    {
        return new MessageMetadata(
            MessageType,
            WordCount,
            AttachmentCount,
            ResponseTimeSeconds,
            ConfidenceScore,
            IsEdited,
            EditedAt,
            Topics,
            ExtractedEntities,
            properties ?? new Dictionary<string, object>());
    }

    public MessageMetadata AddProperty(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return this;

        var newProperties = new Dictionary<string, object>(AdditionalProperties)
        {
            [key] = value
        };

        return WithAdditionalProperties(newProperties);
    }

    public bool HasResponseTime() => ResponseTimeSeconds.HasValue;

    public bool HasConfidenceScore() => ConfidenceScore.HasValue;

    public bool HasTopics() => Topics.Any();

    public bool HasExtractedEntities() => ExtractedEntities.Any();

    public bool HasAttachments() => AttachmentCount > 0;

    public bool IsHighConfidence(double threshold = 0.8) => 
        ConfidenceScore.HasValue && ConfidenceScore.Value >= threshold;

    public bool IsLowConfidence(double threshold = 0.5) => 
        ConfidenceScore.HasValue && ConfidenceScore.Value < threshold;

    public bool IsUserGenerated() => MessageType == MessageType.UserQuery;

    public bool IsAIGenerated() => MessageType == MessageType.AIResponse;

    public bool IsSystemGenerated() => MessageType == MessageType.SystemMessage;

    public ConfidenceLevel GetConfidenceLevel()
    {
        if (!ConfidenceScore.HasValue) return ConfidenceLevel.VeryLow;

        return ConfidenceScore.Value switch
        {
            >= 0.8 => ConfidenceLevel.High,
            >= 0.5 => ConfidenceLevel.Medium,
            >= 0.2 => ConfidenceLevel.Low,
            _ => ConfidenceLevel.VeryLow
        };
    }

    public string GetQualityIndicator()
    {
        var indicators = new List<string>();

        if (HasConfidenceScore())
        {
            var level = GetConfidenceLevel();
            indicators.Add($"Confidence: {level}");
        }

        if (HasResponseTime())
        {
            var time = ResponseTimeSeconds!.Value;
            var timeIndicator = time switch
            {
                < 1.0 => "Fast",
                < 3.0 => "Normal",
                < 10.0 => "Slow",
                _ => "Very Slow"
            };
            indicators.Add($"Speed: {timeIndicator}");
        }

        if (HasAttachments())
        {
            indicators.Add($"Attachments: {AttachmentCount}");
        }

        return indicators.Any() ? string.Join(", ", indicators) : "No quality metrics";
    }

    public T? GetProperty<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !AdditionalProperties.ContainsKey(key))
            return default;

        try
        {
            return (T)AdditionalProperties[key];
        }
        catch
        {
            return default;
        }
    }

    public bool HasProperty(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && AdditionalProperties.ContainsKey(key);
    }
}

