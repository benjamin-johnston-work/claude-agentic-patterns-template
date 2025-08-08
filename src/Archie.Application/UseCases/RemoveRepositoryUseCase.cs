using Archie.Application.Common;
using Archie.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class RemoveRepositoryUseCase
{
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<RemoveRepositoryUseCase> _logger;

    public RemoveRepositoryUseCase(
        IRepositoryRepository repositoryRepository,
        ILogger<RemoveRepositoryUseCase> logger)
    {
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Removing repository: {Id}", id);

            var exists = await _repositoryRepository.ExistsAsync(id, cancellationToken);
            if (!exists)
            {
                _logger.LogWarning("Repository not found: {Id}", id);
                return Result<bool>.Failure("Repository not found");
            }

            var result = await _repositoryRepository.DeleteAsync(id, cancellationToken);

            if (result)
            {
                _logger.LogInformation("Successfully removed repository: {Id}", id);
            }
            else
            {
                _logger.LogWarning("Failed to remove repository: {Id}", id);
            }

            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing repository: {Id}", id);
            return Result<bool>.Failure($"Failed to remove repository: {ex.Message}");
        }
    }
}