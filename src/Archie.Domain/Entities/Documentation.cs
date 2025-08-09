using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Entities;

public class Documentation : BaseEntity, IAggregateRoot
{
    private readonly List<DocumentationSection> _sections = new();

    public Guid RepositoryId { get; private set; }
    public string Title { get; private set; }
    public DocumentationStatus Status { get; private set; }
    public IReadOnlyList<DocumentationSection> Sections => _sections.AsReadOnly();
    public DocumentationMetadata Metadata { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public string Version { get; private set; }
    public DocumentationStatistics Statistics { get; private set; }
    public string? ErrorMessage { get; private set; }

    protected Documentation() // EF Constructor
    {
        Title = string.Empty;
        Version = string.Empty;
        Metadata = new DocumentationMetadata("", "", "", "");
        Statistics = DocumentationStatistics.Empty;
    }

    public Documentation(Guid repositoryId, string title, DocumentationMetadata metadata)
    {
        if (repositoryId == Guid.Empty)
            throw new ArgumentException("Repository ID cannot be empty", nameof(repositoryId));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        RepositoryId = repositoryId;
        Title = title;
        Status = DocumentationStatus.NotStarted;
        Metadata = metadata;
        GeneratedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
        Version = "1.0.0";
        Statistics = DocumentationStatistics.Empty;
    }

    public static Documentation Create(Guid repositoryId, string title, DocumentationMetadata metadata)
    {
        return new Documentation(repositoryId, title, metadata);
    }

    public void UpdateStatus(DocumentationStatus status)
    {
        if (Status == status)
            return;

        var validTransitions = GetValidStatusTransitions(Status);
        if (!validTransitions.Contains(status))
            throw new InvalidOperationException($"Cannot transition from {Status} to {status}");

        Status = status;
        LastUpdatedAt = DateTime.UtcNow;

        // Clear error message when moving away from error state
        if (Status != DocumentationStatus.Error)
        {
            ErrorMessage = null;
        }
    }

    public void AddSection(DocumentationSection section)
    {
        if (section == null)
            throw new ArgumentNullException(nameof(section));

        // Check for duplicate section types (some types should be unique)
        var uniqueTypes = new[]
        {
            DocumentationSectionType.Overview,
            DocumentationSectionType.Architecture,
            DocumentationSectionType.License,
            DocumentationSectionType.Changelog
        };

        if (uniqueTypes.Contains(section.Type) && 
            _sections.Any(s => s.Type == section.Type))
        {
            throw new InvalidOperationException($"A section of type {section.Type} already exists");
        }

        // If no order specified or conflicts, set to next available order
        if (section.Order == 0 || _sections.Any(s => s.Order == section.Order))
        {
            var maxOrder = _sections.Any() ? _sections.Max(s => s.Order) : 0;
            section.UpdateOrder(maxOrder + 1);
        }

        _sections.Add(section);
        UpdateStatistics();
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSection(Guid sectionId, string content)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section == null)
            throw new InvalidOperationException($"Section with ID {sectionId} not found");

        section.UpdateContent(content);
        UpdateStatistics();
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void RemoveSection(Guid sectionId)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId);
        if (section == null)
            return;

