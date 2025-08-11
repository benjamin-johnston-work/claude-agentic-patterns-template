using System.ComponentModel.DataAnnotations;

namespace Archie.Infrastructure.Configuration;

public class KnowledgeGraphOptions
{
    public const string SectionName = "KnowledgeGraph";
    
    [Range(1, 10)]
    public int MaxConcurrentAnalysis { get; set; } = 5;
    
    [Range(1000, 100000)]
    public int MaxEntitiesPerRepository { get; set; } = 50000;
    
    [Range(1, 1000)]
    public int BatchSizeForEntityProcessing { get; set; } = 100;
    
    [Range(1, 10)]
    public int MaxRelationshipDepth { get; set; } = 3;
    
    [Range(0.0, 1.0)]
    public double MinimumRelationshipConfidence { get; set; } = 0.6;
    
    [Range(0.0, 1.0)]
    public double MinimumPatternConfidence { get; set; } = 0.7;
    
    public bool EnablePatternDetection { get; set; } = true;
    public bool EnableCrossRepositoryAnalysis { get; set; } = false;
    public bool EnableComplexityAnalysis { get; set; } = true;
    
    public List<string> SupportedLanguages { get; set; } = new()
    {
        "csharp", "javascript", "typescript", "python", "java", "go", "rust", "cpp"
    };
    
    public Dictionary<string, List<string>> LanguagePatterns { get; set; } = new()
    {
        {
            "csharp", new List<string>
            {
                "Controller", "Service", "Repository", "Entity", "ValueObject",
                "Factory", "Builder", "Singleton", "Observer"
            }
        },
        {
            "javascript", new List<string>
            {
                "Component", "Hook", "Service", "Module", "Factory", "Observer"
            }
        },
        {
            "typescript", new List<string>
            {
                "Component", "Service", "Interface", "Type", "Factory", "Decorator"
            }
        },
        {
            "python", new List<string>
            {
                "Class", "Function", "Module", "Decorator", "Factory", "Singleton"
            }
        },
        {
            "java", new List<string>
            {
                "Controller", "Service", "Repository", "Entity", "Factory", "Builder"
            }
        }
    };
    
    [Range(3600, 86400)]
    public int GraphRefreshIntervalSeconds { get; set; } = 21600; // 6 hours
    
    public bool EnableIncrementalUpdates { get; set; } = true;
    
    [Range(1, 100)]
    public int MaxAnalysisRetries { get; set; } = 3;
    
    [Range(1000, 30000)]
    public int AnalysisTimeoutMilliseconds { get; set; } = 10000; // 10 seconds
}