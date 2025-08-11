using Archie.Application.Common;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class AnalyzeEntityRelationshipsUseCase
{
    private readonly IGraphStorageService _graphStorageService;
    private readonly ICodeAnalysisService _codeAnalysisService;
    private readonly ILogger<AnalyzeEntityRelationshipsUseCase> _logger;

    public AnalyzeEntityRelationshipsUseCase(
        IGraphStorageService graphStorageService,
        ICodeAnalysisService codeAnalysisService,
        ILogger<AnalyzeEntityRelationshipsUseCase> logger)
    {
        _graphStorageService = graphStorageService;
        _codeAnalysisService = codeAnalysisService;
        _logger = logger;
    }

    public async Task<Result<List<CodeRelationship>>> ExecuteAsync(
        string entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the entity
            var entity = await _graphStorageService.GetEntityAsync(entityId, cancellationToken);
            if (entity == null)
            {
                return Result<List<CodeRelationship>>.Failure($"Entity {entityId} not found");
            }

            _logger.LogInformation("Analyzing relationships for entity {EntityId} ({EntityName})", 
                entityId, entity.Name);

            // Get existing relationships
            var existingRelationships = await _graphStorageService.GetEntityRelationshipsAsync(
                entityId, null, cancellationToken);

            _logger.LogInformation("Found {RelationshipCount} existing relationships for entity {EntityId}", 
                existingRelationships.Count, entityId);

            // For now, return existing relationships
            // TODO: In a full implementation, this would re-analyze the entity's code
            // and update relationships based on current source code

            return Result<List<CodeRelationship>>.Success(existingRelationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze relationships for entity {EntityId}", entityId);
            return Result<List<CodeRelationship>>.Failure($"Failed to analyze entity relationships: {ex.Message}");
        }
    }

    public async Task<Result<List<CodeRelationship>>> ExecuteForRepositoryAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing all entity relationships for repository {RepositoryId}", repositoryId);

            // Get all entities for the repository
            var entities = await _graphStorageService.GetEntitiesByRepositoryAsync(repositoryId, cancellationToken);
            
            var allRelationships = new List<CodeRelationship>();
            
            foreach (var entity in entities)
            {
                var relationships = await _graphStorageService.GetEntityRelationshipsAsync(
                    entity.EntityId, null, cancellationToken);
                allRelationships.AddRange(relationships);
            }

            // Remove duplicates (relationships might be counted from both directions)
            var uniqueRelationships = allRelationships
                .GroupBy(r => new { r.SourceEntityId, r.TargetEntityId, r.Type })
                .Select(g => g.First())
                .ToList();

            _logger.LogInformation("Found {RelationshipCount} unique relationships for repository {RepositoryId}", 
                uniqueRelationships.Count, repositoryId);

            return Result<List<CodeRelationship>>.Success(uniqueRelationships);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze relationships for repository {RepositoryId}", repositoryId);
            return Result<List<CodeRelationship>>.Failure($"Failed to analyze repository relationships: {ex.Message}");
        }
    }
}