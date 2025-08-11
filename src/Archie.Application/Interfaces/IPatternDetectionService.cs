using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

public interface IPatternDetectionService
{
    Task<List<ArchitecturalPattern>> DetectPatternsAsync(
        List<CodeEntity> entities,
        List<CodeRelationship> relationships,
        CancellationToken cancellationToken = default);
    
    Task<ArchitecturalPattern?> DetectSpecificPatternAsync(
        PatternType patternType,
        List<CodeEntity> entities,
        List<CodeRelationship> relationships,
        CancellationToken cancellationToken = default);
    
    Task<List<AntiPattern>> DetectAntiPatternsAsync(
        List<CodeEntity> entities,
        List<CodeRelationship> relationships,
        CancellationToken cancellationToken = default);
    
    Task<double> ValidatePatternImplementationAsync(
        ArchitecturalPattern pattern,
        List<CodeEntity> entities,
        CancellationToken cancellationToken = default);

    Task<List<ArchitecturalPattern>> DetectDomainDrivenDesignPatternsAsync(
        List<CodeEntity> entities,
        List<CodeRelationship> relationships,
        CancellationToken cancellationToken = default);

    Task<List<ArchitecturalPattern>> DetectMicroservicePatternsAsync(
        List<CodeEntity> entities,
        List<CodeRelationship> relationships,
        CancellationToken cancellationToken = default);

    Task<List<ArchitecturalPattern>> DetectDesignPatternsAsync(
        List<CodeEntity> entities,
        List<CodeRelationship> relationships,
        CancellationToken cancellationToken = default);

    bool CanDetectPattern(PatternType patternType);
    
    List<PatternType> GetSupportedPatterns();
    
    double GetMinimumConfidenceThreshold();
}