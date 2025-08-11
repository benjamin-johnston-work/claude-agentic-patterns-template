using HotChocolate.Types;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Api.GraphQL.Types;

// Enum types
public class GraphStatusType : EnumType<GraphStatus> { }
public class AnalysisDepthType : EnumType<AnalysisDepth> { }
public class EntityTypeEnumType : EnumType<EntityType> { }
public class AccessModifierType : EnumType<AccessModifier> { }
public class AttributeTypeEnumType : EnumType<AttributeType> { }
public class RelationshipTypeEnumType : EnumType<RelationshipType> { }
public class PatternTypeEnumType : EnumType<PatternType> { }
public class ViolationSeverityType : EnumType<ViolationSeverity> { }

// Knowledge Graph main types
public class KnowledgeGraphType : ObjectType<KnowledgeGraph>
{
    protected override void Configure(IObjectTypeDescriptor<KnowledgeGraph> descriptor)
    {
        descriptor.Description("Represents a knowledge graph constructed from repository analysis");

        descriptor.Field(k => k.Id).Description("Unique identifier for the knowledge graph");
        descriptor.Field(k => k.RepositoryIds).Description("List of repository IDs included in this graph");
        descriptor.Field(k => k.Status).Description("Current status of the knowledge graph");
        descriptor.Field(k => k.CreatedAt).Description("When the knowledge graph was created");
        descriptor.Field(k => k.LastUpdatedAt).Description("When the knowledge graph was last updated");
        descriptor.Field(k => k.Statistics).Description("Statistical information about the knowledge graph");
        descriptor.Field(k => k.Metadata).Description("Metadata about the knowledge graph construction");
    }
}

public class GraphStatisticsType : ObjectType<GraphStatistics>
{
    protected override void Configure(IObjectTypeDescriptor<GraphStatistics> descriptor)
    {
        descriptor.Description("Statistical information about a knowledge graph");

        descriptor.Field(s => s.TotalEntities).Description("Total number of entities in the graph");
        descriptor.Field(s => s.TotalRelationships).Description("Total number of relationships in the graph");
        descriptor.Field(s => s.DetectedPatterns).Description("Number of detected architectural patterns");
        descriptor.Field(s => s.AverageComplexity).Description("Average complexity score of entities");
        descriptor.Field(s => s.CrossRepositoryRelationships).Description("Number of cross-repository relationships");
        descriptor.Field(s => s.LastAnalysis).Description("When the last analysis was performed");
    }
}

public class GraphMetadataType : ObjectType<GraphMetadata>
{
    protected override void Configure(IObjectTypeDescriptor<GraphMetadata> descriptor)
    {
        descriptor.Description("Metadata about knowledge graph construction");

        descriptor.Field(m => m.Description).Description("Description of the knowledge graph");
        descriptor.Field(m => m.AnalysisDepth).Description("Depth of analysis performed");
        descriptor.Field(m => m.IncludePatterns).Description("Whether patterns were included in analysis");
        descriptor.Field(m => m.IncludeCrossRepository).Description("Whether cross-repository analysis was performed");
        descriptor.Field(m => m.SupportedLanguages).Description("List of supported programming languages");
        descriptor.Field(m => m.CreatedBy).Description("Who created the knowledge graph");
        descriptor.Field(m => m.LastUpdatedBy).Description("Who last updated the knowledge graph");
    }
}

// Code Entity types
public class CodeEntityType : ObjectType<CodeEntity>
{
    protected override void Configure(IObjectTypeDescriptor<CodeEntity> descriptor)
    {
        descriptor.Description("Represents a code entity (class, method, interface, etc.)");

        descriptor.Field(e => e.Id).Description("Unique identifier for the entity");
        descriptor.Field(e => e.EntityId).Description("Entity identifier within repository");
        descriptor.Field(e => e.RepositoryId).Description("Repository this entity belongs to");
        descriptor.Field(e => e.Name).Description("Name of the entity");
        descriptor.Field(e => e.FullName).Description("Full qualified name of the entity");
        descriptor.Field(e => e.Type).Description("Type of the entity (class, method, etc.)");
        descriptor.Field(e => e.FilePath).Description("Path to the file containing this entity");
        descriptor.Field(e => e.Language).Description("Programming language of the entity");
        descriptor.Field(e => e.Location).Description("Location within the source file");
        descriptor.Field(e => e.Content).Description("Source code content of the entity");
        descriptor.Field(e => e.Metadata).Description("Additional metadata about the entity");
        descriptor.Field(e => e.Attributes).Description("Attributes, annotations, or decorators");
        
        // Computed fields
        descriptor.Field(e => e.IsClass).Description("Whether this entity is a class");
        descriptor.Field(e => e.IsInterface).Description("Whether this entity is an interface");
        descriptor.Field(e => e.IsMethod).Description("Whether this entity is a method or function");
        descriptor.Field(e => e.IsProperty).Description("Whether this entity is a property or field");
        descriptor.Field(e => e.IsTest).Description("Whether this entity is a test");
        descriptor.Field(e => e.HasDocumentation).Description("Whether this entity has documentation");
        descriptor.Field(e => e.ComplexityScore).Description("Complexity score of the entity");
    }
}

