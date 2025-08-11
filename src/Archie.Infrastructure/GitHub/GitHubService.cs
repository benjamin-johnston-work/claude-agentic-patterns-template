using Archie.Infrastructure.Configuration;
using Archie.Infrastructure.GitHub.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using System.Text.RegularExpressions;

namespace Archie.Infrastructure.GitHub;

/// <summary>
/// GitHub API service implementation using Octokit
/// </summary>
public class GitHubService : IGitHubService
{
    private readonly GitHubOptions _options;
    private readonly ILogger<GitHubService> _logger;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private static readonly Regex GitHubUrlPattern = new(@"https://github\.com/([^/]+)/([^/]+?)(?:\.git)?/?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public GitHubService(IOptions<GitHubOptions> options, ILogger<GitHubService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rateLimitSemaphore = new SemaphoreSlim(_options.RateLimitBuffer, _options.RateLimitBuffer);
    }

    public async Task<GitHubRepositoryInfo> GetRepositoryAsync(string owner, string repository, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting repository information for {Owner}/{Repository}", owner, repository);
        
        var client = CreateGitHubClient(accessToken);
        
        return await ExecuteWithRateLimitProtection(async () =>
        {
            var repo = await client.Repository.Get(owner, repository);
            
            return new GitHubRepositoryInfo
            {
                Id = repo.Id,
                Name = repo.Name,
                FullName = repo.FullName,
                Description = repo.Description ?? string.Empty,
                CloneUrl = repo.CloneUrl,
                HtmlUrl = repo.HtmlUrl,
                Language = repo.Language,
                IsPrivate = repo.Private,
                IsFork = repo.Fork,
                IsArchived = repo.Archived,
                IsDisabled = repo.Archived, // Use Archived as a proxy for disabled
                DefaultBranch = repo.DefaultBranch,
                CreatedAt = repo.CreatedAt.DateTime,
                UpdatedAt = repo.UpdatedAt.DateTime,
                PushedAt = repo.PushedAt?.DateTime,
                Size = (int)repo.Size,
                StargazersCount = repo.StargazersCount,
                ForksCount = repo.ForksCount,
                OpenIssuesCount = repo.OpenIssuesCount,
                License = repo.License?.Name ?? string.Empty,
                Owner = new GitHubOwnerInfo
                {
                    Id = repo.Owner.Id,
                    Login = repo.Owner.Login,
                    Type = repo.Owner.Type.ToString(),
                    AvatarUrl = repo.Owner.AvatarUrl,
                    HtmlUrl = repo.Owner.HtmlUrl
                }
            };
        }, cancellationToken);
    }

    public async Task<IEnumerable<GitHubBranchInfo>> GetBranchesAsync(string owner, string repository, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting branches for {Owner}/{Repository}", owner, repository);
        
        var client = CreateGitHubClient(accessToken);
        
        return await ExecuteWithRateLimitProtection(async () =>
        {
            var branches = await client.Repository.Branch.GetAll(owner, repository);
            
            return branches.Select(branch => new GitHubBranchInfo
            {
                Name = branch.Name,
                IsProtected = branch.Protected,
                Commit = branch.Commit != null ? new GitHubCommitInfo
                {
                    Sha = branch.Commit.Sha,
                    Message = "No commit message",
                    AuthorName = "Unknown",
                    AuthorEmail = "unknown@example.com",
                    AuthorDate = DateTime.UtcNow,
                    CommitterName = "Unknown",
                    CommitterEmail = "unknown@example.com",
                    CommitterDate = DateTime.UtcNow,
                    Url = ""
                } : null
            }).ToList();
        }, cancellationToken);
    }

