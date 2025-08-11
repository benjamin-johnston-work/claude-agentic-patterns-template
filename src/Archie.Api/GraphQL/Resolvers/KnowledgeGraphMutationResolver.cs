using HotChocolate.Types;
using Archie.Domain.Entities;
using Archie.Application.Interfaces;
using Archie.Api.GraphQL.Types;

namespace Archie.Api.GraphQL.Resolvers;

[ExtendObjectType<Mutation>]
public class KnowledgeGraphMutationResolver
{
    private readonly IKnowledgeGraphConstructionService _constructionService;
    private readonly IGraphStorageService _graphStorageService;
    private readonly ILogger<KnowledgeGraphMutationResolver> _logger;

    public KnowledgeGraphMutationResolver(
        IKnowledgeGraphConstructionService constructionService,
        IGraphStorageService graphStorageService,
        ILogger<KnowledgeGraphMutationResolver> logger)
    {
        _constructionService = constructionService;
        _graphStorageService = graphStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Build a knowledge graph for the specified repositories
    /// </summary>
    public async Task<KnowledgeGraph> BuildKnowledgeGraphAsync(
        BuildKnowledgeGraphInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Building knowledge graph for repositories: {RepositoryIds}", 
                string.Join(",", input.RepositoryIds));

            var knowledgeGraph = await _constructionService.BuildKnowledgeGraphAsync(
                input.RepositoryIds,
                input.AnalysisDepth,
                cancellationToken);

            _logger.LogInformation("Successfully built knowledge graph {GraphId} with {EntityCount} entities and {RelationshipCount} relationships",
                knowledgeGraph.Id,
                knowledgeGraph.Statistics.TotalEntities,
                knowledgeGraph.Statistics.TotalRelationships);

            return knowledgeGraph;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building knowledge graph for repositories {RepositoryIds}", 
                string.Join(",", input.RepositoryIds));
            throw;
        }
    }

    /// <summary>
    /// Update an existing knowledge graph
    /// </summary>
    public async Task<KnowledgeGraph> UpdateKnowledgeGraphAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating knowledge graph {GraphId}", id);

            var knowledgeGraph = await _constructionService.UpdateKnowledgeGraphAsync(id, cancellationToken);

            _logger.LogInformation("Successfully updated knowledge graph {GraphId}", id);

            return knowledgeGraph;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating knowledge graph {GraphId}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete a knowledge graph
    /// </summary>
    public async Task<bool> DeleteKnowledgeGraphAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting knowledge graph {GraphId}", id);

            var result = await _constructionService.DeleteKnowledgeGraphAsync(id, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Successfully deleted knowledge graph {GraphId}", id);
            }
            else
            {
                _logger.LogWarning("Knowledge graph {GraphId} was not found or could not be deleted", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting knowledge graph {GraphId}", id);
            throw;
        }
    }

    /// <summary>
    /// Analyze entity relationships (trigger re-analysis of relationships for an entity)
    /// </summary>
    public async Task<List<CodeRelationship>> AnalyzeEntityRelationshipsAsync(
        string entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing relationships for entity {EntityId}", entityId);

            // Get the entity first
            var entity = await _graphStorageService.GetEntityAsync(entityId, cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("Entity {EntityId} not found", entityId);
                return new List<CodeRelationship>();
            }

            // Get existing relationships
            var relationships = await _graphStorageService.GetEntityRelationshipsAsync(entityId, null, cancellationToken);

            _logger.LogInformation("Found {RelationshipCount} relationships for entity {EntityId}", 
                relationships.Count, entityId);

            return relationships;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing relationships for entity {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Delete all graph data for a repository (cleanup operation)
    /// </summary>
    public async Task<bool> DeleteRepositoryGraphDataAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting all graph data for repository {RepositoryId}", repositoryId);

            var result = await _graphStorageService.DeleteAllDataForRepositoryAsync(repositoryId, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Successfully deleted graph data for repository {RepositoryId}", repositoryId);
            }
            else
            {
                _logger.LogWarning("No graph data found for repository {RepositoryId}", repositoryId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting graph data for repository {RepositoryId}", repositoryId);
            throw;
        }
    }
}