public class EntityLocationType : ObjectType<EntityLocation>
{
    protected override void Configure(IObjectTypeDescriptor<EntityLocation> descriptor)
    {
        descriptor.Description("Location of an entity within a source file");

        descriptor.Field(l => l.StartLine).Description("Starting line number");
        descriptor.Field(l => l.EndLine).Description("Ending line number");
        descriptor.Field(l => l.StartColumn).Description("Starting column number");
        descriptor.Field(l => l.EndColumn).Description("Ending column number");
        descriptor.Field(l => l.LineCount).Description("Number of lines the entity spans");
        descriptor.Field(l => l.IsValid).Description("Whether the location is valid");
    }
}

public class EntityMetadataType : ObjectType<EntityMetadata>
{
    protected override void Configure(IObjectTypeDescriptor<EntityMetadata> descriptor)
    {
        descriptor.Description("Metadata about a code entity");

        descriptor.Field(m => m.Namespace).Description("Namespace of the entity");
        descriptor.Field(m => m.AccessModifier).Description("Access modifier (public, private, etc.)");
        descriptor.Field(m => m.Modifiers).Description("List of modifiers (static, abstract, etc.)");
        descriptor.Field(m => m.Annotations).Description("List of annotations or attributes");
        descriptor.Field(m => m.Signature).Description("Method or class signature");
        descriptor.Field(m => m.ComplexityScore).Description("Cyclomatic complexity score");
        descriptor.Field(m => m.IsStatic).Description("Whether the entity is static");
        descriptor.Field(m => m.IsAbstract).Description("Whether the entity is abstract");
        descriptor.Field(m => m.IsVirtual).Description("Whether the entity is virtual");
        descriptor.Field(m => m.IsAsync).Description("Whether the entity is async");
    }
}

public class EntityAttributeType : ObjectType<EntityAttribute>
{
    protected override void Configure(IObjectTypeDescriptor<EntityAttribute> descriptor)
    {
        descriptor.Description("An attribute, annotation, or decorator on a code entity");

        descriptor.Field(a => a.Name).Description("Name of the attribute");
        descriptor.Field(a => a.Value).Description("Value of the attribute");
        descriptor.Field(a => a.Type).Description("Type of the attribute");
    }
}

// Code Relationship types
public class CodeRelationshipType : ObjectType<CodeRelationship>
{
    protected override void Configure(IObjectTypeDescriptor<CodeRelationship> descriptor)
    {
        descriptor.Description("Represents a relationship between two code entities");

        descriptor.Field(r => r.Id).Description("Unique identifier for the relationship");
        descriptor.Field(r => r.SourceEntityId).Description("ID of the source entity");
        descriptor.Field(r => r.TargetEntityId).Description("ID of the target entity");
        descriptor.Field(r => r.Type).Description("Type of relationship");
        descriptor.Field(r => r.Weight).Description("Strength/importance of the relationship");
        descriptor.Field(r => r.Metadata).Description("Additional metadata about the relationship");
        descriptor.Field(r => r.DetectedAt).Description("When the relationship was detected");
        
        // Computed fields
        descriptor.Field(r => r.IsStrongRelationship).Description("Whether this is a strong relationship");
        descriptor.Field(r => r.IsMediumRelationship).Description("Whether this is a medium strength relationship");
        descriptor.Field(r => r.IsWeakRelationship).Description("Whether this is a weak relationship");
        descriptor.Field(r => r.IsStructuralRelationship).Description("Whether this is a structural relationship");
        descriptor.Field(r => r.IsBehavioralRelationship).Description("Whether this is a behavioral relationship");
        descriptor.Field(r => r.IsArchitecturalRelationship).Description("Whether this is an architectural relationship");
        descriptor.Field(r => r.IsHighConfidence).Description("Whether this relationship has high confidence");
    }
}

public class RelationshipMetadataType : ObjectType<RelationshipMetadata>
{
    protected override void Configure(IObjectTypeDescriptor<RelationshipMetadata> descriptor)
    {
        descriptor.Description("Metadata about a code relationship");

        descriptor.Field(m => m.Confidence).Description("Confidence in relationship accuracy (0-100)");
        descriptor.Field(m => m.DetectionMethod).Description("How the relationship was detected");
        descriptor.Field(m => m.SourceReferences).Description("Code locations where relationship is evident");
        descriptor.Field(m => m.IsHighConfidence).Description("Whether confidence is high (>=80%)");
        descriptor.Field(m => m.IsMediumConfidence).Description("Whether confidence is medium (60-79%)");
        descriptor.Field(m => m.IsLowConfidence).Description("Whether confidence is low (<60%)");
    }
}

