using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

public interface IKnowledgeGraphConstructionService
{
    Task<KnowledgeGraph> BuildKnowledgeGraphAsync(
        List<Guid> repositoryIds,
        AnalysisDepth depth = AnalysisDepth.Standard,
        CancellationToken cancellationToken = default);
    
    Task<List<CodeEntity>> ExtractCodeEntitiesAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default);
    
    Task<List<CodeRelationship>> AnalyzeRelationshipsAsync(
        List<CodeEntity> entities,
        CancellationToken cancellationToken = default);
    
    Task<List<ArchitecturalPattern>> DetectArchitecturalPatternsAsync(
        List<CodeEntity> entities,
        List<CodeRelationship> relationships,
        CancellationToken cancellationToken = default);
    
    Task<KnowledgeGraph> UpdateKnowledgeGraphAsync(
        Guid knowledgeGraphId,
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteKnowledgeGraphAsync(
        Guid knowledgeGraphId,
        CancellationToken cancellationToken = default);
}