using Archie.Application.Common;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class FindRelationshipPathUseCase
{
    private readonly IGraphStorageService _graphStorageService;
    private readonly ILogger<FindRelationshipPathUseCase> _logger;

    public FindRelationshipPathUseCase(
        IGraphStorageService graphStorageService,
        ILogger<FindRelationshipPathUseCase> logger)
    {
        _graphStorageService = graphStorageService;
        _logger = logger;
    }

    public async Task<Result<List<CodeRelationship>>> ExecuteAsync(
        string sourceEntityId,
        string targetEntityId,
        int maxDepth = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourceEntityId))
            {
                return Result<List<CodeRelationship>>.Failure("Source entity ID cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(targetEntityId))
            {
                return Result<List<CodeRelationship>>.Failure("Target entity ID cannot be null or empty");
            }

            if (sourceEntityId == targetEntityId)
            {
                return Result<List<CodeRelationship>>.Success(new List<CodeRelationship>());
            }

            // Validate that both entities exist
            var sourceEntity = await _graphStorageService.GetEntityAsync(sourceEntityId, cancellationToken);
            if (sourceEntity == null)
            {
                return Result<List<CodeRelationship>>.Failure($"Source entity {sourceEntityId} not found");
            }

            var targetEntity = await _graphStorageService.GetEntityAsync(targetEntityId, cancellationToken);
            if (targetEntity == null)
            {
                return Result<List<CodeRelationship>>.Failure($"Target entity {targetEntityId} not found");
            }

            _logger.LogInformation("Finding relationship path from {SourceEntity} ({SourceName}) to {TargetEntity} ({TargetName}) with max depth {MaxDepth}", 
                sourceEntityId, sourceEntity.Name, targetEntityId, targetEntity.Name, maxDepth);

            var path = await _graphStorageService.FindRelationshipPathAsync(
                sourceEntityId, 
                targetEntityId, 
                maxDepth, 
                cancellationToken);

            if (path.Count > 0)
            {
                _logger.LogInformation("Found relationship path with {PathLength} relationships from {SourceEntity} to {TargetEntity}", 
                    path.Count, sourceEntityId, targetEntityId);
            }
            else
            {
                _logger.LogInformation("No relationship path found from {SourceEntity} to {TargetEntity} within depth {MaxDepth}", 
                    sourceEntityId, targetEntityId, maxDepth);
            }

            return Result<List<CodeRelationship>>.Success(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find relationship path from {SourceEntity} to {TargetEntity}", 
                sourceEntityId, targetEntityId);
            return Result<List<CodeRelationship>>.Failure($"Failed to find relationship path: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, List<CodeRelationship>>>> ExecuteForMultipleTargetsAsync(
        string sourceEntityId,
        List<string> targetEntityIds,
        int maxDepth = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new Dictionary<string, List<CodeRelationship>>();

            foreach (var targetEntityId in targetEntityIds)
            {
                var pathResult = await ExecuteAsync(sourceEntityId, targetEntityId, maxDepth, cancellationToken);
                if (pathResult.IsSuccess)
                {
                    results[targetEntityId] = pathResult.Value;
                }
                else
                {
                    results[targetEntityId] = new List<CodeRelationship>();
                    _logger.LogWarning("Failed to find path to {TargetEntity}: {Error}", 
                        targetEntityId, pathResult.Error);
                }
            }

            return Result<Dictionary<string, List<CodeRelationship>>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find relationship paths from {SourceEntity} to multiple targets", 
                sourceEntityId);
            return Result<Dictionary<string, List<CodeRelationship>>>.Failure(
                $"Failed to find relationship paths: {ex.Message}");
        }
    }
}