    public async Task<GitHubTree> GetRepositoryTreeWithMetadataAsync(string owner, string repository, string branch = "main", bool recursive = true, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting repository tree for {Owner}/{Repository} on branch {Branch} (recursive: {Recursive})", owner, repository, branch, recursive);
        
        var client = CreateGitHubClient(accessToken);
        
        return await ExecuteWithRateLimitProtection(async () =>
        {
            try
            {
                // First get the branch to get the commit SHA
                var branchInfo = await client.Repository.Branch.Get(owner, repository, branch);
                var commitSha = branchInfo.Commit.Sha;

                // Get the git tree recursively
                var tree = await client.Git.Tree.GetRecursive(owner, repository, commitSha);

                // Convert Octokit TreeItem to our GitHubTreeItem model
                var treeItems = tree.Tree
                    .Select(item => new GitHubTreeItem
                    {
                        Path = item.Path,
                        Mode = item.Mode,
                        Type = item.Type.ToString().ToLowerInvariant(),
                        Sha = item.Sha,
                        Size = item.Size,
                        Url = item.Url
                    })
                    .ToList();

                _logger.LogDebug("Retrieved {FileCount} items from repository tree for {Owner}/{Repository}", 
                    treeItems.Count, owner, repository);

                return new GitHubTree
                {
                    Sha = tree.Sha,
                    Url = tree.Url,
                    Truncated = tree.Truncated,
                    Tree = treeItems
                };
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("Repository or branch not found: {Owner}/{Repository}/{Branch}", owner, repository, branch);
                return new GitHubTree
                {
                    Sha = "",
                    Url = "",
                    Truncated = false,
                    Tree = new List<GitHubTreeItem>()
                };
            }
        }, cancellationToken);
    }

    public async Task<IEnumerable<GitHubCommitInfo>> GetCommitHistoryAsync(string owner, string repository, string branch, int limit = 100, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting commit history for {Owner}/{Repository} on branch {Branch} (limit: {Limit})", owner, repository, branch, limit);
        
        var client = CreateGitHubClient(accessToken);
        
        return await ExecuteWithRateLimitProtection(async () =>
        {
            var request = new ApiOptions
            {
                PageSize = Math.Min(limit, 100),
                PageCount = 1
            };

            var commits = await client.Repository.Commit.GetAll(owner, repository, new CommitRequest { Sha = branch }, request);
            
            return commits.Take(limit).Select(commit => new GitHubCommitInfo
            {
                Sha = commit.Sha,
                Message = commit.Commit.Message,
                AuthorName = commit.Commit.Author?.Name,
                AuthorEmail = commit.Commit.Author?.Email,
                AuthorDate = commit.Commit.Author?.Date.DateTime,
                CommitterName = commit.Commit.Committer?.Name,
                CommitterEmail = commit.Commit.Committer?.Email,
                CommitterDate = commit.Commit.Committer?.Date.DateTime,
                Url = commit.Url
            }).ToList();
        }, cancellationToken);
    }

