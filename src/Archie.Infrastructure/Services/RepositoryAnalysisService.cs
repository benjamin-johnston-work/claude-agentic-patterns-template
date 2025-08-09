using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using Archie.Application.Interfaces;
using Archie.Infrastructure.GitHub;
using Archie.Infrastructure.GitHub.Models;

namespace Archie.Infrastructure.Services;

/// <summary>
/// Service for analyzing repository structure, dependencies, and content
/// </summary>
public class RepositoryAnalysisService : IRepositoryAnalysisService
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IGitHubService _gitHubService;
    private readonly ILogger<RepositoryAnalysisService> _logger;

    // Cache for analysis results
    private static readonly Dictionary<Guid, (RepositoryAnalysisContext Context, DateTime CachedAt)> _analysisCache = new();
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    public RepositoryAnalysisService(
        IRepositoryRepository repositoryRepository,
        IGitHubService gitHubService,
        ILogger<RepositoryAnalysisService> logger)
    {
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RepositoryAnalysisContext> AnalyzeRepositoryAsync(
        Guid repositoryId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting repository analysis for: {RepositoryId}", repositoryId);

            // Check cache first
            if (_analysisCache.TryGetValue(repositoryId, out var cachedResult) &&
                DateTime.UtcNow - cachedResult.CachedAt < CacheExpiration)
            {
                _logger.LogDebug("Using cached analysis for repository: {RepositoryId}", repositoryId);
                return cachedResult.Context;
            }

            // Get repository information
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null)
            {
                throw new InvalidOperationException($"Repository {repositoryId} not found");
            }

            // Perform comprehensive analysis
            var context = new RepositoryAnalysisContext
            {
                RepositoryId = repositoryId,
                RepositoryName = repository.Name,
                RepositoryUrl = repository.Url,
                PrimaryLanguage = repository.Language,
                AnalyzedAt = DateTime.UtcNow
            };

            // Analyze project structure
            context.Structure = await AnalyzeProjectStructureAsync(repository.Url, cancellationToken: cancellationToken);
            
            // Extract dependencies
            context.Dependencies = await ExtractDependenciesAsync(repository.Url, cancellationToken: cancellationToken);
            
            // Get important files for documentation
            context.ImportantFiles = await GetImportantFilesAsync(repository.Url, repository.Language, cancellationToken: cancellationToken);
            
            // Extract languages
            context.Languages = await ExtractLanguagesAsync(repository.Url, cancellationToken);
            
            // Identify architectural patterns
            context.Patterns = await IdentifyArchitecturalPatternsAsync(context, cancellationToken);
            
            // Get repository metadata
            context.Metadata = await GetRepositoryMetadataAsync(repository.Url, cancellationToken: cancellationToken);

            // Cache the result
            _analysisCache[repositoryId] = (context, DateTime.UtcNow);

            _logger.LogInformation("Completed repository analysis for: {RepositoryId}. Found {FileCount} important files, {DependencyCount} dependencies", 
                repositoryId, context.ImportantFiles.Count, context.Dependencies.Count);

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing repository: {RepositoryId}", repositoryId);
            throw;
        }
    }

    public async Task<ProjectStructureAnalysis> AnalyzeProjectStructureAsync(
        string repositoryUrl, 
        string? accessToken = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Analyzing project structure for repository: {RepositoryUrl}", repositoryUrl);

            var structure = new ProjectStructureAnalysis();
            
            // Get repository contents
            var (owner, repo) = _gitHubService.ParseRepositoryUrl(repositoryUrl);
            var tree = await _gitHubService.GetRepositoryTreeWithMetadataAsync(owner, repo, "main", true, accessToken, cancellationToken);
            var files = tree.Tree;
            
            structure.TotalFiles = files.Count();
            structure.TotalSizeBytes = files.Where(f => f.Size.HasValue).Sum(f => f.Size.Value);

            // Analyze directory structure
            var directories = files.Select(f => Path.GetDirectoryName(f.Path))
                                  .Where(d => !string.IsNullOrEmpty(d))
                                  .Distinct()
                                  .ToList();

            foreach (var directory in directories)
            {
                var purpose = DetermineDirectoryPurpose(directory, files);
                if (!string.IsNullOrEmpty(purpose))
                {
                    if (!structure.DirectoryPurpose.ContainsKey(directory))
                        structure.DirectoryPurpose[directory] = new List<string>();
                    structure.DirectoryPurpose[directory].Add(purpose);
                }
            }

            // Identify project type and frameworks
            await AnalyzeProjectTypeAndFrameworks(structure, files, cancellationToken);
            
            // Find entry points
            structure.EntryPoints = FindEntryPoints(files);
            
            // Categorize files
            structure.ConfigurationFiles = FindConfigurationFiles(files);
            structure.TestFiles = FindTestFiles(files);
            structure.DocumentationFiles = FindDocumentationFiles(files);

            _logger.LogDebug("Project structure analysis completed. Type: {ProjectType}, Files: {FileCount}", 
                structure.ProjectType, structure.TotalFiles);

            return structure;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing project structure for: {RepositoryUrl}", repositoryUrl);
            throw;
        }
    }

    public async Task<List<DependencyInfo>> ExtractDependenciesAsync(
        string repositoryUrl, 
        string? accessToken = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Extracting dependencies for repository: {RepositoryUrl}", repositoryUrl);

            var dependencies = new List<DependencyInfo>();

            // Get repository files
            var (owner, repo) = _gitHubService.ParseRepositoryUrl(repositoryUrl);
            var tree = await _gitHubService.GetRepositoryTreeWithMetadataAsync(owner, repo, "main", true, accessToken, cancellationToken);
            var files = tree.Tree;

            // Extract from different dependency files
            foreach (var file in files)
            {
                var fileDependencies = await ExtractDependenciesFromFile(file, cancellationToken);
                dependencies.AddRange(fileDependencies);
            }

            // Remove duplicates and sort by importance
            var uniqueDependencies = dependencies
                .GroupBy(d => new { d.Name, d.Type })
                .Select(g => g.First())
                .OrderByDescending(d => d.IsDirectDependency)
                .ThenBy(d => d.Name)
                .ToList();

            _logger.LogDebug("Extracted {DependencyCount} dependencies for repository: {RepositoryUrl}", 
                uniqueDependencies.Count, repositoryUrl);

            return uniqueDependencies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting dependencies for: {RepositoryUrl}", repositoryUrl);
            return new List<DependencyInfo>();
        }
    }

    public async Task<ArchitecturalPatterns> IdentifyArchitecturalPatternsAsync(
        RepositoryAnalysisContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Identifying architectural patterns for repository: {RepositoryId}", context.RepositoryId);

            var patterns = new ArchitecturalPatterns();

            // Analyze architectural styles based on structure and frameworks
            patterns.ArchitecturalStyles = IdentifyArchitecturalStyles(context);
            
            // Identify design patterns from important files
            patterns.DesignPatterns = await IdentifyDesignPatternsAsync(context, cancellationToken);
            
            // Determine programming paradigms
            patterns.ProgrammingParadigms = IdentifyProgrammingParadigms(context);
            
            // Generate explanations
            patterns.PatternExplanations = GeneratePatternExplanations(patterns);
            
            // Assess code quality indicators
            patterns.CodeQualityIndicators = AssessCodeQualityIndicators(context);

            _logger.LogDebug("Identified {StyleCount} architectural styles and {PatternCount} design patterns", 
                patterns.ArchitecturalStyles.Count, patterns.DesignPatterns.Count);

            return patterns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying architectural patterns for repository: {RepositoryId}", context.RepositoryId);
            return new ArchitecturalPatterns();
        }
    }

    public async Task<List<FileAnalysis>> AnalyzeFilesAsync(
        string repositoryUrl, 
        IEnumerable<string> filePaths, 
        string? accessToken = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var analyses = new List<FileAnalysis>();
            
            foreach (var filePath in filePaths)
            {
                try
                {
                    var (owner, repo) = _gitHubService.ParseRepositoryUrl(repositoryUrl);
                    var fileContent = await _gitHubService.GetFileContentAsync(owner, repo, filePath, "main", accessToken, cancellationToken);
                    
                    var analysis = new FileAnalysis
                    {
                        FilePath = filePath,
                        FileType = Path.GetExtension(filePath).TrimStart('.'),
                        Language = DetermineLanguageFromExtension(filePath),
                        LineCount = fileContent?.Split('\n').Length ?? 0,
                        Content = fileContent,
                        LastModified = DateTime.UtcNow, // GitHub API would provide actual date
                        Purpose = DetermineFilePurpose(filePath, fileContent),
                        ImportanceScore = CalculateFileImportanceScore(filePath, fileContent)
                    };

                    analysis.KeyConcepts = ExtractKeyConcepts(fileContent, analysis.Language);
                    analyses.Add(analysis);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error analyzing file: {FilePath}", filePath);
                }
            }

            return analyses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing files for repository: {RepositoryUrl}", repositoryUrl);
            return new List<FileAnalysis>();
        }
    }

    public async Task<List<FileAnalysis>> GetImportantFilesAsync(
        string repositoryUrl, 
        string primaryLanguage, 
        string? accessToken = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (owner, repo) = _gitHubService.ParseRepositoryUrl(repositoryUrl);
            var tree = await _gitHubService.GetRepositoryTreeWithMetadataAsync(owner, repo, "main", true, accessToken, cancellationToken);
            var files = tree.Tree;
            
            // Score files by importance
            var scoredFiles = files.Select(file => new
            {
                File = file,
                Score = CalculateFileImportanceScore(file.Path, null) // Content will be loaded later
            })
            .Where(sf => sf.Score > 0.3) // Only include moderately important files
            .OrderByDescending(sf => sf.Score)
            .Take(20) // Limit to top 20 files
            .Select(sf => sf.File.Path);

            return await AnalyzeFilesAsync(repositoryUrl, scoredFiles, accessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting important files for: {RepositoryUrl}", repositoryUrl);
            return new List<FileAnalysis>();
        }
    }

    public async Task<Dictionary<string, double>> ExtractKeyConceptsAsync(
        string repositoryUrl, 
        string primaryLanguage, 
        string? accessToken = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var importantFiles = await GetImportantFilesAsync(repositoryUrl, primaryLanguage, accessToken, cancellationToken);
            
            var conceptCounts = new Dictionary<string, int>();
            var totalFiles = importantFiles.Count;

            foreach (var file in importantFiles)
            {
                foreach (var concept in file.KeyConcepts)
                {
                    conceptCounts[concept] = conceptCounts.GetValueOrDefault(concept, 0) + 1;
                }
            }

            // Calculate normalized scores
            return conceptCounts.ToDictionary(
                kvp => kvp.Key,
                kvp => (double)kvp.Value / totalFiles
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting key concepts for: {RepositoryUrl}", repositoryUrl);
            return new Dictionary<string, double>();
        }
    }

    public async Task<bool> HasRepositoryChangedAsync(
        Guid repositoryId, 
        DateTime lastAnalysisDate, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null) return true; // Repository not found, assume changed

            // Check if repository was updated after last analysis
            return repository.UpdatedAt > lastAnalysisDate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if repository changed: {RepositoryId}", repositoryId);
            return true; // Assume changed on error
        }
    }

    public async Task<Dictionary<string, object>> GetRepositoryMetadataAsync(
        string repositoryUrl, 
        string? accessToken = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = new Dictionary<string, object>();

            // This would normally call GitHub API to get detailed repository metadata
            // For now, we'll extract basic information from URL
            var urlParts = repositoryUrl.TrimEnd('/').Split('/');
            if (urlParts.Length >= 2)
            {
                metadata["owner"] = urlParts[^2];
                metadata["name"] = urlParts[^1];
            }

            metadata["analyzed_at"] = DateTime.UtcNow;
            
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repository metadata for: {RepositoryUrl}", repositoryUrl);
            return new Dictionary<string, object>();
        }
    }

    #region Private Helper Methods

    private async Task<List<string>> ExtractLanguagesAsync(string repositoryUrl, CancellationToken cancellationToken)
    {
        try
        {
            var (owner, repo) = _gitHubService.ParseRepositoryUrl(repositoryUrl);
            var tree = await _gitHubService.GetRepositoryTreeWithMetadataAsync(owner, repo, "main", true, null, cancellationToken);
            var files = tree.Tree;
            
            return files.Select(f => DetermineLanguageFromExtension(f.Path))
                       .Where(lang => !string.IsNullOrEmpty(lang))
                       .GroupBy(lang => lang)
                       .OrderByDescending(g => g.Count())
                       .Select(g => g.Key)
                       .Take(10)
                       .ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    private async Task<List<DependencyInfo>> ExtractDependenciesFromFile(
        GitHubTreeItem file, 
        CancellationToken cancellationToken)
    {
        var dependencies = new List<DependencyInfo>();
        var fileName = Path.GetFileName(file.Path);
        
        try
        {
            string? content = null;
            
            // Only analyze known dependency files to avoid unnecessary API calls
            var dependencyFiles = new[] 
            { 
                "package.json", "package-lock.json", "yarn.lock",
                "requirements.txt", "Pipfile", "pyproject.toml",
                "pom.xml", "build.gradle", "build.gradle.kts",
                "Cargo.toml", "Cargo.lock",
                "go.mod", "go.sum",
                "composer.json", "composer.lock"
            };
            
            var csprojPattern = @".*\.csproj$";
            var solutionPattern = @".*\.sln$";
            
            if (dependencyFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase) ||
                Regex.IsMatch(fileName, csprojPattern, RegexOptions.IgnoreCase) ||
                Regex.IsMatch(fileName, solutionPattern, RegexOptions.IgnoreCase))
            {
                // In a real implementation, we would get the file content
                // For now, we'll simulate based on file type
                dependencies.AddRange(GetDependenciesForFileType(fileName));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting dependencies from file: {FilePath}", file.Path);
        }
        
        return dependencies;
    }

    private List<DependencyInfo> GetDependenciesForFileType(string fileName)
    {
        var dependencies = new List<DependencyInfo>();
        
        // Simulate common dependencies based on file type
        switch (fileName.ToLowerInvariant())
        {
            case "package.json":
                dependencies.AddRange(new[]
                {
                    new DependencyInfo { Name = "react", Version = "^18.0.0", Type = "npm", Purpose = "UI Framework", IsDirectDependency = true },
                    new DependencyInfo { Name = "typescript", Version = "^4.9.0", Type = "npm", Purpose = "Type System", IsDirectDependency = true },
                    new DependencyInfo { Name = "axios", Version = "^1.0.0", Type = "npm", Purpose = "HTTP Client", IsDirectDependency = true }
                });
                break;
                
            case "requirements.txt":
                dependencies.AddRange(new[]
                {
                    new DependencyInfo { Name = "flask", Version = "2.3.0", Type = "pip", Purpose = "Web Framework", IsDirectDependency = true },
                    new DependencyInfo { Name = "requests", Version = "2.31.0", Type = "pip", Purpose = "HTTP Client", IsDirectDependency = true }
                });
                break;
                
            case var name when name.EndsWith(".csproj"):
                dependencies.AddRange(new[]
                {
                    new DependencyInfo { Name = "Microsoft.AspNetCore.App", Version = "6.0.0", Type = "NuGet", Purpose = "Web Framework", IsDirectDependency = true },
                    new DependencyInfo { Name = "Microsoft.EntityFrameworkCore", Version = "6.0.0", Type = "NuGet", Purpose = "ORM", IsDirectDependency = true }
                });
                break;
        }
        
        return dependencies;
    }

    private async Task AnalyzeProjectTypeAndFrameworks(ProjectStructureAnalysis structure, IEnumerable<GitHubTreeItem> files, CancellationToken cancellationToken)
    {
        var fileNames = files.Select(f => Path.GetFileName(f.Path).ToLowerInvariant()).ToHashSet();
        
        // Determine project type
        if (fileNames.Any(name => name.EndsWith(".csproj") || name.EndsWith(".sln")))
        {
            structure.ProjectType = fileNames.Contains("program.cs") ? "Console Application" : "Library";
            
            if (fileNames.Any(name => name.Contains("api") || name.Contains("web")))
                structure.ProjectType = "Web API";
                
            structure.Frameworks.Add(".NET");
            
            if (fileNames.Contains("startup.cs") || fileNames.Any(name => name.Contains("aspnetcore")))
                structure.Frameworks.Add("ASP.NET Core");
        }
        else if (fileNames.Contains("package.json"))
        {
            structure.ProjectType = "JavaScript Application";
            structure.Frameworks.Add("Node.js");
            
            // Check for specific frameworks
            if (fileNames.Any(name => name.Contains("react") || name.Contains("jsx")))
                structure.Frameworks.Add("React");
            if (fileNames.Any(name => name.Contains("vue")))
                structure.Frameworks.Add("Vue.js");
            if (fileNames.Any(name => name.Contains("angular")))
                structure.Frameworks.Add("Angular");
        }
        else if (fileNames.Contains("requirements.txt") || fileNames.Contains("pyproject.toml"))
        {
            structure.ProjectType = "Python Application";
            structure.Frameworks.Add("Python");
            
            if (fileNames.Any(name => name.Contains("flask")))
                structure.Frameworks.Add("Flask");
            if (fileNames.Any(name => name.Contains("django")))
                structure.Frameworks.Add("Django");
        }
        else if (fileNames.Contains("pom.xml") || fileNames.Contains("build.gradle"))
        {
            structure.ProjectType = "Java Application";
            structure.Frameworks.Add("Java");
            
            if (fileNames.Any(name => name.Contains("spring")))
                structure.Frameworks.Add("Spring Boot");
        }
        else if (fileNames.Contains("cargo.toml"))
        {
            structure.ProjectType = "Rust Application";
            structure.Frameworks.Add("Rust");
        }
        else if (fileNames.Contains("go.mod"))
        {
            structure.ProjectType = "Go Application";
            structure.Frameworks.Add("Go");
        }
        else
        {
            structure.ProjectType = "Unknown";
        }
    }

    private List<string> IdentifyArchitecturalStyles(RepositoryAnalysisContext context)
    {
        var styles = new List<string>();
        
        // Based on frameworks and structure
        if (context.Structure.Frameworks.Contains("ASP.NET Core"))
        {
            styles.Add("MVC");
            styles.Add("REST API");
        }
        
        if (context.Structure.Frameworks.Contains("React"))
        {
            styles.Add("Component-Based");
            styles.Add("SPA");
        }
        
        // Based on directory structure
        var directories = context.Structure.DirectoryPurpose.Keys.ToList();
        
        if (directories.Any(d => d.Contains("controller", StringComparison.OrdinalIgnoreCase)))
            styles.Add("MVC");
        
        if (directories.Any(d => d.Contains("service", StringComparison.OrdinalIgnoreCase)))
            styles.Add("Service Layer");
        
        if (directories.Any(d => d.Contains("repository", StringComparison.OrdinalIgnoreCase)))
            styles.Add("Repository Pattern");
        
        return styles.Distinct().ToList();
    }

    private async Task<List<string>> IdentifyDesignPatternsAsync(RepositoryAnalysisContext context, CancellationToken cancellationToken)
    {
        var patterns = new List<string>();
        
        // Analyze important files for patterns
        foreach (var file in context.ImportantFiles)
        {
            if (file.Content != null)
            {
                if (file.Content.Contains("interface ") || file.Content.Contains("abstract "))
                    patterns.Add("Interface Pattern");
                
                if (file.Content.Contains("Factory") || file.Content.Contains("Builder"))
                    patterns.Add("Factory Pattern");
                
                if (file.Content.Contains("Singleton"))
                    patterns.Add("Singleton Pattern");
                
                if (file.Content.Contains("Observer") || file.Content.Contains("Event"))
                    patterns.Add("Observer Pattern");
                
                if (file.Content.Contains("Strategy"))
                    patterns.Add("Strategy Pattern");
            }
        }
        
        return patterns.Distinct().ToList();
    }

    private List<string> IdentifyProgrammingParadigms(RepositoryAnalysisContext context)
    {
        var paradigms = new List<string>();
        
        // Based on language and patterns
        if (context.PrimaryLanguage.ToLowerInvariant().Contains("java") || 
            context.PrimaryLanguage.ToLowerInvariant().Contains("c#"))
        {
            paradigms.Add("Object-Oriented");
        }
        
        if (context.Structure.Frameworks.Contains("React") || context.PrimaryLanguage.Contains("JavaScript"))
        {
            paradigms.Add("Functional");
        }
        
        if (context.Dependencies.Any(d => d.Name.Contains("async") || d.Name.Contains("promise")))
        {
            paradigms.Add("Asynchronous");
        }
        
        return paradigms.Distinct().ToList();
    }

    private Dictionary<string, string> GeneratePatternExplanations(ArchitecturalPatterns patterns)
    {
        var explanations = new Dictionary<string, string>();
        
        foreach (var style in patterns.ArchitecturalStyles)
        {
            explanations[style] = style switch
            {
                "MVC" => "Model-View-Controller pattern separating concerns into data, presentation, and logic layers",
                "REST API" => "RESTful web services following REST architectural constraints",
                "Component-Based" => "Modular architecture using reusable components",
                "SPA" => "Single Page Application with dynamic content updates",
                "Service Layer" => "Business logic organized into service classes",
                "Repository Pattern" => "Data access abstraction layer",
                _ => $"Architectural pattern: {style}"
            };
        }
        
        return explanations;
    }

    private List<string> AssessCodeQualityIndicators(RepositoryAnalysisContext context)
    {
        var indicators = new List<string>();
        
        if (context.Structure.TestFiles.Any())
            indicators.Add("Has Test Coverage");
        
        if (context.Structure.DocumentationFiles.Any())
            indicators.Add("Has Documentation");
        
        if (context.Structure.ConfigurationFiles.Any(f => f.Contains("lint") || f.Contains(".editorconfig")))
            indicators.Add("Uses Linting");
        
        if (context.Dependencies.Any(d => d.Name.Contains("test") || d.Name.Contains("jest") || d.Name.Contains("xunit")))
            indicators.Add("Uses Testing Framework");
        
        return indicators;
    }

    private static string DetermineDirectoryPurpose(string directory, IEnumerable<GitHubTreeItem> files)
    {
        var dirName = Path.GetFileName(directory)?.ToLowerInvariant();
        
        return dirName switch
        {
            "src" or "source" => "Source Code",
            "test" or "tests" => "Tests",
            "doc" or "docs" or "documentation" => "Documentation",
            "config" or "configuration" => "Configuration",
            "build" or "dist" or "bin" => "Build Output",
            "lib" or "libs" or "libraries" => "Libraries",
            "assets" or "static" => "Static Assets",
            "components" => "UI Components",
            "services" => "Business Logic",
            "models" => "Data Models",
            "controllers" => "Request Handlers",
            "views" => "User Interface",
            "utils" or "utilities" => "Utility Functions",
            _ => string.Empty
        };
    }

    private static List<string> FindEntryPoints(IEnumerable<GitHubTreeItem> files)
    {
        var entryPoints = new List<string>();
        
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file.Path).ToLowerInvariant();
            
            if (fileName == "main.py" || fileName == "app.py" || fileName == "index.js" || 
                fileName == "program.cs" || fileName == "main.java" || fileName == "main.go")
            {
                entryPoints.Add(file.Path);
            }
        }
        
        return entryPoints;
    }

    private static List<string> FindConfigurationFiles(IEnumerable<GitHubTreeItem> files)
    {
        var configFiles = new List<string>();
        var configPatterns = new[] { "config", "settings", "appsettings", ".env", "web.config" };
        
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file.Path).ToLowerInvariant();
            
            if (configPatterns.Any(pattern => fileName.Contains(pattern)) ||
                fileName.EndsWith(".json") || fileName.EndsWith(".yaml") || fileName.EndsWith(".yml") ||
                fileName.EndsWith(".ini") || fileName.EndsWith(".conf"))
            {
                configFiles.Add(file.Path);
            }
        }
        
        return configFiles;
    }

    private static List<string> FindTestFiles(IEnumerable<GitHubTreeItem> files)
    {
        var testFiles = new List<string>();
        
        foreach (var file in files)
        {
            var path = file.Path.ToLowerInvariant();
            
            if (path.Contains("test") || path.Contains("spec") || 
                path.EndsWith("tests.py") || path.EndsWith("test.js") || 
                path.EndsWith("tests.cs"))
            {
                testFiles.Add(file.Path);
            }
        }
        
        return testFiles;
    }

    private static List<string> FindDocumentationFiles(IEnumerable<GitHubTreeItem> files)
    {
        var docFiles = new List<string>();
        
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file.Path).ToLowerInvariant();
            
            if (fileName == "readme.md" || fileName == "readme.txt" || 
                fileName.Contains("doc") || fileName.EndsWith(".md"))
            {
                docFiles.Add(file.Path);
            }
        }
        
        return docFiles;
    }

    private static string DetermineLanguageFromExtension(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".cs" => "C#",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            ".py" => "Python",
            ".java" => "Java",
            ".go" => "Go",
            ".rs" => "Rust",
            ".cpp" or ".cc" or ".cxx" => "C++",
            ".c" => "C",
            ".php" => "PHP",
            ".rb" => "Ruby",
            ".swift" => "Swift",
            ".kt" => "Kotlin",
            ".scala" => "Scala",
            ".clj" => "Clojure",
            ".hs" => "Haskell",
            ".elm" => "Elm",
            ".dart" => "Dart",
            _ => string.Empty
        };
    }

    private static string DetermineFilePurpose(string filePath, string? content)
    {
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        
        if (fileName.Contains("test") || fileName.Contains("spec"))
            return "Test file";
        if (fileName.Contains("config") || fileName.Contains("settings"))
            return "Configuration file";
        if (fileName == "readme.md" || fileName.Contains("doc"))
            return "Documentation";
        if (fileName.Contains("main") || fileName.Contains("app") || fileName.Contains("index"))
            return "Entry point";
        if (fileName.Contains("controller"))
            return "Request handler";
        if (fileName.Contains("service"))
            return "Business logic";
        if (fileName.Contains("model") || fileName.Contains("entity"))
            return "Data model";
        if (fileName.Contains("component"))
            return "UI component";
        if (fileName.Contains("util"))
            return "Utility functions";
        
        return "Source code";
    }

    private static double CalculateFileImportanceScore(string filePath, string? content)
    {
        var score = 0.0;
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        var path = filePath.ToLowerInvariant();
        
        // Entry points are very important
        if (fileName == "main.py" || fileName == "app.py" || fileName == "index.js" || 
            fileName == "program.cs" || fileName == "main.java")
            score += 1.0;
        
        // Configuration files are important
        if (fileName.Contains("config") || fileName.Contains("settings") || fileName.Contains("package.json"))
            score += 0.8;
        
        // Documentation is moderately important
        if (fileName.Contains("readme") || fileName.EndsWith(".md"))
            score += 0.6;
        
        // Core source files in src directory
        if (path.Contains("/src/") || path.Contains("\\src\\"))
            score += 0.7;
        
        // Controllers and services are important
        if (fileName.Contains("controller") || fileName.Contains("service"))
            score += 0.8;
        
        // Models and entities
        if (fileName.Contains("model") || fileName.Contains("entity"))
            score += 0.6;
        
        // Test files are less important for documentation
        if (fileName.Contains("test") || fileName.Contains("spec"))
            score += 0.3;
        
        // Reduce score for generated files
        if (path.Contains("/bin/") || path.Contains("/obj/") || path.Contains("/node_modules/"))
            score *= 0.1;
        
        return Math.Min(score, 1.0);
    }

    private static List<string> ExtractKeyConcepts(string? content, string language)
    {
        var concepts = new List<string>();
        
        if (string.IsNullOrEmpty(content)) return concepts;
        
        // Extract based on language patterns
        switch (language.ToLowerInvariant())
        {
            case "c#":
                concepts.AddRange(ExtractCSharpConcepts(content));
                break;
            case "javascript":
            case "typescript":
                concepts.AddRange(ExtractJavaScriptConcepts(content));
                break;
            case "python":
                concepts.AddRange(ExtractPythonConcepts(content));
                break;
            default:
                concepts.AddRange(ExtractGenericConcepts(content));
                break;
        }
        
        return concepts.Distinct().Take(10).ToList(); // Limit to top 10 concepts
    }

    private static List<string> ExtractCSharpConcepts(string content)
    {
        var concepts = new List<string>();
        
        if (content.Contains("class ")) concepts.Add("Classes");
        if (content.Contains("interface ")) concepts.Add("Interfaces");
        if (content.Contains("async ")) concepts.Add("Async Programming");
        if (content.Contains("Entity") || content.Contains("DbContext")) concepts.Add("Entity Framework");
        if (content.Contains("[ApiController]") || content.Contains("Controller")) concepts.Add("Web API");
        if (content.Contains("dependency injection") || content.Contains("IServiceCollection")) concepts.Add("Dependency Injection");
        
        return concepts;
    }

    private static List<string> ExtractJavaScriptConcepts(string content)
    {
        var concepts = new List<string>();
        
        if (content.Contains("function") || content.Contains("=>")) concepts.Add("Functions");
        if (content.Contains("class ")) concepts.Add("Classes");
        if (content.Contains("async") || content.Contains("await")) concepts.Add("Async Programming");
        if (content.Contains("React") || content.Contains("useState")) concepts.Add("React");
        if (content.Contains("express")) concepts.Add("Express.js");
        if (content.Contains("import ") || content.Contains("require")) concepts.Add("Modules");
        
        return concepts;
    }

    private static List<string> ExtractPythonConcepts(string content)
    {
        var concepts = new List<string>();
        
        if (content.Contains("def ")) concepts.Add("Functions");
        if (content.Contains("class ")) concepts.Add("Classes");
        if (content.Contains("async def")) concepts.Add("Async Programming");
        if (content.Contains("Flask") || content.Contains("@app.route")) concepts.Add("Flask");
        if (content.Contains("Django") || content.Contains("models.Model")) concepts.Add("Django");
        if (content.Contains("import ") || content.Contains("from ")) concepts.Add("Modules");
        
        return concepts;
    }

    private static List<string> ExtractGenericConcepts(string content)
    {
        var concepts = new List<string>();
        
        if (content.Contains("function") || content.Contains("def ")) concepts.Add("Functions");
        if (content.Contains("class")) concepts.Add("Classes");
        if (content.Contains("api") || content.Contains("API")) concepts.Add("API");
        if (content.Contains("database") || content.Contains("db")) concepts.Add("Database");
        if (content.Contains("test")) concepts.Add("Testing");
        
        return concepts;
    }

    #endregion
}