using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Archie.Infrastructure.Configuration;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Archie.Infrastructure.Services;

public class GitRepositoryService : IGitRepositoryService
{
    private readonly GitOptions _options;
    private readonly ILogger<GitRepositoryService> _logger;
    private readonly SemaphoreSlim _cloneSemaphore;

    public GitRepositoryService(
        IOptions<GitOptions> options,
        ILogger<GitRepositoryService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cloneSemaphore = new SemaphoreSlim(_options.MaxConcurrentClones, _options.MaxConcurrentClones);
    }

    public async Task<Domain.Entities.Repository> CloneRepositoryAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        await _cloneSemaphore.WaitAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Cloning repository: {Url}", url);

            var tempPath = Path.Combine(_options.TempDirectory, "archie_clone_" + Guid.NewGuid().ToString());
            
            var cloneOptions = new CloneOptions
            {
                IsBare = false,
                Checkout = true,
                RecurseSubmodules = false
            };

            // Note: Authentication with access tokens will be implemented in a future iteration
            // For now, only public repositories are supported
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogWarning("Access token provided but authentication is not yet implemented. Only public repositories are supported.");
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(_options.CloneTimeoutMinutes));

            var repoPath = await Task.Run(() =>
            {
                try
                {
                    return LibGit2Sharp.Repository.Clone(url, tempPath, cloneOptions);
                }
                catch (LibGit2SharpException ex)
                {
                    _logger.LogError(ex, "Failed to clone repository: {Url}", url);
                    throw new InvalidOperationException($"Failed to clone repository: {ex.Message}", ex);
                }
            }, cts.Token);

            using var repo = new LibGit2Sharp.Repository(repoPath);
            
            var repositoryName = ExtractRepositoryName(url);
            var language = DetectPrimaryLanguage(repoPath);
            var description = ""; // Could be extracted from README or remote info

            var domainRepo = new Domain.Entities.Repository(repositoryName, url, language, description);

            // Add branches
            foreach (var branch in repo.Branches.Where(b => !b.IsRemote || b.IsTracking))
            {
                var lastCommit = branch.Tip != null ? new Domain.Entities.Commit(
                    branch.Tip.Sha,
                    branch.Tip.MessageShort,
                    branch.Tip.Author.Name,
                    branch.Tip.Author.When.DateTime,
                    domainRepo.Id) : null;

                var domainBranch = new Domain.Entities.Branch(
                    branch.FriendlyName,
                    repo.Head?.FriendlyName == branch.FriendlyName,
                    domainRepo.Id,
                    lastCommit);

                domainRepo.AddBranch(domainBranch);
            }

            _logger.LogInformation("Successfully cloned repository: {Url}", url);
            
            // Cleanup
            if (_options.CleanupAfterAnalysis && Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }

            return domainRepo;
        }
        finally
        {
            _cloneSemaphore.Release();
        }
    }

    public async Task<IEnumerable<Domain.Entities.Branch>> GetBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(repositoryPath) && !Uri.IsWellFormedUriString(repositoryPath, UriKind.Absolute))
            {
                throw new ArgumentException("Invalid repository path", nameof(repositoryPath));
            }

            var branches = new List<Domain.Entities.Branch>();
            
            if (Directory.Exists(repositoryPath))
            {
                using var repo = new LibGit2Sharp.Repository(repositoryPath);
                foreach (var branch in repo.Branches.Where(b => !b.IsRemote || b.IsTracking))
                {
                    var lastCommit = branch.Tip != null ? new Domain.Entities.Commit(
                        branch.Tip.Sha,
                        branch.Tip.MessageShort,
                        branch.Tip.Author.Name,
                        branch.Tip.Author.When.DateTime,
                        Guid.Empty) : null;

                    branches.Add(new Domain.Entities.Branch(
                        branch.FriendlyName,
                        repo.Head?.FriendlyName == branch.FriendlyName,
                        Guid.Empty,
                        lastCommit));
                }
            }

            return branches.AsEnumerable();
        }, cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Commit>> GetCommitHistoryAsync(string repositoryPath, string branch, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            if (!Directory.Exists(repositoryPath))
            {
                throw new ArgumentException("Repository path does not exist", nameof(repositoryPath));
            }

            var commits = new List<Domain.Entities.Commit>();

            using var repo = new LibGit2Sharp.Repository(repositoryPath);
            var targetBranch = repo.Branches[branch] ?? repo.Head;
            
            if (targetBranch?.Tip != null)
            {
                var commitFilter = new CommitFilter
                {
                    SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time,
                    FirstParentOnly = false
                };

                foreach (var commit in repo.Commits.QueryBy(commitFilter).Take(limit))
                {
                    commits.Add(new Domain.Entities.Commit(
                        commit.Sha,
                        commit.MessageShort,
                        commit.Author.Name,
                        commit.Author.When.DateTime,
                        Guid.Empty));
                }
            }

            return commits.AsEnumerable();
        }, cancellationToken);
    }

    public async Task<RepositoryStatistics> AnalyzeRepositoryStructureAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Analyzing repository structure: {Path}", repositoryPath);

            if (!Directory.Exists(repositoryPath))
            {
                throw new ArgumentException("Repository path does not exist", nameof(repositoryPath));
            }

            var languageStats = new ConcurrentDictionary<string, (int fileCount, int lineCount)>();
            var totalFiles = 0;
            var totalLines = 0;

            var files = Directory.GetFiles(repositoryPath, "*", SearchOption.AllDirectories)
                .Where(f => !IsIgnoredPath(f))
                .ToArray();

            Parallel.ForEach(files, file =>
            {
                try
                {
                    var extension = Path.GetExtension(file);
                    var language = GetLanguageFromExtension(extension);
                    
                    if (language != "Unknown" && IsTextFile(file))
                    {
                        var lineCount = File.ReadAllLines(file).Length;
                        
                        languageStats.AddOrUpdate(language,
                            (1, lineCount),
                            (key, value) => (value.fileCount + 1, value.lineCount + lineCount));

                        Interlocked.Increment(ref totalFiles);
                        Interlocked.Add(ref totalLines, lineCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing file: {File}", file);
                }
            });

            var languageBreakdown = languageStats.ToDictionary(
                kvp => kvp.Key,
                kvp => new LanguageStats(
                    kvp.Key,
                    kvp.Value.fileCount,
                    kvp.Value.lineCount,
                    totalFiles > 0 ? (double)kvp.Value.fileCount / totalFiles * 100 : 0));

            _logger.LogInformation("Analysis complete. Files: {FileCount}, Lines: {LineCount}", totalFiles, totalLines);

            return new RepositoryStatistics(totalFiles, totalLines, languageBreakdown);
        }, cancellationToken);
    }

    public async Task<bool> ValidateRepositoryAccessAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating repository access: {Url}", url);

            await Task.Run(() =>
            {
                var references = LibGit2Sharp.Repository.ListRemoteReferences(url);
                return references.Any();
            }, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Repository access validation failed: {Url}", url);
            return false;
        }
    }

    private static string ExtractRepositoryName(string url)
    {
        var uri = new Uri(url);
        var segments = uri.Segments;
        var lastSegment = segments.LastOrDefault()?.TrimEnd('/');
        return lastSegment?.EndsWith(".git") == true ? lastSegment[..^4] : lastSegment ?? "Unknown";
    }

    private static string DetectPrimaryLanguage(string repoPath)
    {
        var languageCounts = new Dictionary<string, int>();

        try
        {
            var files = Directory.GetFiles(repoPath, "*", SearchOption.AllDirectories)
                .Where(f => !IsIgnoredPath(f))
                .Take(1000); // Limit for performance

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                var language = GetLanguageFromExtension(extension);
                
                if (language != "Unknown")
                {
                    languageCounts[language] = languageCounts.GetValueOrDefault(language, 0) + 1;
                }
            }

            return languageCounts.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
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

    private static bool IsTextFile(string filePath)
    {
        try
        {
            var info = new FileInfo(filePath);
            if (info.Length == 0) return true;
            if (info.Length > 1024 * 1024) return false; // Skip files larger than 1MB

            // Read a small portion to detect binary content
            using var stream = File.OpenRead(filePath);
            var buffer = new byte[1024];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            
            // Check for null bytes (common in binary files)
            for (var i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == 0) return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}