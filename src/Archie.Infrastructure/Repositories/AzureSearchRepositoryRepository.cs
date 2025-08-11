using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Archie.Infrastructure.Repositories;

/// <summary>
/// Azure AI Search-based implementation of repository storage
/// Uses in-memory cache for repository metadata and Azure AI Search for searchable content
/// </summary>
public class AzureSearchRepositoryRepository : IRepositoryRepository
{
    private readonly IAzureSearchService _searchService;
    private readonly ILogger<AzureSearchRepositoryRepository> _logger;
    
    // In-memory cache for repository metadata (in production, this could be Redis or Azure Table Storage)
    private static readonly ConcurrentDictionary<Guid, Repository> _repositories = new();
    private static readonly ConcurrentDictionary<string, Repository> _repositoriesByUrl = new();

    public AzureSearchRepositoryRepository(
        IAzureSearchService searchService,
        ILogger<AzureSearchRepositoryRepository> logger)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Repository?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting repository by ID: {RepositoryId}", id);
        
        return _repositories.TryGetValue(id, out var repository) ? repository : null;
    }

    public async Task<Repository?> GetByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting repository by URL: {Url}", url);
        
        return _repositoriesByUrl.TryGetValue(url, out var repository) ? repository : null;
    }

    public async Task<IEnumerable<Repository>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all repositories");
        
        return _repositories.Values.ToList();
    }

    public async Task<IEnumerable<Repository>> GetByFilterAsync(RepositoryFilter filter, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting repositories by filter: Language={Language}, Status={Status}, SearchTerm={SearchTerm}", 
            filter.Language, filter.Status, filter.SearchTerm);

        var repositories = _repositories.Values.AsEnumerable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.Language))
        {
            repositories = repositories.Where(r => r.Language.Equals(filter.Language, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            repositories = repositories.Where(r => r.Status.ToString().Equals(filter.Status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            repositories = repositories.Where(r => 
                r.Name.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                r.Url.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(r.Description) && r.Description.Contains(filter.SearchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        // Note: HasDocumentation filtering is handled in GetRepositoriesUseCase since it requires async documentation service calls

        // Apply pagination
        if (filter.Skip.HasValue)
        {
            repositories = repositories.Skip(filter.Skip.Value);
        }

        if (filter.Take.HasValue)
        {
            repositories = repositories.Take(filter.Take.Value);
        }

        return repositories.ToList();
    }

    public async Task<Repository> AddAsync(Repository repository, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        _logger.LogInformation("Adding repository: {RepositoryId} - {Name}", repository.Id, repository.Name);

        _repositories.TryAdd(repository.Id, repository);
        _repositoriesByUrl.TryAdd(repository.Url, repository);

        _logger.LogInformation("Successfully added repository: {RepositoryId}", repository.Id);
        
        return repository;
    }

    public async Task<Repository> UpdateAsync(Repository repository, CancellationToken cancellationToken = default)
    {
        if (repository == null)
            throw new ArgumentNullException(nameof(repository));

        _logger.LogInformation("Updating repository: {RepositoryId} - {Name}", repository.Id, repository.Name);

        _repositories.AddOrUpdate(repository.Id, repository, (key, oldValue) => repository);
        _repositoriesByUrl.AddOrUpdate(repository.Url, repository, (key, oldValue) => repository);

        _logger.LogInformation("Successfully updated repository: {RepositoryId}", repository.Id);
        
        return repository;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting repository: {RepositoryId}", id);

        if (_repositories.TryRemove(id, out var repository))
        {
            _repositoriesByUrl.TryRemove(repository.Url, out _);
            
            // Also remove from search index
            await _searchService.DeleteRepositoryDocumentsAsync(id, cancellationToken);
            
            _logger.LogInformation("Successfully deleted repository: {RepositoryId}", id);
            return true;
        }

        _logger.LogWarning("Repository not found for deletion: {RepositoryId}", id);
        return false;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repositories.ContainsKey(id);
    }

    public async Task<bool> ExistsByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        return _repositoriesByUrl.ContainsKey(url);
    }
}