using Archie.Application.DTOs;
using Archie.Application.UseCases;
using Archie.Domain.ValueObjects;
using HotChocolate.Types;

namespace Archie.Api.GraphQL.Types;

public class DocumentationType : ObjectType<DocumentationDto>
{
    protected override void Configure(IObjectTypeDescriptor<DocumentationDto> descriptor)
    {
        descriptor.Field(d => d.Id)
            .Type<NonNullType<IdType>>();

        descriptor.Field(d => d.RepositoryId)
            .Type<NonNullType<IdType>>();

        descriptor.Field(d => d.Title)
            .Type<NonNullType<StringType>>();

        descriptor.Field(d => d.Status)
            .Type<NonNullType<DocumentationStatusType>>();

        descriptor.Field(d => d.Sections)
            .Type<NonNullType<ListType<NonNullType<DocumentationSectionType>>>>();

        descriptor.Field(d => d.Metadata)
            .Type<NonNullType<DocumentationMetadataType>>();

        descriptor.Field(d => d.GeneratedAt)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(d => d.LastUpdatedAt)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(d => d.Version)
            .Type<NonNullType<StringType>>();

        descriptor.Field(d => d.Statistics)
            .Type<NonNullType<DocumentationStatisticsType>>();

        descriptor.Field(d => d.ErrorMessage)
            .Type<StringType>();

        descriptor.Field(d => d.TotalSections)
            .Type<NonNullType<IntType>>();

        descriptor.Field(d => d.EstimatedReadingTime)
            .Type<NonNullType<FloatType>>();

        descriptor.Field(d => d.LastGenerated)
            .Type<DateTimeType>();

        descriptor.Field(d => d.GenerationDuration)
            .Type<NonNullType<FloatType>>();

        descriptor.Field(d => d.SectionsGenerated)
            .Type<NonNullType<IntType>>();

        // Navigation property back to repository
        descriptor.Field("repository")
            .Type<RepositoryType>()
            .Resolve(async context =>
            {
                var documentation = context.Parent<DocumentationDto>();
                var repositoryService = context.Service<GetRepositoryUseCase>();
                var result = await repositoryService.ExecuteAsync(documentation.RepositoryId, context.RequestAborted);
                return result.IsSuccess ? result.Value : null;
            });
    }
}

public class DocumentationSectionType : ObjectType<DocumentationSectionDto>
{
    protected override void Configure(IObjectTypeDescriptor<DocumentationSectionDto> descriptor)
    {
        descriptor.Field(s => s.Id)
            .Type<NonNullType<IdType>>();

        descriptor.Field(s => s.Title)
            .Type<NonNullType<StringType>>();

        descriptor.Field(s => s.Content)
            .Type<NonNullType<StringType>>();

        descriptor.Field(s => s.Type)
            .Type<NonNullType<DocumentationSectionTypeEnum>>();

        descriptor.Field(s => s.Order)
            .Type<NonNullType<IntType>>();

        descriptor.Field(s => s.CodeReferences)
            .Type<NonNullType<ListType<NonNullType<CodeReferenceType>>>>();

        descriptor.Field(s => s.Tags)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();

        descriptor.Field(s => s.Metadata)
            .Type<NonNullType<SectionMetadataType>>();
    }
}

public class DocumentationMetadataType : ObjectType<DocumentationMetadataDto>
{
    protected override void Configure(IObjectTypeDescriptor<DocumentationMetadataDto> descriptor)
    {
        descriptor.Field(m => m.RepositoryName)
            .Type<NonNullType<StringType>>();

        descriptor.Field(m => m.RepositoryUrl)
            .Type<NonNullType<StringType>>();

        descriptor.Field(m => m.PrimaryLanguage)
            .Type<NonNullType<StringType>>();

        descriptor.Field(m => m.Languages)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();

        descriptor.Field(m => m.Frameworks)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();

        descriptor.Field(m => m.Dependencies)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();

        descriptor.Field(m => m.ProjectType)
            .Type<NonNullType<StringType>>();

        descriptor.Field(m => m.CustomProperties)
            .Type<AnyType>();

        descriptor.Field(m => m.TotalWords)
            .Type<NonNullType<IntType>>();
    }
}

