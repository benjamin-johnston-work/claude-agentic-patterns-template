using Archie.Application.Common;
using Archie.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Archie.Application.UseCases;

public class DeleteDocumentationUseCase
{
    private readonly IDocumentationRepository _documentationRepository;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DeleteDocumentationUseCase> _logger;

    public DeleteDocumentationUseCase(
        IDocumentationRepository documentationRepository,
        IRepositoryRepository repositoryRepository,
        IEventPublisher eventPublisher,
        ILogger<DeleteDocumentationUseCase> logger)
    {
        _documentationRepository = documentationRepository ?? throw new ArgumentNullException(nameof(documentationRepository));
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> ExecuteAsync(
        Guid repositoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting documentation for repository: {RepositoryId}", repositoryId);

            // Validate repository exists
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null)
            {
                _logger.LogWarning("Repository not found: {RepositoryId}", repositoryId);
                return Result<bool>.Failure("Repository not found");
            }

            // Delete documentation
            var deleted = await _documentationRepository.DeleteByRepositoryIdAsync(repositoryId, cancellationToken);
            
            if (deleted)
            {
                _logger.LogInformation("Successfully deleted documentation for repository: {RepositoryId}", repositoryId);
            }
            else
            {
                _logger.LogInformation("No documentation found to delete for repository: {RepositoryId}", repositoryId);
            }

            return Result<bool>.Success(deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting documentation for repository: {RepositoryId}", repositoryId);
            return Result<bool>.Failure($"Failed to delete documentation: {ex.Message}");
        }
    }
}