using Archie.Domain.Common;

namespace Archie.Domain.Entities;

public class Branch : BaseEntity
{
    public string Name { get; private set; }
    public bool IsDefault { get; private set; }
    public Commit? LastCommit { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid RepositoryId { get; private set; }

    protected Branch() // EF Constructor
    {
        Name = string.Empty;
    }

    public Branch(string name, bool isDefault, Guid repositoryId, Commit? lastCommit = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Branch name cannot be null or empty", nameof(name));
        
        if (repositoryId == Guid.Empty)
            throw new ArgumentException("Repository ID cannot be empty", nameof(repositoryId));

        Name = name;
        IsDefault = isDefault;
        RepositoryId = repositoryId;
        LastCommit = lastCommit;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateLastCommit(Commit commit)
    {
        LastCommit = commit ?? throw new ArgumentNullException(nameof(commit));
    }

    public void MarkAsDefault()
    {
        IsDefault = true;
    }

    public void UnmarkAsDefault()
    {
        IsDefault = false;
    }
}