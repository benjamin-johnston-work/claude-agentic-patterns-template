namespace Archie.Infrastructure.GitHub.Models;

/// <summary>
/// Represents basic repository information from GitHub API
/// </summary>
public class GitHubRepositoryInfo
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CloneUrl { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public string? Language { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsFork { get; set; }
    public bool IsArchived { get; set; }
    public bool IsDisabled { get; set; }
    public string DefaultBranch { get; set; } = "main";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PushedAt { get; set; }
    public int Size { get; set; }
    public int StargazersCount { get; set; }
    public int ForksCount { get; set; }
    public int OpenIssuesCount { get; set; }
    public string? License { get; set; }
    public GitHubOwnerInfo Owner { get; set; } = new();
}

/// <summary>
/// Represents repository owner information from GitHub API
/// </summary>
public class GitHubOwnerInfo
{
    public long Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // User or Organization
    public string AvatarUrl { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
}

/// <summary>
/// Represents branch information from GitHub API
/// </summary>
public class GitHubBranchInfo
{
    public string Name { get; set; } = string.Empty;
    public bool IsProtected { get; set; }
    public GitHubCommitInfo? Commit { get; set; }
}

/// <summary>
/// Represents commit information from GitHub API
/// </summary>
public class GitHubCommitInfo
{
    public string Sha { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? AuthorName { get; set; }
    public string? AuthorEmail { get; set; }
    public DateTime? AuthorDate { get; set; }
    public string? CommitterName { get; set; }
    public string? CommitterEmail { get; set; }
    public DateTime? CommitterDate { get; set; }
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Represents tree/file information from GitHub API
/// </summary>
public class GitHubTreeItem
{
    public string Path { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // blob, tree, commit
    public string Sha { get; set; } = string.Empty;
    public int? Size { get; set; }
    public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Represents a GitHub tree response
/// </summary>
public class GitHubTree
{
    public string Sha { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool Truncated { get; set; }
    public List<GitHubTreeItem> Tree { get; set; } = new();
}