namespace Archie.Domain.ValueObjects;

public record GraphStatistics
{
    public int TotalEntities { get; init; }
    public int TotalRelationships { get; init; }
    public int DetectedPatterns { get; init; }
    public Dictionary<EntityType, int> EntityTypeDistribution { get; init; } = new();
    public Dictionary<RelationshipType, int> RelationshipTypeDistribution { get; init; } = new();
    public Dictionary<PatternType, int> PatternTypeDistribution { get; init; } = new();
    public double AverageComplexity { get; init; }
    public int CrossRepositoryRelationships { get; init; }
    public DateTime LastAnalysis { get; init; }

    public static GraphStatistics Empty => new()
    {
        TotalEntities = 0,
        TotalRelationships = 0,
        DetectedPatterns = 0,
        AverageComplexity = 0.0,
        CrossRepositoryRelationships = 0,
        LastAnalysis = DateTime.MinValue
    };

    public static GraphStatistics Create(
        int totalEntities,
        int totalRelationships,
        int detectedPatterns,
        Dictionary<EntityType, int> entityTypeDistribution,
        Dictionary<RelationshipType, int> relationshipTypeDistribution,
        Dictionary<PatternType, int> patternTypeDistribution,
        double averageComplexity = 0.0,
        int crossRepositoryRelationships = 0)
    {
        return new GraphStatistics
        {
            TotalEntities = totalEntities,
            TotalRelationships = totalRelationships,
            DetectedPatterns = detectedPatterns,
            EntityTypeDistribution = entityTypeDistribution ?? new Dictionary<EntityType, int>(),
            RelationshipTypeDistribution = relationshipTypeDistribution ?? new Dictionary<RelationshipType, int>(),
            PatternTypeDistribution = patternTypeDistribution ?? new Dictionary<PatternType, int>(),
            AverageComplexity = averageComplexity,
            CrossRepositoryRelationships = crossRepositoryRelationships,
            LastAnalysis = DateTime.UtcNow
        };
    }
}