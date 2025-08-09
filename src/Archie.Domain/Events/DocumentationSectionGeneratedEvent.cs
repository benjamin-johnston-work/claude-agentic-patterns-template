using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Events;

public class DocumentationSectionGeneratedEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    
    public Guid DocumentationId { get; }
    public Guid SectionId { get; }
    public Guid RepositoryId { get; }
    public DocumentationSectionType SectionType { get; }
    public int WordCount { get; }
    public int CodeReferencesCount { get; }

    public DocumentationSectionGeneratedEvent(
        Guid documentationId,
        Guid sectionId,
        Guid repositoryId,
        DocumentationSectionType sectionType,
        int wordCount,
        int codeReferencesCount)
    {
        DocumentationId = documentationId;
        SectionId = sectionId;
        RepositoryId = repositoryId;
        SectionType = sectionType;
        WordCount = wordCount;
        CodeReferencesCount = codeReferencesCount;
    }
}