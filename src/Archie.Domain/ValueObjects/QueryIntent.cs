namespace Archie.Domain.ValueObjects;

public record QueryIntent
{
    public IntentType Type { get; }
    public string Domain { get; }
    public IReadOnlyList<string> Entities { get; }
    public double Confidence { get; }
    public IReadOnlyDictionary<string, object> Parameters { get; }

    public QueryIntent(
        IntentType type,
        string domain,
        IReadOnlyList<string> entities,
        double confidence,
        IReadOnlyDictionary<string, object> parameters)
    {
        if (confidence < 0.0 || confidence > 1.0)
            throw new ArgumentException("Confidence must be between 0.0 and 1.0", nameof(confidence));

        Type = type;
        Domain = domain ?? string.Empty;
        Entities = entities ?? throw new ArgumentNullException(nameof(entities));
        Confidence = confidence;
        Parameters = parameters ?? new Dictionary<string, object>();
    }

    public static QueryIntent Create(
        IntentType type,
        string? domain = null,
        List<string>? entities = null,
        double confidence = 0.0,
        Dictionary<string, object>? parameters = null)
    {
        return new QueryIntent(
            type,
            domain ?? string.Empty,
            entities ?? new List<string>(),
            confidence,
            parameters ?? new Dictionary<string, object>());
    }

    public static QueryIntent Unknown => Create(IntentType.Unknown, confidence: 0.0);

    public QueryIntent WithConfidence(double confidence)
    {
        return new QueryIntent(Type, Domain, Entities, confidence, Parameters);
    }

    public QueryIntent WithDomain(string domain)
    {
        return new QueryIntent(Type, domain ?? string.Empty, Entities, Confidence, Parameters);
    }

    public QueryIntent WithEntities(List<string> entities)
    {
        return new QueryIntent(Type, Domain, entities ?? new List<string>(), Confidence, Parameters);
    }

    public QueryIntent WithParameters(Dictionary<string, object> parameters)
    {
        return new QueryIntent(Type, Domain, Entities, Confidence, parameters ?? new Dictionary<string, object>());
    }

    public QueryIntent AddEntity(string entity)
    {
        if (string.IsNullOrWhiteSpace(entity))
            return this;

        if (Entities.Contains(entity, StringComparer.OrdinalIgnoreCase))
            return this;

        var newEntities = new List<string>(Entities) { entity };
        return WithEntities(newEntities);
    }

    public QueryIntent AddParameter(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return this;

        var newParameters = new Dictionary<string, object>(Parameters)
        {
            [key] = value
        };

        return WithParameters(newParameters);
    }

    public bool IsHighConfidence(double threshold = 0.8) => Confidence >= threshold;

    public bool IsMediumConfidence(double lowThreshold = 0.5, double highThreshold = 0.8) => 
        Confidence >= lowThreshold && Confidence < highThreshold;

    public bool IsLowConfidence(double threshold = 0.5) => Confidence < threshold;

    public bool HasDomain() => !string.IsNullOrWhiteSpace(Domain);

    public bool HasEntities() => Entities.Any();

    public bool HasParameters() => Parameters.Any();

    public bool HasEntity(string entity)
    {
        return !string.IsNullOrWhiteSpace(entity) && 
               Entities.Any(e => e.Equals(entity, StringComparison.OrdinalIgnoreCase));
    }

    public T? GetParameter<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !Parameters.ContainsKey(key))
            return default;

        try
        {
            return (T)Parameters[key];
        }
        catch
        {
            return default;
        }
    }

    public bool HasParameter(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && Parameters.ContainsKey(key);
    }

    public string GetDescription()
    {
        return Type switch
        {
            IntentType.ExplainConcept => $"Explain concept in {Domain}",
            IntentType.FindImplementation => $"Find implementation of {string.Join(", ", Entities)}",
            IntentType.CompareApproaches => $"Compare approaches in {Domain}",
            IntentType.Troubleshoot => $"Troubleshoot issue with {string.Join(", ", Entities)}",
            IntentType.ProvideExample => $"Provide example of {Domain}",
            IntentType.ArchitecturalQuery => $"Architectural query about {Domain}",
            IntentType.CodeReview => $"Code review for {string.Join(", ", Entities)}",
            IntentType.Documentation => $"Documentation for {string.Join(", ", Entities)}",
            IntentType.Testing => $"Testing guidance for {Domain}",
            IntentType.Unknown => "Unknown intent",
            _ => Type.ToString()
        };
    }

    public ConfidenceLevel GetConfidenceLevel()
    {
        return Confidence switch
        {
            >= 0.8 => ConfidenceLevel.High,
            >= 0.5 => ConfidenceLevel.Medium,
            >= 0.2 => ConfidenceLevel.Low,
            _ => ConfidenceLevel.VeryLow
        };
    }

    public bool RequiresCodeSearch()
    {
        return Type switch
        {
            IntentType.FindImplementation => true,
            IntentType.CodeReview => true,
            IntentType.ProvideExample => true,
            IntentType.Testing => true,
            _ => false
        };
    }

    public bool RequiresDocumentationSearch()
    {
        return Type switch
        {
            IntentType.ExplainConcept => true,
            IntentType.Documentation => true,
            IntentType.ArchitecturalQuery => true,
            _ => false
        };
    }

    public bool RequiresComparison()
    {
        return Type == IntentType.CompareApproaches;
    }
}

public enum IntentType
{
    Unknown,             // Unable to determine intent
    ExplainConcept,      // "How does authentication work?"
    FindImplementation,  // "Where is user validation implemented?"
    CompareApproaches,   // "What's the difference between these two methods?"
    Troubleshoot,        // "Why might this code throw an exception?"
    ProvideExample,      // "Show me an example of dependency injection"
    ArchitecturalQuery,  // "What design patterns are used here?"
    CodeReview,          // "What could be improved in this function?"
    Documentation,       // "Document this API endpoint"
    Testing             // "How should I test this functionality?"
}

public enum ConfidenceLevel
{
    VeryLow,    // < 0.2
    Low,        // 0.2 - 0.5
    Medium,     // 0.5 - 0.8
    High        // >= 0.8
}