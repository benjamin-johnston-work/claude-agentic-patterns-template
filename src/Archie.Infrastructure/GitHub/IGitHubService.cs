using Archie.Infrastructure.GitHub.Models;

namespace Archie.Infrastructure.GitHub;

/// <summary>
/// Interface for GitHub API operations
/// </summary>
public interface IGitHubService
{
    /// <summary>
    /// Get repository information from GitHub
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repository">Repository name</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository information</returns>
    Task<GitHubRepositoryInfo> GetRepositoryAsync(string owner, string repository, string? accessToken = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all branches for a repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repository">Repository name</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of branches</returns>
    Task<IEnumerable<GitHubBranchInfo>> GetBranchesAsync(string owner, string repository, string? accessToken = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get repository file tree structure with full metadata
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repository">Repository name</param>
    /// <param name="branch">Branch name</param>
    /// <param name="recursive">Whether to get recursive tree</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository tree structure</returns>
    Task<GitHubTree> GetRepositoryTreeWithMetadataAsync(string owner, string repository, string branch = "main", bool recursive = true, string? accessToken = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get commit history for a repository branch
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repository">Repository name</param>
    /// <param name="branch">Branch name</param>
    /// <param name="limit">Maximum number of commits to retrieve</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of commits</returns>
    Task<IEnumerable<GitHubCommitInfo>> GetCommitHistoryAsync(string owner, string repository, string branch, int limit = 100, string? accessToken = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate that a repository exists and is accessible
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repository">Repository name</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if repository is accessible, false otherwise</returns>
    Task<bool> ValidateRepositoryAccessAsync(string owner, string repository, string? accessToken = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Parse a GitHub repository URL to extract owner and repository name
    /// </summary>
    /// <param name="url">GitHub repository URL</param>
    /// <returns>Tuple of owner and repository name</returns>
    (string Owner, string Repository) ParseRepositoryUrl(string url);

    /// <summary>
    /// Get repository file tree as a list of file paths
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repository">Repository name</param>
    /// <param name="branch">Branch name</param>
    /// <param name="recursive">Whether to get recursive tree</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of file paths</returns>
    Task<List<string>> GetRepositoryTreeAsync(string owner, string repository, string branch, bool recursive = true, string? accessToken = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the content of a specific file from a repository
    /// </summary>
    /// <param name="owner">Repository owner</param>
    /// <param name="repository">Repository name</param>
    /// <param name="filePath">Path to the file</param>
    /// <param name="branch">Branch name</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File content as string</returns>
    Task<string> GetFileContentAsync(string owner, string repository, string filePath, string branch = "main", string? accessToken = null, CancellationToken cancellationToken = default);
}