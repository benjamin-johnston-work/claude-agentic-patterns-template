namespace Archie.Domain.ValueObjects;

public record RelationshipMetadata
{
    public int Confidence { get; init; } // 0-100 confidence in relationship accuracy
    public string DetectionMethod { get; init; } = string.Empty; // How relationship was detected
    public Dictionary<string, object> Properties { get; init; } = new(); // Additional relationship properties
    public List<string> SourceReferences { get; init; } = new(); // Code locations where relationship is evident

    public static RelationshipMetadata Create(
        int confidence = 100,
        string detectionMethod = "Static Analysis",
        Dictionary<string, object>? properties = null,
        List<string>? sourceReferences = null)
    {
        if (confidence < 0 || confidence > 100)
            throw new ArgumentException("Confidence must be between 0 and 100", nameof(confidence));

        return new RelationshipMetadata
        {
            Confidence = confidence,
            DetectionMethod = detectionMethod,
            Properties = properties ?? new Dictionary<string, object>(),
            SourceReferences = sourceReferences ?? new List<string>()
        };
    }

    public static RelationshipMetadata Default => new()
    {
        Confidence = 100,
        DetectionMethod = "Static Analysis"
    };

    public bool IsHighConfidence => Confidence >= 80;
    public bool IsMediumConfidence => Confidence >= 60 && Confidence < 80;
    public bool IsLowConfidence => Confidence < 60;

    public RelationshipMetadata WithConfidence(int confidence)
    {
        return this with { Confidence = Math.Clamp(confidence, 0, 100) };
    }

    public RelationshipMetadata WithDetectionMethod(string detectionMethod)
    {
        return this with { DetectionMethod = detectionMethod ?? string.Empty };
    }

    public RelationshipMetadata AddProperty(string key, object value)
    {
        var newProperties = new Dictionary<string, object>(Properties) { [key] = value };
        return this with { Properties = newProperties };
    }

    public RelationshipMetadata AddSourceReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return this;

        var newReferences = new List<string>(SourceReferences);
        if (!newReferences.Contains(reference))
            newReferences.Add(reference);

        return this with { SourceReferences = newReferences };
    }
}