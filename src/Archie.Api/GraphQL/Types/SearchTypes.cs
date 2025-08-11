using HotChocolate.Types;
using Archie.Infrastructure.AzureSearch.Models;

namespace Archie.Api.GraphQL.Types;

/// <summary>
/// GraphQL types for Azure Search functionality
/// </summary>
public class SearchableDocumentType : ObjectType<SearchableDocument>
{
    protected override void Configure(IObjectTypeDescriptor<SearchableDocument> descriptor)
    {
        descriptor.Description("A document that has been indexed for search");

        descriptor.Field(f => f.Id)
            .Description("Unique identifier for the document");

        descriptor.Field(f => f.RepositoryId)
            .Description("ID of the repository this document belongs to");

        descriptor.Field(f => f.FilePath)
            .Description("Relative path of the file within the repository");

        descriptor.Field(f => f.FileName)
            .Description("Name of the file");

        descriptor.Field(f => f.FileExtension)
            .Description("File extension");

        descriptor.Field(f => f.Language)
            .Description("Programming language detected from file extension");

        descriptor.Field(f => f.Content)
            .Description("File content (may be truncated for large files)");

        descriptor.Field(f => f.ContentVector)
            .Ignore(); // Don't expose raw vector data in GraphQL

        descriptor.Field(f => f.LineCount)
            .Description("Number of lines in the file");

        descriptor.Field(f => f.SizeInBytes)
            .Description("Size of the file in bytes");

        descriptor.Field(f => f.LastModified)
            .Description("When the file was last modified");

        descriptor.Field(f => f.BranchName)
            .Description("Branch where this version of the file exists");

        descriptor.Field(f => f.Metadata)
            .Description("Additional metadata about the document");
    }
}

public class DocumentMetadataType : ObjectType<DocumentMetadata>
{
    protected override void Configure(IObjectTypeDescriptor<DocumentMetadata> descriptor)
    {
        descriptor.Description("Metadata associated with a searchable document");

        descriptor.Field(f => f.RepositoryName)
            .Description("Name of the repository");

        descriptor.Field(f => f.RepositoryOwner)
            .Description("Owner/organization of the repository");

        descriptor.Field(f => f.RepositoryUrl)
            .Description("URL to the repository");

        descriptor.Field(f => f.CodeSymbols)
            .Description("Code symbols extracted from the file (functions, classes, etc.)");

        descriptor.Field(f => f.CustomFields)
            .Description("Additional custom metadata fields");
    }
}

public class SearchResultType : ObjectType<SearchResult>
{
    protected override void Configure(IObjectTypeDescriptor<SearchResult> descriptor)
    {
        descriptor.Description("A search result with relevance score and highlights");

        descriptor.Field(f => f.DocumentId)
            .Description("ID of the matching document");

        descriptor.Field(f => f.Score)
            .Description("Relevance score for this result");

        descriptor.Field(f => f.Document)
            .Description("The matching document");

        descriptor.Field(f => f.Highlights)
            .Description("Highlighted text snippets from the search");
    }
}

public class SearchResultsType : ObjectType<SearchResults>
{
    protected override void Configure(IObjectTypeDescriptor<SearchResults> descriptor)
    {
        descriptor.Description("Search results with metadata");

        descriptor.Field(f => f.TotalCount)
            .Description("Total number of matching documents");

        descriptor.Field(f => f.Results)
            .Description("The search results");

        descriptor.Field(f => f.Facets)
            .Description("Facet information for refining searches");

        descriptor.Field(f => f.SearchDuration)
            .Description("Time taken to execute the search");
    }
}

public class FacetResultType : ObjectType<FacetResult>
{
    protected override void Configure(IObjectTypeDescriptor<FacetResult> descriptor)
    {
        descriptor.Description("Facet value with count");

        descriptor.Field(f => f.Value)
            .Description("The facet value");

        descriptor.Field(f => f.Count)
            .Description("Number of documents with this facet value");
    }
}

public class IndexStatusType : ObjectType<IndexStatus>
{
    protected override void Configure(IObjectTypeDescriptor<IndexStatus> descriptor)
    {
        descriptor.Description("Status of repository indexing operation");

        descriptor.Field(f => f.Status)
            .Description("Current indexing status");

        descriptor.Field(f => f.DocumentsIndexed)
            .Description("Number of documents already indexed");

        descriptor.Field(f => f.TotalDocuments)
            .Description("Total number of documents to index");

        descriptor.Field(f => f.LastIndexed)
            .Description("When the repository was last indexed");

        descriptor.Field(f => f.EstimatedCompletion)
            .Description("Estimated completion time for ongoing operations");

        descriptor.Field(f => f.ErrorMessage)
            .Description("Error message if indexing failed");

        descriptor.Field(f => f.ProgressPercentage)
            .Description("Progress percentage (0-100)");
    }
}

public class IndexingStatusType : EnumType<IndexingStatus>
{
    protected override void Configure(IEnumTypeDescriptor<IndexingStatus> descriptor)
    {
        descriptor.Description("Status of indexing operations");
        
        descriptor.Value(IndexingStatus.NOT_STARTED)
            .Description("Indexing has not been started");
        
        descriptor.Value(IndexingStatus.IN_PROGRESS)
            .Description("Indexing is currently in progress");
        
        descriptor.Value(IndexingStatus.COMPLETED)
            .Description("Indexing completed successfully");
        
        descriptor.Value(IndexingStatus.ERROR)
            .Description("Indexing failed with an error");
        
        descriptor.Value(IndexingStatus.REFRESHING)
            .Description("Incremental indexing in progress");
    }
}

public class SearchTypeEnum : EnumType<SearchType>
{
    protected override void Configure(IEnumTypeDescriptor<SearchType> descriptor)
    {
        descriptor.Description("Type of search to perform");
        
        descriptor.Value(SearchType.Semantic)
            .Description("Vector-based semantic search only");
        
        descriptor.Value(SearchType.Keyword)
            .Description("Traditional keyword/text search only");
        
        descriptor.Value(SearchType.Hybrid)
            .Description("Combination of semantic and keyword search (recommended)");
    }
}

// Input types - using record-based approach for cleaner input handling

// Input type for the search input
public record SearchRepositoriesInput(
    string Query,
    SearchType SearchType = SearchType.Hybrid,
    List<SearchFilterInput>? Filters = null,
    int Top = 50,
    int Skip = 0);

public record SearchFilterInput(
    string Field,
    string Operator,
    string Value);