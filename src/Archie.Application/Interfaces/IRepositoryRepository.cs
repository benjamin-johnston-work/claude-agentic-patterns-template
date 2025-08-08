using Archie.Domain.Entities;

namespace Archie.Application.Interfaces;

public interface IRepositoryRepository
{
    Task<Repository?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Repository?> GetByUrlAsync(string url, CancellationToken cancellationToken = default);
    Task<IEnumerable<Repository>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Repository>> GetByFilterAsync(RepositoryFilter filter, CancellationToken cancellationToken = default);
    Task<Repository> AddAsync(Repository repository, CancellationToken cancellationToken = default);
    Task<Repository> UpdateAsync(Repository repository, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUrlAsync(string url, CancellationToken cancellationToken = default);
}

public record RepositoryFilter
{
    public string? Language { get; init; }
    public string? Status { get; init; }
    public string? SearchTerm { get; init; }
    public int? Skip { get; init; }
    public int? Take { get; init; }
}