    public async Task<bool> ValidateRepositoryAccessAsync(string owner, string repository, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Validating repository access for {Owner}/{Repository}", owner, repository);
            
            var client = CreateGitHubClient(accessToken);
            
            return await ExecuteWithRateLimitProtection(async () =>
            {
                await client.Repository.Get(owner, repository);
                return true;
            }, cancellationToken);
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Repository {Owner}/{Repository} not found or not accessible", owner, repository);
            return false;
        }
        catch (ForbiddenException)
        {
            _logger.LogWarning("Access forbidden to repository {Owner}/{Repository}", owner, repository);
            return false;
        }
        catch (AuthorizationException)
        {
            _logger.LogWarning("Unauthorized access to repository {Owner}/{Repository}", owner, repository);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating repository access for {Owner}/{Repository}", owner, repository);
            return false;
        }
    }

    public (string Owner, string Repository) ParseRepositoryUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        var match = GitHubUrlPattern.Match(url.Trim());
        if (!match.Success)
        {
            throw new ArgumentException($"Invalid GitHub repository URL: {url}", nameof(url));
        }

        return (match.Groups[1].Value, match.Groups[2].Value);
    }

    public async Task<List<string>> GetRepositoryTreeAsync(string owner, string repository, string branch, bool recursive = true, string? accessToken = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting repository file tree for {Owner}/{Repository} on branch {Branch} (recursive: {Recursive})", 
            owner, repository, branch, recursive);
        
        var client = CreateGitHubClient(accessToken);
        
        return await ExecuteWithRateLimitProtection(async () =>
        {
            try
            {
                // First get the branch to get the commit SHA
                var branchInfo = await client.Repository.Branch.Get(owner, repository, branch);
                var commitSha = branchInfo.Commit.Sha;

                // Get the git tree recursively
                var tree = await client.Git.Tree.GetRecursive(owner, repository, commitSha);

                // Filter to only blob (file) entries and extract paths
                return tree.Tree
                    .Where(item => item.Type == TreeType.Blob)
                    .Select(item => item.Path)
                    .ToList();
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("Repository or branch not found: {Owner}/{Repository}/{Branch}", owner, repository, branch);
                return new List<string>();
            }
        }, cancellationToken);
    }

    public async Task<string> GetFileContentAsync(string owner, string repository, string filePath, string branch = "main", string? accessToken = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting file content for {Owner}/{Repository}/{Branch}/{FilePath}", 
            owner, repository, branch, filePath);
        
        var client = CreateGitHubClient(accessToken);
        
        return await ExecuteWithRateLimitProtection(async () =>
        {
            try
            {
                var contents = await client.Repository.Content.GetAllContentsByRef(owner, repository, filePath, branch);
                
                if (contents?.Count > 0)
                {
                    var fileContent = contents.First();
                    
                    // GitHub API returns base64 encoded content for files
                    if (fileContent.Type == ContentType.File && !string.IsNullOrEmpty(fileContent.Content))
                    {
                        try
                        {
                            var bytes = Convert.FromBase64String(fileContent.Content);
                            return System.Text.Encoding.UTF8.GetString(bytes);
                        }
                        catch (FormatException)
                        {
                            // If it's not base64, return as is (shouldn't happen but just in case)
                            return fileContent.Content;
                        }
                    }
                }
                
                return string.Empty;
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("File not found: {Owner}/{Repository}/{Branch}/{FilePath}", owner, repository, branch, filePath);
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file content: {Owner}/{Repository}/{Branch}/{FilePath}", owner, repository, branch, filePath);
                return string.Empty;
            }
        }, cancellationToken);
    }

    private GitHubClient CreateGitHubClient(string? accessToken = null)
    {
        var client = new GitHubClient(new ProductHeaderValue(_options.UserAgent));

        var token = accessToken ?? _options.DefaultAccessToken;
        if (!string.IsNullOrEmpty(token))
        {
            client.Credentials = new Credentials(token);
        }

        return client;
    }

    private async Task<T> ExecuteWithRateLimitProtection<T>(Func<Task<T>> operation, CancellationToken cancellationToken)
    {
        if (!_options.EnableRateLimitProtection)
        {
            return await operation();
        }

        await _rateLimitSemaphore.WaitAsync(cancellationToken);

        try
        {
            return await operation();
        }
        catch (RateLimitExceededException ex)
        {
            _logger.LogWarning("GitHub API rate limit exceeded. Reset at: {ResetTime}", ex.Reset);
            
            var delay = ex.Reset - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                _logger.LogInformation("Waiting {DelaySeconds} seconds for rate limit reset", delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
            
            // Retry the operation
            return await operation();
        }
        catch (AbuseException)
        {
            _logger.LogWarning("GitHub API abuse detected. Waiting 60 seconds before retry");
            
            var delay = TimeSpan.FromSeconds(60); // GitHub usually requires 60 seconds for abuse rate limiting
            await Task.Delay(delay, cancellationToken);
            
            // Retry the operation
            return await operation();
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }
}