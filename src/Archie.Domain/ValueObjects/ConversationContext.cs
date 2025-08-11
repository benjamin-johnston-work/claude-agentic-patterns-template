namespace Archie.Domain.ValueObjects;

public record ConversationContext
{
    public IReadOnlyList<Guid> RepositoryIds { get; }
    public IReadOnlyList<string> RepositoryNames { get; }
    public string Domain { get; }
    public IReadOnlyList<string> TechnicalTags { get; }
    public IReadOnlyDictionary<string, object> SessionData { get; }
    public ConversationPreferences Preferences { get; }

    public ConversationContext(
        IReadOnlyList<Guid> repositoryIds,
        IReadOnlyList<string> repositoryNames,
        string domain,
        IReadOnlyList<string> technicalTags,
        IReadOnlyDictionary<string, object> sessionData,
        ConversationPreferences preferences)
    {
        RepositoryIds = repositoryIds ?? throw new ArgumentNullException(nameof(repositoryIds));
        RepositoryNames = repositoryNames ?? throw new ArgumentNullException(nameof(repositoryNames));
        Domain = domain ?? string.Empty;
        TechnicalTags = technicalTags ?? throw new ArgumentNullException(nameof(technicalTags));
        SessionData = sessionData ?? new Dictionary<string, object>();
        Preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));

        if (repositoryIds.Count != repositoryNames.Count)
            throw new ArgumentException("Repository IDs and names count must match");
        
        if (repositoryIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("Repository IDs cannot be empty", nameof(repositoryIds));
        
        if (repositoryNames.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Repository names cannot be null or empty", nameof(repositoryNames));
    }

    public static ConversationContext Empty => new(
        Array.Empty<Guid>(),
        Array.Empty<string>(),
        string.Empty,
        Array.Empty<string>(),
        new Dictionary<string, object>(),
        ConversationPreferences.Default);

    public static ConversationContext Create(
        List<Guid> repositoryIds,
        List<string> repositoryNames,
        string? domain = null,
        List<string>? technicalTags = null,
        Dictionary<string, object>? sessionData = null,
        ConversationPreferences? preferences = null)
    {
        return new ConversationContext(
            repositoryIds ?? new List<Guid>(),
            repositoryNames ?? new List<string>(),
            domain ?? string.Empty,
            technicalTags ?? new List<string>(),
            sessionData ?? new Dictionary<string, object>(),
            preferences ?? ConversationPreferences.Default);
    }

    public ConversationContext WithRepositories(List<Guid> repositoryIds, List<string> repositoryNames)
    {
        return new ConversationContext(
            repositoryIds,
            repositoryNames,
            Domain,
            TechnicalTags,
            SessionData,
            Preferences);
    }

    public ConversationContext WithDomain(string domain)
    {
        return new ConversationContext(
            RepositoryIds,
            RepositoryNames,
            domain ?? string.Empty,
            TechnicalTags,
            SessionData,
            Preferences);
    }

    public ConversationContext WithTechnicalTags(List<string> technicalTags)
    {
        return new ConversationContext(
            RepositoryIds,
            RepositoryNames,
            Domain,
            technicalTags ?? new List<string>(),
            SessionData,
            Preferences);
    }

    public ConversationContext WithSessionData(Dictionary<string, object> sessionData)
    {
        return new ConversationContext(
            RepositoryIds,
            RepositoryNames,
            Domain,
            TechnicalTags,
            sessionData ?? new Dictionary<string, object>(),
            Preferences);
    }

    public ConversationContext WithPreferences(ConversationPreferences preferences)
    {
        return new ConversationContext(
            RepositoryIds,
            RepositoryNames,
            Domain,
            TechnicalTags,
            SessionData,
            preferences ?? ConversationPreferences.Default);
    }

    public ConversationContext AddSessionData(string key, object value)
    {
        var newSessionData = new Dictionary<string, object>(SessionData)
        {
            [key] = value
        };

        return WithSessionData(newSessionData);
    }

    public ConversationContext AddTechnicalTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return this;

        if (TechnicalTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            return this;

        var newTags = new List<string>(TechnicalTags) { tag };
        return WithTechnicalTags(newTags);
    }

    public bool HasRepository(Guid repositoryId)
    {
        return RepositoryIds.Contains(repositoryId);
    }

    public bool HasDomain() => !string.IsNullOrWhiteSpace(Domain);

    public bool HasRepositories() => RepositoryIds.Any();

    public bool HasTechnicalTags() => TechnicalTags.Any();

    public bool HasSessionData() => SessionData.Any();

    public T? GetSessionData<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !SessionData.ContainsKey(key))
            return default;

        try
        {
            return (T)SessionData[key];
        }
        catch
        {
            return default;
        }
    }

    public string GetContextSummary()
    {
        var parts = new List<string>();

        if (HasRepositories())
            parts.Add($"{RepositoryIds.Count} repository(s)");

        if (HasDomain())
            parts.Add($"Domain: {Domain}");

        if (HasTechnicalTags())
            parts.Add($"{TechnicalTags.Count} technical tag(s)");

        return parts.Any() ? string.Join(", ", parts) : "No context";
    }
}

