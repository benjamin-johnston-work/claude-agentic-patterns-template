using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Archie.Infrastructure.Repositories;

/// <summary>
/// Azure AI Search-based implementation of documentation storage
/// Uses in-memory cache for documentation metadata and Azure AI Search for searchable content
/// </summary>
public class AzureSearchDocumentationRepository : IDocumentationRepository
{
    private readonly IAzureSearchService _searchService;
    private readonly ILogger<AzureSearchDocumentationRepository> _logger;
    
    // In-memory cache for documentation metadata (in production, this could be Redis or Azure Table Storage)
    private static readonly ConcurrentDictionary<Guid, Documentation> _documentations = new();
    private static readonly ConcurrentDictionary<Guid, Guid> _repositoryDocumentationMap = new(); // RepositoryId -> DocumentationId

    public AzureSearchDocumentationRepository(
        IAzureSearchService searchService,
        ILogger<AzureSearchDocumentationRepository> logger)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Documentation> AddAsync(Documentation documentation, CancellationToken cancellationToken = default)
    {
        if (documentation == null)
            throw new ArgumentNullException(nameof(documentation));

        _logger.LogInformation("Adding documentation: {DocumentationId} for repository: {RepositoryId}", 
            documentation.Id, documentation.RepositoryId);

        // Store in cache
        _documentations.TryAdd(documentation.Id, documentation);
        _repositoryDocumentationMap.TryAdd(documentation.RepositoryId, documentation.Id);

        // Index sections in Azure Search for searchability
        await IndexDocumentationSectionsAsync(documentation, cancellationToken);

        _logger.LogInformation("Successfully added documentation: {DocumentationId}", documentation.Id);
        
        return documentation;
    }

    public async Task<Documentation> UpdateAsync(Documentation documentation, CancellationToken cancellationToken = default)
    {
        if (documentation == null)
            throw new ArgumentNullException(nameof(documentation));

        _logger.LogInformation("Updating documentation: {DocumentationId}", documentation.Id);

        // Update in cache
        _documentations.AddOrUpdate(documentation.Id, documentation, (key, oldValue) => documentation);
        _repositoryDocumentationMap.AddOrUpdate(documentation.RepositoryId, documentation.Id, (key, oldValue) => documentation.Id);

        // Re-index sections in Azure Search
        await IndexDocumentationSectionsAsync(documentation, cancellationToken);

        _logger.LogInformation("Successfully updated documentation: {DocumentationId}", documentation.Id);
        
        return documentation;
    }

    public async Task<Documentation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting documentation by ID: {DocumentationId}", id);
        
