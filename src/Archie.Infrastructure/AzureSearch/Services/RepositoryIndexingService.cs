using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.AzureSearch.Models;
using Archie.Infrastructure.Configuration;
using Archie.Infrastructure.GitHub;

namespace Archie.Infrastructure.AzureSearch.Services;

/// <summary>
/// Orchestrates the repository indexing workflow, coordinating file retrieval,
/// processing, and search index updates for repositories.
/// </summary>
public class RepositoryIndexingService : IRepositoryIndexingService
{
    private readonly IAzureSearchService _searchService;
    private readonly FileContentProcessor _fileProcessor;
    private readonly IGitHubService _gitHubService;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IndexingOptions _indexingOptions;
    private readonly ILogger<RepositoryIndexingService> _logger;

    // In-memory tracking of indexing operations (in production, this should be persisted)
    private readonly Dictionary<Guid, IndexStatus> _indexingStatus = new();
    private readonly object _statusLock = new();

    public RepositoryIndexingService(
        IAzureSearchService searchService,
        FileContentProcessor fileProcessor,
        IGitHubService gitHubService,
        IRepositoryRepository repositoryRepository,
        IOptions<IndexingOptions> indexingOptions,
        ILogger<RepositoryIndexingService> logger)
    {
        _searchService = searchService;
        _fileProcessor = fileProcessor;
        _gitHubService = gitHubService;
        _repositoryRepository = repositoryRepository;
        _indexingOptions = indexingOptions.Value;
        _logger = logger;
    }

    public async Task<IndexStatus> IndexRepositoryAsync(Guid repositoryId, bool forceReindex = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting repository indexing for {RepositoryId}, forceReindex: {ForceReindex}", 
                repositoryId, forceReindex);

            // Check if already indexing
            lock (_statusLock)
            {
                if (_indexingStatus.TryGetValue(repositoryId, out var existingStatus) && 
                    existingStatus.Status == IndexingStatus.IN_PROGRESS)
                {
                    _logger.LogWarning("Repository {RepositoryId} is already being indexed", repositoryId);
                    return existingStatus;
                }
            }

