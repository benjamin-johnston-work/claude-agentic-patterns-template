namespace Archie.Domain.ValueObjects;

public record GraphMetadata
{
    public string? Description { get; init; }
    public AnalysisDepth AnalysisDepth { get; init; }
    public bool IncludePatterns { get; init; }
    public bool IncludeCrossRepository { get; init; }
    public List<string> SupportedLanguages { get; init; } = new();
    public Dictionary<string, object> Properties { get; init; } = new();
    public string? CreatedBy { get; init; }
    public string? LastUpdatedBy { get; init; }

    public static GraphMetadata Default => new()
    {
        AnalysisDepth = AnalysisDepth.Standard,
        IncludePatterns = true,
        IncludeCrossRepository = false,
        SupportedLanguages = new List<string> { "csharp", "javascript", "typescript", "python", "java" }
    };

    public static GraphMetadata Create(
        AnalysisDepth analysisDepth = AnalysisDepth.Standard,
        bool includePatterns = true,
        bool includeCrossRepository = false,
        List<string>? supportedLanguages = null,
        string? description = null,
        string? createdBy = null)
    {
        return new GraphMetadata
        {
            Description = description,
            AnalysisDepth = analysisDepth,
            IncludePatterns = includePatterns,
            IncludeCrossRepository = includeCrossRepository,
            SupportedLanguages = supportedLanguages ?? new List<string> { "csharp", "javascript", "typescript", "python", "java" },
            CreatedBy = createdBy,
            LastUpdatedBy = createdBy
        };
    }
}

public enum AnalysisDepth
{
    Basic,        // Entities and direct relationships only
    Standard,     // Include patterns and indirect relationships  
    Comprehensive // Deep analysis with cross-repository connections
}