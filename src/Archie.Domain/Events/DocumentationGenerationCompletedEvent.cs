using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Events;

public class DocumentationGenerationCompletedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid DocumentationId { get; }
    public Guid RepositoryId { get; }
    public DocumentationStatistics Statistics { get; }
    public TimeSpan TotalGenerationTime { get; }

    public DocumentationGenerationCompletedEvent(
        Guid documentationId, 
        Guid repositoryId, 
        DocumentationStatistics statistics,
        TimeSpan totalGenerationTime)
    {
        DocumentationId = documentationId;
        RepositoryId = repositoryId;
        Statistics = statistics;
        TotalGenerationTime = totalGenerationTime;
    }
}