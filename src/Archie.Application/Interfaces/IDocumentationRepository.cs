using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

public interface IDocumentationRepository
{
    /// <summary>
    /// Add a new documentation to the repository
    /// </summary>
    /// <param name="documentation">Documentation entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The saved documentation</returns>
    Task<Documentation> AddAsync(Documentation documentation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing documentation
    /// </summary>
    /// <param name="documentation">Documentation entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated documentation</returns>
    Task<Documentation> UpdateAsync(Documentation documentation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documentation by ID
    /// </summary>
    /// <param name="id">Documentation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Documentation entity or null if not found</returns>
    Task<Documentation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documentation by repository ID
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Documentation entity or null if not found</returns>
    Task<Documentation?> GetByRepositoryIdAsync(Guid repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all documentation with specified status
    /// </summary>
    /// <param name="status">Documentation status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of documentation entities</returns>
    Task<IEnumerable<Documentation>> GetByStatusAsync(DocumentationStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all documentation
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all documentation entities</returns>
    Task<IEnumerable<Documentation>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete documentation by ID
    /// </summary>
    /// <param name="id">Documentation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete documentation by repository ID
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteByRepositoryIdAsync(Guid repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if documentation exists for a repository
    /// </summary>
    /// <param name="repositoryId">Repository ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if documentation exists, false otherwise</returns>
    Task<bool> ExistsForRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search documentation sections by content
    /// </summary>
    /// <param name="searchQuery">Search query</param>
    /// <param name="repositoryIds">Optional list of repository IDs to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matching documentation sections</returns>
    Task<IEnumerable<DocumentationSection>> SearchSectionsAsync(
        string searchQuery, 
        IEnumerable<Guid>? repositoryIds = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documentation that requires regeneration
    /// </summary>
    /// <param name="repositoryLastModified">Dictionary of repository ID to last modified date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of documentation requiring regeneration</returns>
    Task<IEnumerable<Documentation>> GetDocumentationRequiringRegenerationAsync(
        Dictionary<Guid, DateTime> repositoryLastModified, 
        CancellationToken cancellationToken = default);
}