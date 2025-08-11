using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Archie.Domain.Entities;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.AzureSearch.Models;
using Archie.Infrastructure.Configuration;

namespace Archie.Infrastructure.AzureSearch.Services;

/// <summary>
/// Processes repository files to create searchable documents with embeddings and extracted code symbols.
/// Handles content preprocessing, symbol extraction, and embedding generation.
/// </summary>
public class FileContentProcessor
{
    private readonly IAzureOpenAIEmbeddingService _embeddingService;
    private readonly ICodeSymbolExtractor _symbolExtractor;
    private readonly IndexingOptions _indexingOptions;
    private readonly ILogger<FileContentProcessor> _logger;

    public FileContentProcessor(
        IAzureOpenAIEmbeddingService embeddingService,
        ICodeSymbolExtractor symbolExtractor,
        IOptions<IndexingOptions> indexingOptions,
        ILogger<FileContentProcessor> logger)
    {
        _embeddingService = embeddingService;
        _symbolExtractor = symbolExtractor;
        _indexingOptions = indexingOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Process a single file to create a searchable document with embeddings and metadata.
    /// </summary>
    /// <param name="repository">The repository containing the file</param>
    /// <param name="filePath">The relative path to the file within the repository</param>
    /// <param name="content">The raw file content</param>
    /// <param name="branchName">The branch name where the file is located</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A searchable document ready for indexing</returns>
    public async Task<SearchableDocument?> ProcessFileAsync(
        Repository repository,
        string filePath,
        string content,
        string branchName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Processing file {FilePath} from repository {RepositoryName}", 
                filePath, repository.Name);

            // 1. Validate file size and content
            if (!IsFileIndexable(filePath, content))
            {
                _logger.LogDebug("Skipping non-indexable file: {FilePath}", filePath);
                return null;
            }

            // 2. Preprocess content for embedding
            var processedContent = PreprocessContent(content, filePath);

            // 3. Generate embedding vector
            var embedding = await GenerateEmbeddingWithRetry(processedContent, cancellationToken);
            if (embedding.Length == 0)
            {
                _logger.LogWarning("Failed to generate embedding for file: {FilePath}", filePath);
                return null;
            }

            // 4. Extract code symbols if enabled
            var codeSymbols = new List<string>();
            if (_indexingOptions.ExtractCodeSymbols)
            {
                var language = GetLanguageFromPath(filePath);
                codeSymbols = await _symbolExtractor.ExtractSymbolsAsync(content, language, cancellationToken);
            }

            // 5. Create document metadata
            var metadata = CreateDocumentMetadata(repository, codeSymbols);

            // 6. Create and return the searchable document
            var document = SearchableDocument.Create(
                repository.Id,
                filePath,
                processedContent,
                embedding,
                branchName,
                metadata);

            _logger.LogDebug("Successfully processed file {FilePath}, extracted {SymbolCount} symbols, content length: {ContentLength}", 
                filePath, codeSymbols.Count, processedContent.Length);

            return document;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("File processing cancelled for: {FilePath}", filePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file {FilePath} from repository {RepositoryName}", 
                filePath, repository.Name);
            return null;
        }
    }

    /// <summary>
    /// Process multiple files in batch for better performance.
    /// </summary>
    /// <param name="repository">The repository containing the files</param>
    /// <param name="fileData">Dictionary of file paths to content</param>
    /// <param name="branchName">The branch name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of successfully processed searchable documents</returns>
    public async Task<List<SearchableDocument>> ProcessFilesAsync(
        Repository repository,
        Dictionary<string, string> fileData,
        string branchName,
        CancellationToken cancellationToken = default)
    {
        if (!fileData.Any())
        {
            _logger.LogWarning("No files provided for batch processing");
            return new List<SearchableDocument>();
        }

        try
        {
            _logger.LogInformation("Starting batch processing of {FileCount} files from repository {RepositoryName}", 
                fileData.Count, repository.Name);

            var results = new List<SearchableDocument>();
            var processingTasks = new List<Task<SearchableDocument?>>();

            // Process files concurrently, respecting the max concurrent operations limit
            using var semaphore = new SemaphoreSlim(_indexingOptions.MaxConcurrentIndexingOperations);
            
            foreach (var (filePath, content) in fileData)
            {
                if (!IsFileIndexable(filePath, content))
                    continue;

                processingTasks.Add(ProcessFileWithSemaphore(
                    repository, filePath, content, branchName, semaphore, cancellationToken));
            }

            var processedFiles = await Task.WhenAll(processingTasks);
            
            results.AddRange(processedFiles.Where(doc => doc != null)!);

            _logger.LogInformation("Batch processing completed. Successfully processed {SuccessCount} out of {TotalCount} files", 
                results.Count, fileData.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch file processing for repository {RepositoryName}", repository.Name);
            return new List<SearchableDocument>();
        }
    }

    #region Private Helper Methods

    private async Task<SearchableDocument?> ProcessFileWithSemaphore(
        Repository repository,
        string filePath,
        string content,
        string branchName,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            return await ProcessFileAsync(repository, filePath, content, branchName, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private bool IsFileIndexable(string filePath, string content)
    {
        // Check file extension
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (!_indexingOptions.IndexableFileExtensions.Contains(extension))
        {
            return false;
        }

        // Check if in ignored directory
        var pathParts = filePath.Split('/', '\\');
        if (pathParts.Any(part => _indexingOptions.IgnoredDirectories.Contains(part, StringComparer.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Check file size
        if (content.Length > _indexingOptions.MaxFileContentLength)
        {
            _logger.LogDebug("File {FilePath} too large ({ContentLength} chars), truncating to {MaxLength}", 
                filePath, content.Length, _indexingOptions.MaxFileContentLength);
        }

        // Check for binary content (simple heuristic)
        if (ContainsBinaryContent(content))
        {
            return false;
        }

        return true;
    }

    private static bool ContainsBinaryContent(string content)
    {
        // Simple heuristic: if more than 30% of characters are null or control characters, consider it binary
        if (string.IsNullOrEmpty(content))
            return false;

        var controlCharCount = content.Count(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t');
        var threshold = content.Length * 0.3;

        return controlCharCount > threshold;
    }

    private string PreprocessContent(string content, string filePath)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        // Truncate if too long
        if (content.Length > _indexingOptions.MaxFileContentLength)
        {
            content = content.Substring(0, _indexingOptions.MaxFileContentLength);
            
            // Try to cut at a line boundary
            var lastNewlineIndex = content.LastIndexOf('\n');
            if (lastNewlineIndex > content.Length * 0.9) // Only if we don't lose more than 10%
            {
                content = content.Substring(0, lastNewlineIndex);
            }
        }

        // Normalize line endings
        content = content.Replace("\r\n", "\n").Replace("\r", "\n");

        // Add file context for better embedding quality
        var fileName = Path.GetFileName(filePath);
        var language = GetLanguageFromPath(filePath);
        
        return $"File: {fileName} (Language: {language})\nPath: {filePath}\n\n{content}";
    }

    private async Task<float[]> GenerateEmbeddingWithRetry(string content, CancellationToken cancellationToken)
    {
        try
        {
            return await _embeddingService.GenerateEmbeddingAsync(content, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding for content of length {ContentLength}", content.Length);
            return Array.Empty<float>();
        }
    }

    private static string GetLanguageFromPath(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".cs" => "csharp",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".py" => "python",
            ".java" => "java",
            ".cpp" or ".cc" or ".cxx" => "cpp",
            ".c" => "c",
            ".h" or ".hpp" => "cpp", // Header files
            ".go" => "go",
            ".rs" => "rust",
            ".php" => "php",
            ".rb" => "ruby",
            ".swift" => "swift",
            ".kt" => "kotlin",
            ".scala" => "scala",
            ".html" or ".htm" => "html",
            ".css" => "css",
            ".scss" or ".sass" => "css",
            ".sql" => "sql",
            ".md" => "markdown",
            ".json" => "json",
            ".xml" => "xml",
            ".yml" or ".yaml" => "yaml",
            ".sh" => "bash",
            ".ps1" => "powershell",
            ".dockerfile" => "dockerfile",
            ".r" => "r",
            ".m" => "matlab",
            ".pl" => "perl",
            ".lua" => "lua",
            ".dart" => "dart",
            ".elm" => "elm",
            ".ex" or ".exs" => "elixir",
            ".clj" or ".cljs" => "clojure",
            ".fs" or ".fsx" => "fsharp",
            ".vb" => "visualbasic",
            _ => "text"
        };
    }

    private static DocumentMetadata CreateDocumentMetadata(Repository repository, List<string> codeSymbols)
    {
        return new DocumentMetadata
        {
            RepositoryName = repository.Name,
            RepositoryOwner = repository.Owner,
            RepositoryUrl = repository.CloneUrl,
            CodeSymbols = codeSymbols,
            CustomFields = new Dictionary<string, string>
            {
                ["repository_description"] = repository.Description ?? string.Empty,
                ["is_private"] = repository.IsPrivate.ToString(),
                ["default_branch"] = repository.DefaultBranch,
                ["created_at"] = repository.CreatedAt.ToString("O"),
                ["updated_at"] = repository.UpdatedAt.ToString("O")
            }
        };
    }

    #endregion
}