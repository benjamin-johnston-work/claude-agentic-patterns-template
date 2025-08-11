using System.ComponentModel.DataAnnotations;

namespace Archie.Infrastructure.Configuration;

public class CodeAnalysisOptions
{
    public const string SectionName = "CodeAnalysis";
    
    [Required]
    public string AzureOpenAIEndpoint { get; set; } = "https://ract-ai-foundry-dev.openai.azure.com/";
    
    [Required]
    public string AzureOpenAIApiKey { get; set; } = string.Empty; // From Azure Key Vault
    
    [Required]
    public string GPTDeploymentName { get; set; } = "gpt-4";
    
    [Required]
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-ada-002";
    
    [Range(100, 4000)]
    public int MaxTokensForAnalysis { get; set; } = 2000;
    
    [Range(0.0, 1.0)]
    public double AnalysisTemperature { get; set; } = 0.2; // Lower for consistent analysis
    
    public bool UseSemanticSimilarity { get; set; } = true;
    public bool EnablePatternRecognitionAI { get; set; } = true;
    
    [Range(0.0, 1.0)]
    public double SemanticSimilarityThreshold { get; set; } = 0.8;
    
    [Range(100, 2000)]
    public int MaxAnalysisTokens { get; set; } = 1000;
    
    [Range(1, 100)]
    public int MaxConcurrentAnalysisCalls { get; set; } = 10;
    
    [Range(1000, 60000)]
    public int AnalysisRequestTimeoutMs { get; set; } = 30000; // 30 seconds
    
    public Dictionary<string, string> LanguageAnalysisPrompts { get; set; } = new()
    {
        {
            "csharp", "Analyze this C# code and identify classes, methods, properties, fields, interfaces, and their relationships. Focus on inheritance, composition, method calls, and dependencies."
        },
        {
            "javascript", "Analyze this JavaScript code and identify functions, classes, modules, exports, imports, and their relationships. Focus on function calls, module dependencies, and object relationships."
        },
        {
            "typescript", "Analyze this TypeScript code and identify classes, interfaces, types, functions, and their relationships. Focus on inheritance, composition, type usage, and method calls."
        },
        {
            "python", "Analyze this Python code and identify classes, functions, modules, imports, and their relationships. Focus on inheritance, function calls, module dependencies, and class relationships."
        }
    };
    
    public Dictionary<string, List<string>> ComplexityIndicators { get; set; } = new()
    {
        {
            "csharp", new List<string> { "if", "while", "for", "foreach", "switch", "try", "catch", "?" }
        },
        {
            "javascript", new List<string> { "if", "while", "for", "switch", "try", "catch", "?" }
        },
        {
            "typescript", new List<string> { "if", "while", "for", "switch", "try", "catch", "?" }
        },
        {
            "python", new List<string> { "if", "while", "for", "try", "except", "with" }
        }
    };
}