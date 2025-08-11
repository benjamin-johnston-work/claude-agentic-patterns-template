namespace Archie.Domain.ValueObjects;

public class SectionMetadata
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }
    public string? GeneratedBy { get; private set; }
    public string? Model { get; private set; }
    public int TokenCount { get; private set; }
    public double? ConfidenceScore { get; private set; }
    public Dictionary<string, object> AdditionalProperties { get; private set; }

    protected SectionMetadata() // EF Constructor
    {
        CreatedAt = DateTime.UtcNow;
        AdditionalProperties = new Dictionary<string, object>();
    }

    public SectionMetadata(
        string? generatedBy = null, 
        string? model = null, 
        int tokenCount = 0, 
        double? confidenceScore = null)
    {
        if (tokenCount < 0)
            throw new ArgumentOutOfRangeException(nameof(tokenCount), "Token count cannot be negative");
        
        if (confidenceScore.HasValue && (confidenceScore.Value < 0.0 || confidenceScore.Value > 1.0))
            throw new ArgumentOutOfRangeException(nameof(confidenceScore), "Confidence score must be between 0.0 and 1.0");

        CreatedAt = DateTime.UtcNow;
        GeneratedBy = generatedBy;
        Model = model;
        TokenCount = tokenCount;
        ConfidenceScore = confidenceScore;
        AdditionalProperties = new Dictionary<string, object>();
    }

    public void UpdateModifiedTime()
    {
        LastModifiedAt = DateTime.UtcNow;
    }

    public void AddProperty(string key, object value)
    {
        if (!string.IsNullOrWhiteSpace(key) && value != null)
        {
            AdditionalProperties[key] = value;
            UpdateModifiedTime();
        }
    }

    public T? GetProperty<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !AdditionalProperties.ContainsKey(key))
            return default;

        var value = AdditionalProperties[key];
        if (value is T typedValue)
            return typedValue;

        return default;
    }
}