namespace Archie.Domain.ValueObjects;

public record EntityAttribute
{
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public AttributeType Type { get; init; }

    public static EntityAttribute Create(string name, string value, AttributeType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or empty", nameof(name));

        return new EntityAttribute
        {
            Name = name,
            Value = value ?? string.Empty,
            Type = type
        };
    }

    public static EntityAttribute Annotation(string name, string value = "")
        => Create(name, value, AttributeType.Annotation);

    public static EntityAttribute Comment(string value)
        => Create("Comment", value, AttributeType.Comment);

    public static EntityAttribute Documentation(string value)
        => Create("Documentation", value, AttributeType.Documentation);

    public static EntityAttribute Tag(string name, string value = "")
        => Create(name, value, AttributeType.Tag);
}