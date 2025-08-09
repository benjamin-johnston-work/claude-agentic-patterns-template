using Archie.Domain.Common;

namespace Archie.Domain.Events;

public class DocumentationGenerationStartedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid DocumentationId { get; }
    public Guid RepositoryId { get; }
    public List<string> RequestedSectionTypes { get; }

    public DocumentationGenerationStartedEvent(
        Guid documentationId, 
        Guid repositoryId, 
        List<string> requestedSectionTypes)
    {
        DocumentationId = documentationId;
        RepositoryId = repositoryId;
        RequestedSectionTypes = requestedSectionTypes ?? new List<string>();
    }
}