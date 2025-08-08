using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class AddRepositoryUseCase
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IGitRepositoryService _gitService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AddRepositoryUseCase> _logger;

    public AddRepositoryUseCase(
        IRepositoryRepository repositoryRepository,
        IGitRepositoryService gitService,
        IEventPublisher eventPublisher,
        ILogger<AddRepositoryUseCase> logger)
    {
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RepositoryDto>> ExecuteAsync(AddRepositoryInput input, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Adding repository: {Url}", input.Url);

            // Check if repository already exists
            var existingRepository = await _repositoryRepository.GetByUrlAsync(input.Url, cancellationToken);
            if (existingRepository != null)
            {
                _logger.LogWarning("Repository already exists: {Url}", input.Url);
                return Result<RepositoryDto>.Failure("Repository already exists");
            }

            // Validate repository access
            var isValidAccess = await _gitService.ValidateRepositoryAccessAsync(input.Url, input.AccessToken, cancellationToken);
            if (!isValidAccess)
            {
                _logger.LogWarning("Invalid repository access: {Url}", input.Url);
                return Result<RepositoryDto>.Failure("Repository is not accessible or does not exist");
            }

            // Clone repository to get metadata
            var clonedRepository = await _gitService.CloneRepositoryAsync(input.Url, input.AccessToken, cancellationToken);
            
            // Create and save repository
            var repository = new Repository(
                clonedRepository.Name,
                input.Url,
                clonedRepository.Language,
                clonedRepository.Description);

            repository.UpdateStatus(Domain.ValueObjects.RepositoryStatus.Connected);

            var savedRepository = await _repositoryRepository.AddAsync(repository, cancellationToken);

            // Publish domain event
            var repositoryAddedEvent = new RepositoryAddedEvent(savedRepository.Id, savedRepository.Url);
            await _eventPublisher.PublishAsync(repositoryAddedEvent, cancellationToken);

            _logger.LogInformation("Successfully added repository: {Id}", savedRepository.Id);

            return Result<RepositoryDto>.Success(MapToDto(savedRepository));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding repository: {Url}", input.Url);
            return Result<RepositoryDto>.Failure($"Failed to add repository: {ex.Message}");
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