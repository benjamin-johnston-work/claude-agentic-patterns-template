using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Events;

public record RepositoryAnalysisCompletedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid RepositoryId { get; }
    public RepositoryStatistics Statistics { get; }

    public RepositoryAnalysisCompletedEvent(Guid repositoryId, RepositoryStatistics statistics)
    {
        RepositoryId = repositoryId;
        Statistics = statistics;
    }
}