public record ConversationPreferences
{
    public ResponseStyle ResponseStyle { get; }
    public bool IncludeCodeExamples { get; }
    public bool IncludeReferences { get; }
    public int MaxResponseLength { get; }
    public IReadOnlyList<string> PreferredLanguages { get; }

    public ConversationPreferences(
        ResponseStyle responseStyle,
        bool includeCodeExamples,
        bool includeReferences,
        int maxResponseLength,
        IReadOnlyList<string> preferredLanguages)
    {
        if (maxResponseLength <= 0)
            throw new ArgumentException("Max response length must be positive", nameof(maxResponseLength));

        ResponseStyle = responseStyle;
        IncludeCodeExamples = includeCodeExamples;
        IncludeReferences = includeReferences;
        MaxResponseLength = maxResponseLength;
        PreferredLanguages = preferredLanguages ?? throw new ArgumentNullException(nameof(preferredLanguages));
    }

    public static ConversationPreferences Default => new(
        ResponseStyle.Balanced,
        true,
        true,
        2000,
        Array.Empty<string>());

    public static ConversationPreferences Create(
        ResponseStyle responseStyle = ResponseStyle.Balanced,
        bool includeCodeExamples = true,
        bool includeReferences = true,
        int maxResponseLength = 2000,
        List<string>? preferredLanguages = null)
    {
        return new ConversationPreferences(
            responseStyle,
            includeCodeExamples,
            includeReferences,
            maxResponseLength,
            preferredLanguages ?? new List<string>());
    }

    public ConversationPreferences WithResponseStyle(ResponseStyle responseStyle)
    {
        return new ConversationPreferences(
            responseStyle,
            IncludeCodeExamples,
            IncludeReferences,
            MaxResponseLength,
            PreferredLanguages);
    }

    public ConversationPreferences WithMaxResponseLength(int maxResponseLength)
    {
        return new ConversationPreferences(
            ResponseStyle,
            IncludeCodeExamples,
            IncludeReferences,
            maxResponseLength,
            PreferredLanguages);
    }

    public ConversationPreferences WithPreferredLanguages(List<string> preferredLanguages)
    {
        return new ConversationPreferences(
            ResponseStyle,
            IncludeCodeExamples,
            IncludeReferences,
            MaxResponseLength,
            preferredLanguages ?? new List<string>());
    }

    public bool HasPreferredLanguages() => PreferredLanguages.Any();

    public bool IsPreferredLanguage(string language)
    {
        return !string.IsNullOrWhiteSpace(language) && 
               PreferredLanguages.Any(l => l.Equals(language, StringComparison.OrdinalIgnoreCase));
    }
}

public enum ResponseStyle
{
    Concise,    // Brief, direct answers
    Balanced,   // Moderate detail with examples
    Detailed,   // Comprehensive explanations
    Tutorial    // Step-by-step guidance
}