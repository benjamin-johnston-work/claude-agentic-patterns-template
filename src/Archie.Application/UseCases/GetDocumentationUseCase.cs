using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class GetDocumentationUseCase
{
    private readonly IDocumentationRepository _documentationRepository;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<GetDocumentationUseCase> _logger;

    public GetDocumentationUseCase(
        IDocumentationRepository documentationRepository,
        IRepositoryRepository repositoryRepository,
        ILogger<GetDocumentationUseCase> logger)
    {
        _documentationRepository = documentationRepository ?? throw new ArgumentNullException(nameof(documentationRepository));
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<DocumentationDto?>> ExecuteAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving documentation for repository: {RepositoryId}", repositoryId);

            // Validate repository exists
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null)
            {
                _logger.LogWarning("Repository not found: {RepositoryId}", repositoryId);
                return Result<DocumentationDto?>.Failure("Repository not found");
            }

            // Get documentation
            var documentation = await _documentationRepository.GetByRepositoryIdAsync(repositoryId, cancellationToken);
            if (documentation == null)
            {
                _logger.LogInformation("No documentation found for repository: {RepositoryId}", repositoryId);
                return Result<DocumentationDto?>.Success(null);
            }

            _logger.LogInformation("Successfully retrieved documentation for repository: {RepositoryId}", repositoryId);
            return Result<DocumentationDto?>.Success(MapToDto(documentation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documentation for repository: {RepositoryId}", repositoryId);
            return Result<DocumentationDto?>.Failure($"Failed to retrieve documentation: {ex.Message}");
        }
    }

    private static DocumentationDto MapToDto(Domain.Entities.Documentation documentation)
    {
        return new DocumentationDto
        {
            Id = documentation.Id,
            RepositoryId = documentation.RepositoryId,
            Title = documentation.Title,
            Status = documentation.Status,
            Sections = documentation.Sections.OrderBy(s => s.Order).Select(s => new DocumentationSectionDto
            {
                Id = s.Id,
                Title = s.Title,
                Content = s.Content,
                Type = s.Type,
                Order = s.Order,
                CodeReferences = s.CodeReferences.Select(cr => new CodeReferenceDto
                {
                    FilePath = cr.FilePath,
                    StartLine = cr.StartLine,
                    EndLine = cr.EndLine,
                    CodeSnippet = cr.CodeSnippet,
                    Description = cr.Description,
                    ReferenceType = cr.ReferenceType
                }).ToList(),
                Tags = s.Tags.ToList(),
                Metadata = new SectionMetadataDto
                {
                    CreatedAt = s.Metadata.CreatedAt,
                    LastModifiedAt = s.Metadata.LastModifiedAt,
                    GeneratedBy = s.Metadata.GeneratedBy,
                    Model = s.Metadata.Model,
                    TokenCount = s.Metadata.TokenCount,
                    ConfidenceScore = s.Metadata.ConfidenceScore,
                    AdditionalProperties = s.Metadata.AdditionalProperties
                }
            }).ToList(),
            Metadata = new DocumentationMetadataDto
            {
                RepositoryName = documentation.Metadata.RepositoryName,
                RepositoryUrl = documentation.Metadata.RepositoryUrl,
                PrimaryLanguage = documentation.Metadata.PrimaryLanguage,
                Languages = documentation.Metadata.Languages,
                Frameworks = documentation.Metadata.Frameworks,
                Dependencies = documentation.Metadata.Dependencies,
                ProjectType = documentation.Metadata.ProjectType,
                CustomProperties = documentation.Metadata.CustomProperties
            },
            GeneratedAt = documentation.GeneratedAt,
            LastUpdatedAt = documentation.LastUpdatedAt,
            Version = documentation.Version,
            Statistics = new DocumentationStatisticsDto
            {
                TotalSections = documentation.Statistics.TotalSections,
                CodeReferences = documentation.Statistics.CodeReferences,
                WordCount = documentation.Statistics.WordCount,
                GenerationTimeSeconds = documentation.Statistics.GenerationTime.TotalSeconds,
                AccuracyScore = documentation.Statistics.AccuracyScore,
                CoveredTopics = documentation.Statistics.CoveredTopics
            },
            ErrorMessage = documentation.ErrorMessage,
            
            // Populate the missing frontend fields with calculated values
            TotalSections = documentation.Sections.Count(),
            EstimatedReadingTime = documentation.Sections.Sum(s => s.Content?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length ?? 0) / 200.0, // 200 words per minute
            LastGenerated = documentation.GeneratedAt,
            GenerationDuration = documentation.Statistics.GenerationTime.TotalSeconds,
            SectionsGenerated = documentation.Sections.Count(s => !string.IsNullOrWhiteSpace(s.Content))
        };
    }
}