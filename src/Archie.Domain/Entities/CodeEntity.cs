using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Entities;

public class CodeEntity : BaseEntity
{
    private readonly List<EntityAttribute> _attributes = new();

    public string EntityId { get; private set; } = string.Empty; // Unique identifier within repository
    public Guid RepositoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public EntityType Type { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public EntityLocation Location { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public float[] ContentVector { get; private set; } = Array.Empty<float>(); // Semantic embedding
    public EntityMetadata Metadata { get; private set; }
    public IReadOnlyList<EntityAttribute> Attributes => _attributes.AsReadOnly();

    protected CodeEntity() // EF Constructor
    {
        Location = EntityLocation.Unknown;
        Metadata = EntityMetadata.Empty;
    }

    public CodeEntity(string entityId, Guid repositoryId, string name, EntityType type, string filePath, string language)
    {
        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("Entity ID cannot be null or empty", nameof(entityId));
        if (repositoryId == Guid.Empty)
            throw new ArgumentException("Repository ID cannot be empty", nameof(repositoryId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Entity name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        if (string.IsNullOrWhiteSpace(language))
            throw new ArgumentException("Language cannot be null or empty", nameof(language));

        EntityId = entityId;
        RepositoryId = repositoryId;
        Name = name;
        FullName = name; // Initially same as name, can be updated
        Type = type;
        FilePath = filePath;
        Language = language;
        Location = EntityLocation.Unknown;
        Metadata = EntityMetadata.Empty;
    }

    public static CodeEntity Create(string entityId, Guid repositoryId, string name, EntityType type, string filePath, string language)
    {
        return new CodeEntity(entityId, repositoryId, name, type, filePath, language);
    }

    public void UpdateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be null or empty", nameof(fullName));

        FullName = fullName;
    }

    public void UpdateLocation(EntityLocation location)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
    }

    public void UpdateContent(string content, float[]? contentVector = null)
    {
        Content = content ?? string.Empty;
        ContentVector = contentVector ?? Array.Empty<float>();
    }

    public void UpdateMetadata(EntityMetadata metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    public void AddAttribute(EntityAttribute attribute)
    {
        if (attribute == null)
            throw new ArgumentNullException(nameof(attribute));

        // Remove existing attribute with same name and type to avoid duplicates
        _attributes.RemoveAll(a => a.Name == attribute.Name && a.Type == attribute.Type);
        _attributes.Add(attribute);
    }

    public void RemoveAttribute(string name, AttributeType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or empty", nameof(name));

        _attributes.RemoveAll(a => a.Name == name && a.Type == type);
    }

    public void ClearAttributes()
    {
        _attributes.Clear();
    }

    public bool HasAttribute(string name, AttributeType? type = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return type.HasValue
            ? _attributes.Any(a => a.Name == name && a.Type == type.Value)
            : _attributes.Any(a => a.Name == name);
    }

    public EntityAttribute? GetAttribute(string name, AttributeType? type = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return type.HasValue
            ? _attributes.FirstOrDefault(a => a.Name == name && a.Type == type.Value)
            : _attributes.FirstOrDefault(a => a.Name == name);
    }

    public List<EntityAttribute> GetAttributesByType(AttributeType type)
    {
        return _attributes.Where(a => a.Type == type).ToList();
    }

    // Convenience properties
    public bool IsClass => Type == EntityType.Class;
    public bool IsInterface => Type == EntityType.Interface;
    public bool IsMethod => Type == EntityType.Method || Type == EntityType.Function;
    public bool IsProperty => Type == EntityType.Property || Type == EntityType.Field;
    public bool IsType => IsClass || IsInterface || Type == EntityType.Enum || Type == EntityType.Struct;
    public bool IsTest => Type == EntityType.Test || HasAttribute("Test") || HasAttribute("TestMethod");
    public bool HasDocumentation => _attributes.Any(a => a.Type == AttributeType.Documentation);
    public bool HasSemanticVector => ContentVector.Length > 0;
    public int ComplexityScore => Metadata.ComplexityScore;
}