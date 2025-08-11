namespace Archie.Domain.ValueObjects;

public record AntiPattern
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public ViolationSeverity Severity { get; init; }
    public List<string> AffectedEntities { get; init; } = new();
    public string Remediation { get; init; } = string.Empty;

    public static AntiPattern Create(
        string name,
        string description,
        ViolationSeverity severity,
        List<string> affectedEntities,
        string remediation = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Anti-pattern name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Anti-pattern description cannot be null or empty", nameof(description));

        return new AntiPattern
        {
            Name = name,
            Description = description,
            Severity = severity,
            AffectedEntities = affectedEntities ?? new List<string>(),
            Remediation = remediation
        };
    }

    public bool IsCritical => Severity == ViolationSeverity.Critical;
    public bool IsError => Severity == ViolationSeverity.Error;
    public bool IsWarning => Severity == ViolationSeverity.Warning;
    public int AffectedEntityCount => AffectedEntities.Count;

    // Common anti-patterns
    public static AntiPattern GodClass(List<string> affectedEntities)
        => Create("God Class", "A class that knows too much or does too much", ViolationSeverity.Error, affectedEntities, "Break down into smaller, more focused classes");

    public static AntiPattern LongMethod(List<string> affectedEntities)
        => Create("Long Method", "Methods that are too long and do too many things", ViolationSeverity.Warning, affectedEntities, "Extract smaller methods with single responsibilities");

    public static AntiPattern LongParameterList(List<string> affectedEntities)
        => Create("Long Parameter List", "Methods with too many parameters", ViolationSeverity.Warning, affectedEntities, "Use parameter objects or builder pattern");

    public static AntiPattern DuplicateCode(List<string> affectedEntities)
        => Create("Duplicate Code", "Identical or very similar code in multiple locations", ViolationSeverity.Warning, affectedEntities, "Extract common functionality into shared methods or classes");

    public static AntiPattern CircularDependency(List<string> affectedEntities)
        => Create("Circular Dependency", "Classes that depend on each other in a circular fashion", ViolationSeverity.Error, affectedEntities, "Break circular dependencies using interfaces or dependency inversion");
}