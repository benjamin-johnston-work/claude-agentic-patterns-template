using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

public interface ICodeAnalysisService
{
    Task<List<CodeEntity>> ParseCodeFileAsync(
        string filePath,
        string content,
        string language,
        Guid repositoryId,
        CancellationToken cancellationToken = default);
    
    Task<List<CodeRelationship>> AnalyzeRelationshipsInFileAsync(
        List<CodeEntity> entities,
        string content,
        string language,
        CancellationToken cancellationToken = default);
    
    Task<int> CalculateComplexityScoreAsync(
        CodeEntity entity,
        string content,
        CancellationToken cancellationToken = default);
    
    Task<List<string>> ExtractDependenciesAsync(
        string filePath,
        string content,
        string language,
        CancellationToken cancellationToken = default);

    Task<List<CodeRelationship>> AnalyzeCrossFileRelationshipsAsync(
        List<CodeEntity> entities,
        CancellationToken cancellationToken = default);

    Task<float[]> GenerateSemanticEmbeddingAsync(
        string content,
        CancellationToken cancellationToken = default);

    Task<List<CodeEntity>> FindSimilarEntitiesAsync(
        CodeEntity entity,
        List<CodeEntity> candidateEntities,
        double threshold = 0.8,
        CancellationToken cancellationToken = default);

    bool IsLanguageSupported(string language);
    
    List<string> GetSupportedLanguages();
}