// Architectural Pattern types
public class ArchitecturalPatternType : ObjectType<ArchitecturalPattern>
{
    protected override void Configure(IObjectTypeDescriptor<ArchitecturalPattern> descriptor)
    {
        descriptor.Description("Represents a detected architectural pattern");

        descriptor.Field(p => p.Id).Description("Unique identifier for the pattern");
        descriptor.Field(p => p.Name).Description("Name of the pattern");
        descriptor.Field(p => p.Type).Description("Type of architectural pattern");
        descriptor.Field(p => p.ParticipatingEntityIds).Description("IDs of entities participating in this pattern");
        descriptor.Field(p => p.RepositoryId).Description("Repository where the pattern was found");
        descriptor.Field(p => p.Confidence).Description("Confidence in pattern detection");
        descriptor.Field(p => p.Metadata).Description("Additional metadata about the pattern");
        descriptor.Field(p => p.DetectedAt).Description("When the pattern was detected");
        
        // Computed fields
        descriptor.Field(p => p.IsHighConfidence).Description("Whether pattern detection has high confidence");
        descriptor.Field(p => p.IsMediumConfidence).Description("Whether pattern detection has medium confidence");
        descriptor.Field(p => p.IsLowConfidence).Description("Whether pattern detection has low confidence");
        descriptor.Field(p => p.ParticipantCount).Description("Number of entities participating in the pattern");
        descriptor.Field(p => p.IsCreationalPattern).Description("Whether this is a creational pattern");
        descriptor.Field(p => p.IsStructuralPattern).Description("Whether this is a structural pattern");
        descriptor.Field(p => p.IsBehavioralPattern).Description("Whether this is a behavioral pattern");
        descriptor.Field(p => p.IsArchitecturalPattern).Description("Whether this is an architectural pattern");
        descriptor.Field(p => p.HasViolations).Description("Whether the pattern has violations");
    }
}

public class PatternMetadataType : ObjectType<PatternMetadata>
{
    protected override void Configure(IObjectTypeDescriptor<PatternMetadata> descriptor)
    {
        descriptor.Description("Metadata about an architectural pattern");

        descriptor.Field(m => m.PatternCharacteristics).Description("Characteristics of the pattern");
        descriptor.Field(m => m.Description).Description("Description of the pattern");
        descriptor.Field(m => m.Violations).Description("Pattern violations found");
        descriptor.Field(m => m.HasViolations).Description("Whether the pattern has violations");
        descriptor.Field(m => m.HasCriticalViolations).Description("Whether the pattern has critical violations");
        descriptor.Field(m => m.ViolationCount).Description("Total number of violations");
    }
}

public class PatternViolationType : ObjectType<PatternViolation>
{
    protected override void Configure(IObjectTypeDescriptor<PatternViolation> descriptor)
    {
        descriptor.Description("A violation of an architectural pattern");

        descriptor.Field(v => v.Type).Description("Type of violation");
        descriptor.Field(v => v.Description).Description("Description of the violation");
        descriptor.Field(v => v.EntityId).Description("ID of the entity causing the violation");
        descriptor.Field(v => v.Severity).Description("Severity of the violation");
        descriptor.Field(v => v.IsCritical).Description("Whether the violation is critical");
        descriptor.Field(v => v.IsError).Description("Whether the violation is an error");
        descriptor.Field(v => v.IsWarning).Description("Whether the violation is a warning");
        descriptor.Field(v => v.IsInfo).Description("Whether the violation is informational");
    }
}

public class AntiPatternType : ObjectType<AntiPattern>
{
    protected override void Configure(IObjectTypeDescriptor<AntiPattern> descriptor)
    {
        descriptor.Description("An anti-pattern detected in the codebase");

        descriptor.Field(a => a.Name).Description("Name of the anti-pattern");
        descriptor.Field(a => a.Description).Description("Description of the anti-pattern");
        descriptor.Field(a => a.Severity).Description("Severity of the anti-pattern");
        descriptor.Field(a => a.AffectedEntities).Description("Entities affected by this anti-pattern");
        descriptor.Field(a => a.Remediation).Description("Suggested remediation for the anti-pattern");
        descriptor.Field(a => a.IsCritical).Description("Whether the anti-pattern is critical");
        descriptor.Field(a => a.IsError).Description("Whether the anti-pattern is an error");
        descriptor.Field(a => a.IsWarning).Description("Whether the anti-pattern is a warning");
        descriptor.Field(a => a.AffectedEntityCount).Description("Number of affected entities");
    }
}