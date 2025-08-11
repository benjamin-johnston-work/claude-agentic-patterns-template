using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

public interface IGitRepositoryService
{
    /// <summary>
    /// Connect to a repository using GitHub API and extract metadata
    /// </summary>
    /// <param name="url">Repository URL</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository domain entity with metadata</returns>
    Task<Repository> ConnectRepositoryAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get branches for a repository using GitHub API
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository branches</returns>
    Task<IEnumerable<Branch>> GetBranchesAsync(string repositoryUrl, string? accessToken = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get commit history for a repository branch using GitHub API
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="branch">Branch name</param>
    /// <param name="limit">Maximum number of commits to retrieve</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Commit history</returns>
    Task<IEnumerable<Commit>> GetCommitHistoryAsync(string repositoryUrl, string branch, int limit = 100, string? accessToken = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analyze repository structure using GitHub API
    /// </summary>
    /// <param name="repositoryUrl">Repository URL</param>
    /// <param name="branch">Branch name to analyze</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Repository statistics</returns>
    Task<RepositoryStatistics> AnalyzeRepositoryStructureAsync(string repositoryUrl, string branch = "main", string? accessToken = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validate repository access using GitHub API
    /// </summary>
    /// <param name="url">Repository URL</param>
    /// <param name="accessToken">Optional access token for private repositories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if accessible, false otherwise</returns>
    Task<bool> ValidateRepositoryAccessAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default);
}