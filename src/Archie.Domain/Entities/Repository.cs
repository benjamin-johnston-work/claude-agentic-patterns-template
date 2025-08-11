using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Entities;

public class Repository : BaseEntity, IAggregateRoot
{
    private readonly List<Branch> _branches = new();

    public string Name { get; private set; }
    public string Url { get; private set; }
    public string Owner { get; private set; } = string.Empty; // NEW: GitHub owner/org
    public string FullName { get; private set; } = string.Empty; // NEW: owner/repo format
    public string CloneUrl { get; private set; } = string.Empty; // NEW: for cloning
    public string Language { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastPushedAt { get; private set; } // NEW
    
    // Repository Metadata
    public bool IsPrivate { get; private set; } // NEW
    public bool IsFork { get; private set; } // NEW
    public bool IsArchived { get; private set; } // NEW
    public string DefaultBranch { get; private set; } = "main"; // NEW
    public RepositoryStatus Status { get; private set; }
    public IReadOnlyList<Branch> Branches => _branches.AsReadOnly();
    public RepositoryStatistics Statistics { get; private set; }

    protected Repository() // EF Constructor
    {
        Name = string.Empty;
        Url = string.Empty;
        Owner = string.Empty;
        FullName = string.Empty;
        CloneUrl = string.Empty;
        Language = string.Empty;
        Statistics = RepositoryStatistics.Empty;
    }

    public Repository(string name, string url, string owner, string language, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Repository name cannot be null or empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Repository URL cannot be null or empty", nameof(url));
        
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("Repository owner cannot be null or empty", nameof(owner));
        
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Repository language cannot be null or empty", nameof(language));

        if (!IsValidUrl(url))
            throw new ArgumentException("Repository URL is not valid", nameof(url));

        Name = name;
        Url = url;
        Owner = owner;
        FullName = $"{owner}/{name}";
        CloneUrl = url.EndsWith(".git") ? url : $"{url}.git";
        Language = language;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Status = RepositoryStatus.Connecting;
        Statistics = RepositoryStatistics.Empty;
    }

    public void UpdateStatus(RepositoryStatus status)
    {
        if (Status == status)
            return;

        var validTransitions = GetValidStatusTransitions(Status);
        if (!validTransitions.Contains(status))
            throw new InvalidOperationException($"Cannot transition from {Status} to {status}");

        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDefaultBranch(string defaultBranch)
    {
        if (string.IsNullOrWhiteSpace(defaultBranch))
            throw new ArgumentException("Default branch cannot be null or empty", nameof(defaultBranch));
        
        DefaultBranch = defaultBranch;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatistics(RepositoryStatistics statistics)
    {
        Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMetadata(bool? isPrivate = null, bool? isFork = null, bool? isArchived = null, 
                              string? defaultBranch = null, DateTime? lastPushedAt = null)
    {
        if (isPrivate.HasValue) IsPrivate = isPrivate.Value;
        if (isFork.HasValue) IsFork = isFork.Value;
        if (isArchived.HasValue) IsArchived = isArchived.Value;
        if (!string.IsNullOrWhiteSpace(defaultBranch)) DefaultBranch = defaultBranch;
        if (lastPushedAt.HasValue) LastPushedAt = lastPushedAt.Value;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddBranch(Branch branch)
    {
        if (branch == null)
            throw new ArgumentNullException(nameof(branch));

        if (branch.RepositoryId != Id)
            throw new ArgumentException("Branch does not belong to this repository");

        if (_branches.Any(b => b.Name.Equals(branch.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Branch '{branch.Name}' already exists");

        // If this is the first branch or marked as default, ensure only one default exists
        if (branch.IsDefault)
        {
            foreach (var existingBranch in _branches.Where(b => b.IsDefault))
            {
                existingBranch.UnmarkAsDefault();
            }
        }

        _branches.Add(branch);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveBranch(string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
            throw new ArgumentException("Branch name cannot be null or empty", nameof(branchName));

        var branch = _branches.FirstOrDefault(b => b.Name.Equals(branchName, StringComparison.OrdinalIgnoreCase));
        if (branch == null)
            return;

        if (branch.IsDefault && _branches.Count > 1)
            throw new InvalidOperationException("Cannot remove the default branch when other branches exist");

        _branches.Remove(branch);
        UpdatedAt = DateTime.UtcNow;
    }

    public Branch? GetDefaultBranch()
    {
        return _branches.FirstOrDefault(b => b.IsDefault);
    }

    public Branch? GetBranch(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return _branches.FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsReady() => Status == RepositoryStatus.Ready;

    public bool IsConnected() => Status == RepositoryStatus.Connected || Status == RepositoryStatus.Analyzing || Status == RepositoryStatus.Ready;

    public bool HasError() => Status == RepositoryStatus.Error;

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static List<RepositoryStatus> GetValidStatusTransitions(RepositoryStatus currentStatus)
    {
        return currentStatus switch
        {
            RepositoryStatus.Connecting => new List<RepositoryStatus> { RepositoryStatus.Connected, RepositoryStatus.Error, RepositoryStatus.Disconnected },
            RepositoryStatus.Connected => new List<RepositoryStatus> { RepositoryStatus.Analyzing, RepositoryStatus.Error, RepositoryStatus.Disconnected },
            RepositoryStatus.Analyzing => new List<RepositoryStatus> { RepositoryStatus.Ready, RepositoryStatus.Error, RepositoryStatus.Disconnected },
            RepositoryStatus.Ready => new List<RepositoryStatus> { RepositoryStatus.Analyzing, RepositoryStatus.Error, RepositoryStatus.Disconnected },
            RepositoryStatus.Error => new List<RepositoryStatus> { RepositoryStatus.Connecting, RepositoryStatus.Disconnected },
            RepositoryStatus.Disconnected => new List<RepositoryStatus> { RepositoryStatus.Connecting },
            _ => new List<RepositoryStatus>()
        };
    }
}