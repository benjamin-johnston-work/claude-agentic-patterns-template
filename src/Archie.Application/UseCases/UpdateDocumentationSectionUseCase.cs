using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class UpdateDocumentationSectionUseCase
{
    private readonly IDocumentationRepository _documentationRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<UpdateDocumentationSectionUseCase> _logger;

    public UpdateDocumentationSectionUseCase(
        IDocumentationRepository documentationRepository,
        IEventPublisher eventPublisher,
        ILogger<UpdateDocumentationSectionUseCase> logger)
    {
        _documentationRepository = documentationRepository ?? throw new ArgumentNullException(nameof(documentationRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<DocumentationSectionDto>> ExecuteAsync(
        UpdateDocumentationSectionInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating documentation section: {SectionId} for documentation: {DocumentationId}", 
                input.SectionId, input.DocumentationId);

            // Get documentation
            var documentation = await _documentationRepository.GetByIdAsync(input.DocumentationId, cancellationToken);
            if (documentation == null)
            {
                _logger.LogWarning("Documentation not found: {DocumentationId}", input.DocumentationId);
                return Result<DocumentationSectionDto>.Failure("Documentation not found");
            }

            // Find the section
            var section = documentation.GetSection(input.SectionId);
            if (section == null)
            {
                _logger.LogWarning("Documentation section not found: {SectionId}", input.SectionId);
                return Result<DocumentationSectionDto>.Failure("Documentation section not found");
            }

            // Update the section
            documentation.UpdateSection(input.SectionId, input.Content);

            // Update tags if provided
            if (input.Tags.Any())
            {
                section.ClearTags();
                foreach (var tag in input.Tags)
                {
                    section.AddTag(tag);
                }
            }

            // Save changes
            documentation = await _documentationRepository.UpdateAsync(documentation, cancellationToken);

            // Get the updated section
            var updatedSection = documentation.GetSection(input.SectionId);
            if (updatedSection == null)
            {
                _logger.LogError("Failed to retrieve updated section: {SectionId}", input.SectionId);
                return Result<DocumentationSectionDto>.Failure("Failed to update section");
            }

            _logger.LogInformation("Successfully updated documentation section: {SectionId}", input.SectionId);

            return Result<DocumentationSectionDto>.Success(MapSectionToDto(updatedSection));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating documentation section: {SectionId}", input.SectionId);
            return Result<DocumentationSectionDto>.Failure($"Failed to update documentation section: {ex.Message}");
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