public class DocumentationStatisticsType : ObjectType<DocumentationStatisticsDto>
{
    protected override void Configure(IObjectTypeDescriptor<DocumentationStatisticsDto> descriptor)
    {
        descriptor.Field(s => s.TotalSections)
            .Type<NonNullType<IntType>>();

        descriptor.Field(s => s.CodeReferences)
            .Type<NonNullType<IntType>>();

        descriptor.Field(s => s.WordCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field("generationTime")
            .Resolve(context => context.Parent<DocumentationStatisticsDto>().GenerationTimeSeconds)
            .Type<NonNullType<FloatType>>();

        descriptor.Field(s => s.AccuracyScore)
            .Type<NonNullType<FloatType>>();

        descriptor.Field(s => s.CoveredTopics)
            .Type<NonNullType<ListType<NonNullType<StringType>>>>();
    }
}

public class CodeReferenceType : ObjectType<CodeReferenceDto>
{
    protected override void Configure(IObjectTypeDescriptor<CodeReferenceDto> descriptor)
    {
        descriptor.Field(cr => cr.FilePath)
            .Type<NonNullType<StringType>>();

        descriptor.Field(cr => cr.StartLine)
            .Type<IntType>();

        descriptor.Field(cr => cr.EndLine)
            .Type<IntType>();

        descriptor.Field(cr => cr.CodeSnippet)
            .Type<NonNullType<StringType>>();

        descriptor.Field(cr => cr.Description)
            .Type<NonNullType<StringType>>();

        descriptor.Field(cr => cr.ReferenceType)
            .Type<NonNullType<StringType>>();
    }
}

public class SectionMetadataType : ObjectType<SectionMetadataDto>
{
    protected override void Configure(IObjectTypeDescriptor<SectionMetadataDto> descriptor)
    {
        descriptor.Field(m => m.CreatedAt)
            .Type<NonNullType<DateTimeType>>();

        descriptor.Field(m => m.LastModifiedAt)
            .Type<DateTimeType>();

        descriptor.Field(m => m.GeneratedBy)
            .Type<StringType>();

        descriptor.Field(m => m.Model)
            .Type<StringType>();

        descriptor.Field(m => m.TokenCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.ConfidenceScore)
            .Type<FloatType>();

        descriptor.Field(m => m.AdditionalProperties)
            .Type<AnyType>();

        descriptor.Field(m => m.WordCount)
            .Type<NonNullType<IntType>>();

        descriptor.Field(m => m.ReadingTime)
            .Type<NonNullType<FloatType>>();

        descriptor.Field(m => m.LastModified)
            .Type<DateTimeType>();
    }
}

public class DocumentationStatusType : EnumType<DocumentationStatus>
{
    protected override void Configure(IEnumTypeDescriptor<DocumentationStatus> descriptor)
    {
        descriptor.Name("DocumentationStatus");
        
        descriptor.Value(DocumentationStatus.NotStarted)
            .Name("NOT_STARTED");
        
        descriptor.Value(DocumentationStatus.Analyzing)
            .Name("ANALYZING");
        
        descriptor.Value(DocumentationStatus.GeneratingContent)
            .Name("GENERATING_CONTENT");
        
        descriptor.Value(DocumentationStatus.Enriching)
            .Name("ENRICHING");
        
        descriptor.Value(DocumentationStatus.Indexing)
            .Name("INDEXING");
        
        descriptor.Value(DocumentationStatus.Completed)
            .Name("COMPLETED");
        
        descriptor.Value(DocumentationStatus.Error)
            .Name("ERROR");
        
        descriptor.Value(DocumentationStatus.UpdateRequired)
            .Name("UPDATE_REQUIRED");
    }
}

public class DocumentationSectionTypeEnum : EnumType<Domain.ValueObjects.DocumentationSectionType>
{
    protected override void Configure(IEnumTypeDescriptor<Domain.ValueObjects.DocumentationSectionType> descriptor)
    {
        descriptor.Name("DocumentationSectionType");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Overview)
            .Name("OVERVIEW");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Architecture)
            .Name("ARCHITECTURE");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.GettingStarted)
            .Name("GETTING_STARTED");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Installation)
            .Name("INSTALLATION");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Usage)
            .Name("USAGE");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.ApiReference)
            .Name("API_REFERENCE");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Configuration)
            .Name("CONFIGURATION");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Testing)
            .Name("TESTING");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Deployment)
            .Name("DEPLOYMENT");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Contributing)
            .Name("CONTRIBUTING");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Troubleshooting)
            .Name("TROUBLESHOOTING");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Examples)
            .Name("EXAMPLES");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.Changelog)
            .Name("CHANGELOG");
        
        descriptor.Value(Domain.ValueObjects.DocumentationSectionType.License)
            .Name("LICENSE");
    }
}

// Input types
public class GenerateDocumentationInputType : InputObjectType<GenerateDocumentationInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<GenerateDocumentationInput> descriptor)
    {
        descriptor.Field(i => i.RepositoryId)
            .Type<NonNullType<IdType>>();

        descriptor.Field(i => i.Sections)
            .Type<ListType<NonNullType<DocumentationSectionTypeEnum>>>();

        descriptor.Field(i => i.IncludeCodeExamples)
            .Type<BooleanType>()
            .DefaultValue(true);

        descriptor.Field(i => i.IncludeApiReference)
            .Type<BooleanType>()
            .DefaultValue(true);

        descriptor.Field(i => i.CustomInstructions)
            .Type<StringType>();

        descriptor.Field(i => i.Regenerate)
            .Type<BooleanType>()
            .DefaultValue(false);
    }
}

public class UpdateDocumentationSectionInputType : InputObjectType<UpdateDocumentationSectionInput>
{
    protected override void Configure(IInputObjectTypeDescriptor<UpdateDocumentationSectionInput> descriptor)
    {
        descriptor.Field(i => i.DocumentationId)
            .Type<NonNullType<IdType>>();

        descriptor.Field(i => i.SectionId)
            .Type<NonNullType<IdType>>();

        descriptor.Field(i => i.Content)
            .Type<NonNullType<StringType>>();

        descriptor.Field(i => i.Tags)
            .Type<ListType<NonNullType<StringType>>>();
    }
}

public class DocumentationGenerationProgressType : ObjectType<DocumentationGenerationProgressDto>
{
    protected override void Configure(IObjectTypeDescriptor<DocumentationGenerationProgressDto> descriptor)
    {
        descriptor.Field(p => p.RepositoryId)
            .Type<NonNullType<IdType>>();

        descriptor.Field(p => p.Status)
            .Type<NonNullType<DocumentationStatusType>>();

        descriptor.Field(p => p.Progress)
            .Type<NonNullType<FloatType>>();

        descriptor.Field(p => p.CurrentSection)
            .Type<StringType>();

        descriptor.Field(p => p.EstimatedTimeRemainingSeconds)
            .Type<FloatType>();

        descriptor.Field(p => p.Message)
            .Type<StringType>();
    }
}