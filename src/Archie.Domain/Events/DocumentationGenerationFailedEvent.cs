using Archie.Domain.Common;

namespace Archie.Domain.Events;

public class DocumentationGenerationFailedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid DocumentationId { get; }
    public Guid RepositoryId { get; }
    public string ErrorMessage { get; }
    public string? ErrorDetails { get; }

    public DocumentationGenerationFailedEvent(
        Guid documentationId, 
        Guid repositoryId, 
        string errorMessage,
        string? errorDetails = null)
    {
        DocumentationId = documentationId;
        RepositoryId = repositoryId;
        ErrorMessage = errorMessage;
        ErrorDetails = errorDetails;
    }
}