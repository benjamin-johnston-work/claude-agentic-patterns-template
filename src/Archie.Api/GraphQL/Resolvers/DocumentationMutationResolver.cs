using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Application.UseCases;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;

namespace Archie.Api.GraphQL.Resolvers;

[ExtendObjectType<Mutation>]
public class DocumentationMutationResolver
{
    private readonly ILogger<DocumentationMutationResolver> _logger;

    public DocumentationMutationResolver(ILogger<DocumentationMutationResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generate documentation for a repository
    /// </summary>
    public async Task<DocumentationDto?> GenerateDocumentationAsync(
        GenerateDocumentationInput input,
        GenerateDocumentationUseCase useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generating documentation for repository: {RepositoryId}", input.RepositoryId);
            
            var result = await useCase.ExecuteAsync(input, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully generated documentation for repository: {RepositoryId}", input.RepositoryId);
                return result.Value;
            }
            
            _logger.LogError("Failed to generate documentation for repository {RepositoryId}: {Error}", 
                input.RepositoryId, result.Error);
            
            // Throw GraphQL error to provide feedback to client
            throw new GraphQLException($"Failed to generate documentation: {result.Error}");
        }
        catch (GraphQLException)
        {
            throw; // Re-throw GraphQL exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating documentation for repository: {RepositoryId}", input.RepositoryId);
            throw new GraphQLException($"Error generating documentation: {ex.Message}");
        }
    }

    /// <summary>
    /// Update a documentation section
    /// </summary>
    public async Task<DocumentationSectionDto?> UpdateDocumentationSectionAsync(
        UpdateDocumentationSectionInput input,
        UpdateDocumentationSectionUseCase useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating documentation section: {SectionId} for documentation: {DocumentationId}", 
                input.SectionId, input.DocumentationId);
            
            var result = await useCase.ExecuteAsync(input, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated documentation section: {SectionId}", input.SectionId);
                return result.Value;
            }
            
            _logger.LogError("Failed to update documentation section {SectionId}: {Error}", 
                input.SectionId, result.Error);
            
            throw new GraphQLException($"Failed to update documentation section: {result.Error}");
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating documentation section: {SectionId}", input.SectionId);
            throw new GraphQLException($"Error updating documentation section: {ex.Message}");
        }
    }

    /// <summary>
    /// Regenerate a specific documentation section
    /// </summary>
    public async Task<DocumentationSectionDto?> RegenerateDocumentationSectionAsync(
        [ID] Guid documentationId,
        [ID] Guid sectionId,
        [Service] IDocumentationRepository documentationRepository,
        [Service] IAIDocumentationGeneratorService aiDocumentationService,
        [Service] IRepositoryAnalysisService repositoryAnalysisService,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Regenerating documentation section: {SectionId} for documentation: {DocumentationId}", 
                sectionId, documentationId);
            
            // Get the documentation
            var documentation = await documentationRepository.GetByIdAsync(documentationId, cancellationToken);
            if (documentation == null)
            {
                throw new GraphQLException("Documentation not found");
            }

            // Get the section
            var section = documentation.GetSection(sectionId);
            if (section == null)
            {
                throw new GraphQLException("Documentation section not found");
            }

            // Get repository analysis context
            var analysisContext = await repositoryAnalysisService.AnalyzeRepositoryAsync(
                documentation.RepositoryId, cancellationToken);

            // Regenerate the section
            var newSection = await aiDocumentationService.GenerateSectionAsync(
                analysisContext, section.Type, cancellationToken: cancellationToken);

            // Update the documentation
            documentation.UpdateSection(sectionId, newSection.Content);
            
            // Update code references and tags
            var updatedSection = documentation.GetSection(sectionId);
            if (updatedSection != null)
            {
                // Clear existing code references and add new ones
                foreach (var codeRef in newSection.CodeReferences)
                {
                    updatedSection.AddCodeReference(
                        codeRef.FilePath,
                        codeRef.CodeSnippet,
                        codeRef.Description,
                        codeRef.ReferenceType,
                        codeRef.StartLine,
                        codeRef.EndLine);
                }

                // Update tags
                updatedSection.ClearTags();
                foreach (var tag in newSection.Tags)
                {
                    updatedSection.AddTag(tag);
                }
            }

            // Save the updated documentation
            await documentationRepository.UpdateAsync(documentation, cancellationToken);

            _logger.LogInformation("Successfully regenerated documentation section: {SectionId}", sectionId);

            // Return the updated section
            var finalSection = documentation.GetSection(sectionId);
            return finalSection != null ? MapSectionToDto(finalSection) : null;
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating documentation section: {SectionId}", sectionId);
            throw new GraphQLException($"Error regenerating documentation section: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete documentation for a repository
    /// </summary>
    public async Task<bool> DeleteDocumentationAsync(
        [ID] Guid repositoryId,
        DeleteDocumentationUseCase useCase,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting documentation for repository: {RepositoryId}", repositoryId);
            
            var result = await useCase.ExecuteAsync(repositoryId, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted documentation for repository: {RepositoryId}", repositoryId);
                return result.Value;
            }
            
            _logger.LogError("Failed to delete documentation for repository {RepositoryId}: {Error}", 
                repositoryId, result.Error);
            
            throw new GraphQLException($"Failed to delete documentation: {result.Error}");
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting documentation for repository: {RepositoryId}", repositoryId);
            throw new GraphQLException($"Error deleting documentation: {ex.Message}");
        }
    }

    #region Private Helper Methods

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

    #endregion
}

public class Mutation
{
    // This class serves as a placeholder for the root Mutation type
    // Individual resolvers extend this using ExtendObjectType
}