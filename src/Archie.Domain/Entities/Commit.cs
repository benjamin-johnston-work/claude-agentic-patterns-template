using Archie.Domain.Common;

namespace Archie.Domain.Entities;

public class Commit : BaseEntity
{
    public string Hash { get; private set; }
    public string Message { get; private set; }
    public string Author { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Guid RepositoryId { get; private set; }

    protected Commit() // EF Constructor
    {
        Hash = string.Empty;
        Message = string.Empty;
        Author = string.Empty;
    }

    public Commit(string hash, string message, string author, DateTime timestamp, Guid repositoryId)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Hash cannot be null or empty", nameof(hash));
        
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be null or empty", nameof(author));
        
        if (repositoryId == Guid.Empty)
            throw new ArgumentException("Repository ID cannot be empty", nameof(repositoryId));

        Hash = hash;
        Message = string.IsNullOrWhiteSpace(message) ? "No commit message" : message;
        Author = author;
        Timestamp = timestamp;
        RepositoryId = repositoryId;
    }

    public void Update(string message, string author, DateTime timestamp)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new ArgumentException("Author cannot be null or empty", nameof(author));

        Message = string.IsNullOrWhiteSpace(message) ? "No commit message" : message;
        Author = author;
        Timestamp = timestamp;
    }
}