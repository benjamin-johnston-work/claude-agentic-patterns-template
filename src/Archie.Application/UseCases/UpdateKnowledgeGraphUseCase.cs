using Archie.Application.Common;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class UpdateKnowledgeGraphUseCase
{
    private readonly IKnowledgeGraphConstructionService _constructionService;
    private readonly IGraphStorageService _graphStorageService;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<UpdateKnowledgeGraphUseCase> _logger;

    public UpdateKnowledgeGraphUseCase(
        IKnowledgeGraphConstructionService constructionService,
        IGraphStorageService graphStorageService,
        IRepositoryRepository repositoryRepository,
        ILogger<UpdateKnowledgeGraphUseCase> logger)
    {
        _constructionService = constructionService;
        _graphStorageService = graphStorageService;
        _repositoryRepository = repositoryRepository;
        _logger = logger;
    }

    public async Task<Result<KnowledgeGraph>> ExecuteAsync(
        Guid knowledgeGraphId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if knowledge graph exists
            var existingGraph = await _graphStorageService.GetKnowledgeGraphAsync(knowledgeGraphId, cancellationToken);
            if (existingGraph == null)
            {
                return Result<KnowledgeGraph>.Failure($"Knowledge graph {knowledgeGraphId} not found");
            }

            // Check if repositories are still ready
            var repositoryIds = existingGraph.RepositoryIds;
            foreach (var repositoryId in repositoryIds)
            {
                var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
                if (repository == null)
                {
                    _logger.LogWarning("Repository {RepositoryId} no longer exists, removing from knowledge graph", repositoryId);
                    existingGraph.RemoveRepository(repositoryId);
                }
                else if (!repository.IsReady())
                {
                    return Result<KnowledgeGraph>.Failure($"Repository {repositoryId} ({repository.Name}) is not ready for analysis. Current status: {repository.Status}");
                }
            }

            _logger.LogInformation("Updating knowledge graph {GraphId} for {RepositoryCount} repositories", 
                knowledgeGraphId, existingGraph.RepositoryIds.Count);

            var updatedGraph = await _constructionService.UpdateKnowledgeGraphAsync(
                knowledgeGraphId, 
                cancellationToken);

            _logger.LogInformation("Successfully updated knowledge graph {GraphId} with {EntityCount} entities and {RelationshipCount} relationships", 
                updatedGraph.Id, 
                updatedGraph.Statistics.TotalEntities, 
                updatedGraph.Statistics.TotalRelationships);

            return Result<KnowledgeGraph>.Success(updatedGraph);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Knowledge graph update was cancelled for graph {GraphId}", knowledgeGraphId);
            return Result<KnowledgeGraph>.Failure("Knowledge graph update was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update knowledge graph {GraphId}", knowledgeGraphId);
            return Result<KnowledgeGraph>.Failure($"Failed to update knowledge graph: {ex.Message}");
        }
    }
}