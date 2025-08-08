using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

public interface IGitRepositoryService
{
    Task<Repository> CloneRepositoryAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Branch>> GetBranchesAsync(string repositoryPath, CancellationToken cancellationToken = default);
    Task<IEnumerable<Commit>> GetCommitHistoryAsync(string repositoryPath, string branch, int limit = 100, CancellationToken cancellationToken = default);
    Task<RepositoryStatistics> AnalyzeRepositoryStructureAsync(string repositoryPath, CancellationToken cancellationToken = default);
    Task<bool> ValidateRepositoryAccessAsync(string url, string? accessToken = null, CancellationToken cancellationToken = default);
}