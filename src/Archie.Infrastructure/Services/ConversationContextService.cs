using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.Configuration;

namespace Archie.Infrastructure.Services;

/// <summary>
/// Service for managing conversation context and retrieving relevant information from repositories
/// </summary>
public class ConversationContextService : IConversationContextService
{
    private readonly IAzureSearchService _searchService;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ConversationalAIOptions _options;
    private readonly ILogger<ConversationContextService> _logger;

    public ConversationContextService(
        IAzureSearchService searchService,
        IRepositoryRepository repositoryRepository,
        IOptions<ConversationalAIOptions> options,
        ILogger<ConversationContextService> logger)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<SearchResult>> RetrieveRelevantContextAsync(
        string query,
        ConversationContext context,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<SearchResult>();

        if (!context.HasRepositories())
        {
            _logger.LogWarning("No repositories specified in conversation context");
            return Array.Empty<SearchResult>();
        }

        try
        {
            _logger.LogInformation("Retrieving relevant context for query: {Query} from {RepositoryCount} repositories", 
                query, context.RepositoryIds.Count);

            var searchResults = new List<SearchResult>();

            // Search each repository for relevant content
            foreach (var repositoryId in context.RepositoryIds)
            {
                try
                {
                    var repositoryResults = await SearchRepositoryAsync(
                        query, repositoryId, maxResults / context.RepositoryIds.Count + 1, cancellationToken);
                    searchResults.AddRange(repositoryResults);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to search repository {RepositoryId} for context", repositoryId);
                }
            }

            // Rank and filter results
            var rankedResults = RankSearchResults(searchResults, query, context)
                .Take(maxResults)
                .ToList();

            _logger.LogInformation("Retrieved {ResultCount} relevant context items", rankedResults.Count);

            return rankedResults.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving relevant context for query: {Query}", query);
            return Array.Empty<SearchResult>();
        }
    }

    public async Task<string> BuildContextPromptAsync(
        string query,
        IReadOnlyList<SearchResult> searchResults,
        IReadOnlyList<ConversationMessage> messageHistory,
        ConversationContext context,
        CancellationToken cancellationToken = default)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("Based on the following context from the repository, please answer the user's question:");
        prompt.AppendLine();

        // Add search results as context
        if (searchResults.Any())
        {
            prompt.AppendLine("=== REPOSITORY CONTEXT ===");
            
            var contextTokenCount = 0;
            var maxContextTokens = _options.Defaults.MaxContextTokens;

            foreach (var result in searchResults.Take(10))
            {
                var resultContext = FormatSearchResultForContext(result);
                var estimatedTokens = resultContext.Length / 4; // Rough token estimate
                
                if (contextTokenCount + estimatedTokens > maxContextTokens)
                    break;

                prompt.AppendLine(resultContext);
                prompt.AppendLine();
                contextTokenCount += estimatedTokens;
            }
        }

        // Add recent conversation history for context
        if (messageHistory.Any())
        {
            prompt.AppendLine("=== CONVERSATION HISTORY ===");
            var recentMessages = messageHistory.TakeLast(5);
            
            foreach (var message in recentMessages)
            {
                var roleLabel = message.Type switch
                {
                    MessageType.UserQuery => "User",
                    MessageType.AIResponse => "Assistant",
                    _ => "System"
                };
                
                prompt.AppendLine($"{roleLabel}: {message.Content.Substring(0, Math.Min(200, message.Content.Length))}");
            }
            prompt.AppendLine();
        }

        // Add conversation context information
        if (context.HasDomain() || context.HasTechnicalTags())
        {
            prompt.AppendLine("=== CONVERSATION CONTEXT ===");
            
            if (context.HasDomain())
                prompt.AppendLine($"Focus Area: {context.Domain}");
            
            if (context.HasTechnicalTags())
                prompt.AppendLine($"Technical Tags: {string.Join(", ", context.TechnicalTags)}");
            
            prompt.AppendLine();
        }

        prompt.AppendLine("=== USER QUESTION ===");
        prompt.AppendLine(query);

