using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class SearchDocumentationUseCase
{
    private readonly IDocumentationRepository _documentationRepository;
    private readonly ILogger<SearchDocumentationUseCase> _logger;

    public SearchDocumentationUseCase(
        IDocumentationRepository documentationRepository,
        ILogger<SearchDocumentationUseCase> logger)
    {
        _documentationRepository = documentationRepository ?? throw new ArgumentNullException(nameof(documentationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<DocumentationSectionDto>>> ExecuteAsync(
        string query,
        List<Guid>? repositoryIds = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Result<List<DocumentationSectionDto>>.Failure("Search query cannot be empty");
            }

            _logger.LogInformation("Searching documentation with query: '{Query}', Repository filter: {RepositoryCount}", 
                query, repositoryIds?.Count ?? 0);

            // Search documentation sections
            var sections = await _documentationRepository.SearchSectionsAsync(
                query, 
                repositoryIds, 
                cancellationToken);

            var sectionDtos = sections.Select(MapSectionToDto).ToList();

            _logger.LogInformation("Found {SectionCount} documentation sections matching query: '{Query}'", 
                sectionDtos.Count, query);

            return Result<List<DocumentationSectionDto>>.Success(sectionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documentation with query: '{Query}'", query);
            return Result<List<DocumentationSectionDto>>.Failure($"Failed to search documentation: {ex.Message}");
        }
    }

    private static DocumentationSectionDto MapSectionToDto(Domain.Entities.DocumentationSection section)
    {
        return new DocumentationSectionDto
        {
            Id = section.Id,
            Title = section.Title,
            Content = section.Content,
            Type = section.Type,
            Order = section.Order,
            CodeReferences = section.CodeReferences.Select(cr => new CodeReferenceDto
            {
                FilePath = cr.FilePath,
                StartLine = cr.StartLine,
                EndLine = cr.EndLine,
                CodeSnippet = cr.CodeSnippet,
                Description = cr.Description,
                ReferenceType = cr.ReferenceType
            }).ToList(),
            Tags = section.Tags.ToList(),
            Metadata = new SectionMetadataDto
            {
                CreatedAt = section.Metadata.CreatedAt,
                LastModifiedAt = section.Metadata.LastModifiedAt,
                GeneratedBy = section.Metadata.GeneratedBy,
                Model = section.Metadata.Model,
                TokenCount = section.Metadata.TokenCount,
                ConfidenceScore = section.Metadata.ConfidenceScore,
                AdditionalProperties = section.Metadata.AdditionalProperties
            }
        };
    }
}