using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Archie.Domain.Entities;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.AzureSearch.Models;
using Archie.Infrastructure.AzureSearch.Services;
using Archie.Infrastructure.Configuration;

namespace Archie.Infrastructure.Tests.AzureSearch;

[TestFixture]
public class FileContentProcessorTests
{
    private Mock<IAzureOpenAIEmbeddingService> _mockEmbeddingService;
    private Mock<ICodeSymbolExtractor> _mockSymbolExtractor;
    private Mock<IOptions<IndexingOptions>> _mockIndexingOptions;
    private Mock<ILogger<FileContentProcessor>> _mockLogger;
    private FileContentProcessor _processor;
    private IndexingOptions _indexingOptions;
    private Repository _testRepository;

    [SetUp]
    public void Setup()
    {
        _mockEmbeddingService = new Mock<IAzureOpenAIEmbeddingService>();
        _mockSymbolExtractor = new Mock<ICodeSymbolExtractor>();
        _mockIndexingOptions = new Mock<IOptions<IndexingOptions>>();
        _mockLogger = new Mock<ILogger<FileContentProcessor>>();

        _indexingOptions = new IndexingOptions
        {
            MaxConcurrentIndexingOperations = 3,
            MaxFileContentLength = 16384,
            IndexableFileExtensions = new List<string> { ".cs", ".js", ".ts", ".py", ".java" },
            IgnoredDirectories = new List<string> { ".git", ".vs", "node_modules", "bin", "obj" },
            ExtractCodeSymbols = true,
            EnableIncrementalIndexing = true
        };

        _mockIndexingOptions.Setup(x => x.Value).Returns(_indexingOptions);

        _testRepository = new Repository(
            "test-repo",
            "https://github.com/test-owner/test-repo",
            "test-owner",
            "csharp",
            "Test Repository");

        _processor = new FileContentProcessor(
            _mockEmbeddingService.Object,
            _mockSymbolExtractor.Object,
            _mockIndexingOptions.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task ProcessFileAsync_WithValidFile_ReturnsSearchableDocument()
    {
        // Arrange
        var filePath = "src/test.cs";
        var content = "using System;\nclass TestClass { }";
        var branchName = "main";
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var codeSymbols = new List<string> { "class:TestClass" };

        _mockEmbeddingService
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);

        _mockSymbolExtractor
            .Setup(x => x.ExtractSymbolsAsync(content, "csharp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(codeSymbols);

        // Act
        var result = await _processor.ProcessFileAsync(_testRepository, filePath, content, branchName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testRepository.Id, result.RepositoryId);
        Assert.AreEqual(filePath, result.FilePath);
        Assert.AreEqual("test.cs", result.FileName);
        Assert.AreEqual(".cs", result.FileExtension);
        Assert.AreEqual("csharp", result.Language);
        Assert.AreEqual(branchName, result.BranchName);
        Assert.IsTrue(result.Content.Contains(content));
        Assert.AreEqual(embedding, result.ContentVector);
        Assert.AreEqual(codeSymbols, result.Metadata.CodeSymbols);
        Assert.AreEqual(_testRepository.Name, result.Metadata.RepositoryName);
        Assert.AreEqual(_testRepository.Owner, result.Metadata.RepositoryOwner);
    }

    [Test]
    public async Task ProcessFileAsync_WithUnsupportedFileExtension_ReturnsNull()
    {
        // Arrange
        var filePath = "test.txt"; // Not in IndexableFileExtensions for this test setup
        var content = "Some text content";
        var branchName = "main";

        // Update options to exclude .txt
        _indexingOptions.IndexableFileExtensions = new List<string> { ".cs", ".js" };

        // Act
        var result = await _processor.ProcessFileAsync(_testRepository, filePath, content, branchName);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task ProcessFileAsync_WithFileInIgnoredDirectory_ReturnsNull()
    {
        // Arrange
        var filePath = "node_modules/package/index.js";
        var content = "console.log('test');";
        var branchName = "main";

        // Act
        var result = await _processor.ProcessFileAsync(_testRepository, filePath, content, branchName);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task ProcessFileAsync_WithEmbeddingGenerationFailure_ReturnsNull()
    {
        // Arrange
        var filePath = "src/test.cs";
        var content = "class TestClass { }";
        var branchName = "main";

        _mockEmbeddingService
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<float>()); // Empty array indicates failure

        // Act
        var result = await _processor.ProcessFileAsync(_testRepository, filePath, content, branchName);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task ProcessFileAsync_WithCodeSymbolExtractionDisabled_ProcessesWithoutSymbols()
    {
        // Arrange
        _indexingOptions.ExtractCodeSymbols = false;
        var filePath = "src/test.cs";
        var content = "class TestClass { }";
        var branchName = "main";
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        _mockEmbeddingService
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);

        // Act
        var result = await _processor.ProcessFileAsync(_testRepository, filePath, content, branchName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Metadata.CodeSymbols.Count);
        
        // Verify symbol extractor was not called
        _mockSymbolExtractor.Verify(
            x => x.ExtractSymbolsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ProcessFileAsync_WithLargeContent_TruncatesContent()
    {
        // Arrange
        var filePath = "src/large.cs";
        var largeContent = new string('x', _indexingOptions.MaxFileContentLength + 1000); // Exceeds limit
        var branchName = "main";
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        _mockEmbeddingService
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);

        _mockSymbolExtractor
            .Setup(x => x.ExtractSymbolsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        // Act
        var result = await _processor.ProcessFileAsync(_testRepository, filePath, largeContent, branchName);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Content.Length <= _indexingOptions.MaxFileContentLength + 100); // Allow some overhead for metadata
    }

    [Test]
    public async Task ProcessFilesAsync_WithValidFiles_ReturnsProcessedDocuments()
    {
        // Arrange
        var fileData = new Dictionary<string, string>
        {
            { "src/test1.cs", "class TestClass1 { }" },
            { "src/test2.cs", "class TestClass2 { }" }
        };
        var branchName = "main";
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        _mockEmbeddingService
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);

        _mockSymbolExtractor
            .Setup(x => x.ExtractSymbolsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "class:TestClass" });

        // Act
        var result = await _processor.ProcessFilesAsync(_testRepository, fileData, branchName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(d => d.RepositoryId == _testRepository.Id));
        Assert.IsTrue(result.All(d => d.BranchName == branchName));
    }

    [Test]
    public async Task ProcessFilesAsync_WithEmptyFileData_ReturnsEmptyList()
    {
        // Arrange
        var fileData = new Dictionary<string, string>();
        var branchName = "main";

        // Act
        var result = await _processor.ProcessFilesAsync(_testRepository, fileData, branchName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [Test]
    public async Task ProcessFilesAsync_WithMixedValidAndInvalidFiles_ReturnsOnlyValidDocuments()
    {
        // Arrange
        var fileData = new Dictionary<string, string>
        {
            { "src/valid.cs", "class ValidClass { }" },
            { "node_modules/invalid.js", "console.log('ignored');" }, // In ignored directory
            { "src/valid2.py", "def test_function(): pass" }
        };
        var branchName = "main";
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        _mockEmbeddingService
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);

        _mockSymbolExtractor
            .Setup(x => x.ExtractSymbolsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "symbol" });

        // Act
        var result = await _processor.ProcessFilesAsync(_testRepository, fileData, branchName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count); // Only valid.cs and valid2.py should be processed
        Assert.IsTrue(result.Any(d => d.FilePath == "src/valid.cs"));
        Assert.IsTrue(result.Any(d => d.FilePath == "src/valid2.py"));
        Assert.IsFalse(result.Any(d => d.FilePath.Contains("node_modules")));
    }

    [Test]
    public async Task ProcessFilesAsync_WithCancellation_ThrowsOperationCancelledException()
    {
        // Arrange
        var fileData = new Dictionary<string, string>
        {
            { "src/test.cs", "class TestClass { }" }
        };
        var branchName = "main";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancel immediately

        // Act & Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _processor.ProcessFilesAsync(_testRepository, fileData, branchName, cancellationTokenSource.Token));
    }

    [Test]
    public async Task ProcessFileAsync_WithDifferentFileTypes_DetectsLanguageCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            ("test.cs", "class Test { }", "csharp"),
            ("test.js", "function test() { }", "javascript"),
            ("test.py", "def test(): pass", "python"),
            ("test.java", "class Test { }", "java"),
            ("test.go", "func test() { }", "go"),
            ("test.rs", "fn test() { }", "rust"),
            ("test.md", "# Test", "markdown")
        };

        var embedding = new float[] { 0.1f, 0.2f, 0.3f };

        _mockEmbeddingService
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);

        _mockSymbolExtractor
            .Setup(x => x.ExtractSymbolsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());

        foreach (var (fileName, content, expectedLanguage) in testCases)
        {
            // Add the file extension to indexable extensions
            if (!_indexingOptions.IndexableFileExtensions.Contains(Path.GetExtension(fileName)))
            {
                _indexingOptions.IndexableFileExtensions.Add(Path.GetExtension(fileName));
            }

            // Act
            var result = await _processor.ProcessFileAsync(_testRepository, fileName, content, "main");

            // Assert
            Assert.IsNotNull(result, $"Failed to process {fileName}");
            Assert.AreEqual(expectedLanguage, result.Language, $"Language detection failed for {fileName}");
        }
    }

    [Test]
    public void Constructor_WithNullEmbeddingService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new FileContentProcessor(null!, _mockSymbolExtractor.Object, _mockIndexingOptions.Object, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullSymbolExtractor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new FileContentProcessor(_mockEmbeddingService.Object, null!, _mockIndexingOptions.Object, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullIndexingOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new FileContentProcessor(_mockEmbeddingService.Object, _mockSymbolExtractor.Object, null!, _mockLogger.Object));
    }

    [Test]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new FileContentProcessor(_mockEmbeddingService.Object, _mockSymbolExtractor.Object, _mockIndexingOptions.Object, null!));
    }
}