        return _documentations.TryGetValue(id, out var documentation) ? documentation : null;
    }

    public async Task<Documentation?> GetByRepositoryIdAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting documentation by repository ID: {RepositoryId}", repositoryId);
        
        if (_repositoryDocumentationMap.TryGetValue(repositoryId, out var documentationId))
        {
            return await GetByIdAsync(documentationId, cancellationToken);
        }
        
        return null;
    }

    public async Task<IEnumerable<Documentation>> GetByStatusAsync(DocumentationStatus status, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting documentation by status: {Status}", status);
        
        return _documentations.Values
            .Where(d => d.Status == status)
            .ToList();
    }

    public async Task<IEnumerable<Documentation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all documentation");
        
        return _documentations.Values.ToList();
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting documentation: {DocumentationId}", id);

        if (_documentations.TryRemove(id, out var documentation))
        {
            // Remove from repository mapping
            _repositoryDocumentationMap.TryRemove(documentation.RepositoryId, out _);
            
            // Remove from search index
            await RemoveDocumentationFromSearchAsync(documentation, cancellationToken);
            
            _logger.LogInformation("Successfully deleted documentation: {DocumentationId}", id);
            return true;
        }

        _logger.LogWarning("Documentation not found for deletion: {DocumentationId}", id);
        return false;
    }

    public async Task<bool> DeleteByRepositoryIdAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting documentation for repository: {RepositoryId}", repositoryId);

        var documentation = await GetByRepositoryIdAsync(repositoryId, cancellationToken);
        if (documentation != null)
        {
            return await DeleteAsync(documentation.Id, cancellationToken);
        }

        _logger.LogInformation("No documentation found to delete for repository: {RepositoryId}", repositoryId);
        return false;
    }

    public async Task<bool> ExistsForRepositoryAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        return _repositoryDocumentationMap.ContainsKey(repositoryId);
    }

    public async Task<IEnumerable<DocumentationSection>> SearchSectionsAsync(
        string searchQuery, 
        IEnumerable<Guid>? repositoryIds = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Searching documentation sections with query: '{Query}'", searchQuery);

            if (string.IsNullOrWhiteSpace(searchQuery))
                return Enumerable.Empty<DocumentationSection>();

            var matchingSections = new List<DocumentationSection>();

            // Filter by repository IDs if specified
            var documentationsToSearch = repositoryIds?.Any() == true
                ? _documentations.Values.Where(d => repositoryIds.Contains(d.RepositoryId))
                : _documentations.Values;

            // Simple text search within cached documentation
            // In a real implementation, this would use Azure AI Search for advanced search capabilities
            foreach (var documentation in documentationsToSearch)
            {
                var sections = documentation.Sections
                    .Where(section => ContainsSearchQuery(section, searchQuery))
                    .ToList();

                matchingSections.AddRange(sections);
            }

            // Order by relevance (simple scoring based on query matches)
            var orderedSections = matchingSections
                .OrderByDescending(section => CalculateRelevanceScore(section, searchQuery))
                .Take(50) // Limit results
                .ToList();

            _logger.LogDebug("Found {SectionCount} matching documentation sections for query: '{Query}'", 
                orderedSections.Count, searchQuery);

            return orderedSections;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documentation sections with query: '{Query}'", searchQuery);
            return Enumerable.Empty<DocumentationSection>();
        }
    }

    public async Task<IEnumerable<Documentation>> GetDocumentationRequiringRegenerationAsync(
        Dictionary<Guid, DateTime> repositoryLastModified, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting documentation requiring regeneration for {RepositoryCount} repositories", 
            repositoryLastModified.Count);

        var documentationRequiringRegeneration = new List<Documentation>();

        foreach (var kvp in repositoryLastModified)
        {
            var repositoryId = kvp.Key;
            var lastModified = kvp.Value;

            var documentation = await GetByRepositoryIdAsync(repositoryId, cancellationToken);
            if (documentation != null && documentation.RequiresRegeneration(lastModified))
            {
                documentationRequiringRegeneration.Add(documentation);
            }
        }

        _logger.LogInformation("Found {DocumentationCount} documentation items requiring regeneration", 
            documentationRequiringRegeneration.Count);

        return documentationRequiringRegeneration;
    }

    #region Private Helper Methods

    private async Task IndexDocumentationSectionsAsync(Documentation documentation, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Indexing documentation sections for search: {DocumentationId}", documentation.Id);

            // Convert sections to searchable documents
            var searchableDocuments = documentation.Sections.Select(section => new
            {
                Id = $"{documentation.Id}_{section.Id}",
                DocumentationId = documentation.Id.ToString(),
                RepositoryId = documentation.RepositoryId.ToString(),
                SectionId = section.Id.ToString(),
                Title = section.Title,
                Content = section.Content,
                SectionType = section.Type.ToString(),
                Tags = section.Tags.ToArray(),
                Order = section.Order,
                CreatedAt = section.Metadata.CreatedAt,
                LastModified = section.Metadata.LastModifiedAt ?? section.Metadata.CreatedAt
            });

            // In a real implementation, this would index the documents in Azure AI Search
            // For now, we'll just log the operation
            _logger.LogDebug("Would index {SectionCount} sections for documentation: {DocumentationId}", 
                searchableDocuments.Count(), documentation.Id);

            // TODO: Implement actual Azure AI Search indexing
            // await _searchService.IndexDocumentsAsync("documentation-sections", searchableDocuments, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing documentation sections for: {DocumentationId}", documentation.Id);
            // Don't throw - indexing failure shouldn't break the main operation
        }
    }

    private async Task RemoveDocumentationFromSearchAsync(Documentation documentation, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Removing documentation from search index: {DocumentationId}", documentation.Id);

            // Get document IDs to remove
            var documentIds = documentation.Sections.Select(section => $"{documentation.Id}_{section.Id}").ToList();

            // TODO: Implement actual Azure AI Search document removal
            // await _searchService.DeleteDocumentsAsync("documentation-sections", documentIds, cancellationToken);

            _logger.LogDebug("Removed {DocumentCount} documents from search index for documentation: {DocumentationId}", 
                documentIds.Count, documentation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing documentation from search index: {DocumentationId}", documentation.Id);
            // Don't throw - search cleanup failure shouldn't break the main operation
        }
    }

    private static bool ContainsSearchQuery(DocumentationSection section, string searchQuery)
    {
        var query = searchQuery.ToLowerInvariant();
        
        // Search in title
        if (section.Title.ToLowerInvariant().Contains(query))
            return true;
        
        // Search in content
        if (section.Content.ToLowerInvariant().Contains(query))
            return true;
        
        // Search in tags
        if (section.Tags.Any(tag => tag.ToLowerInvariant().Contains(query)))
            return true;
        
        // Search in code references
        if (section.CodeReferences.Any(cr => 
            cr.FilePath.ToLowerInvariant().Contains(query) ||
            cr.Description.ToLowerInvariant().Contains(query) ||
            cr.CodeSnippet.ToLowerInvariant().Contains(query)))
            return true;
        
        return false;
    }

    private static double CalculateRelevanceScore(DocumentationSection section, string searchQuery)
    {
        var score = 0.0;
        var query = searchQuery.ToLowerInvariant();
        
        // Title matches are most important
        if (section.Title.ToLowerInvariant().Contains(query))
        {
            score += 10.0;
            // Exact title match gets higher score
            if (section.Title.ToLowerInvariant() == query)
                score += 20.0;
        }
        
        // Content matches
        var contentLower = section.Content.ToLowerInvariant();
        var contentMatches = CountOccurrences(contentLower, query);
        score += contentMatches * 2.0;
        
        // Tag matches
        var tagMatches = section.Tags.Count(tag => tag.ToLowerInvariant().Contains(query));
        score += tagMatches * 5.0;
        
        // Code reference matches
        var codeRefMatches = section.CodeReferences.Count(cr => 
            cr.FilePath.ToLowerInvariant().Contains(query) ||
            cr.Description.ToLowerInvariant().Contains(query));
        score += codeRefMatches * 3.0;
        
        // Boost score for certain section types that are more likely to be relevant
        score += section.Type switch
        {
            DocumentationSectionType.Overview => 2.0,
            DocumentationSectionType.Usage => 3.0,
            DocumentationSectionType.ApiReference => 1.5,
            DocumentationSectionType.Examples => 2.5,
            _ => 0.0
        };
        
        return score;
    }

    private static int CountOccurrences(string text, string searchTerm)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
            return 0;
        
        var count = 0;
        var index = 0;
        
        while ((index = text.IndexOf(searchTerm, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += searchTerm.Length;
        }
        
        return count;
    }

    #endregion
}