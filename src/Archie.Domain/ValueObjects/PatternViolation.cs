namespace Archie.Domain.ValueObjects;

public record PatternViolation
{
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public ViolationSeverity Severity { get; init; }

    public static PatternViolation Create(
        string type,
        string description,
        string entityId,
        ViolationSeverity severity = ViolationSeverity.Warning)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Violation type cannot be null or empty", nameof(type));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Violation description cannot be null or empty", nameof(description));
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));

        return new PatternViolation
        {
            Type = type,
            Description = description,
            EntityId = entityId,
            Severity = severity
        };
    }

    public bool IsCritical => Severity == ViolationSeverity.Critical;
    public bool IsError => Severity == ViolationSeverity.Error;
    public bool IsWarning => Severity == ViolationSeverity.Warning;
    public bool IsInfo => Severity == ViolationSeverity.Info;

    public override string ToString()
    {
        return $"{Severity}: {Type} - {Description} (Entity: {EntityId})";
    }
}