using Archie.Domain.ValueObjects;

namespace Archie.Application.DTOs;

public class DocumentationDto
{
    public Guid Id { get; set; }
    public Guid RepositoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentationStatus Status { get; set; }
    public List<DocumentationSectionDto> Sections { get; set; } = new();
    public DocumentationMetadataDto Metadata { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string Version { get; set; } = string.Empty;
    public DocumentationStatisticsDto Statistics { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class DocumentationSectionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DocumentationSectionType Type { get; set; }
    public int Order { get; set; }
    public List<CodeReferenceDto> CodeReferences { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public SectionMetadataDto Metadata { get; set; } = new();
}

public class DocumentationMetadataDto
{
    public string RepositoryName { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public string PrimaryLanguage { get; set; } = string.Empty;
    public List<string> Languages { get; set; } = new();
    public List<string> Frameworks { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public string ProjectType { get; set; } = string.Empty;
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}

public class DocumentationStatisticsDto
{
    public int TotalSections { get; set; }
    public int CodeReferences { get; set; }
    public int WordCount { get; set; }
    public double GenerationTimeSeconds { get; set; }
    public double AccuracyScore { get; set; }
    public List<string> CoveredTopics { get; set; } = new();
}

public class CodeReferenceDto
{
    public string FilePath { get; set; } = string.Empty;
    public int? StartLine { get; set; }
    public int? EndLine { get; set; }
    public string CodeSnippet { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
}

public class SectionMetadataDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? GeneratedBy { get; set; }
    public string? Model { get; set; }
    public int TokenCount { get; set; }
    public double? ConfidenceScore { get; set; }
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
}

public class GenerateDocumentationInput
{
    public Guid RepositoryId { get; set; }
    public List<DocumentationSectionType> Sections { get; set; } = new();
    public bool IncludeCodeExamples { get; set; } = true;
    public bool IncludeApiReference { get; set; } = true;
    public string CustomInstructions { get; set; } = string.Empty;
    public bool Regenerate { get; set; } = false;
}

public class UpdateDocumentationSectionInput
{
    public Guid DocumentationId { get; set; }
    public Guid SectionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class DocumentationGenerationProgressDto
{
    public Guid RepositoryId { get; set; }
    public DocumentationStatus Status { get; set; }
    public double Progress { get; set; } // 0.0 to 1.0
    public string? CurrentSection { get; set; }
    public double? EstimatedTimeRemainingSeconds { get; set; }
    public string? Message { get; set; }
}