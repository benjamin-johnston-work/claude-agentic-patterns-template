using Archie.Infrastructure.AzureSearch.Models;

namespace Archie.Infrastructure.AzureSearch.Interfaces;

public interface IRepositoryIndexingService
{
    Task<IndexStatus> IndexRepositoryAsync(Guid repositoryId, bool forceReindex = false, CancellationToken cancellationToken = default);
    Task<IndexStatus> RefreshRepositoryIndexAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    Task<bool> RemoveRepositoryFromIndexAsync(Guid repositoryId, CancellationToken cancellationToken = default);
    Task<IndexStatus> GetIndexingStatusAsync(Guid repositoryId, CancellationToken cancellationToken = default);
}