using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Archie.Infrastructure.Configuration;
using Archie.Infrastructure.GitHub;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Archie.Infrastructure.Services;

public class GitRepositoryService : IGitRepositoryService
{
    private readonly GitOptions _options;
    private readonly ILogger<GitRepositoryService> _logger;
    private readonly IGitHubService _gitHubService;
    private readonly SemaphoreSlim _requestSemaphore;

    public GitRepositoryService(
        IOptions<GitOptions> options,
        ILogger<GitRepositoryService> logger,
        IGitHubService gitHubService)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gitHubService = gitHubService ?? throw new ArgumentNullException(nameof(gitHubService));
        _requestSemaphore = new SemaphoreSlim(_options.MaxConcurrentRequests, _options.MaxConcurrentRequests);
    }

    public async Task<Domain.Entities.Repository> ConnectRepositoryAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        await _requestSemaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Connecting to repository: {Url}", url);

            // Parse GitHub URL
            var (owner, repository) = _gitHubService.ParseRepositoryUrl(url);
            
            // Get repository information from GitHub API
            var repoInfo = await _gitHubService.GetRepositoryAsync(owner, repository, accessToken, cancellationToken);
            
            var domainRepo = new Domain.Entities.Repository(
                repoInfo.Name,
                repoInfo.HtmlUrl,
                repoInfo.Owner.Login,
                repoInfo.Language ?? "Unknown",
                repoInfo.Description);

            // Set the default branch from GitHub information
            domainRepo.UpdateDefaultBranch(repoInfo.DefaultBranch ?? "main");

            // Get and add branches
            var branches = await _gitHubService.GetBranchesAsync(owner, repository, accessToken, cancellationToken);
            
            foreach (var branchInfo in branches)
            {
                var lastCommit = branchInfo.Commit != null ? new Domain.Entities.Commit(
                    branchInfo.Commit.Sha,
                    !string.IsNullOrWhiteSpace(branchInfo.Commit.Message) ? branchInfo.Commit.Message : "No commit message",
                    !string.IsNullOrWhiteSpace(branchInfo.Commit.AuthorName) ? branchInfo.Commit.AuthorName : "Unknown",
                    branchInfo.Commit.AuthorDate ?? DateTime.UtcNow,
                    domainRepo.Id) : null;

                var domainBranch = new Domain.Entities.Branch(
                    branchInfo.Name,
                    branchInfo.Name == repoInfo.DefaultBranch,
                    domainRepo.Id,
                    lastCommit);

                domainRepo.AddBranch(domainBranch);
            }

            _logger.LogInformation("Successfully connected to repository: {Url}", url);
            
            return domainRepo;
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    public async Task<IEnumerable<Domain.Entities.Branch>> GetBranchesAsync(string repositoryUrl, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        await _requestSemaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Getting branches for repository: {Url}", repositoryUrl);

            var (owner, repository) = _gitHubService.ParseRepositoryUrl(repositoryUrl);
            var branches = await _gitHubService.GetBranchesAsync(owner, repository, accessToken, cancellationToken);
            
            var domainBranches = new List<Domain.Entities.Branch>();
            
            foreach (var branchInfo in branches)
            {
                var lastCommit = branchInfo.Commit != null ? new Domain.Entities.Commit(
                    branchInfo.Commit.Sha,
                    branchInfo.Commit.Message,
                    branchInfo.Commit.AuthorName ?? "Unknown",
                    branchInfo.Commit.AuthorDate ?? DateTime.UtcNow,
                    Guid.NewGuid()) : null;

                domainBranches.Add(new Domain.Entities.Branch(
                    branchInfo.Name,
                    false, // We don't have default branch info in this context
                    Guid.NewGuid(),
                    lastCommit));
            }

            return domainBranches;
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    public async Task<IEnumerable<Domain.Entities.Commit>> GetCommitHistoryAsync(string repositoryUrl, string branch, int limit = 100, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        await _requestSemaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Getting commit history for repository: {Url}, branch: {Branch}", repositoryUrl, branch);

            var (owner, repository) = _gitHubService.ParseRepositoryUrl(repositoryUrl);
            var commits = await _gitHubService.GetCommitHistoryAsync(owner, repository, branch, limit, accessToken, cancellationToken);
            
            return commits.Select(commitInfo => new Domain.Entities.Commit(
                commitInfo.Sha,
                commitInfo.Message,
                commitInfo.AuthorName ?? "Unknown",
                commitInfo.AuthorDate ?? DateTime.UtcNow,
                Guid.NewGuid())).ToList();
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    public async Task<RepositoryStatistics> AnalyzeRepositoryStructureAsync(string repositoryUrl, string branch = "main", string? accessToken = null, CancellationToken cancellationToken = default)
    {
        await _requestSemaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Analyzing repository structure: {Url}, branch: {Branch}", repositoryUrl, branch);

            var (owner, repository) = _gitHubService.ParseRepositoryUrl(repositoryUrl);
            var tree = await _gitHubService.GetRepositoryTreeWithMetadataAsync(owner, repository, branch, true, accessToken, cancellationToken);
            
            var languageStats = new ConcurrentDictionary<string, (int fileCount, int lineCount)>();
            var totalFiles = 0;
            var totalLines = 0;

            // Filter to only blob (file) entries and exclude ignored paths
            var files = tree.Tree
                .Where(item => item.Type == "blob" && !IsIgnoredPath(item.Path))
                .ToArray();

            Parallel.ForEach(files, file =>
            {
                try
                {
                    var extension = Path.GetExtension(file.Path);
                    var language = GetLanguageFromExtension(extension);
                    
                    if (language != "Unknown")
                    {
                        // For GitHub API analysis, we can't get line counts without downloading each file
                        // So we'll estimate based on file size (rough approximation)
                        var estimatedLines = EstimateLinesFromSize(file.Size ?? 0);
                        
                        languageStats.AddOrUpdate(language,
                            (1, estimatedLines),
                            (key, value) => (value.fileCount + 1, value.lineCount + estimatedLines));

                        Interlocked.Increment(ref totalFiles);
                        Interlocked.Add(ref totalLines, estimatedLines);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing file: {File}", file.Path);
                }
            });

            var languageBreakdown = languageStats.ToDictionary(
                kvp => kvp.Key,
                kvp => new LanguageStats(
                    kvp.Key,
                    kvp.Value.fileCount,
                    kvp.Value.lineCount,
                    totalFiles > 0 ? (double)kvp.Value.fileCount / totalFiles * 100 : 0));

            _logger.LogInformation("Analysis complete. Files: {FileCount}, Estimated Lines: {LineCount}", totalFiles, totalLines);

            return new RepositoryStatistics(totalFiles, totalLines, languageBreakdown);
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    public async Task<bool> ValidateRepositoryAccessAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating repository access: {Url}", url);

            var (owner, repository) = _gitHubService.ParseRepositoryUrl(url);
            return await _gitHubService.ValidateRepositoryAccessAsync(owner, repository, accessToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Repository access validation failed: {Url}", url);
            return false;
        }
    }

    private static int EstimateLinesFromSize(int sizeInBytes)
    {
        // Rough estimation: average 50 characters per line (including whitespace and newlines)
        // This is a very rough approximation since we can't get actual line counts from GitHub API without downloading files
        return sizeInBytes > 0 ? Math.Max(1, sizeInBytes / 50) : 1;
    }

    private static string GetLanguageFromExtension(string extension)
    {
        return extension?.ToLowerInvariant() switch
        {
            ".cs" => "C#",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            ".py" => "Python",
            ".java" => "Java",
            ".cpp" or ".cc" or ".cxx" => "C++",
            ".c" => "C",
            ".go" => "Go",
            ".rs" => "Rust",
            ".php" => "PHP",
            ".rb" => "Ruby",
            ".swift" => "Swift",
            ".kt" => "Kotlin",
            ".scala" => "Scala",
            ".html" => "HTML",
            ".css" => "CSS",
            ".sql" => "SQL",
            ".sh" => "Shell",
            ".ps1" => "PowerShell",
            ".yml" or ".yaml" => "YAML",
            ".json" => "JSON",
            ".xml" => "XML",
            ".md" => "Markdown",
            _ => "Unknown"
        };
    }

    private static bool IsIgnoredPath(string path)
    {
        var ignoredPatterns = new[]
        {
            @"\.git[\\/]",
            @"node_modules[\\/]",
            @"bin[\\/]",
            @"obj[\\/]",
            @"\.vs[\\/]",
            @"\.vscode[\\/]",
            @"packages[\\/]",
            @"\.nuget[\\/]"
        };

        return ignoredPatterns.Any(pattern => Regex.IsMatch(path, pattern, RegexOptions.IgnoreCase));
    }

}