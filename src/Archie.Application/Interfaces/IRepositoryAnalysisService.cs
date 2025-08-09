using Archie.Application.Interfaces;

namespace Archie.Application.Interfaces;

/// <summary>
/// Service for analyzing repository structure and content
/// </summary>
public interface IRepositoryAnalysisService
{
    /// <summary>
    /// Perform complete analysis of a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete repository analysis context</returns>
    Task<RepositoryAnalysisContext> AnalyzeRepositoryAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analyze project structure from repository URL
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Project structure analysis</returns>
    Task<ProjectStructureAnalysis> AnalyzeProjectStructureAsync(
        string repositoryUrl,
        string? accessToken = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Extract dependencies from repository
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of dependencies found</returns>
    Task<List<DependencyInfo>> ExtractDependenciesAsync(
        string repositoryUrl,
        string? accessToken = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Identify architectural patterns in repository
    /// </summary>
    /// <param name="context">Repository analysis context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Architectural patterns identified</returns>
    Task<ArchitecturalPatterns> IdentifyArchitecturalPatternsAsync(
        RepositoryAnalysisContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze specific files in the repository
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="filePaths">List of file paths to analyze</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of file analyses</returns>
    Task<List<FileAnalysis>> AnalyzeFilesAsync(
        string repositoryUrl,
        IEnumerable<string> filePaths,
        string? accessToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get important files from repository for documentation purposes
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="primaryLanguage">Primary programming language</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of important files with analysis</returns>
    Task<List<FileAnalysis>> GetImportantFilesAsync(
        string repositoryUrl,
        string primaryLanguage,
        string? accessToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract key concepts from repository content
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="primaryLanguage">Primary programming language</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of concepts and their importance scores</returns>
    Task<Dictionary<string, double>> ExtractKeyConceptsAsync(
        string repositoryUrl,
        string primaryLanguage,
        string? accessToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if repository has changed since last analysis
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="lastAnalysisDate">Date of last analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if repository has changed</returns>
    Task<bool> HasRepositoryChangedAsync(
        Guid repositoryId,
        DateTime lastAnalysisDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get repository metadata for analysis
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository metadata</returns>
    Task<Dictionary<string, object>> GetRepositoryMetadataAsync(
        string repositoryUrl,
        string? accessToken = null,
        CancellationToken cancellationToken = default);
}