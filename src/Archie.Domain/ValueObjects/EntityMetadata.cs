namespace Archie.Domain.ValueObjects;

public record EntityMetadata
{
    public string? Namespace { get; init; }
    public AccessModifier AccessModifier { get; init; }
    public List<string> Modifiers { get; init; } = new(); // static, abstract, virtual, etc.
    public List<string> Annotations { get; init; } = new(); // Attributes, decorators
    public string? Signature { get; init; }
    public int ComplexityScore { get; init; }
    public Dictionary<string, object> LanguageSpecific { get; init; } = new();

    public static EntityMetadata Create(
        string? @namespace = null,
        AccessModifier accessModifier = AccessModifier.Public,
        List<string>? modifiers = null,
        List<string>? annotations = null,
        string? signature = null,
        int complexityScore = 0,
        Dictionary<string, object>? languageSpecific = null)
    {
        return new EntityMetadata
        {
            Namespace = @namespace,
            AccessModifier = accessModifier,
            Modifiers = modifiers ?? new List<string>(),
            Annotations = annotations ?? new List<string>(),
            Signature = signature,
            ComplexityScore = Math.Max(0, complexityScore),
            LanguageSpecific = languageSpecific ?? new Dictionary<string, object>()
        };
    }

    public static EntityMetadata Empty => new();

    public bool HasModifier(string modifier)
    {
        return Modifiers.Contains(modifier, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasAnnotation(string annotation)
    {
        return Annotations.Any(a => a.Contains(annotation, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsStatic => HasModifier("static");
    public bool IsAbstract => HasModifier("abstract");
    public bool IsVirtual => HasModifier("virtual");
    public bool IsAsync => HasModifier("async");
}