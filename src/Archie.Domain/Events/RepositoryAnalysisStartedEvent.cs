using Archie.Domain.Common;

namespace Archie.Domain.Events;

public record RepositoryAnalysisStartedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid RepositoryId { get; }

    public RepositoryAnalysisStartedEvent(Guid repositoryId)
    {
        RepositoryId = repositoryId;
    }
}