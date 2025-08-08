using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class GetRepositoryUseCase
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<GetRepositoryUseCase> _logger;

    public GetRepositoryUseCase(
        IRepositoryRepository repositoryRepository,
        ILogger<GetRepositoryUseCase> logger)
    {
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RepositoryDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting repository: {Id}", id);

            var repository = await _repositoryRepository.GetByIdAsync(id, cancellationToken);
            if (repository == null)
            {
                _logger.LogWarning("Repository not found: {Id}", id);
                return Result<RepositoryDto>.Failure("Repository not found");
            }

            return Result<RepositoryDto>.Success(MapToDto(repository));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repository: {Id}", id);
            return Result<RepositoryDto>.Failure($"Failed to get repository: {ex.Message}");
        }
    }

    private static RepositoryDto MapToDto(Repository repository)
    {
        return new RepositoryDto
        {
            Id = repository.Id,
            Name = repository.Name,
            Url = repository.Url,
            Language = repository.Language,
            Description = repository.Description,
            Status = repository.Status.ToString(),
            Branches = repository.Branches.Select(b => new BranchDto
            {
                Name = b.Name,
                IsDefault = b.IsDefault,
                CreatedAt = b.CreatedAt,
                LastCommit = b.LastCommit != null ? new CommitDto
                {
                    Hash = b.LastCommit.Hash,
                    Message = b.LastCommit.Message,
                    Author = b.LastCommit.Author,
                    Timestamp = b.LastCommit.Timestamp
                } : null
            }),
            Statistics = repository.Statistics != null ? new RepositoryStatisticsDto
            {
                FileCount = repository.Statistics.FileCount,
                LineCount = repository.Statistics.LineCount,
                LanguageBreakdown = repository.Statistics.LanguageBreakdown.Select(kvp => new LanguageStatsDto
                {
                    Language = kvp.Value.Language,
                    FileCount = kvp.Value.FileCount,
                    LineCount = kvp.Value.LineCount,
                    Percentage = kvp.Value.Percentage
                })
            } : null,
            CreatedAt = repository.CreatedAt,
            UpdatedAt = repository.UpdatedAt
        };
    }
}