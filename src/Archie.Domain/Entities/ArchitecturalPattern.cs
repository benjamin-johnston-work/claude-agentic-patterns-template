using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Entities;

public class ArchitecturalPattern : BaseEntity
{
    private readonly List<string> _participatingEntityIds = new();

    public string Name { get; private set; } = string.Empty;
    public PatternType Type { get; private set; }
    public IReadOnlyList<string> ParticipatingEntityIds => _participatingEntityIds.AsReadOnly();
    public Guid RepositoryId { get; private set; }
    public double Confidence { get; private set; } // 0.0 to 1.0
    public PatternMetadata Metadata { get; private set; }
    public DateTime DetectedAt { get; private set; }

    protected ArchitecturalPattern() // EF Constructor
    {
        Metadata = PatternMetadata.Empty;
    }

    public ArchitecturalPattern(string name, PatternType type, Guid repositoryId, double confidence = 1.0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Pattern name cannot be null or empty", nameof(name));
        if (repositoryId == Guid.Empty)
            throw new ArgumentException("Repository ID cannot be empty", nameof(repositoryId));
        if (confidence < 0 || confidence > 1)
            throw new ArgumentException("Confidence must be between 0 and 1", nameof(confidence));

        Name = name;
        Type = type;
        RepositoryId = repositoryId;
        Confidence = confidence;
        Metadata = PatternMetadata.Empty;
        DetectedAt = DateTime.UtcNow;
    }

    public static ArchitecturalPattern Create(string name, PatternType type, Guid repositoryId, double confidence = 1.0)
    {
        return new ArchitecturalPattern(name, type, repositoryId, confidence);
    }

    public void AddParticipant(string entityId, string role)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be null or empty", nameof(role));

        if (!_participatingEntityIds.Contains(entityId))
        {
            _participatingEntityIds.Add(entityId);
        }

        Metadata = Metadata.AddParticipant(entityId, role);
    }

    public void RemoveParticipant(string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            return;

        _participatingEntityIds.Remove(entityId);

        // Remove from metadata as well
        var newParticipantRoles = new Dictionary<string, string>(Metadata.ParticipantRoles);
        newParticipantRoles.Remove(entityId);

        Metadata = Metadata with { ParticipantRoles = newParticipantRoles };
    }

    public void UpdateConfidence(double confidence)
    {
        if (confidence < 0 || confidence > 1)
            throw new ArgumentException("Confidence must be between 0 and 1", nameof(confidence));

        Confidence = confidence;
    }

    public void UpdateMetadata(PatternMetadata metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    public void AddCharacteristic(string characteristic)
    {
        if (!string.IsNullOrWhiteSpace(characteristic))
        {
            Metadata = Metadata.AddCharacteristic(characteristic);
        }
    }

    public void AddViolation(PatternViolation violation)
    {
        if (violation != null)
        {
            Metadata = Metadata.AddViolation(violation);
        }
    }

    public void SetDescription(string description)
    {
        Metadata = Metadata.WithDescription(description);
    }

    // Convenience methods
    public bool IsHighConfidence => Confidence >= 0.8;
    public bool IsMediumConfidence => Confidence >= 0.6 && Confidence < 0.8;
    public bool IsLowConfidence => Confidence < 0.6;

    public bool HasParticipant(string entityId)
    {
        return !string.IsNullOrWhiteSpace(entityId) && _participatingEntityIds.Contains(entityId);
    }

    public string? GetParticipantRole(string entityId)
    {
        return Metadata.GetRole(entityId);
    }

    public List<string> GetParticipantsWithRole(string role)
    {
        return Metadata.GetEntitiesWithRole(role);
    }

    public int ParticipantCount => _participatingEntityIds.Count;

    public bool IsCreationalPattern => Type switch
    {
        PatternType.Singleton => true,
        PatternType.Factory => true,
        PatternType.AbstractFactory => true,
        PatternType.Builder => true,
        _ => false
    };

    public bool IsStructuralPattern => Type switch
    {
        PatternType.Adapter => true,
        PatternType.Decorator => true,
        PatternType.Facade => true,
        PatternType.Proxy => true,
        _ => false
    };

    public bool IsBehavioralPattern => Type switch
    {
        PatternType.Observer => true,
        PatternType.Strategy => true,
        PatternType.Command => true,
        PatternType.Template => true,
        _ => false
    };

    public bool IsArchitecturalPattern => Type switch
    {
        PatternType.MVC => true,
        PatternType.Repository => true,
        PatternType.UnitOfWork => true,
        PatternType.DependencyInjection => true,
        PatternType.LayeredArchitecture => true,
        _ => false
    };

    public bool IsDomainDrivenDesignPattern => Type switch
    {
        PatternType.Aggregate => true,
        PatternType.Entity => true,
        PatternType.ValueObject => true,
        PatternType.DomainService => true,
        _ => false
    };

    public bool IsMicroservicesPattern => Type switch
    {
        PatternType.APIGateway => true,
        PatternType.ServiceMesh => true,
        PatternType.EventSourcing => true,
        PatternType.CQRS => true,
        _ => false
    };

    public bool HasViolations => Metadata.HasViolations;
    public bool HasCriticalViolations => Metadata.HasCriticalViolations;
    public int ViolationCount => Metadata.ViolationCount;

    public string GetPatternCategory()
    {
        return Type switch
        {
            _ when IsCreationalPattern => "Creational",
            _ when IsStructuralPattern => "Structural", 
            _ when IsBehavioralPattern => "Behavioral",
            _ when IsArchitecturalPattern => "Architectural",
            _ when IsDomainDrivenDesignPattern => "Domain-Driven Design",
            _ when IsMicroservicesPattern => "Microservices",
            _ => "Unknown"
        };
    }

    public override string ToString()
    {
        return $"{Name} ({GetPatternCategory()}) - {ParticipantCount} participants, Confidence: {Confidence:P0}";
    }
}