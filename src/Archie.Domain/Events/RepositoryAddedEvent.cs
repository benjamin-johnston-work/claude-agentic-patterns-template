using Archie.Domain.Common;

namespace Archie.Domain.Events;

public record RepositoryAddedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid RepositoryId { get; }
    public string Url { get; }

    public RepositoryAddedEvent(Guid repositoryId, string url)
    {
        RepositoryId = repositoryId;
        Url = url;
    }
}