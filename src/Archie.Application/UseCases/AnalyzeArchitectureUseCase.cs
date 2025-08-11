using Archie.Application.Common;
using Archie.Application.Interfaces;
using Archie.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

// Analysis result types
public record ArchitectureAnalysis(
    Guid RepositoryId,
    double OverallComplexity,
    double LayeringScore,
    double PatternCompliance,
    double DependencyHealth,
    List<ArchitecturalRecommendation> Recommendations,
    List<AntiPattern> DetectedAntiPatterns
);

public record ArchitecturalRecommendation(
    RecommendationType Type,
    Priority Priority,
    string Description,
    List<string> AffectedEntities,
    string PotentialImpact
);

public enum RecommendationType
{
    Refactoring,
    PatternIntroduction,
    DependencyCleanup,
    ComplexityReduction,
    ArchitectureImprovement
}

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}

public record DependencyImpactAnalysis(
    string EntityId,
    List<string> DirectDependents,
    List<string> IndirectDependents,
    int ImpactRadius,
    RiskLevel RiskAssessment,
    List<string> RecommendedActions
);

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

public class AnalyzeArchitectureUseCase
{
    private readonly IGraphStorageService _graphStorageService;
    private readonly IPatternDetectionService _patternDetectionService;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<AnalyzeArchitectureUseCase> _logger;

    public AnalyzeArchitectureUseCase(
        IGraphStorageService graphStorageService,
        IPatternDetectionService patternDetectionService,
        IRepositoryRepository repositoryRepository,
        ILogger<AnalyzeArchitectureUseCase> logger)
    {
        _graphStorageService = graphStorageService;
        _patternDetectionService = patternDetectionService;
        _repositoryRepository = repositoryRepository;
        _logger = logger;
    }

