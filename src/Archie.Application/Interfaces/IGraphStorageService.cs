using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

public interface IGraphStorageService
{
    // Entity operations
    Task<bool> StoreEntitiesAsync(List<CodeEntity> entities, CancellationToken cancellationToken = default);
    Task<CodeEntity?> GetEntityAsync(string entityId, CancellationToken cancellationToken = default);
    Task<List<CodeEntity>> QueryEntitiesAsync(EntityQueryCriteria criteria, CancellationToken cancellationToken = default);
    Task<bool> DeleteEntityAsync(string entityId, CancellationToken cancellationToken = default);
    Task<bool> DeleteEntitiesAsync(List<string> entityIds, CancellationToken cancellationToken = default);
    Task<List<CodeEntity>> GetEntitiesByRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    
    // Relationship operations
    Task<bool> StoreRelationshipsAsync(List<CodeRelationship> relationships, CancellationToken cancellationToken = default);
    Task<CodeRelationship?> GetRelationshipAsync(Guid relationshipId, CancellationToken cancellationToken = default);
    Task<List<CodeRelationship>> GetEntityRelationshipsAsync(string entityId, RelationshipType? type = null, CancellationToken cancellationToken = default);
    Task<List<CodeRelationship>> FindRelationshipPathAsync(string sourceEntityId, string targetEntityId, int maxDepth, CancellationToken cancellationToken = default);
    Task<bool> DeleteRelationshipAsync(Guid relationshipId, CancellationToken cancellationToken = default);
    Task<bool> DeleteRelationshipsAsync(List<Guid> relationshipIds, CancellationToken cancellationToken = default);
    Task<List<CodeRelationship>> GetRelationshipsByRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    
    // Pattern operations
    Task<bool> StorePatternsAsync(List<ArchitecturalPattern> patterns, CancellationToken cancellationToken = default);
    Task<ArchitecturalPattern?> GetPatternAsync(Guid patternId, CancellationToken cancellationToken = default);
    Task<List<ArchitecturalPattern>> GetRepositoryPatternsAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    Task<List<ArchitecturalPattern>> QueryPatternsAsync(PatternType? patternType = null, double? minimumConfidence = null, CancellationToken cancellationToken = default);
    Task<bool> DeletePatternAsync(Guid patternId, CancellationToken cancellationToken = default);
    Task<bool> DeletePatternsAsync(List<Guid> patternIds, CancellationToken cancellationToken = default);
    
    // Graph operations
    Task<KnowledgeGraph> StoreKnowledgeGraphAsync(KnowledgeGraph graph, CancellationToken cancellationToken = default);
    Task<KnowledgeGraph?> GetKnowledgeGraphAsync(Guid id, CancellationToken cancellationToken = default);
    Task<KnowledgeGraph?> GetKnowledgeGraphByRepositoriesAsync(List<Guid> repositoryIds, CancellationToken cancellationToken = default);
    Task<List<KnowledgeGraph>> GetAllKnowledgeGraphsAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteKnowledgeGraphAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateKnowledgeGraphStatusAsync(Guid id, GraphStatus status, CancellationToken cancellationToken = default);
    
    // Search and analysis operations
    Task<List<CodeEntity>> SearchEntitiesAsync(string query, List<Guid>? repositoryIds = null, int limit = 100, CancellationToken cancellationToken = default);
    Task<List<CodeEntity>> FindSimilarEntitiesAsync(CodeEntity entity, double threshold = 0.8, CancellationToken cancellationToken = default);
    Task<List<CodeRelationship>> GetStrongRelationshipsAsync(double minimumWeight = 0.7, CancellationToken cancellationToken = default);
    Task<Dictionary<EntityType, int>> GetEntityTypeDistributionAsync(Guid? repositoryId = null, CancellationToken cancellationToken = default);
    Task<Dictionary<RelationshipType, int>> GetRelationshipTypeDistributionAsync(Guid? repositoryId = null, CancellationToken cancellationToken = default);
    
    // Cleanup operations
    Task<bool> DeleteAllDataForRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    Task<int> CleanupOrphanedEntitiesAsync(CancellationToken cancellationToken = default);
    Task<int> CleanupOrphanedRelationshipsAsync(CancellationToken cancellationToken = default);
}