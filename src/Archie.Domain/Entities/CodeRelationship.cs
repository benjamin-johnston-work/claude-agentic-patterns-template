using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Entities;

public class CodeRelationship : BaseEntity
{
    public string SourceEntityId { get; private set; } = string.Empty;
    public string TargetEntityId { get; private set; } = string.Empty;
    public RelationshipType Type { get; private set; }
    public double Weight { get; private set; } // Strength/importance of relationship
    public RelationshipMetadata Metadata { get; private set; }
    public DateTime DetectedAt { get; private set; }

    protected CodeRelationship() // EF Constructor
    {
        Metadata = RelationshipMetadata.Default;
    }

    public CodeRelationship(string sourceEntityId, string targetEntityId, RelationshipType type, double weight = 1.0)
    {
        if (string.IsNullOrWhiteSpace(sourceEntityId))
            throw new ArgumentException("Source entity ID cannot be null or empty", nameof(sourceEntityId));
        if (string.IsNullOrWhiteSpace(targetEntityId))
            throw new ArgumentException("Target entity ID cannot be null or empty", nameof(targetEntityId));
        if (sourceEntityId == targetEntityId)
            throw new ArgumentException("Source and target entity IDs cannot be the same");
        if (weight < 0 || weight > 1)
            throw new ArgumentException("Weight must be between 0 and 1", nameof(weight));

        SourceEntityId = sourceEntityId;
        TargetEntityId = targetEntityId;
        Type = type;
        Weight = weight;
        Metadata = RelationshipMetadata.Default;
        DetectedAt = DateTime.UtcNow;
    }

    public static CodeRelationship Create(
        string sourceEntityId,
        string targetEntityId,
        RelationshipType type,
        double weight = 1.0)
    {
        return new CodeRelationship(sourceEntityId, targetEntityId, type, weight);
    }

    public void UpdateWeight(double weight)
    {
        if (weight < 0 || weight > 1)
            throw new ArgumentException("Weight must be between 0 and 1", nameof(weight));

        Weight = weight;
    }

    public void UpdateMetadata(RelationshipMetadata metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    public void UpdateConfidence(int confidence)
    {
        Metadata = Metadata.WithConfidence(confidence);
    }

    public void AddSourceReference(string reference)
    {
        if (!string.IsNullOrWhiteSpace(reference))
        {
            Metadata = Metadata.AddSourceReference(reference);
        }
    }

    public void AddProperty(string key, object value)
    {
        if (!string.IsNullOrWhiteSpace(key))
        {
            Metadata = Metadata.AddProperty(key, value);
        }
    }

    // Convenience methods for relationship analysis
    public bool IsStrongRelationship => Weight >= 0.7;
    public bool IsMediumRelationship => Weight >= 0.4 && Weight < 0.7;
    public bool IsWeakRelationship => Weight < 0.4;

    public bool IsStructuralRelationship => Type switch
    {
        RelationshipType.Inheritance => true,
        RelationshipType.Implementation => true,
        RelationshipType.Composition => true,
        RelationshipType.Aggregation => true,
        RelationshipType.Association => true,
        _ => false
    };

    public bool IsBehavioralRelationship => Type switch
    {
        RelationshipType.Calls => true,
        RelationshipType.Uses => true,
        RelationshipType.Depends => true,
        RelationshipType.Creates => true,
        RelationshipType.Returns => true,
        RelationshipType.Accepts => true,
        _ => false
    };

    public bool IsArchitecturalRelationship => Type switch
    {
        RelationshipType.LayerDependency => true,
        RelationshipType.ServiceConsumption => true,
        RelationshipType.EventPublishing => true,
        RelationshipType.EventSubscription => true,
        _ => false
    };

    public bool IsCrossRepositoryRelationship => Type switch
    {
        RelationshipType.SharedInterface => true,
        RelationshipType.SimilarConcept => true,
        RelationshipType.SharedDependency => true,
        _ => false
    };

    public bool IsPatternRelationship => Type switch
    {
        RelationshipType.PatternInstance => true,
        RelationshipType.PatternComponent => true,
        _ => false
    };

    public bool IsHighConfidence => Metadata.IsHighConfidence;
    public bool IsMediumConfidence => Metadata.IsMediumConfidence;
    public bool IsLowConfidence => Metadata.IsLowConfidence;

    public string GetRelationshipDescription()
    {
        return Type switch
        {
            RelationshipType.Inheritance => "inherits from",
            RelationshipType.Implementation => "implements",
            RelationshipType.Composition => "contains",
            RelationshipType.Aggregation => "aggregates",
            RelationshipType.Association => "is associated with",
            RelationshipType.Calls => "calls",
            RelationshipType.Uses => "uses",
            RelationshipType.Depends => "depends on",
            RelationshipType.Creates => "creates",
            RelationshipType.Returns => "returns",
            RelationshipType.Accepts => "accepts",
            RelationshipType.LayerDependency => "depends on layer",
            RelationshipType.ServiceConsumption => "consumes service",
            RelationshipType.EventPublishing => "publishes events to",
            RelationshipType.EventSubscription => "subscribes to events from",
            RelationshipType.SharedInterface => "shares interface pattern with",
            RelationshipType.SimilarConcept => "is similar to",
            RelationshipType.SharedDependency => "shares dependency with",
            RelationshipType.PatternInstance => "is instance of pattern",
            RelationshipType.PatternComponent => "is component of pattern",
            _ => "relates to"
        };
    }

    public override string ToString()
    {
        return $"{SourceEntityId} {GetRelationshipDescription()} {TargetEntityId} (Weight: {Weight:F2}, Confidence: {Metadata.Confidence}%)";
    }
}