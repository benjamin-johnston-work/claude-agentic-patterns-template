using Archie.Application.Common;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class BuildKnowledgeGraphUseCase
{
    private readonly IKnowledgeGraphConstructionService _constructionService;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<BuildKnowledgeGraphUseCase> _logger;

    public BuildKnowledgeGraphUseCase(
        IKnowledgeGraphConstructionService constructionService,
        IRepositoryRepository repositoryRepository,
        ILogger<BuildKnowledgeGraphUseCase> logger)
    {
        _constructionService = constructionService;
        _repositoryRepository = repositoryRepository;
        _logger = logger;
    }

    public async Task<Result<KnowledgeGraph>> ExecuteAsync(
        List<Guid> repositoryIds,
        AnalysisDepth depth = AnalysisDepth.Standard,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate that all repositories exist and are ready
            foreach (var repositoryId in repositoryIds)
            {
                var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
                if (repository == null)
                {
                    return Result<KnowledgeGraph>.Failure($"Repository {repositoryId} not found");
                }

                if (!repository.IsReady())
                {
                    return Result<KnowledgeGraph>.Failure($"Repository {repositoryId} ({repository.Name}) is not ready for analysis. Current status: {repository.Status}");
                }
            }

            _logger.LogInformation("Building knowledge graph for {RepositoryCount} repositories with {Depth} analysis depth", 
                repositoryIds.Count, depth);

            var knowledgeGraph = await _constructionService.BuildKnowledgeGraphAsync(
                repositoryIds, 
                depth, 
                cancellationToken);

            _logger.LogInformation("Successfully built knowledge graph {GraphId} with {EntityCount} entities and {RelationshipCount} relationships", 
                knowledgeGraph.Id, 
                knowledgeGraph.Statistics.TotalEntities, 
                knowledgeGraph.Statistics.TotalRelationships);

            return Result<KnowledgeGraph>.Success(knowledgeGraph);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Knowledge graph construction was cancelled for repositories {RepositoryIds}", 
                string.Join(",", repositoryIds));
            return Result<KnowledgeGraph>.Failure("Knowledge graph construction was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build knowledge graph for repositories {RepositoryIds}", 
                string.Join(",", repositoryIds));
            return Result<KnowledgeGraph>.Failure($"Failed to build knowledge graph: {ex.Message}");
        }
    }
}