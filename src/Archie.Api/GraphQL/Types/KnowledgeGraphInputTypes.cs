using HotChocolate.Types;
using Archie.Domain.ValueObjects;

namespace Archie.Api.GraphQL.Types;

// Input types for knowledge graph operations
public record BuildKnowledgeGraphInput(
    List<Guid> RepositoryIds,
    bool IncludePatterns = true,
    bool IncludeCrossRepository = false,
    AnalysisDepth AnalysisDepth = AnalysisDepth.Standard
);

public class BuildKnowledgeGraphInputType : InputObjectType<BuildKnowledgeGraphInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<BuildKnowledgeGraphInput> descriptor)
    {
        descriptor.Description("Input for building a knowledge graph");

        descriptor.Field(i => i.RepositoryIds).Description("Repository IDs to include in the graph");
        descriptor.Field(i => i.IncludePatterns).Description("Whether to include pattern detection");
        descriptor.Field(i => i.IncludeCrossRepository).Description("Whether to include cross-repository analysis");
        descriptor.Field(i => i.AnalysisDepth).Description("Depth of analysis to perform");
    }
}

public record EntityQueryInput(
    List<Guid> RepositoryIds,
    List<EntityType> EntityTypes,
    string? NamePattern = null,
    RelationshipType? HasRelationshipType = null,
    IntRange? ComplexityRange = null,
    List<string> LanguageFilter = null!,
    int Limit = 100,
    int Offset = 0
);

public class EntityQueryInputType : InputObjectType<EntityQueryInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<EntityQueryInput> descriptor)
    {
        descriptor.Description("Input for querying code entities");

        descriptor.Field(i => i.RepositoryIds).Description("Repository IDs to search in");
        descriptor.Field(i => i.EntityTypes).Description("Types of entities to find");
        descriptor.Field(i => i.NamePattern).Description("Name pattern to match");
        descriptor.Field(i => i.HasRelationshipType).Description("Entities that have this relationship type");
        descriptor.Field(i => i.ComplexityRange).Description("Range of complexity scores");
        descriptor.Field(i => i.LanguageFilter).Description("Programming languages to include");
        descriptor.Field(i => i.Limit).Description("Maximum number of results");
        descriptor.Field(i => i.Offset).Description("Offset for pagination");
    }
}

public record IntRange(int Min, int Max);

public class IntRangeType : InputObjectType<IntRange>
{
    protected override void Configure(IInputObjectTypeDescriptor<IntRange> descriptor)
    {
        descriptor.Description("Integer range for filtering");

        descriptor.Field(r => r.Min).Description("Minimum value");
        descriptor.Field(r => r.Max).Description("Maximum value");
    }
}

public record RelationshipQueryInput(
    string? SourceEntityId = null,
    string? TargetEntityId = null,
    List<RelationshipType> RelationshipTypes = null!,
    double? MinimumWeight = null,
    List<Guid> RepositoryIds = null!
);

public class RelationshipQueryInputType : InputObjectType<RelationshipQueryInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<RelationshipQueryInput> descriptor)
    {
        descriptor.Description("Input for querying code relationships");

        descriptor.Field(i => i.SourceEntityId).Description("Source entity ID");
        descriptor.Field(i => i.TargetEntityId).Description("Target entity ID");
        descriptor.Field(i => i.RelationshipTypes).Description("Types of relationships to find");
        descriptor.Field(i => i.MinimumWeight).Description("Minimum relationship weight");
        descriptor.Field(i => i.RepositoryIds).Description("Repository IDs to search in");
    }
}

public record PatternQueryInput(
    List<Guid> RepositoryIds,
    List<PatternType> PatternTypes = null!,
    double? MinimumConfidence = null,
    bool IncludeViolations = false
);

public class PatternQueryInputType : InputObjectType<PatternQueryInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<PatternQueryInput> descriptor)
    {
        descriptor.Description("Input for querying architectural patterns");

        descriptor.Field(i => i.RepositoryIds).Description("Repository IDs to search in");
        descriptor.Field(i => i.PatternTypes).Description("Types of patterns to find");
        descriptor.Field(i => i.MinimumConfidence).Description("Minimum pattern confidence");
        descriptor.Field(i => i.IncludeViolations).Description("Whether to include patterns with violations");
    }
}

public record FindRelationshipPathInput(
    string SourceEntityId,
    string TargetEntityId,
    int MaxDepth = 5
);

public class FindRelationshipPathInputType : InputObjectType<FindRelationshipPathInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<FindRelationshipPathInput> descriptor)
    {
        descriptor.Description("Input for finding relationship paths between entities");

        descriptor.Field(i => i.SourceEntityId).Description("Source entity ID");
        descriptor.Field(i => i.TargetEntityId).Description("Target entity ID");
        descriptor.Field(i => i.MaxDepth).Description("Maximum depth to search");
    }
}