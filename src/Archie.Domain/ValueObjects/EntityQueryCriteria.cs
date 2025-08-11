namespace Archie.Domain.ValueObjects;

public record EntityQueryCriteria
{
    public List<Guid> RepositoryIds { get; init; } = new();
    public List<EntityType> EntityTypes { get; init; } = new();
    public string? NamePattern { get; init; }
    public RelationshipType? HasRelationshipType { get; init; }
    public (int Min, int Max)? ComplexityRange { get; init; }
    public List<string> LanguageFilter { get; init; } = new();
    public string? Namespace { get; init; }
    public AccessModifier? AccessModifier { get; init; }
    public List<string> RequiredModifiers { get; init; } = new();
    public List<string> RequiredAnnotations { get; init; } = new();
    public int Limit { get; init; } = 100;
    public int Offset { get; init; } = 0;

    public static EntityQueryCriteria Create(
        List<Guid>? repositoryIds = null,
        List<EntityType>? entityTypes = null,
        string? namePattern = null,
        RelationshipType? hasRelationshipType = null,
        (int Min, int Max)? complexityRange = null,
        List<string>? languageFilter = null,
        int limit = 100,
        int offset = 0)
    {
        return new EntityQueryCriteria
        {
            RepositoryIds = repositoryIds ?? new List<Guid>(),
            EntityTypes = entityTypes ?? new List<EntityType>(),
            NamePattern = namePattern,
            HasRelationshipType = hasRelationshipType,
            ComplexityRange = complexityRange,
            LanguageFilter = languageFilter ?? new List<string>(),
            Limit = Math.Clamp(limit, 1, 1000),
            Offset = Math.Max(0, offset)
        };
    }

    public static EntityQueryCriteria All => new()
    {
        Limit = 1000
    };

    public static EntityQueryCriteria ForRepository(Guid repositoryId, int limit = 100)
    {
        return new EntityQueryCriteria
        {
            RepositoryIds = new List<Guid> { repositoryId },
            Limit = Math.Clamp(limit, 1, 1000)
        };
    }

    public static EntityQueryCriteria ForEntityType(EntityType entityType, int limit = 100)
    {
        return new EntityQueryCriteria
        {
            EntityTypes = new List<EntityType> { entityType },
            Limit = Math.Clamp(limit, 1, 1000)
        };
    }

    public static EntityQueryCriteria ForLanguage(string language, int limit = 100)
    {
        return new EntityQueryCriteria
        {
            LanguageFilter = new List<string> { language },
            Limit = Math.Clamp(limit, 1, 1000)
        };
    }

    public bool HasFilters => RepositoryIds.Count > 0 ||
                             EntityTypes.Count > 0 ||
                             !string.IsNullOrWhiteSpace(NamePattern) ||
                             HasRelationshipType.HasValue ||
                             ComplexityRange.HasValue ||
                             LanguageFilter.Count > 0 ||
                             !string.IsNullOrWhiteSpace(Namespace) ||
                             AccessModifier.HasValue ||
                             RequiredModifiers.Count > 0 ||
                             RequiredAnnotations.Count > 0;
}