            // Get repository from database
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null)
            {
                _logger.LogError("Repository {RepositoryId} not found", repositoryId);
                return CreateErrorStatus(repositoryId, "Repository not found");
            }

            // Initialize indexing status
            var status = new IndexStatus
            {
                Status = IndexingStatus.IN_PROGRESS,
                DocumentsIndexed = 0,
                TotalDocuments = 0,
                EstimatedCompletion = DateTime.UtcNow.AddMinutes(10) // Initial estimate
            };

            UpdateIndexingStatus(repositoryId, status);

            // Execute the indexing workflow
            status = await ExecuteIndexingWorkflow(repository, forceReindex, status, cancellationToken);

            // Update final status
            UpdateIndexingStatus(repositoryId, status);

            _logger.LogInformation("Repository indexing completed for {RepositoryId} with status {Status}. " +
                "Indexed {DocumentCount} documents", repositoryId, status.Status, status.DocumentsIndexed);

            return status;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Repository indexing cancelled for {RepositoryId}", repositoryId);
            var cancelledStatus = CreateErrorStatus(repositoryId, "Indexing cancelled");
            UpdateIndexingStatus(repositoryId, cancelledStatus);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing repository {RepositoryId}", repositoryId);
            var errorStatus = CreateErrorStatus(repositoryId, ex.Message);
            UpdateIndexingStatus(repositoryId, errorStatus);
            return errorStatus;
        }
    }

    public async Task<IndexStatus> RefreshRepositoryIndexAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting incremental repository index refresh for {RepositoryId}", repositoryId);

        if (!_indexingOptions.EnableIncrementalIndexing)
        {
            _logger.LogInformation("Incremental indexing disabled, performing full reindex");
            return await IndexRepositoryAsync(repositoryId, forceReindex: true, cancellationToken);
        }

        // For incremental indexing, we would need to:
        // 1. Get the last indexed timestamp
        // 2. Fetch only files modified since then
        // 3. Update/remove documents as needed
        // For now, we'll do a full reindex
        
        return await IndexRepositoryAsync(repositoryId, forceReindex: true, cancellationToken);
    }

    public async Task<bool> RemoveRepositoryFromIndexAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Removing repository {RepositoryId} from search index", repositoryId);

            // Update status to indicate removal in progress
            var status = new IndexStatus
            {
                Status = IndexingStatus.IN_PROGRESS,
                DocumentsIndexed = 0,
                TotalDocuments = 0
            };
            UpdateIndexingStatus(repositoryId, status);

            // Remove all documents for this repository from the search index
            var success = await _searchService.DeleteRepositoryDocumentsAsync(repositoryId, cancellationToken);

            // Clean up status tracking
            lock (_statusLock)
            {
                _indexingStatus.Remove(repositoryId);
            }

            _logger.LogInformation("Repository {RepositoryId} removal {Result}", 
                repositoryId, success ? "completed successfully" : "failed");

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing repository {RepositoryId} from index", repositoryId);
            return false;
        }
    }

    public async Task<IndexStatus> GetIndexingStatusAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        // First check in-memory status for active operations
        lock (_statusLock)
        {
            if (_indexingStatus.TryGetValue(repositoryId, out var status))
            {
                return status;
            }
        }

        // If not in memory, check the search service for existing indexed documents
        try
        {
            return await _searchService.GetIndexStatusAsync(repositoryId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting index status for repository {RepositoryId}", repositoryId);
            return CreateErrorStatus(repositoryId, "Error retrieving status");
        }
    }

    #region Private Helper Methods

    private async Task<IndexStatus> ExecuteIndexingWorkflow(
        Repository repository, 
        bool forceReindex, 
        IndexStatus status, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Remove existing documents if force reindex
            if (forceReindex)
            {
                _logger.LogInformation("Force reindex requested, removing existing documents for repository {RepositoryId}", 
                    repository.Id);
                
                await _searchService.DeleteRepositoryDocumentsAsync(repository.Id, cancellationToken);
            }

            // Step 2: Get repository file tree
            _logger.LogDebug("Fetching repository tree for {Owner}/{Name}", repository.Owner, repository.Name);
            
            var fileTree = await _gitHubService.GetRepositoryTreeAsync(
                repository.Owner, 
                repository.Name, 
                repository.DefaultBranch, 
                recursive: true);

            // Step 3: Filter indexable files
            var indexableFiles = FilterIndexableFiles(fileTree);
            status.TotalDocuments = indexableFiles.Count;
            
            _logger.LogInformation("Found {IndexableFileCount} indexable files out of {TotalFileCount} total files",
                indexableFiles.Count, fileTree.Count);

            UpdateIndexingStatus(repository.Id, status);

            // Step 4: Process files in batches
            var documents = new List<SearchableDocument>();
            var processedBatches = 0;
            var totalBatches = (int)Math.Ceiling((double)indexableFiles.Count / _indexingOptions.MaxConcurrentIndexingOperations);

            foreach (var fileBatch in indexableFiles.Chunk(_indexingOptions.MaxConcurrentIndexingOperations))
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogDebug("Processing batch {BatchNumber}/{TotalBatches} ({FileCount} files)",
                    ++processedBatches, totalBatches, fileBatch.Length);

                // Get file contents for the batch
                var fileContents = await GetFileContentsAsync(
                    repository.Owner, 
                    repository.Name, 
                    repository.DefaultBranch,
                    fileBatch, 
                    cancellationToken);

                // Process files to create searchable documents
                var batchDocuments = await _fileProcessor.ProcessFilesAsync(
                    repository, 
                    fileContents, 
                    repository.DefaultBranch, 
                    cancellationToken);

                documents.AddRange(batchDocuments);

                // Update progress
                status.DocumentsIndexed = documents.Count;
                status.EstimatedCompletion = EstimateCompletion(processedBatches, totalBatches, DateTime.UtcNow);
                UpdateIndexingStatus(repository.Id, status);

                _logger.LogDebug("Batch {BatchNumber} completed. Processed {BatchDocuments} documents. " +
                    "Total: {TotalDocuments}/{ExpectedTotal}",
                    processedBatches, batchDocuments.Count, documents.Count, status.TotalDocuments);
            }

            // Step 5: Index all documents in Azure AI Search
            if (documents.Any())
            {
                _logger.LogInformation("Indexing {DocumentCount} documents in Azure AI Search", documents.Count);
                
                var indexSuccess = await _searchService.IndexDocumentsAsync(documents, cancellationToken);
                
                if (!indexSuccess)
                {
                    _logger.LogError("Failed to index documents for repository {RepositoryId}", repository.Id);
                    return CreateErrorStatus(repository.Id, "Failed to index documents in Azure AI Search");
                }
            }

            // Step 6: Complete successfully
            status.Status = IndexingStatus.COMPLETED;
            status.LastIndexed = DateTime.UtcNow;
            status.EstimatedCompletion = null;

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during indexing workflow for repository {RepositoryId}", repository.Id);
            return CreateErrorStatus(repository.Id, ex.Message);
        }
    }

    private List<string> FilterIndexableFiles(List<string> allFiles)
    {
        return allFiles.Where(filePath =>
        {
            // Check file extension
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (!_indexingOptions.IndexableFileExtensions.Contains(extension))
                return false;

            // Check if in ignored directory
            var pathParts = filePath.Split('/', '\\');
            if (pathParts.Any(part => _indexingOptions.IgnoredDirectories.Contains(part, StringComparer.OrdinalIgnoreCase)))
                return false;

            return true;
        }).ToList();
    }

    private async Task<Dictionary<string, string>> GetFileContentsAsync(
        string owner,
        string repo,
        string branch,
        string[] filePaths,
        CancellationToken cancellationToken)
    {
        var fileContents = new Dictionary<string, string>();
        var tasks = new List<Task>();

        using var semaphore = new SemaphoreSlim(10); // Limit concurrent GitHub API calls

        foreach (var filePath in filePaths)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var content = await _gitHubService.GetFileContentAsync(owner, repo, filePath, branch);
                    if (!string.IsNullOrEmpty(content))
                    {
                        lock (fileContents)
                        {
                            fileContents[filePath] = content;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get content for file {FilePath}", filePath);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
        return fileContents;
    }

    private static DateTime EstimateCompletion(int processedBatches, int totalBatches, DateTime startTime)
    {
        if (processedBatches == 0)
            return DateTime.UtcNow.AddMinutes(10); // Default estimate

        var elapsed = DateTime.UtcNow - startTime;
        var avgTimePerBatch = elapsed.TotalMilliseconds / processedBatches;
        var remainingBatches = totalBatches - processedBatches;
        var estimatedRemainingTime = TimeSpan.FromMilliseconds(avgTimePerBatch * remainingBatches);

        return DateTime.UtcNow.Add(estimatedRemainingTime);
    }

    private static IndexStatus CreateErrorStatus(Guid repositoryId, string errorMessage)
    {
        return new IndexStatus
        {
            Status = IndexingStatus.ERROR,
            DocumentsIndexed = 0,
            TotalDocuments = 0,
            ErrorMessage = errorMessage
        };
    }

    private void UpdateIndexingStatus(Guid repositoryId, IndexStatus status)
    {
        lock (_statusLock)
        {
            _indexingStatus[repositoryId] = status;
        }
    }

    #endregion
}