using System.ComponentModel.DataAnnotations;
using Archie.Domain.ValueObjects;

namespace Archie.Infrastructure.Configuration;

public class DocumentationGenerationSettings
{
    public const string SectionName = "DocumentationGeneration";
    
    [Required]
    public string AzureOpenAIEndpoint { get; set; } = "https://ract-ai-foundry-dev.openai.azure.com/";
    
    [Required]
    public string AzureOpenAIApiKey { get; set; } = string.Empty; // From Azure Key Vault
    
    [Required]
    public string GPTDeploymentName { get; set; } = "gpt-4";
    
    public string APIVersion { get; set; } = "2024-02-01";
    
    [Range(100, 8000)]
    public int MaxTokensPerSection { get; set; } = 4000;
    
    [Range(0.0, 2.0)]
    public double Temperature { get; set; } = 0.3; // Lower temperature for consistent documentation
    
    [Range(1, 10)]
    public int MaxConcurrentGenerations { get; set; } = 3;
    
    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 120;
    
    [Range(1, 5)]
    public int RetryAttempts { get; set; } = 3;
    
    public List<DocumentationSectionType> DefaultSections { get; set; } = new()
    {
        DocumentationSectionType.Overview,
        DocumentationSectionType.GettingStarted,
        DocumentationSectionType.Usage,
        DocumentationSectionType.Configuration
    };
    
    public Dictionary<string, string> LanguageSpecificPrompts { get; set; } = new()
    {
        { "csharp", "Focus on .NET patterns, dependency injection, Entity Framework usage, and ASP.NET Core features" },
        { "c#", "Focus on .NET patterns, dependency injection, Entity Framework usage, and ASP.NET Core features" },
        { "javascript", "Highlight React/Vue patterns, async/await usage, npm dependencies, and modern ES6+ features" },
        { "typescript", "Highlight React/Vue patterns, async/await usage, npm dependencies, type definitions, and modern ES6+ features" },
        { "python", "Emphasize framework usage (Django/Flask), virtual environments, pip package management, and Python best practices" },
        { "java", "Focus on Spring Boot, Maven/Gradle dependencies, enterprise patterns, and Java best practices" },
        { "go", "Highlight Go modules, concurrency patterns, standard library usage, and Go idioms" }
    };
    
    public bool EnableCodeExtraction { get; set; } = true;
    public bool EnableDependencyAnalysis { get; set; } = true;
    public bool EnableArchitecturalAnalysis { get; set; } = true;
    public bool EnableQualityValidation { get; set; } = true;
    
    // Token usage monitoring
    public int MaxTokensPerDay { get; set; } = 1000000; // 1M tokens per day limit
    public bool EnableTokenUsageTracking { get; set; } = true;
    
    // Content generation settings
    public int MaxContentLength { get; set; } = 50000; // Max characters per section
    public int MinContentLength { get; set; } = 100; // Min characters per section
    public double MinQualityScore { get; set; } = 0.7; // Minimum acceptable quality score
    
    // Rate limiting
    public bool EnableRateLimitProtection { get; set; } = true;
    public int RequestsPerMinute { get; set; } = 20; // Max requests per minute
}