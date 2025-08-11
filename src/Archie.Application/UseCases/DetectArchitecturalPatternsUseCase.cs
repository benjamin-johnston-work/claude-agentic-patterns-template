using Archie.Application.Common;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class DetectArchitecturalPatternsUseCase
{
    private readonly IPatternDetectionService _patternDetectionService;
    private readonly IGraphStorageService _graphStorageService;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<DetectArchitecturalPatternsUseCase> _logger;

    public DetectArchitecturalPatternsUseCase(
        IPatternDetectionService patternDetectionService,
        IGraphStorageService graphStorageService,
        IRepositoryRepository repositoryRepository,
        ILogger<DetectArchitecturalPatternsUseCase> logger)
    {
        _patternDetectionService = patternDetectionService;
        _graphStorageService = graphStorageService;
        _repositoryRepository = repositoryRepository;
        _logger = logger;
    }

    public async Task<Result<List<ArchitecturalPattern>>> ExecuteAsync(
        Guid repositoryId,
        List<PatternType>? patternTypes = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate repository exists and is ready
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null)
            {
                return Result<List<ArchitecturalPattern>>.Failure($"Repository {repositoryId} not found");
            }

            if (!repository.IsReady())
            {
                return Result<List<ArchitecturalPattern>>.Failure(
                    $"Repository {repositoryId} ({repository.Name}) is not ready for analysis. Current status: {repository.Status}");
            }

            _logger.LogInformation("Detecting architectural patterns for repository {RepositoryId} ({RepositoryName})", 
                repositoryId, repository.Name);

            // Get entities and relationships for the repository
            var entities = await _graphStorageService.GetEntitiesByRepositoryAsync(repositoryId, cancellationToken);
            var relationships = await _graphStorageService.GetRelationshipsByRepositoryAsync(repositoryId, cancellationToken);

            _logger.LogInformation("Analyzing {EntityCount} entities and {RelationshipCount} relationships for patterns", 
                entities.Count, relationships.Count);

            List<ArchitecturalPattern> detectedPatterns;

            if (patternTypes != null && patternTypes.Count > 0)
            {
                // Detect specific pattern types
                detectedPatterns = new List<ArchitecturalPattern>();
                foreach (var patternType in patternTypes)
                {
                    var pattern = await _patternDetectionService.DetectSpecificPatternAsync(
                        patternType, entities, relationships, cancellationToken);
                    
                    if (pattern != null)
                    {
                        detectedPatterns.Add(pattern);
                    }
                }
            }
            else
            {
                // Detect all patterns
                detectedPatterns = await _patternDetectionService.DetectPatternsAsync(
                    entities, relationships, cancellationToken);
            }

            // Store the detected patterns
            if (detectedPatterns.Count > 0)
            {
                await _graphStorageService.StorePatternsAsync(detectedPatterns, cancellationToken);
            }

            _logger.LogInformation("Detected {PatternCount} architectural patterns for repository {RepositoryId}", 
                detectedPatterns.Count, repositoryId);

            return Result<List<ArchitecturalPattern>>.Success(detectedPatterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect architectural patterns for repository {RepositoryId}", repositoryId);
            return Result<List<ArchitecturalPattern>>.Failure($"Failed to detect architectural patterns: {ex.Message}");
        }
    }

    public async Task<Result<ArchitecturalPattern?>> ExecuteForSpecificPatternAsync(
        Guid repositoryId,
        PatternType patternType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null)
            {
                return Result<ArchitecturalPattern?>.Failure($"Repository {repositoryId} not found");
            }

            _logger.LogInformation("Detecting {PatternType} pattern for repository {RepositoryId} ({RepositoryName})", 
                patternType, repositoryId, repository.Name);

            var entities = await _graphStorageService.GetEntitiesByRepositoryAsync(repositoryId, cancellationToken);
            var relationships = await _graphStorageService.GetRelationshipsByRepositoryAsync(repositoryId, cancellationToken);

            var pattern = await _patternDetectionService.DetectSpecificPatternAsync(
                patternType, entities, relationships, cancellationToken);

            if (pattern != null)
            {
                await _graphStorageService.StorePatternsAsync(new List<ArchitecturalPattern> { pattern }, cancellationToken);
                
                _logger.LogInformation("Detected {PatternType} pattern with {ParticipantCount} participants and {Confidence}% confidence", 
                    patternType, pattern.ParticipantCount, pattern.Confidence * 100);
            }
            else
            {
                _logger.LogInformation("No {PatternType} pattern found in repository {RepositoryId}", 
                    patternType, repositoryId);
            }

            return Result<ArchitecturalPattern?>.Success(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect {PatternType} pattern for repository {RepositoryId}", 
                patternType, repositoryId);
            return Result<ArchitecturalPattern?>.Failure($"Failed to detect {patternType} pattern: {ex.Message}");
        }
    }

    public async Task<Result<List<AntiPattern>>> DetectAntiPatternsAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null)
            {
                return Result<List<AntiPattern>>.Failure($"Repository {repositoryId} not found");
            }

            _logger.LogInformation("Detecting anti-patterns for repository {RepositoryId} ({RepositoryName})", 
                repositoryId, repository.Name);

            var entities = await _graphStorageService.GetEntitiesByRepositoryAsync(repositoryId, cancellationToken);
            var relationships = await _graphStorageService.GetRelationshipsByRepositoryAsync(repositoryId, cancellationToken);

            var antiPatterns = await _patternDetectionService.DetectAntiPatternsAsync(
                entities, relationships, cancellationToken);

            _logger.LogInformation("Detected {AntiPatternCount} anti-patterns for repository {RepositoryId}", 
                antiPatterns.Count, repositoryId);

            return Result<List<AntiPattern>>.Success(antiPatterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to detect anti-patterns for repository {RepositoryId}", repositoryId);
            return Result<List<AntiPattern>>.Failure($"Failed to detect anti-patterns: {ex.Message}");
        }
    }
}