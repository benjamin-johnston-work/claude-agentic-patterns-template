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
            _logger.LogInformation("Step 1: About to validate repository access");
            try 
            {
                var isValidAccess = await _gitService.ValidateRepositoryAccessAsync(input.Url, input.AccessToken, cancellationToken);
                _logger.LogInformation("Step 1 completed: Repository access validation result: {IsValid}", isValidAccess);
                if (!isValidAccess)
                {
                    _logger.LogWarning("Invalid repository access: {Url}", input.Url);
                    return Result<RepositoryDto>.Failure("Repository is not accessible or does not exist");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Step 1 failed: GitHub repository validation error");
                return Result<RepositoryDto>.Failure($"STEP 1 FAILED - GitHub validation error: {ex.Message}");
            }

            // Connect to repository to get metadata
            _logger.LogInformation("Step 2: About to connect to repository");
            Repository clonedRepository;
            try 
            {
                clonedRepository = await _gitService.ConnectRepositoryAsync(input.Url, input.AccessToken, cancellationToken);
                _logger.LogInformation("Step 2 completed: Repository connection successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Step 2 failed: GitHub repository connection error");
                return Result<RepositoryDto>.Failure($"STEP 2 FAILED - GitHub connection error: {ex.Message}");
            }
            
            // The GitRepositoryService.ConnectRepositoryAsync already creates a properly constructed Repository
            // with all GitHub API information, so we can use it directly
            var repository = clonedRepository;

            repository.UpdateStatus(Domain.ValueObjects.RepositoryStatus.Connected);

            _logger.LogInformation("Step 3: About to save repository to storage");
            Repository savedRepository;
            try 
            {
                savedRepository = await _repositoryRepository.AddAsync(repository, cancellationToken);
                _logger.LogInformation("Step 3 completed: Repository saved to storage successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Step 3 failed: Repository save error");
                return Result<RepositoryDto>.Failure($"STEP 3 FAILED - Repository save error: {ex.Message}");
            }

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