        return prompt.ToString();
    }

    public async Task<ConversationContext> EnrichContextAsync(
        ConversationContext context,
        string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Enriching conversation context based on query: {Query}", query);

            var enrichedContext = context;

            // Extract and add technical tags from query
            var extractedTags = ExtractTechnicalTerms(query);
            foreach (var tag in extractedTags)
            {
                enrichedContext = enrichedContext.AddTechnicalTag(tag);
            }

            // Detect domain if not specified
            if (!enrichedContext.HasDomain())
            {
                var detectedDomain = DetectDomainFromQuery(query);
                if (!string.IsNullOrWhiteSpace(detectedDomain))
                {
                    enrichedContext = enrichedContext.WithDomain(detectedDomain);
                }
            }

            // Add query metadata to session data
            enrichedContext = enrichedContext.AddSessionData("lastQuery", query);
            enrichedContext = enrichedContext.AddSessionData("queryTimestamp", DateTime.UtcNow);

            return enrichedContext;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enriching conversation context");
            return context;
        }
    }

    public async Task<IReadOnlyList<CodeReference>> RetrieveCodeReferencesAsync(
        string query,
        IReadOnlyList<Guid> repositoryIds,
        int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        var codeReferences = new List<CodeReference>();

        try
        {
            foreach (var repositoryId in repositoryIds)
            {
                var searchResults = await SearchRepositoryAsync(query, repositoryId, maxResults, cancellationToken);
                
                foreach (var result in searchResults.Where(r => !string.IsNullOrWhiteSpace(r.FilePath)))
                {
                    var codeRef = new CodeReference(
                        result.FilePath,
                        result.Content,
                        $"Code from {result.FilePath}",
                        "CodeReference",
                        result.LineNumber);
                    
                    codeReferences.Add(codeRef);
                }
            }

            return codeReferences.Take(maxResults).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving code references");
            return Array.Empty<CodeReference>();
        }
    }

    public async Task<IReadOnlyList<DocumentationSection>> RetrieveDocumentationAsync(
        string query,
        IReadOnlyList<Guid> repositoryIds,
        int maxResults = 3,
        CancellationToken cancellationToken = default)
    {
        var documentationSections = new List<DocumentationSection>();

        try
        {
            // For now, return empty list as documentation retrieval would require
            // integration with the documentation generation system
            _logger.LogInformation("Documentation retrieval requested for query: {Query}", query);
            
            return documentationSections.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving documentation");
            return Array.Empty<DocumentationSection>();
        }
    }

    public async Task<CrossRepositoryContext> BuildCrossRepositoryContextAsync(
        string query,
        IReadOnlyList<Guid> repositoryIds,
        int maxResultsPerRepository = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resultsByRepository = new Dictionary<Guid, IReadOnlyList<SearchResult>>();
            var allResults = new List<SearchResult>();

            // Search each repository
            foreach (var repositoryId in repositoryIds)
            {
                var results = await SearchRepositoryAsync(query, repositoryId, maxResultsPerRepository, cancellationToken);
                resultsByRepository[repositoryId] = results;
                allResults.AddRange(results);
            }

            // Analyze patterns and differences
            var commonPatterns = AnalyzeCommonPatterns(resultsByRepository);
            var differences = AnalyzeDifferences(resultsByRepository);

            return new CrossRepositoryContext
            {
                RepositoryIds = repositoryIds,
                Results = allResults.AsReadOnly(),
                ResultsByRepository = resultsByRepository,
                CommonPatterns = commonPatterns,
                Differences = differences,
                ComparisonMetadata = new Dictionary<string, object>
                {
                    ["totalResults"] = allResults.Count,
                    ["repositoryCount"] = repositoryIds.Count,
                    ["queryTimestamp"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building cross-repository context");
            return new CrossRepositoryContext
            {
                RepositoryIds = repositoryIds,
                Results = Array.Empty<SearchResult>(),
                ResultsByRepository = new Dictionary<Guid, IReadOnlyList<SearchResult>>(),
                CommonPatterns = Array.Empty<string>(),
                Differences = Array.Empty<string>(),
                ComparisonMetadata = new Dictionary<string, object>()
            };
        }
    }

    public async Task<IReadOnlyList<string>> SuggestQueryRefinementsAsync(
        string query,
        ConversationContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestions = new List<string>();

            // Suggest more specific terms if query is too general
            if (query.Split(' ').Length <= 3)
            {
                if (context.HasTechnicalTags())
                {
                    var tags = context.TechnicalTags.Take(3);
                    suggestions.Add($"{query} {string.Join(" ", tags)}");
                }

                if (context.HasDomain())
                {
                    suggestions.Add($"{query} in {context.Domain}");
                }
            }

            // Suggest alternative phrasings
            var alternativePhrases = GenerateAlternativePhrasings(query);
            suggestions.AddRange(alternativePhrases);

            return suggestions.Take(5).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error suggesting query refinements");
            return Array.Empty<string>();
        }
    }

    private async Task<IReadOnlyList<SearchResult>> SearchRepositoryAsync(
        string query,
        Guid repositoryId,
        int maxResults,
        CancellationToken cancellationToken)
    {
        // This would integrate with the Azure Search service
        // For now, return empty results as a placeholder
        return Array.Empty<SearchResult>();
    }

    private IReadOnlyList<SearchResult> RankSearchResults(
        IReadOnlyList<SearchResult> results,
        string query,
        ConversationContext context)
    {
        return results
            .OrderByDescending(r => r.RelevanceScore)
            .ThenByDescending(r => CalculateContextRelevance(r, context))
            .ToList()
            .AsReadOnly();
    }

    private double CalculateContextRelevance(SearchResult result, ConversationContext context)
    {
        var relevance = 0.0;

        // Boost results from preferred languages
        if (context.Preferences.HasPreferredLanguages())
        {
            if (context.Preferences.IsPreferredLanguage(result.Language))
                relevance += 0.2;
        }

        // Boost results that match technical tags
        if (context.HasTechnicalTags())
        {
            var matchingTags = context.TechnicalTags.Count(tag => 
                result.Content.Contains(tag, StringComparison.OrdinalIgnoreCase) ||
                result.Title.Contains(tag, StringComparison.OrdinalIgnoreCase));
            
            relevance += matchingTags * 0.1;
        }

        return relevance;
    }

    private string FormatSearchResultForContext(SearchResult result)
    {
        var context = new StringBuilder();
        
        context.AppendLine($"File: {result.FilePath}");
        if (result.LineNumber > 0)
            context.AppendLine($"Line: {result.LineNumber}");
        
        if (!string.IsNullOrWhiteSpace(result.Language))
            context.AppendLine($"Language: {result.Language}");
        
        context.AppendLine("Content:");
        context.AppendLine(result.Content);
        
        return context.ToString();
    }

    private IReadOnlyList<string> ExtractTechnicalTerms(string query)
    {
        var terms = new List<string>();
        var technicalPatterns = new[]
        {
            @"\b(API|REST|HTTP|JSON|XML|SQL|NoSQL|JWT|OAuth)\b",
            @"\b\w*(Service|Controller|Repository|Manager|Factory|Builder|Handler)\b",
            @"\b(async|await|Promise|Observable|Stream)\b",
            @"\b(Docker|Kubernetes|microservice|database|cache|queue)\b"
        };

        foreach (var pattern in technicalPatterns)
        {
            var matches = System.Text.RegularExpressions.Regex.Matches(query, pattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (!terms.Contains(match.Value, StringComparer.OrdinalIgnoreCase))
                    terms.Add(match.Value);
            }
        }

        return terms.Take(10).ToList().AsReadOnly();
    }

    private string DetectDomainFromQuery(string query)
    {
        var lowercaseQuery = query.ToLowerInvariant();

        var domainPatterns = new Dictionary<string, string[]>
        {
            ["authentication"] = new[] { "auth", "login", "jwt", "oauth", "token", "credential" },
            ["database"] = new[] { "database", "sql", "query", "table", "entity", "migration" },
            ["api"] = new[] { "api", "endpoint", "rest", "http", "controller", "route" },
            ["testing"] = new[] { "test", "mock", "assert", "spec", "unit", "integration" },
            ["architecture"] = new[] { "pattern", "design", "architecture", "structure", "layer" },
            ["performance"] = new[] { "performance", "optimize", "cache", "memory", "speed" }
        };

        foreach (var domain in domainPatterns)
        {
            if (domain.Value.Any(keyword => lowercaseQuery.Contains(keyword)))
            {
                return domain.Key;
            }
        }

        return string.Empty;
    }

    private IReadOnlyList<string> AnalyzeCommonPatterns(Dictionary<Guid, IReadOnlyList<SearchResult>> resultsByRepository)
    {
        var patterns = new List<string>();

        // Find common file patterns, naming conventions, etc.
        var allFilePaths = resultsByRepository.Values
            .SelectMany(results => results)
            .Select(r => r.FilePath)
            .ToList();

        var commonExtensions = allFilePaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => Path.GetExtension(path))
            .Where(ext => !string.IsNullOrWhiteSpace(ext))
            .GroupBy(ext => ext, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => $"Common file type: {g.Key}")
            .ToList();

        patterns.AddRange(commonExtensions);

        return patterns.AsReadOnly();
    }

    private IReadOnlyList<string> AnalyzeDifferences(Dictionary<Guid, IReadOnlyList<SearchResult>> resultsByRepository)
    {
        var differences = new List<string>();

        // Simple analysis of differences between repositories
        var repositoryLanguages = resultsByRepository
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.GroupBy(r => r.Language).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "unknown");

        var languageGroups = repositoryLanguages
            .GroupBy(kvp => kvp.Value)
            .ToList();

        if (languageGroups.Count > 1)
        {
            foreach (var group in languageGroups)
            {
                differences.Add($"{group.Count()} repository(ies) primarily use {group.Key}");
            }
        }

        return differences.AsReadOnly();
    }

    private IReadOnlyList<string> GenerateAlternativePhrasings(string query)
    {
        var alternatives = new List<string>();

        // Simple transformations
        if (query.StartsWith("How "))
        {
            alternatives.Add(query.Replace("How ", "What is the way to "));
            alternatives.Add(query.Replace("How ", "Can you explain how to "));
        }

        if (query.StartsWith("What "))
        {
            alternatives.Add(query.Replace("What ", "How "));
            alternatives.Add(query.Replace("What ", "Can you show me "));
        }

        if (query.Contains(" is "))
        {
            alternatives.Add(query.Replace(" is ", " works "));
        }

        return alternatives.Take(3).ToList().AsReadOnly();
    }
}