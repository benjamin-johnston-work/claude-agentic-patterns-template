using HotChocolate.Types;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Archie.Application.Interfaces;
using Archie.Api.GraphQL.Types;

namespace Archie.Api.GraphQL.Resolvers;

[ExtendObjectType<Query>]
public class KnowledgeGraphQueryResolver
{
    private readonly IGraphStorageService _graphStorageService;
    private readonly IKnowledgeGraphConstructionService _constructionService;
    private readonly ILogger<KnowledgeGraphQueryResolver> _logger;

    public KnowledgeGraphQueryResolver(
        IGraphStorageService graphStorageService,
        IKnowledgeGraphConstructionService constructionService,
        ILogger<KnowledgeGraphQueryResolver> logger)
    {
        _graphStorageService = graphStorageService;
        _constructionService = constructionService;
        _logger = logger;
    }

    /// <summary>
    /// Get a knowledge graph by ID
    /// </summary>
    public async Task<KnowledgeGraph?> GetKnowledgeGraphAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetKnowledgeGraphAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting knowledge graph with ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Get a knowledge graph by repository IDs
    /// </summary>
    public async Task<KnowledgeGraph?> GetKnowledgeGraphByRepositoriesAsync(
        List<Guid> repositoryIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetKnowledgeGraphByRepositoriesAsync(repositoryIds, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting knowledge graph for repositories {RepositoryIds}", string.Join(",", repositoryIds));
            throw;
        }
    }

    /// <summary>
    /// Get all knowledge graphs
    /// </summary>
    public async Task<List<KnowledgeGraph>> GetKnowledgeGraphsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetAllKnowledgeGraphsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all knowledge graphs");
            throw;
        }
    }

    /// <summary>
    /// Get a code entity by ID
    /// </summary>
    public async Task<CodeEntity?> GetEntityAsync(
        string entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetEntityAsync(entityId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity with ID {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Query code entities with filters
    /// </summary>
    public async Task<List<CodeEntity>> GetEntitiesAsync(
        EntityQueryInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var criteria = EntityQueryCriteria.Create(
                repositoryIds: input.RepositoryIds?.Count > 0 ? input.RepositoryIds : null,
                entityTypes: input.EntityTypes?.Count > 0 ? input.EntityTypes : null,
                namePattern: input.NamePattern,
                hasRelationshipType: input.HasRelationshipType,
                complexityRange: input.ComplexityRange != null ? (input.ComplexityRange.Min, input.ComplexityRange.Max) : null,
                languageFilter: input.LanguageFilter?.Count > 0 ? input.LanguageFilter : null,
                limit: input.Limit,
                offset: input.Offset
            );

            return await _graphStorageService.QueryEntitiesAsync(criteria, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying entities");
            throw;
        }
    }

    /// <summary>
    /// Get entities by repository
    /// </summary>
    public async Task<List<CodeEntity>> GetEntitiesByRepositoryAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetEntitiesByRepositoryAsync(repositoryId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entities for repository {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <summary>
    /// Get a code relationship by ID
    /// </summary>
    public async Task<CodeRelationship?> GetRelationshipAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetRelationshipAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationship with ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Get relationships for an entity
    /// </summary>
    public async Task<List<CodeRelationship>> GetEntityRelationshipsAsync(
        string entityId,
        RelationshipType? type = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetEntityRelationshipsAsync(entityId, type, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationships for entity {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Find relationship path between two entities
    /// </summary>
    public async Task<List<CodeRelationship>> FindRelationshipPathAsync(
        FindRelationshipPathInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.FindRelationshipPathAsync(
                input.SourceEntityId,
                input.TargetEntityId,
                input.MaxDepth,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding relationship path from {Source} to {Target}",
                input.SourceEntityId, input.TargetEntityId);
            throw;
        }
    }

    /// <summary>
    /// Get an architectural pattern by ID
    /// </summary>
    public async Task<ArchitecturalPattern?> GetArchitecturalPatternAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetPatternAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting architectural pattern with ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Get architectural patterns for a repository
    /// </summary>
    public async Task<List<ArchitecturalPattern>> GetRepositoryPatternsAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetRepositoryPatternsAsync(repositoryId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patterns for repository {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <summary>
    /// Query architectural patterns with filters
    /// </summary>
    public async Task<List<ArchitecturalPattern>> GetArchitecturalPatternsAsync(
        PatternQueryInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, just get patterns for the first repository
            // TODO: Implement proper pattern querying with filters
            if (input.RepositoryIds?.Count > 0)
            {
                var allPatterns = new List<ArchitecturalPattern>();
                foreach (var repositoryId in input.RepositoryIds)
                {
                    var patterns = await _graphStorageService.GetRepositoryPatternsAsync(repositoryId, cancellationToken);
                    allPatterns.AddRange(patterns);
                }

                // Apply filters
                if (input.PatternTypes?.Count > 0)
                {
                    allPatterns = allPatterns.Where(p => input.PatternTypes.Contains(p.Type)).ToList();
                }

                if (input.MinimumConfidence.HasValue)
                {
                    allPatterns = allPatterns.Where(p => p.Confidence >= input.MinimumConfidence.Value).ToList();
                }

                if (!input.IncludeViolations)
                {
                    allPatterns = allPatterns.Where(p => !p.HasViolations).ToList();
                }

                return allPatterns;
            }

            return new List<ArchitecturalPattern>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying architectural patterns");
            throw;
        }
    }

    /// <summary>
    /// Search entities by text query
    /// </summary>
    public async Task<List<CodeEntity>> SearchEntitiesAsync(
        string query,
        List<Guid>? repositoryIds = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.SearchEntitiesAsync(query, repositoryIds, limit, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching entities with query '{Query}'", query);
            throw;
        }
    }

    /// <summary>
    /// Find similar entities
    /// </summary>
    public async Task<List<CodeEntity>> FindSimilarEntitiesAsync(
        string entityId,
        double threshold = 0.8,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _graphStorageService.GetEntityAsync(entityId, cancellationToken);
            if (entity == null)
            {
                return new List<CodeEntity>();
            }

            return await _graphStorageService.FindSimilarEntitiesAsync(entity, threshold, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar entities for {EntityId}", entityId);
            throw;
        }
    }

    /// <summary>
    /// Get entity type distribution
    /// </summary>
    public async Task<Dictionary<EntityType, int>> GetEntityTypeDistributionAsync(
        Guid? repositoryId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetEntityTypeDistributionAsync(repositoryId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity type distribution for repository {RepositoryId}", repositoryId);
            throw;
        }
    }

    /// <summary>
    /// Get relationship type distribution
    /// </summary>
    public async Task<Dictionary<RelationshipType, int>> GetRelationshipTypeDistributionAsync(
        Guid? repositoryId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _graphStorageService.GetRelationshipTypeDistributionAsync(repositoryId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relationship type distribution for repository {RepositoryId}", repositoryId);
            throw;
        }
    }
}