        _sections.Remove(section);
        UpdateStatistics();
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void ReorderSections()
    {
        var orderedSections = _sections
            .OrderBy(s => GetSectionTypeOrder(s.Type))
            .ThenBy(s => s.Order)
            .ToList();

        _sections.Clear();
        for (int i = 0; i < orderedSections.Count; i++)
        {
            orderedSections[i].UpdateOrder(i + 1);
            _sections.Add(orderedSections[i]);
        }

        LastUpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted()
    {
        UpdateStatus(DocumentationStatus.Completed);
        IncrementVersion();
        UpdateStatistics();
    }

    public void MarkAsFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));

        ErrorMessage = errorMessage;
        UpdateStatus(DocumentationStatus.Error);
    }

    public bool RequiresRegeneration(DateTime repositoryLastModified)
    {
        return Status == DocumentationStatus.UpdateRequired ||
               Status == DocumentationStatus.Error ||
               repositoryLastModified > LastUpdatedAt;
    }

    public void MarkForRegeneration()
    {
        UpdateStatus(DocumentationStatus.UpdateRequired);
    }

    public DocumentationSection? GetSection(DocumentationSectionType type)
    {
        return _sections.FirstOrDefault(s => s.Type == type);
    }

    public DocumentationSection? GetSection(Guid sectionId)
    {
        return _sections.FirstOrDefault(s => s.Id == sectionId);
    }

    public List<DocumentationSection> GetSectionsByTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return new List<DocumentationSection>();

        return _sections.Where(s => s.HasTag(tag)).ToList();
    }

    public bool IsInProgress()
    {
        return Status == DocumentationStatus.Analyzing ||
               Status == DocumentationStatus.GeneratingContent ||
               Status == DocumentationStatus.Enriching ||
               Status == DocumentationStatus.Indexing;
    }

    public bool IsCompleted() => Status == DocumentationStatus.Completed;

    public bool HasError() => Status == DocumentationStatus.Error;

    public bool IsReady() => Status == DocumentationStatus.Completed;

    private void UpdateStatistics()
    {
        var totalWordCount = _sections.Sum(s => s.GetWordCount());
        var totalCodeReferences = _sections.Sum(s => s.CodeReferences.Count);
        var coveredTopics = _sections.SelectMany(s => s.Tags).Distinct().ToList();

        Statistics = new DocumentationStatistics(
            _sections.Count,
            totalCodeReferences,
            totalWordCount,
            Statistics.GenerationTime,
            Statistics.AccuracyScore,
            coveredTopics
        );
    }

    private void IncrementVersion()
    {
        var versionParts = Version.Split('.');
        if (versionParts.Length >= 3 && int.TryParse(versionParts[2], out var patch))
        {
            Version = $"{versionParts[0]}.{versionParts[1]}.{patch + 1}";
        }
        else
        {
            var versionNumber = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            Version = $"1.0.{versionNumber}";
        }
    }

    private static int GetSectionTypeOrder(DocumentationSectionType type)
    {
        return type switch
        {
            DocumentationSectionType.Overview => 1,
            DocumentationSectionType.GettingStarted => 2,
            DocumentationSectionType.Installation => 3,
            DocumentationSectionType.Usage => 4,
            DocumentationSectionType.Configuration => 5,
            DocumentationSectionType.Architecture => 6,
            DocumentationSectionType.ApiReference => 7,
            DocumentationSectionType.Examples => 8,
            DocumentationSectionType.Testing => 9,
            DocumentationSectionType.Deployment => 10,
            DocumentationSectionType.Contributing => 11,
            DocumentationSectionType.Troubleshooting => 12,
            DocumentationSectionType.Changelog => 13,
            DocumentationSectionType.License => 14,
            _ => 99
        };
    }

    private static List<DocumentationStatus> GetValidStatusTransitions(DocumentationStatus currentStatus)
    {
        return currentStatus switch
        {
            DocumentationStatus.NotStarted => new List<DocumentationStatus> 
            { 
                DocumentationStatus.Analyzing, 
                DocumentationStatus.Error 
            },
            DocumentationStatus.Analyzing => new List<DocumentationStatus> 
            { 
                DocumentationStatus.GeneratingContent, 
                DocumentationStatus.Error 
            },
            DocumentationStatus.GeneratingContent => new List<DocumentationStatus> 
            { 
                DocumentationStatus.Enriching, 
                DocumentationStatus.Indexing,
                DocumentationStatus.Error 
            },
            DocumentationStatus.Enriching => new List<DocumentationStatus> 
            { 
                DocumentationStatus.Indexing, 
                DocumentationStatus.Error 
            },
            DocumentationStatus.Indexing => new List<DocumentationStatus> 
            { 
                DocumentationStatus.Completed, 
                DocumentationStatus.Error 
            },
            DocumentationStatus.Completed => new List<DocumentationStatus> 
            { 
                DocumentationStatus.UpdateRequired,
                DocumentationStatus.Analyzing 
            },
            DocumentationStatus.Error => new List<DocumentationStatus> 
            { 
                DocumentationStatus.NotStarted,
                DocumentationStatus.Analyzing 
            },
            DocumentationStatus.UpdateRequired => new List<DocumentationStatus> 
            { 
                DocumentationStatus.Analyzing 
            },
            _ => new List<DocumentationStatus>()
        };
    }
}