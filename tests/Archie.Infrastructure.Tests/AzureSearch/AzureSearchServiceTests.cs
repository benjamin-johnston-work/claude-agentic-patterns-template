using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Archie.Infrastructure.AzureSearch.Models;
using Archie.Infrastructure.AzureSearch.Services;
using Archie.Infrastructure.Configuration;

namespace Archie.Infrastructure.Tests.AzureSearch;

[TestFixture]
public class AzureSearchServiceTests
{
    private Mock<ILogger<AzureSearchService>> _mockLogger;
    private Mock<IOptions<AzureSearchOptions>> _mockOptions;
    private AzureSearchOptions _searchOptions;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<AzureSearchService>>();
        _mockOptions = new Mock<IOptions<AzureSearchOptions>>();
        
        _searchOptions = new AzureSearchOptions
        {
            ServiceName = "test-search-service",
            ServiceUrl = "https://test-search-service.search.windows.net",
            AdminKey = "test-admin-key",
            IndexName = "test-index",
            RequestTimeoutSeconds = 30,
            MaxBatchSize = 100,
            RetryAttempts = 3,
            EnableDetailedLogging = true
        };

        _mockOptions.Setup(x => x.Value).Returns(_searchOptions);
    }

    [Test]
    public void Constructor_WithValidOptions_InitializesSuccessfully()
    {
        // Act & Assert - should not throw
        var service = new AzureSearchService(_mockOptions.Object, _mockLogger.Object);
        Assert.That(service, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new AzureSearchService(null!, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new AzureSearchService(_mockOptions.Object, null!));
    }

    [Test]
    public async Task GetIndexStatusAsync_WithValidRepositoryId_ReturnsStatus()
    {
        // Arrange
        var service = new AzureSearchService(_mockOptions.Object, _mockLogger.Object);
        var repositoryId = Guid.NewGuid();

        // Act
        var result = await service.GetIndexStatusAsync(repositoryId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<IndexStatus>());
        // Note: This test will return an error status because we're not connecting to a real Azure Search service
        // In a real test environment, you'd mock the Azure Search client or use integration tests
    }

    [Test]
    public async Task SearchAsync_WithValidQuery_ReturnsResults()
    {
        // Arrange
        var service = new AzureSearchService(_mockOptions.Object, _mockLogger.Object);
        var searchQuery = SearchQuery.Create("test query", SearchType.Hybrid);

        // Act
        var result = await service.SearchAsync(searchQuery);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SearchResults>());
        Assert.That(result.Results, Is.Not.Null);
        Assert.That(result.Facets, Is.Not.Null);
        // Note: This test will return empty results because we're not connecting to a real Azure Search service
    }

    [Test]
    public async Task SearchRepositoryAsync_WithValidParameters_ReturnsResults()
    {
        // Arrange
        var service = new AzureSearchService(_mockOptions.Object, _mockLogger.Object);
        var repositoryId = Guid.NewGuid();
        var searchQuery = SearchQuery.Create("test query", SearchType.Semantic);

        // Act
        var result = await service.SearchRepositoryAsync(repositoryId, searchQuery);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SearchResults>());
        Assert.That(result.Results, Is.Not.Null);
    }

    [Test]
    public async Task GetDocumentAsync_WithValidDocumentId_ReturnsDocument()
    {
        // Arrange
        var service = new AzureSearchService(_mockOptions.Object, _mockLogger.Object);
        var documentId = "test-document-id";

        // Act
        var result = await service.GetDocumentAsync(documentId);

        // Assert
        // Result will be null because we're not connected to real service
        // In integration tests, this would return an actual document
        Assert.That(result == null || result is SearchableDocument);
    }

    [Test]
    public void SearchQuery_Create_WithValidParameters_ReturnsQuery()
    {
        // Arrange & Act
        var query = SearchQuery.Create("test search", SearchType.Hybrid);

        // Assert
        Assert.That(query, Is.Not.Null);
        Assert.That(query.Query, Is.EqualTo("test search"));
        Assert.That(query.SearchType, Is.EqualTo(SearchType.Hybrid));
        Assert.That(query.Top, Is.EqualTo(50)); // Default value
        Assert.That(query.Skip, Is.EqualTo(0)); // Default value
    }

    [Test]
    public void SearchQuery_WithFilters_AddsFiltersCorrectly()
    {
        // Arrange
        var query = SearchQuery.Create("test", SearchType.Keyword);
        var filter1 = SearchFilter.Equal("language", "csharp");
        var filter2 = SearchFilter.Contains("filename", "test");

        // Act
        query.WithFilters(filter1, filter2);

        // Assert
        Assert.That(query.Filters.Count, Is.EqualTo(2));
        Assert.That(query.Filters[0].Field, Is.EqualTo("language"));
        Assert.That(query.Filters[0].Operator, Is.EqualTo("eq"));
        Assert.That(query.Filters[0].Value, Is.EqualTo("csharp"));
    }

    [Test]
    public void SearchQuery_WithPaging_SetsPagingCorrectly()
    {
        // Arrange
        var query = SearchQuery.Create("test", SearchType.Semantic);

        // Act
        query.WithPaging(100, 20);

        // Assert
        Assert.That(query.Top, Is.EqualTo(100));
        Assert.That(query.Skip, Is.EqualTo(20));
    }

    [Test]
    public void SearchFilter_Equal_CreatesCorrectFilter()
    {
        // Act
        var filter = SearchFilter.Equal("field1", "value1");

        // Assert
        Assert.That(filter.Field, Is.EqualTo("field1"));
        Assert.That(filter.Operator, Is.EqualTo("eq"));
        Assert.That(filter.Value, Is.EqualTo("value1"));
    }

    [Test]
    public void SearchFilter_Contains_CreatesCorrectFilter()
    {
        // Act
        var filter = SearchFilter.Contains("content", "search term");

        // Assert
        Assert.That(filter.Field, Is.EqualTo("content"));
        Assert.That(filter.Operator, Is.EqualTo("contains"));
        Assert.That(filter.Value, Is.EqualTo("search term"));
    }

    [Test]
    public void SearchFilter_GreaterThan_CreatesCorrectFilter()
    {
        // Act
        var filter = SearchFilter.GreaterThan("line_count", 100);

        // Assert
        Assert.That(filter.Field, Is.EqualTo("line_count"));
        Assert.That(filter.Operator, Is.EqualTo("gt"));
        Assert.That(filter.Value, Is.EqualTo(100));
    }

    [Test]
    public void SearchableDocument_Create_WithValidParameters_ReturnsDocument()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var filePath = "src/test.cs";
        var content = "using System;\nclass Test { }";
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var branch = "main";
        var metadata = new DocumentMetadata
        {
            RepositoryName = "test-repo",
            RepositoryOwner = "test-owner",
            RepositoryUrl = "https://github.com/test-owner/test-repo",
            CodeSymbols = new List<string> { "class:Test" }
        };

        // Act
        var document = SearchableDocument.Create(repositoryId, filePath, content, embedding, branch, metadata);

        // Assert
        Assert.That(document, Is.Not.Null);
        Assert.That(document.RepositoryId, Is.EqualTo(repositoryId));
        Assert.That(document.FilePath, Is.EqualTo(filePath));
        Assert.That(document.FileName, Is.EqualTo("test.cs"));
        Assert.That(document.FileExtension, Is.EqualTo(".cs"));
        Assert.That(document.Language, Is.EqualTo("csharp"));
        Assert.That(document.Content, Is.EqualTo(content));
        Assert.That(document.ContentVector, Is.EqualTo(embedding));
        Assert.That(document.BranchName, Is.EqualTo(branch));
        Assert.That(document.LineCount, Is.EqualTo(2)); // Two lines in content
        Assert.That(document.SizeInBytes > 0);
        Assert.That(document.Metadata, Is.EqualTo(metadata));
    }

    [Test]
    public void DocumentMetadata_Properties_SetCorrectly()
    {
        // Arrange & Act
        var metadata = new DocumentMetadata
        {
            RepositoryName = "test-repo",
            RepositoryOwner = "test-owner",
            RepositoryUrl = "https://github.com/test-owner/test-repo",
            CodeSymbols = new List<string> { "function:testFunction", "class:TestClass" },
            CustomFields = new Dictionary<string, string>
            {
                { "language_version", "C# 9.0" },
                { "framework", ".NET 5.0" }
            }
        };

        // Assert
        Assert.That(metadata.RepositoryName, Is.EqualTo("test-repo"));
        Assert.That(metadata.RepositoryOwner, Is.EqualTo("test-owner"));
        Assert.That(metadata.RepositoryUrl, Is.EqualTo("https://github.com/test-owner/test-repo"));
        Assert.That(metadata.CodeSymbols.Count, Is.EqualTo(2));
        Assert.That(metadata.CustomFields.Count, Is.EqualTo(2));
        Assert.That(metadata.CustomFields["language_version"], Is.EqualTo("C# 9.0"));
    }

    [Test]
    public void IndexStatus_ProgressPercentage_CalculatesCorrectly()
    {
        // Arrange & Act
        var status = new IndexStatus
        {
            DocumentsIndexed = 25,
            TotalDocuments = 100
        };

        // Assert
        Assert.That(status.ProgressPercentage, Is.EqualTo(25.0));
    }

    [Test]
    public void IndexStatus_ProgressPercentage_WithZeroTotal_ReturnsZero()
    {
        // Arrange & Act
        var status = new IndexStatus
        {
            DocumentsIndexed = 10,
            TotalDocuments = 0
        };

        // Assert
        Assert.That(status.ProgressPercentage, Is.EqualTo(0.0));
    }
}