using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.Events;
using Archie.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class GenerateDocumentationUseCase
{
    private readonly IDocumentationRepository _documentationRepository;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IAIDocumentationGeneratorService _aiDocumentationService;
    private readonly IRepositoryAnalysisService _repositoryAnalysisService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<GenerateDocumentationUseCase> _logger;

    public GenerateDocumentationUseCase(
        IDocumentationRepository documentationRepository,
        IRepositoryRepository repositoryRepository,
        IAIDocumentationGeneratorService aiDocumentationService,
        IRepositoryAnalysisService repositoryAnalysisService,
        IEventPublisher eventPublisher,
        ILogger<GenerateDocumentationUseCase> logger)
    {
        _documentationRepository = documentationRepository ?? throw new ArgumentNullException(nameof(documentationRepository));
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _aiDocumentationService = aiDocumentationService ?? throw new ArgumentNullException(nameof(aiDocumentationService));
        _repositoryAnalysisService = repositoryAnalysisService ?? throw new ArgumentNullException(nameof(repositoryAnalysisService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<DocumentationDto>> ExecuteAsync(
        GenerateDocumentationInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting documentation generation for repository: {RepositoryId}", input.RepositoryId);

            // Validate repository exists
            var repository = await _repositoryRepository.GetByIdAsync(input.RepositoryId, cancellationToken);
            if (repository == null)
            {
                _logger.LogWarning("Repository not found: {RepositoryId}", input.RepositoryId);
                return Result<DocumentationDto>.Failure("Repository not found");
            }

            if (!repository.IsReady() && !repository.IsConnected())
            {
                _logger.LogWarning("Repository is not ready for documentation generation: {RepositoryId}, Status: {Status}", 
                    input.RepositoryId, repository.Status);
                return Result<DocumentationDto>.Failure($"Repository is not ready for documentation generation. Current status: {repository.Status}");
            }

            // Check if documentation already exists and handle regeneration
            var existingDocumentation = await _documentationRepository.GetByRepositoryIdAsync(input.RepositoryId, cancellationToken);
            if (existingDocumentation != null && !input.Regenerate)
            {
                if (existingDocumentation.IsCompleted())
                {
                    _logger.LogInformation("Documentation already exists and is completed for repository: {RepositoryId}", input.RepositoryId);
                    return Result<DocumentationDto>.Success(MapToDto(existingDocumentation));
                }

                if (existingDocumentation.IsInProgress())
                {
                    _logger.LogInformation("Documentation generation already in progress for repository: {RepositoryId}", input.RepositoryId);
                    return Result<DocumentationDto>.Success(MapToDto(existingDocumentation));
                }
            }

            // Create documentation metadata
            var metadata = new DocumentationMetadata(
                repository.Name,
                repository.Url,
                repository.Language,
                DetermineProjectType(repository)
            );

            // Create or update documentation entity
            Documentation documentation;
            if (existingDocumentation != null)
            {
                documentation = existingDocumentation;
                documentation.UpdateStatus(DocumentationStatus.NotStarted);
                _logger.LogInformation("Regenerating existing documentation for repository: {RepositoryId}", input.RepositoryId);
            }
            else
            {
                documentation = Documentation.Create(input.RepositoryId, $"{repository.Name} Documentation", metadata);
                documentation = await _documentationRepository.AddAsync(documentation, cancellationToken);
                _logger.LogInformation("Created new documentation for repository: {RepositoryId}", input.RepositoryId);
            }

            // Publish generation started event
            var startedEvent = new DocumentationGenerationStartedEvent(
                documentation.Id,
                input.RepositoryId,
                input.Sections.Select(s => s.ToString()).ToList()
            );
            await _eventPublisher.PublishAsync(startedEvent, cancellationToken);

            // Create documentation generation options
            var options = new DocumentationGenerationOptions
            {
                RequestedSections = input.Sections.Any() ? input.Sections : 
                    DocumentationGenerationOptions.GetDefaultSections(metadata.ProjectType, metadata.PrimaryLanguage),
                IncludeCodeExamples = input.IncludeCodeExamples,
                IncludeApiReference = input.IncludeApiReference,
                CustomInstructions = input.CustomInstructions
            };

            _logger.LogInformation("Starting AI documentation generation with {SectionCount} sections", 
                options.RequestedSections.Count);

            // Generate documentation using AI service
            var startTime = DateTime.UtcNow;
            var generatedDocumentation = await _aiDocumentationService.GenerateDocumentationAsync(
                input.RepositoryId,
                options,
                cancellationToken
            );

            var generationTime = DateTime.UtcNow - startTime;
            
            // Update the documentation with generated content
            foreach (var section in generatedDocumentation.Sections)
            {
                documentation.AddSection(section);
            }

            // Update statistics with generation time
            var updatedStats = documentation.Statistics.WithUpdatedGenerationTime(generationTime);
            typeof(Documentation).GetProperty(nameof(Documentation.Statistics))?.SetValue(documentation, updatedStats);

            documentation.MarkAsCompleted();
            documentation = await _documentationRepository.UpdateAsync(documentation, cancellationToken);

            // Publish generation completed event
            var completedEvent = new DocumentationGenerationCompletedEvent(
                documentation.Id,
                input.RepositoryId,
                documentation.Statistics,
                generationTime
            );
            await _eventPublisher.PublishAsync(completedEvent, cancellationToken);

            _logger.LogInformation("Successfully completed documentation generation for repository: {RepositoryId} in {GenerationTime}ms", 
                input.RepositoryId, generationTime.TotalMilliseconds);

            return Result<DocumentationDto>.Success(MapToDto(documentation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating documentation for repository: {RepositoryId}", input.RepositoryId);

            // If we have a documentation entity, mark it as failed
            var existingDoc = await _documentationRepository.GetByRepositoryIdAsync(input.RepositoryId, cancellationToken);
            if (existingDoc != null)
            {
                existingDoc.MarkAsFailed(ex.Message);
                await _documentationRepository.UpdateAsync(existingDoc, cancellationToken);

                var failedEvent = new DocumentationGenerationFailedEvent(
                    existingDoc.Id,
                    input.RepositoryId,
                    ex.Message,
                    ex.StackTrace
                );
                await _eventPublisher.PublishAsync(failedEvent, cancellationToken);
            }

            return Result<DocumentationDto>.Failure($"Failed to generate documentation: {ex.Message}");
        }
    }

    private static string DetermineProjectType(Repository repository)
    {
        // Simple heuristics to determine project type based on language and name
        var language = repository.Language.ToLowerInvariant();
        var name = repository.Name.ToLowerInvariant();

        if (name.Contains("api") || name.Contains("service") || name.Contains("server"))
            return "Application";
        
        if (name.Contains("lib") || name.Contains("framework") || name.Contains("core"))
            return "Library";

        if (name.Contains("cli") || name.Contains("tool") || name.Contains("console"))
            return "Application";

        // Default based on language
        return language switch
        {
            "csharp" or "c#" => "Application",
            "javascript" or "typescript" => "Application",
            "python" => "Application",
            "java" => "Application",
            _ => "Library"
        };
    }

    private static DocumentationDto MapToDto(Documentation documentation)
    {
        return new DocumentationDto
        {
            Id = documentation.Id,
            RepositoryId = documentation.RepositoryId,
            Title = documentation.Title,
            Status = documentation.Status,
            Sections = documentation.Sections.Select(s => new DocumentationSectionDto
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
            ErrorMessage = documentation.ErrorMessage
        };
    }
}