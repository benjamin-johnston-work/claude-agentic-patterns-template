using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Application.UseCases;
using Archie.Domain.ValueObjects;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;

namespace Archie.Api.GraphQL.Resolvers;

[ExtendObjectType(typeof(Query))]
public class DocumentationQueryResolver
{
    private readonly ILogger<DocumentationQueryResolver> _logger;

    public DocumentationQueryResolver(ILogger<DocumentationQueryResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get documentation for a specific repository
    /// </summary>
    public async Task<DocumentationDto?> GetDocumentationAsync(
        [ID] Guid repositoryId,
        GetDocumentationUseCase useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting documentation for repository: {RepositoryId}", repositoryId);
            
            var result = await useCase.ExecuteAsync(repositoryId, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogDebug("Successfully retrieved documentation for repository: {RepositoryId}", repositoryId);
                return result.Value;
            }
            
            _logger.LogWarning("Failed to get documentation for repository {RepositoryId}: {Error}", 
                repositoryId, result.Error);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documentation for repository: {RepositoryId}", repositoryId);
            return null;
        }
    }

    /// <summary>
    /// Get documentation by documentation ID
    /// </summary>
    public async Task<DocumentationDto?> GetDocumentationByIdAsync(
        [ID] Guid documentationId,
        [Service] IDocumentationRepository documentationRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting documentation by ID: {DocumentationId}", documentationId);
            
            var documentation = await documentationRepository.GetByIdAsync(documentationId, cancellationToken);
            
            if (documentation != null)
            {
                _logger.LogDebug("Successfully retrieved documentation: {DocumentationId}", documentationId);
                return MapToDto(documentation);
            }
            
            _logger.LogWarning("Documentation not found: {DocumentationId}", documentationId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documentation by ID: {DocumentationId}", documentationId);
            return null;
        }
    }

    /// <summary>
    /// Search documentation sections across repositories
    /// </summary>
    public async Task<List<DocumentationSectionDto>> SearchDocumentationAsync(
        string query,
        [ID] List<Guid>? repositoryIds,
        SearchDocumentationUseCase useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Searching documentation with query: '{Query}', Repository filter: {RepositoryCount}", 
                query, repositoryIds?.Count ?? 0);
            
            var result = await useCase.ExecuteAsync(query, repositoryIds, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogDebug("Found {SectionCount} documentation sections for query: '{Query}'", 
                    result.Value!.Count, query);
                return result.Value!;
            }
            
            _logger.LogWarning("Failed to search documentation with query '{Query}': {Error}", 
                query, result.Error);
            return new List<DocumentationSectionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documentation with query: '{Query}'", query);
            return new List<DocumentationSectionDto>();
        }
    }

    /// <summary>
    /// Get all documentation with a specific status
    /// </summary>
    public async Task<List<DocumentationDto>> GetDocumentationsByStatusAsync(
        DocumentationStatus status,
        [Service] IDocumentationRepository documentationRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting documentation by status: {Status}", status);
            
            var documentations = await documentationRepository.GetByStatusAsync(status, cancellationToken);
            var dtos = documentations.Select(MapToDto).ToList();
            
            _logger.LogDebug("Found {DocumentationCount} documentation items with status: {Status}", 
                dtos.Count, status);
            
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documentation by status: {Status}", status);
            return new List<DocumentationDto>();
        }
    }

    /// <summary>
    /// Get all documentation
    /// </summary>
    public async Task<List<DocumentationDto>> GetAllDocumentationAsync(
        [Service] IDocumentationRepository documentationRepository,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Getting all documentation");
            
            var documentations = await documentationRepository.GetAllAsync(cancellationToken);
            var dtos = documentations.Select(MapToDto).ToList();
            
            _logger.LogDebug("Retrieved {DocumentationCount} documentation items", dtos.Count);
            
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all documentation");
            return new List<DocumentationDto>();
        }
    }

    #region Private Helper Methods

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

    #endregion
}