    public async Task<Result<ArchitectureAnalysis>> ExecuteAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null)
            {
                return Result<ArchitectureAnalysis>.Failure($"Repository {repositoryId} not found");
            }

            _logger.LogInformation("Analyzing architecture for repository {RepositoryId} ({RepositoryName})", 
                repositoryId, repository.Name);

            // Get all the data we need for analysis
            var entities = await _graphStorageService.GetEntitiesByRepositoryAsync(repositoryId, cancellationToken);
            var relationships = await _graphStorageService.GetRelationshipsByRepositoryAsync(repositoryId, cancellationToken);
            var patterns = await _graphStorageService.GetRepositoryPatternsAsync(repositoryId, cancellationToken);
            var antiPatterns = await _patternDetectionService.DetectAntiPatternsAsync(entities, relationships, cancellationToken);

            // Calculate metrics
            var overallComplexity = CalculateOverallComplexity(entities);
            var layeringScore = CalculateLayeringScore(entities, relationships);
            var patternCompliance = CalculatePatternCompliance(patterns);
            var dependencyHealth = CalculateDependencyHealth(relationships);

            // Generate recommendations
            var recommendations = GenerateRecommendations(entities, relationships, patterns, antiPatterns);

            var analysis = new ArchitectureAnalysis(
                RepositoryId: repositoryId,
                OverallComplexity: overallComplexity,
                LayeringScore: layeringScore,
                PatternCompliance: patternCompliance,
                DependencyHealth: dependencyHealth,
                Recommendations: recommendations,
                DetectedAntiPatterns: antiPatterns
            );

            _logger.LogInformation("Architecture analysis completed for repository {RepositoryId}. " +
                                 "Complexity: {Complexity:F2}, Layering: {Layering:F2}, Patterns: {Patterns:F2}, Dependencies: {Dependencies:F2}", 
                repositoryId, overallComplexity, layeringScore, patternCompliance, dependencyHealth);

            return Result<ArchitectureAnalysis>.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze architecture for repository {RepositoryId}", repositoryId);
            return Result<ArchitectureAnalysis>.Failure($"Failed to analyze architecture: {ex.Message}");
        }
    }

    public async Task<Result<DependencyImpactAnalysis>> AnalyzeDependencyImpactAsync(
        string entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _graphStorageService.GetEntityAsync(entityId, cancellationToken);
            if (entity == null)
            {
                return Result<DependencyImpactAnalysis>.Failure($"Entity {entityId} not found");
            }

            _logger.LogInformation("Analyzing dependency impact for entity {EntityId} ({EntityName})", 
                entityId, entity.Name);

            // Get all relationships where this entity is a target (things that depend on it)
            var incomingRelationships = await _graphStorageService.GetEntityRelationshipsAsync(entityId, null, cancellationToken);
            var directDependents = incomingRelationships
                .Where(r => r.TargetEntityId == entityId)
                .Select(r => r.SourceEntityId)
                .Distinct()
                .ToList();

            // Calculate indirect dependents (entities that depend on the direct dependents)
            var indirectDependents = new HashSet<string>();
            foreach (var dependent in directDependents)
            {
                var dependentRelationships = await _graphStorageService.GetEntityRelationshipsAsync(dependent, null, cancellationToken);
                var indirectDeps = dependentRelationships
                    .Where(r => r.TargetEntityId == dependent)
                    .Select(r => r.SourceEntityId)
                    .Where(id => id != entityId && !directDependents.Contains(id));
                
                foreach (var indirectDep in indirectDeps)
                {
                    indirectDependents.Add(indirectDep);
                }
            }

            var impactRadius = directDependents.Count + indirectDependents.Count;
            var riskAssessment = CalculateRiskLevel(impactRadius, entity);
            var recommendedActions = GenerateImpactRecommendations(entity, directDependents.Count, indirectDependents.Count);

            var analysis = new DependencyImpactAnalysis(
                EntityId: entityId,
                DirectDependents: directDependents,
                IndirectDependents: indirectDependents.ToList(),
                ImpactRadius: impactRadius,
                RiskAssessment: riskAssessment,
                RecommendedActions: recommendedActions
            );

            _logger.LogInformation("Dependency impact analysis completed for entity {EntityId}. " +
                                 "Direct dependents: {DirectCount}, Indirect: {IndirectCount}, Risk: {Risk}", 
                entityId, directDependents.Count, indirectDependents.Count, riskAssessment);

            return Result<DependencyImpactAnalysis>.Success(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze dependency impact for entity {EntityId}", entityId);
            return Result<DependencyImpactAnalysis>.Failure($"Failed to analyze dependency impact: {ex.Message}");
        }
    }

    private double CalculateOverallComplexity(List<Domain.Entities.CodeEntity> entities)
    {
        if (entities.Count == 0) return 0.0;
        return entities.Average(e => (double)e.ComplexityScore);
    }

    private double CalculateLayeringScore(List<Domain.Entities.CodeEntity> entities, List<Domain.Entities.CodeRelationship> relationships)
    {
        // Simple layering score based on architectural relationships
        var architecturalRelationships = relationships.Count(r => r.IsArchitecturalRelationship);
        var totalRelationships = relationships.Count;
        
        if (totalRelationships == 0) return 1.0;
        
        return 1.0 - (double)architecturalRelationships / totalRelationships;
    }

    private double CalculatePatternCompliance(List<Domain.Entities.ArchitecturalPattern> patterns)
    {
        if (patterns.Count == 0) return 1.0;
        
        var patternsWithoutViolations = patterns.Count(p => !p.HasViolations);
        return (double)patternsWithoutViolations / patterns.Count;
    }

    private double CalculateDependencyHealth(List<Domain.Entities.CodeRelationship> relationships)
    {
        if (relationships.Count == 0) return 1.0;
        
        var healthyRelationships = relationships.Count(r => r.IsHighConfidence && r.Weight > 0.5);
        return (double)healthyRelationships / relationships.Count;
    }

    private List<ArchitecturalRecommendation> GenerateRecommendations(
        List<Domain.Entities.CodeEntity> entities, 
        List<Domain.Entities.CodeRelationship> relationships, 
        List<Domain.Entities.ArchitecturalPattern> patterns,
        List<AntiPattern> antiPatterns)
    {
        var recommendations = new List<ArchitecturalRecommendation>();

        // Add recommendations based on anti-patterns
        foreach (var antiPattern in antiPatterns.Where(ap => ap.Severity >= ViolationSeverity.Error))
        {
            var priority = antiPattern.Severity == ViolationSeverity.Critical ? Priority.Critical : Priority.High;
            recommendations.Add(new ArchitecturalRecommendation(
                RecommendationType.Refactoring,
                priority,
                $"Address {antiPattern.Name}: {antiPattern.Remediation}",
                antiPattern.AffectedEntities,
                "Improved code maintainability and reduced technical debt"
            ));
        }

        // Add complexity recommendations
        var highComplexityEntities = entities.Where(e => e.ComplexityScore > 10).ToList();
        if (highComplexityEntities.Count > 0)
        {
            recommendations.Add(new ArchitecturalRecommendation(
                RecommendationType.ComplexityReduction,
                Priority.Medium,
                $"Reduce complexity in {highComplexityEntities.Count} high-complexity entities",
                highComplexityEntities.Select(e => e.EntityId).ToList(),
                "Improved code readability and maintainability"
            ));
        }

        return recommendations;
    }

    private RiskLevel CalculateRiskLevel(int impactRadius, Domain.Entities.CodeEntity entity)
    {
        // Consider both impact radius and entity importance
        var baseRisk = impactRadius switch
        {
            0 => RiskLevel.Low,
            <= 5 => RiskLevel.Low,
            <= 15 => RiskLevel.Medium,
            <= 30 => RiskLevel.High,
            _ => RiskLevel.Critical
        };

        // Increase risk for critical entity types
        if (entity.Type == EntityType.Interface || entity.Type == EntityType.Service || entity.Type == EntityType.Repository)
        {
            return baseRisk == RiskLevel.Critical ? RiskLevel.Critical : (RiskLevel)Math.Min((int)baseRisk + 1, (int)RiskLevel.Critical);
        }

        return baseRisk;
    }

    private List<string> GenerateImpactRecommendations(Domain.Entities.CodeEntity entity, int directDependents, int indirectDependents)
    {
        var recommendations = new List<string>();

        if (directDependents > 10)
        {
            recommendations.Add("Consider breaking this entity into smaller, more focused components");
            recommendations.Add("Implement interface segregation to reduce coupling");
        }

        if (indirectDependents > 20)
        {
            recommendations.Add("This entity has significant indirect impact - changes should be carefully planned");
            recommendations.Add("Consider implementing the Facade pattern to provide a stable interface");
        }

        if (entity.ComplexityScore > 15)
        {
            recommendations.Add("High complexity combined with high impact suggests this entity needs refactoring");
        }

        if (recommendations.Count == 0)
        {
            recommendations.Add("Impact level is manageable - normal change management practices apply");
        }

        return recommendations;
    }
}