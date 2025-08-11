using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

/// <summary>
/// Service for generating documentation using AI (Azure OpenAI)
/// </summary>
public interface IAIDocumentationGeneratorService
{
    /// <summary>
    /// Generate complete documentation for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="options">Documentation generation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated documentation</returns>
    Task<Documentation> GenerateDocumentationAsync(
        Guid repositoryId, 
        DocumentationGenerationOptions options,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate a specific documentation section
    /// </summary>
    /// <param name="context">Repository analysis context</param>
    /// <param name="sectionType">Type of section to generate</param>
    /// <param name="customInstructions">Optional custom instructions for generation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated documentation section</returns>
    Task<DocumentationSection> GenerateSectionAsync(
        RepositoryAnalysisContext context,
        DocumentationSectionType sectionType,
        string? customInstructions = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Extract code references from content
    /// </summary>
    /// <param name="content">Content to analyze</param>
    /// <param name="context">Repository analysis context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of code references found in content</returns>
    Task<List<CodeReference>> ExtractCodeReferencesAsync(
        string content,
        RepositoryAnalysisContext context,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Enrich content with examples and additional context
    /// </summary>
    /// <param name="content">Original content</param>
    /// <param name="context">Repository analysis context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enriched content with examples</returns>
    Task<string> EnrichContentWithExamplesAsync(
        string content,
        RepositoryAnalysisContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate and score the quality of generated documentation
    /// </summary>
    /// <param name="documentation">Documentation to validate</param>
    /// <param name="context">Repository analysis context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Quality score between 0.0 and 1.0</returns>
    Task<double> ValidateDocumentationQualityAsync(
        Documentation documentation,
        RepositoryAnalysisContext context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for documentation generation
/// </summary>
public class DocumentationGenerationOptions
{
    public List<DocumentationSectionType> RequestedSections { get; set; } = new();
    public bool IncludeCodeExamples { get; set; } = true;
    public bool IncludeApiReference { get; set; } = true;
    public bool IncludeArchitectureDiagrams { get; set; } = false;
    public string CustomInstructions { get; set; } = string.Empty;
    public DocumentationStyle Style { get; set; } = DocumentationStyle.Technical;
    public int MaxTokensPerSection { get; set; } = 4000;
    public double Temperature { get; set; } = 0.3;
    
    /// <summary>
    /// Get default sections based on repository type and language
    /// </summary>
    public static List<DocumentationSectionType> GetDefaultSections(string projectType, string primaryLanguage)
    {
        var sections = new List<DocumentationSectionType>
        {
            DocumentationSectionType.Overview,
            DocumentationSectionType.GettingStarted,
            DocumentationSectionType.Installation,
            DocumentationSectionType.Usage
        };

        // Add language-specific sections
        if (primaryLanguage.ToLowerInvariant() switch
        {
            "csharp" or "c#" => true,
            "javascript" or "typescript" => true,
            "python" => true,
            "java" => true,
            _ => false
        })
        {
            sections.Add(DocumentationSectionType.Configuration);
            sections.Add(DocumentationSectionType.ApiReference);
        }

        // Add project type-specific sections
        if (projectType.ToLowerInvariant() switch
        {
            "library" or "framework" => true,
            "application" => true,
            _ => false
        })
        {
            sections.Add(DocumentationSectionType.Examples);
            sections.Add(DocumentationSectionType.Testing);
        }

        return sections;
    }
}

/// <summary>
/// Documentation generation style
/// </summary>
public enum DocumentationStyle
{
    Technical,      // Developer-focused
    Business,       // Stakeholder-focused
    Tutorial,       // Learning-focused
    Reference       // API documentation focused
}

/// <summary>
/// Context from repository analysis for documentation generation
/// </summary>
public class RepositoryAnalysisContext
{
    public Guid RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string PrimaryLanguage { get; set; } = string.Empty;
    public List<string> Languages { get; set; } = new();
    public List<FileAnalysis> ImportantFiles { get; set; } = new();
    public ProjectStructureAnalysis Structure { get; set; } = new();
    public List<DependencyInfo> Dependencies { get; set; } = new();
    public ArchitecturalPatterns Patterns { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    
    // Enhanced content analysis properties
    public ProjectPurpose Purpose { get; set; } = new();
    public ComponentRelationshipMap ComponentMap { get; set; } = new();
    public List<ContentSummary> ContentSummaries { get; set; } = new();
}

/// <summary>
/// Analysis of individual files in the repository
/// </summary>
public class FileAnalysis
{
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public List<string> KeyConcepts { get; set; } = new();
    public string Purpose { get; set; } = string.Empty;
    public double ImportanceScore { get; set; }
    public string? Content { get; set; }
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Analysis of project structure
/// </summary>
public class ProjectStructureAnalysis
{
    public string ProjectType { get; set; } = string.Empty; // Web API, Console App, Library, etc.
    public List<string> Frameworks { get; set; } = new();
    public Dictionary<string, List<string>> DirectoryPurpose { get; set; } = new();
    public List<string> EntryPoints { get; set; } = new();
    public List<string> ConfigurationFiles { get; set; } = new();
    public List<string> TestFiles { get; set; } = new();
    public List<string> DocumentationFiles { get; set; } = new();
    public int TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }
}

/// <summary>
/// Information about dependencies
/// </summary>
public class DependencyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // NuGet, npm, pip, etc.
    public string Purpose { get; set; } = string.Empty;
    public bool IsDirectDependency { get; set; }
    public string? Description { get; set; }
    public string? LicenseType { get; set; }
}

/// <summary>
/// Architectural patterns identified in the repository
/// </summary>
public class ArchitecturalPatterns
{
    public List<string> DesignPatterns { get; set; } = new();
    public List<string> ArchitecturalStyles { get; set; } = new();
    public List<string> ProgrammingParadigms { get; set; } = new();
    public Dictionary<string, string> PatternExplanations { get; set; } = new();
    public List<string> CodeQualityIndicators { get; set; } = new();
}