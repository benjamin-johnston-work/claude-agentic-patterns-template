using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class RefreshRepositoryUseCase
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IGitRepositoryService _gitService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<RefreshRepositoryUseCase> _logger;

    public RefreshRepositoryUseCase(
        IRepositoryRepository repositoryRepository,
        IGitRepositoryService gitService,
        IEventPublisher eventPublisher,
        ILogger<RefreshRepositoryUseCase> logger)
    {
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RepositoryDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Refreshing repository: {Id}", id);

            var repository = await _repositoryRepository.GetByIdAsync(id, cancellationToken);
            if (repository == null)
            {
                _logger.LogWarning("Repository not found: {Id}", id);
                return Result<RepositoryDto>.Failure("Repository not found");
            }

            // Start analysis
            repository.UpdateStatus(Domain.ValueObjects.RepositoryStatus.Analyzing);
            await _repositoryRepository.UpdateAsync(repository, cancellationToken);

            // Publish analysis started event
            var analysisStartedEvent = new RepositoryAnalysisStartedEvent(repository.Id);
            await _eventPublisher.PublishAsync(analysisStartedEvent, cancellationToken);

            // Get updated repository information (this would typically be done asynchronously)
            var branches = await _gitService.GetBranchesAsync(repository.Url, null, cancellationToken);
            var statistics = await _gitService.AnalyzeRepositoryStructureAsync(repository.Url, "main", null, cancellationToken);

            // Update repository with new information
            repository.UpdateStatistics(statistics);

            // Update branches
            foreach (var branch in branches)
            {
                var existingBranch = repository.GetBranch(branch.Name);
                if (existingBranch == null)
                {
                    repository.AddBranch(branch);
                }
                else if (branch.LastCommit != null)
                {
                    existingBranch.UpdateLastCommit(branch.LastCommit);
                }
            }

            repository.UpdateStatus(Domain.ValueObjects.RepositoryStatus.Ready);
            var updatedRepository = await _repositoryRepository.UpdateAsync(repository, cancellationToken);

            // Publish analysis completed event
            var analysisCompletedEvent = new RepositoryAnalysisCompletedEvent(repository.Id, statistics);
            await _eventPublisher.PublishAsync(analysisCompletedEvent, cancellationToken);

            _logger.LogInformation("Successfully refreshed repository: {Id}", id);

            return Result<RepositoryDto>.Success(MapToDto(updatedRepository));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing repository: {Id}", id);
            
            // Update repository status to error if possible
            try
            {
                var repository = await _repositoryRepository.GetByIdAsync(id, cancellationToken);
                if (repository != null)
                {
                    repository.UpdateStatus(Domain.ValueObjects.RepositoryStatus.Error);
                    await _repositoryRepository.UpdateAsync(repository, cancellationToken);
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update repository status to error: {Id}", id);
            }

            return Result<RepositoryDto>.Failure($"Failed to refresh repository: {ex.Message}");
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