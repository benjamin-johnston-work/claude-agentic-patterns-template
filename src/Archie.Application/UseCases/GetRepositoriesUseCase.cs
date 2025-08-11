using Archie.Application.Common;
using Archie.Application.DTOs;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class GetRepositoriesUseCase
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IDocumentationRepository _documentationRepository;
    private readonly ILogger<GetRepositoriesUseCase> _logger;

    public GetRepositoriesUseCase(
        IRepositoryRepository repositoryRepository,
        IDocumentationRepository documentationRepository,
        ILogger<GetRepositoriesUseCase> logger)
    {
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _documentationRepository = documentationRepository ?? throw new ArgumentNullException(nameof(documentationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<RepositoryDto>>> ExecuteAsync(RepositoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting repositories with filter: {@Filter}", filter);

            var repositories = filter != null 
                ? await _repositoryRepository.GetByFilterAsync(filter, cancellationToken)
                : await _repositoryRepository.GetAllAsync(cancellationToken);

            // Apply HasDocumentation filter if specified
            if (filter?.HasDocumentation.HasValue == true)
            {
                var filteredRepositories = new List<Repository>();
                
                foreach (var repository in repositories)
                {
                    var hasDocumentation = await _documentationRepository.ExistsForRepositoryAsync(repository.Id, cancellationToken);
                    
                    if (hasDocumentation == filter.HasDocumentation.Value)
                    {
                        filteredRepositories.Add(repository);
                    }
                }
                
                repositories = filteredRepositories;
            }

            var repositoryDtos = repositories.Select(MapToDto);

            return Result<IEnumerable<RepositoryDto>>.Success(repositoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting repositories");
            return Result<IEnumerable<RepositoryDto>>.Failure($"Failed to get repositories: {ex.Message}");
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