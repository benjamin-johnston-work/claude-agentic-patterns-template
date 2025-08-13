using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Archie.Infrastructure.Configuration;
using Archie.Infrastructure.AzureSearch.Interfaces;
using Archie.Infrastructure.AzureSearch.Models;

namespace Archie.Infrastructure.Services;

/// <summary>
/// Azure OpenAI service implementation for generating documentation using GPT models.
/// Includes rate limiting, retry logic, and content quality validation.
/// </summary>
public class AIDocumentationGeneratorService : IAIDocumentationGeneratorService
{
    private readonly AzureOpenAIClient _openAIClient;
    private readonly DocumentationGenerationSettings _options;
    private readonly IAzureSearchService _azureSearchService;
    private readonly IRepositoryRepository _repositoryRepository;
    private readonly ILogger<AIDocumentationGeneratorService> _logger;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private readonly ConcurrentQueue<DateTime> _requestHistory;
    private readonly ConcurrentDictionary<string, int> _dailyTokenUsage;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

    public AIDocumentationGeneratorService(
        IOptions<DocumentationGenerationSettings> options,
        IAzureSearchService azureSearchService,
        IRepositoryRepository repositoryRepository,
        ILogger<AIDocumentationGeneratorService> logger)
    {
        _options = options.Value;
        _azureSearchService = azureSearchService ?? throw new ArgumentNullException(nameof(azureSearchService));
        _repositoryRepository = repositoryRepository ?? throw new ArgumentNullException(nameof(repositoryRepository));
        _logger = logger;

        var endpoint = new Uri(_options.AzureOpenAIEndpoint);
        var credential = new AzureKeyCredential(_options.AzureOpenAIApiKey);
        
        // Use default client options - let SDK handle retries without NetworkTimeout override
        var clientOptions = new AzureOpenAIClientOptions();
        
        _openAIClient = new AzureOpenAIClient(endpoint, credential, clientOptions);
        
        // Initialize rate limiting
        _rateLimitSemaphore = new SemaphoreSlim(_options.MaxConcurrentGenerations, _options.MaxConcurrentGenerations);
        _requestHistory = new ConcurrentQueue<DateTime>();
        _dailyTokenUsage = new ConcurrentDictionary<string, int>();
    }

    public async Task<Documentation> GenerateDocumentationAsync(
        Guid repositoryId, 
        DocumentationGenerationOptions options, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting documentation generation for repository: {RepositoryId}", repositoryId);

            // Get repository information
            var repository = await _repositoryRepository.GetByIdAsync(repositoryId, cancellationToken);
            if (repository == null)
            {
                throw new InvalidOperationException($"Repository {repositoryId} not found");
            }

            // Build repository analysis context from indexed Azure Search documents
            var analysisContext = await BuildAnalysisContextFromIndexedDocumentsAsync(repository, cancellationToken);
            
            // Validate repository analysis results to prevent AI hallucination
            if (analysisContext.ImportantFiles == null || !analysisContext.ImportantFiles.Any())
            {
                var errorMessage = "No indexed documents found for repository. Ensure repository has been indexed first.";
                
                _logger.LogError("Cannot generate documentation for repository {RepositoryId}: {Error}", repositoryId, errorMessage);
                throw new InvalidOperationException($"Documentation generation failed for repository {repositoryId}: {errorMessage}");
            }
            
            _logger.LogInformation("Repository analysis completed from indexed documents for {RepositoryId}: Found {FileCount} important files", 
                repositoryId, analysisContext.ImportantFiles.Count);
            
            // Create documentation metadata
            var metadata = new DocumentationMetadata(
                repository.Name,
                repository.Url,
                analysisContext.PrimaryLanguage,
                analysisContext.Structure.ProjectType
            );

            // Add languages, frameworks, and dependencies
            foreach (var language in analysisContext.Languages)
            {
                metadata.AddLanguage(language);
            }
            foreach (var framework in analysisContext.Structure.Frameworks)
            {
                metadata.AddFramework(framework);
            }
            foreach (var dependency in analysisContext.Dependencies.Take(20)) // Limit to top 20 dependencies
            {
                metadata.AddDependency($"{dependency.Name} ({dependency.Version})");
            }

            // Create documentation entity
            var documentation = Documentation.Create(repositoryId, $"{repository.Name} Documentation", metadata);
            documentation.UpdateStatus(DocumentationStatus.Analyzing);

            var startTime = DateTime.UtcNow;

            // Transition to content generation phase
            documentation.UpdateStatus(DocumentationStatus.GeneratingContent);

            // Generate sections concurrently with rate limiting
            var sectionTasks = options.RequestedSections.Select(async sectionType =>
            {
                try
                {
                    _logger.LogDebug("Generating section: {SectionType} for repository: {RepositoryId}", sectionType, repositoryId);
                    var section = await GenerateSectionAsync(analysisContext, sectionType, options.CustomInstructions, cancellationToken);
                    return section;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating section {SectionType} for repository {RepositoryId}", sectionType, repositoryId);
                    // Return a basic section with error information
                    return new DocumentationSection(
                        GetSectionTitle(sectionType),
                        $"Error generating this section: {ex.Message}",
                        sectionType,
                        GetSectionOrder(sectionType)
                    );
                }
            });

            var sections = await Task.WhenAll(sectionTasks);
            var generationTime = DateTime.UtcNow - startTime;

            // Add sections to documentation
            foreach (var section in sections.Where(s => s != null))
            {
                documentation.AddSection(section);
            }

            // Update statistics
            var totalWordCount = sections.Sum(s => s.GetWordCount());
            var totalCodeReferences = sections.Sum(s => s.CodeReferences.Count);
            var coveredTopics = sections.SelectMany(s => s.Tags).Distinct().ToList();

            var statistics = new DocumentationStatistics(
                sections.Length,
                totalCodeReferences,
                totalWordCount,
                generationTime,
                0.85, // Default accuracy score, can be improved with validation
                coveredTopics
            );

            // Update documentation with final statistics
            typeof(Documentation).GetProperty(nameof(Documentation.Statistics))?.SetValue(documentation, statistics);

            // Transition through enriching and indexing phases
            documentation.UpdateStatus(DocumentationStatus.Enriching);
            documentation.UpdateStatus(DocumentationStatus.Indexing);

            _logger.LogInformation("Successfully generated documentation for repository: {RepositoryId} with {SectionCount} sections in {GenerationTime}ms", 
                repositoryId, sections.Length, generationTime.TotalMilliseconds);

            return documentation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating documentation for repository: {RepositoryId}", repositoryId);
            throw;
        }
    }

    public async Task<DocumentationSection> GenerateSectionAsync(
        RepositoryAnalysisContext context, 
        DocumentationSectionType sectionType, 
        string? customInstructions = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply rate limiting
            if (_options.EnableRateLimitProtection)
            {
                await ApplyRateLimitingAsync(cancellationToken);
            }

            // Check daily token usage
            if (_options.EnableTokenUsageTracking && await HasExceededDailyTokenLimitAsync())
            {
                throw new InvalidOperationException("Daily token usage limit exceeded");
            }

            var prompt = BuildSectionPrompt(context, sectionType, customInstructions);
            var content = await GenerateContentAsync(prompt, cancellationToken);

            // Extract code references from the generated content
            var codeReferences = new List<CodeReference>();
            if (_options.EnableCodeExtraction)
            {
                codeReferences = await ExtractCodeReferencesAsync(content, context, cancellationToken);
            }

            // Create section with metadata
            var metadata = new SectionMetadata(
                "Azure OpenAI",
                _options.GPTDeploymentName,
                EstimateTokenCount(content),
                0.85 // Default confidence score
            );

            var section = new DocumentationSection(
                GetSectionTitle(sectionType),
                content,
                sectionType,
                GetSectionOrder(sectionType),
                metadata
            );

            // Add code references
            foreach (var codeRef in codeReferences)
            {
                section.AddCodeReference(
                    codeRef.FilePath,
                    codeRef.CodeSnippet,
                    codeRef.Description,
                    codeRef.ReferenceType,
                    codeRef.StartLine,
                    codeRef.EndLine
                );
            }

            // Add relevant tags
            var tags = GenerateSectionTags(context, sectionType);
            foreach (var tag in tags)
            {
                section.AddTag(tag);
            }

            _logger.LogDebug("Generated section: {SectionType} with {WordCount} words and {CodeRefCount} code references", 
                sectionType, section.GetWordCount(), codeReferences.Count);

            return section;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating section {SectionType}", sectionType);
            throw;
        }
    }

    public async Task<List<CodeReference>> ExtractCodeReferencesAsync(
        string content, 
        RepositoryAnalysisContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var codeReferences = new List<CodeReference>();

            // Extract code blocks from markdown content
            var codeBlockPattern = @"```(?<language>\w+)?\n?(?<code>[\s\S]*?)\n?```";
            var matches = Regex.Matches(content, codeBlockPattern);

            foreach (Match match in matches)
            {
                var code = match.Groups["code"].Value.Trim();
                var language = match.Groups["language"].Value;

                if (string.IsNullOrWhiteSpace(code)) continue;

                // Try to find corresponding file in repository analysis
                var matchingFile = context.ImportantFiles
                    .Where(f => !string.IsNullOrEmpty(language) ? 
                               f.Language.Equals(language, StringComparison.OrdinalIgnoreCase) :
                               true)
                    .FirstOrDefault(f => f.Content?.Contains(code.Substring(0, Math.Min(code.Length, 100))) == true);

                var filePath = matchingFile?.FilePath ?? $"example.{GetFileExtension(language)}";
                var description = GenerateCodeDescription(code, language);

                codeReferences.Add(new CodeReference(
                    filePath,
                    code,
                    description,
                    DetermineReferenceType(code, language)
                ));
            }

            // Also extract inline code references
            var inlineCodePattern = @"`([^`]+)`";
            var inlineMatches = Regex.Matches(content, inlineCodePattern);

            foreach (Match match in inlineMatches.Cast<Match>().Take(10)) // Limit to first 10
            {
                var code = match.Groups[1].Value;
                if (code.Length > 5 && code.Length < 100) // Reasonable length for inline code
                {
                    var matchingFile = context.ImportantFiles
                        .FirstOrDefault(f => f.Content?.Contains(code) == true);

                    if (matchingFile != null)
                    {
                        codeReferences.Add(new CodeReference(
                            matchingFile.FilePath,
                            code,
                            $"Inline reference to {code}",
                            "Inline"
                        ));
                    }
                }
            }

            return codeReferences.DistinctBy(cr => cr.CodeSnippet).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting code references from content");
            return new List<CodeReference>();
        }
    }

    public async Task<string> EnrichContentWithExamplesAsync(
        string content, 
        RepositoryAnalysisContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var enrichmentPrompt = $@"
Enhance the following documentation content by adding practical examples, code snippets, and specific implementation details based on this repository context:

Repository: {context.RepositoryName}
Primary Language: {context.PrimaryLanguage}
Project Type: {context.Structure.ProjectType}
Key Frameworks: {string.Join(", ", context.Structure.Frameworks.Take(3))}

Original Content:
{content}

Please enrich this content with:
1. Practical code examples relevant to this specific repository
2. Step-by-step implementation guides where appropriate
3. Common usage patterns for this type of project
4. Troubleshooting tips specific to the technologies used

Keep the enhanced content under {_options.MaxTokensPerSection} tokens and maintain a professional, clear tone.";

            return await GenerateContentAsync(enrichmentPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enriching content with examples, returning original content");
            return content;
        }
    }

    public async Task<double> ValidateDocumentationQualityAsync(
        Documentation documentation, 
        RepositoryAnalysisContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_options.EnableQualityValidation)
            {
                return 0.85; // Default score when validation is disabled
            }

            var qualityMetrics = new List<double>();

            // Check content completeness
            var completenessScore = CalculateCompletenessScore(documentation);
            qualityMetrics.Add(completenessScore);

            // Check content relevance
            var relevanceScore = CalculateRelevanceScore(documentation, context);
            qualityMetrics.Add(relevanceScore);

            // Check content length and structure
            var structureScore = CalculateStructureScore(documentation);
            qualityMetrics.Add(structureScore);

            // Calculate overall quality score
            var overallScore = qualityMetrics.Average();

            _logger.LogInformation("Documentation quality validation completed. Overall score: {Score:F2}", overallScore);

            return overallScore;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating documentation quality, returning default score");
            return 0.75; // Conservative default
        }
    }

    #region Private Helper Methods

    private async Task<string> GenerateContentAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var chatClient = _openAIClient.GetChatClient(_options.GPTDeploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert technical documentation writer specializing in software development. Generate clear, comprehensive, and accurate documentation that follows best practices. Use markdown formatting and include practical examples where appropriate."),
                new UserChatMessage(prompt)
            };

            var chatCompletionsOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = _options.MaxTokensPerSection,
                Temperature = (float)_options.Temperature,
            };

            var response = await ExecuteWithRetryAsync(
                () => chatClient.CompleteChatAsync(messages, chatCompletionsOptions, cancellationToken),
                cancellationToken
            );

            if (response?.Value?.Content?.Any() == true)
            {
                var content = string.Join("", response.Value.Content.Select(c => c.Text));
                
                // Track token usage
                if (_options.EnableTokenUsageTracking)
                {
                    var tokenCount = EstimateTokenCount(content) + EstimateTokenCount(prompt);
                    UpdateDailyTokenUsage(tokenCount);
                }

                return content;
            }

            // Handle case where retry mechanism exhausted all attempts
            _logger.LogError("Azure OpenAI service failed to return content after {RetryAttempts} attempts", _options.RetryAttempts);
            throw new InvalidOperationException($"Azure OpenAI service is currently unavailable. Please try again later. (Failed after {_options.RetryAttempts} retry attempts)");
        }
        catch (RequestFailedException ex) when (!IsRetryableException(ex))
        {
            // Handle non-retryable errors (4xx client errors)
            _logger.LogError(ex, "Azure OpenAI request failed with non-retryable error: {ErrorCode}", ex.ErrorCode);
            throw new InvalidOperationException($"Azure OpenAI request failed: {ex.Message} (Error Code: {ex.ErrorCode})");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Azure OpenAI request was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating content with Azure OpenAI");
            throw new InvalidOperationException("An unexpected error occurred while generating documentation. Please try again.", ex);
        }
    }

    private string BuildSectionPrompt(RepositoryAnalysisContext context, DocumentationSectionType sectionType, string? customInstructions)
    {
        try
        {
            // Input validation
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "Repository analysis context cannot be null");
            }

            if (string.IsNullOrWhiteSpace(context.RepositoryName))
            {
                _logger.LogWarning("Repository name is missing in context");
            }

            var basePrompt = new StringBuilder();
        
        basePrompt.AppendLine($"Generate comprehensive {sectionType} documentation for the following repository:");
        basePrompt.AppendLine();

        // ENHANCED: Project Purpose (from content analysis)
        if (context.Purpose != null && !context.Purpose.IsEmpty)
        {
            basePrompt.AppendLine("**Project Purpose:**");
            
            if (!string.IsNullOrWhiteSpace(context.Purpose.Description))
            {
                basePrompt.AppendLine($"- Description: {context.Purpose.Description}");
            }
            
            if (!string.IsNullOrWhiteSpace(context.Purpose.BusinessDomain))
            {
                basePrompt.AppendLine($"- Business Domain: {context.Purpose.BusinessDomain}");
            }
            
            if (!string.IsNullOrWhiteSpace(context.Purpose.UserValue))
            {
                basePrompt.AppendLine($"- User Value: {context.Purpose.UserValue}");
            }
            
            if (context.Purpose.KeyFeatures?.Any() == true)
            {
                basePrompt.AppendLine($"- Key Features: {string.Join(", ", context.Purpose.KeyFeatures.Take(5))}");
            }

            if (context.Purpose.TechnicalHighlights?.Any() == true)
            {
                basePrompt.AppendLine("- Technical Highlights:");
                foreach (var highlight in context.Purpose.TechnicalHighlights.Take(3))
                {
                    if (!string.IsNullOrWhiteSpace(highlight.Key) && !string.IsNullOrWhiteSpace(highlight.Value))
                    {
                        basePrompt.AppendLine($"  - {highlight.Key}: {highlight.Value}");
                    }
                }
            }
            basePrompt.AppendLine();
        }

        // Enhanced Repository Information
        basePrompt.AppendLine($"**Repository Information:**");
        basePrompt.AppendLine($"- Name: {context.RepositoryName ?? "Unknown"}");
        basePrompt.AppendLine($"- Primary Language: {context.PrimaryLanguage ?? "Unknown"}");
        
        if (context.Structure != null && !string.IsNullOrWhiteSpace(context.Structure.ProjectType))
        {
            basePrompt.AppendLine($"- Project Type: {context.Structure.ProjectType}");
        }
        
        if (context.Structure?.Frameworks?.Any() == true)
        {
            basePrompt.AppendLine($"- Frameworks: {string.Join(", ", context.Structure.Frameworks.Take(5))}");
        }
        
        if (context.Dependencies?.Any() == true)
        {
            var dependencyNames = context.Dependencies.Take(5)
                .Where(d => !string.IsNullOrWhiteSpace(d?.Name))
                .Select(d => d.Name);
            if (dependencyNames.Any())
            {
                basePrompt.AppendLine($"- Key Dependencies: {string.Join(", ", dependencyNames)}");
            }
        }
        basePrompt.AppendLine();

        // ENHANCED: How It Works (from component analysis)
        if (context.ComponentMap != null && context.ComponentMap.HasArchitecture)
        {
            basePrompt.AppendLine("**How This Project Works:**");
            
            if (context.ComponentMap.EntryPoints?.Any() == true)
            {
                basePrompt.AppendLine($"- Entry Points: {string.Join(", ", context.ComponentMap.EntryPoints.Take(3))}");
            }
            
            if (context.ComponentMap.ComponentPurposes?.Any() == true)
            {
                basePrompt.AppendLine("- Key Components:");
                foreach (var component in context.ComponentMap.ComponentPurposes.Take(5))
                {
                    if (!string.IsNullOrWhiteSpace(component.Key) && !string.IsNullOrWhiteSpace(component.Value))
                    {
                        basePrompt.AppendLine($"  - {component.Key}: {component.Value}");
                    }
                }
            }
            basePrompt.AppendLine();
        }

        // ENHANCED: Actual Code Examples (showing functionality)
        if (context.ContentSummaries?.Any() == true)
        {
            basePrompt.AppendLine("**Code Examples (from actual repository):**");
            foreach (var summary in context.ContentSummaries.Take(3))
            {
                if (summary != null && !string.IsNullOrWhiteSpace(summary.CodeSnippet))
                {
                    var language = !string.IsNullOrWhiteSpace(summary.Language) ? summary.Language : "text";
                    var description = !string.IsNullOrWhiteSpace(summary.FunctionalityDescription) 
                        ? summary.FunctionalityDescription 
                        : "Code functionality";
                    
                    basePrompt.AppendLine($"```{language}");
                    basePrompt.AppendLine($"// {description}");
                    basePrompt.AppendLine(summary.CodeSnippet);
                    basePrompt.AppendLine("```");
                    basePrompt.AppendLine();
                }
            }
        }

        // Context-aware section requirements
        basePrompt.AppendLine($"**Section Requirements for {sectionType}:**");
        basePrompt.AppendLine(GetContextAwareRequirements(context, sectionType));
        basePrompt.AppendLine();

        // Domain-specific instructions
        var businessDomain = context.Purpose?.BusinessDomain ?? "Software";
        var domainInstructions = GetDomainSpecificInstructions(businessDomain, sectionType);
        if (!string.IsNullOrWhiteSpace(domainInstructions))
        {
            basePrompt.AppendLine("**Domain-Specific Focus:**");
            basePrompt.AppendLine(domainInstructions);
            basePrompt.AppendLine();
        }

        // Add language-specific instructions
        var primaryLanguage = context.PrimaryLanguage?.ToLowerInvariant() ?? "unknown";
        if (_options.LanguageSpecificPrompts?.TryGetValue(primaryLanguage, out var languagePrompt) == true 
            && !string.IsNullOrWhiteSpace(languagePrompt))
        {
            basePrompt.AppendLine($"**Language-Specific Focus:**");
            basePrompt.AppendLine(languagePrompt);
            basePrompt.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(customInstructions))
        {
            basePrompt.AppendLine("**Additional Instructions:**");
            basePrompt.AppendLine(customInstructions);
            basePrompt.AppendLine();
        }

        // Final instructions emphasizing accuracy based on actual content
        basePrompt.AppendLine("**IMPORTANT:** Generate documentation based on the ACTUAL project purpose and functionality described above.");
        basePrompt.AppendLine("Do not make assumptions or add features that are not evidenced by the code analysis.");
        
        var domainType = context.Purpose?.BusinessDomain?.ToLowerInvariant() ?? "software";
        basePrompt.AppendLine($"Focus on what this {domainType} project actually does for users.");
        basePrompt.AppendLine();
        basePrompt.AppendLine($"Please generate detailed, professional documentation that is {_options.MinContentLength}-{_options.MaxContentLength} characters long. Use markdown formatting and include practical examples where relevant.");

            return basePrompt.ToString();
        }
        catch (ArgumentNullException)
        {
            throw; // Re-throw argument null exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building section prompt for {SectionType}", sectionType);
            
            // Return a basic prompt as fallback
            var fallbackPrompt = $"Generate comprehensive {sectionType} documentation for the repository '{context?.RepositoryName ?? "Unknown"}'.";
            if (!string.IsNullOrWhiteSpace(customInstructions))
            {
                fallbackPrompt += $"\n\nAdditional instructions: {customInstructions}";
            }
            return fallbackPrompt;
        }
    }

    private string GetSectionRequirements(DocumentationSectionType sectionType)
    {
        return sectionType switch
        {
            DocumentationSectionType.Overview => "Provide a clear, concise overview of what this repository does, its main purpose, key features, and target audience. Include the problem it solves and its value proposition.",
            DocumentationSectionType.GettingStarted => "Create a step-by-step guide for new users to get started with this project. Include prerequisites, initial setup, and first steps to see the project working.",
            DocumentationSectionType.Installation => "Provide detailed installation instructions for all supported platforms. Include system requirements, dependencies, and verification steps.",
            DocumentationSectionType.Usage => "Explain how to use this project with practical examples. Include common use cases, code examples, and expected outputs.",
            DocumentationSectionType.Configuration => "Document all configuration options, environment variables, and settings. Include examples of common configurations.",
            DocumentationSectionType.Architecture => "Explain the system architecture, design decisions, key components, and how they interact. Include architectural diagrams in text form if helpful.",
            DocumentationSectionType.ApiReference => "Document all public APIs, endpoints, methods, classes, and interfaces. Include parameters, return values, and usage examples.",
            DocumentationSectionType.Examples => "Provide comprehensive examples showing different ways to use this project. Include real-world scenarios and complete code samples.",
            DocumentationSectionType.Testing => "Explain how to run tests, test structure, and testing practices. Include examples of writing new tests.",
            DocumentationSectionType.Deployment => "Provide deployment instructions for different environments. Include CI/CD considerations and production best practices.",
            DocumentationSectionType.Contributing => "Explain how others can contribute to this project. Include coding standards, pull request process, and development setup.",
            DocumentationSectionType.Troubleshooting => "Document common issues, error messages, and their solutions. Include debugging tips and FAQ.",
            DocumentationSectionType.Changelog => "Document version history, changes, and release notes if available.",
            DocumentationSectionType.License => "Explain the project's license, usage rights, and any legal considerations.",
            _ => "Generate appropriate content for this documentation section based on the repository context."
        };
    }

    private static string GetSectionTitle(DocumentationSectionType sectionType)
    {
        return sectionType switch
        {
            DocumentationSectionType.GettingStarted => "Getting Started",
            DocumentationSectionType.ApiReference => "API Reference",
            _ => sectionType.ToString()
        };
    }

    private static int GetSectionOrder(DocumentationSectionType sectionType)
    {
        return sectionType switch
        {
            DocumentationSectionType.Overview => 1,
            DocumentationSectionType.GettingStarted => 2,
            DocumentationSectionType.Installation => 3,
            DocumentationSectionType.Usage => 4,
            DocumentationSectionType.Configuration => 5,
            DocumentationSectionType.Architecture => 6,
            DocumentationSectionType.ApiReference => 7,
            DocumentationSectionType.Examples => 8,
            DocumentationSectionType.Testing => 9,
            DocumentationSectionType.Deployment => 10,
            DocumentationSectionType.Contributing => 11,
            DocumentationSectionType.Troubleshooting => 12,
            DocumentationSectionType.Changelog => 13,
            DocumentationSectionType.License => 14,
            _ => 99
        };
    }

    private static List<string> GenerateSectionTags(RepositoryAnalysisContext context, DocumentationSectionType sectionType)
    {
        var tags = new List<string> { sectionType.ToString().ToLowerInvariant() };
        
        tags.Add(context.PrimaryLanguage.ToLowerInvariant());
        tags.Add(context.Structure.ProjectType.ToLowerInvariant());
        
        // Add framework-specific tags
        tags.AddRange(context.Structure.Frameworks.Take(3).Select(f => f.ToLowerInvariant()));
        
        return tags.Distinct().ToList();
    }

    private async Task ApplyRateLimitingAsync(CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);

        try
        {
            // Clean up old requests
            var cutoffTime = DateTime.UtcNow - RateLimitWindow;
            while (_requestHistory.TryPeek(out var oldestRequest) && oldestRequest < cutoffTime)
            {
                _requestHistory.TryDequeue(out _);
            }

            // Check rate limit
            if (_requestHistory.Count >= _options.RequestsPerMinute)
            {
                var oldestInWindow = _requestHistory.FirstOrDefault();
                var waitTime = RateLimitWindow - (DateTime.UtcNow - oldestInWindow);
                
                if (waitTime > TimeSpan.Zero)
                {
                    _logger.LogWarning("Rate limit reached, waiting {WaitTimeMs}ms", waitTime.TotalMilliseconds);
                    await Task.Delay(waitTime, cancellationToken);
                }
            }

            _requestHistory.Enqueue(DateTime.UtcNow);
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    private async Task<T?> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken) where T : class
    {
        for (int attempt = 1; attempt <= _options.RetryAttempts; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsRetryableException(ex) && attempt < _options.RetryAttempts)
            {
                var delayMs = CalculateExponentialBackoffDelay(attempt);
                
                _logger.LogWarning("Azure OpenAI request failed (attempt {Attempt}/{MaxAttempts}), retrying in {DelayMs}ms: {ExceptionType} - {Message}",
                    attempt, _options.RetryAttempts, delayMs, ex.GetType().Name, ex.Message);
                
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        return null;
    }

    private static bool IsRetryableException(Exception ex)
    {
        return ex switch
        {
            RequestFailedException reqEx => reqEx.Status is 429 or 408 or >= 500,
            TaskCanceledException => false, // Don't retry cancellations
            OperationCanceledException => false, // Don't retry cancellations
            HttpRequestException => true, // Retry HTTP issues
            SocketException => true, // Retry network issues
            IOException => true, // Retry I/O issues
            TimeoutException => true, // Retry timeouts
            _ => false
        };
    }

    private static int CalculateExponentialBackoffDelay(int attempt)
    {
        var baseDelay = 2000; // 2 seconds
        var exponentialDelay = baseDelay * Math.Pow(2, attempt - 1);
        var jitter = Random.Shared.Next(0, 1000);
        
        return (int)Math.Min(exponentialDelay + jitter, 60000); // Cap at 60 seconds
    }

    private static int EstimateTokenCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        
        // Rough estimation: 1 token ≈ 4 characters for English text
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    private async Task<bool> HasExceededDailyTokenLimitAsync()
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var todayUsage = _dailyTokenUsage.GetValueOrDefault(today, 0);
        
        return todayUsage >= _options.MaxTokensPerDay;
    }

    private void UpdateDailyTokenUsage(int tokenCount)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        _dailyTokenUsage.AddOrUpdate(today, tokenCount, (key, existing) => existing + tokenCount);
        
        // Clean up old entries (keep only last 7 days)
        var cutoffDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var keysToRemove = _dailyTokenUsage.Keys.Where(key => string.Compare(key, cutoffDate) < 0).ToList();
        foreach (var key in keysToRemove)
        {
            _dailyTokenUsage.TryRemove(key, out _);
        }
    }

    private static double CalculateCompletenessScore(Documentation documentation)
    {
        var requiredSections = new[] { DocumentationSectionType.Overview, DocumentationSectionType.Usage };
        var hasAllRequired = requiredSections.All(rs => documentation.Sections.Any(s => s.Type == rs));
        
        var sectionsScore = Math.Min(documentation.Sections.Count / 5.0, 1.0); // Target 5+ sections
        var requiredScore = hasAllRequired ? 1.0 : 0.5;
        
        return (sectionsScore + requiredScore) / 2.0;
    }

    private static double CalculateRelevanceScore(Documentation documentation, RepositoryAnalysisContext context)
    {
        var relevanceIndicators = 0;
        var totalIndicators = 4;

        // Check if language is mentioned
        var content = string.Join(" ", documentation.Sections.Select(s => s.Content));
        if (content.Contains(context.PrimaryLanguage, StringComparison.OrdinalIgnoreCase))
            relevanceIndicators++;

        // Check if project type is mentioned
        if (content.Contains(context.Structure.ProjectType, StringComparison.OrdinalIgnoreCase))
            relevanceIndicators++;

        // Check if frameworks are mentioned
        if (context.Structure.Frameworks.Any(f => content.Contains(f, StringComparison.OrdinalIgnoreCase)))
            relevanceIndicators++;

        // Check if repository name is mentioned
        if (content.Contains(context.RepositoryName, StringComparison.OrdinalIgnoreCase))
            relevanceIndicators++;

        return (double)relevanceIndicators / totalIndicators;
    }

    private static double CalculateStructureScore(Documentation documentation)
    {
        var structureIndicators = 0;
        var totalIndicators = 3;

        // Check average section length
        var avgWordCount = documentation.Sections.Any() ? 
            documentation.Sections.Average(s => s.GetWordCount()) : 0;
        
        if (avgWordCount >= 100 && avgWordCount <= 2000) // Reasonable length
            structureIndicators++;

        // Check for code references
        if (documentation.Sections.Any(s => s.CodeReferences.Any()))
            structureIndicators++;

        // Check section ordering
        var orderedSections = documentation.Sections.OrderBy(s => s.Order).ToList();
        if (orderedSections.SequenceEqual(documentation.Sections))
            structureIndicators++;

        return (double)structureIndicators / totalIndicators;
    }

    private static string GetFileExtension(string language)
    {
        return language?.ToLowerInvariant() switch
        {
            "csharp" or "c#" => "cs",
            "javascript" => "js",
            "typescript" => "ts",
            "python" => "py",
            "java" => "java",
            "go" => "go",
            "rust" => "rs",
            "cpp" or "c++" => "cpp",
            "c" => "c",
            _ => "txt"
        };
    }

    private static string GenerateCodeDescription(string code, string language)
    {
        if (code.Contains("class ")) return "Class definition";
        if (code.Contains("function ") || code.Contains("def ")) return "Function implementation";
        if (code.Contains("interface ")) return "Interface definition";
        if (code.Contains("import ") || code.Contains("using ")) return "Import/dependency statement";
        if (code.Contains("const ") || code.Contains("var ")) return "Variable declaration";
        
        return $"Code example in {language ?? "unknown language"}";
    }

    private static string DetermineReferenceType(string code, string language)
    {
        if (code.Contains("class ")) return "Class";
        if (code.Contains("function ") || code.Contains("def ") || code.Contains("public ") || code.Contains("private ")) return "Method";
        if (code.Contains("interface ")) return "Interface";
        if (code.Contains("enum ")) return "Enum";
        if (code.Contains("struct ")) return "Struct";
        
        return "CodeBlock";
    }

    #endregion

    #region Enhanced Prompt Building Methods

    /// <summary>
    /// Get context-aware section requirements based on project purpose
    /// </summary>
    private string GetContextAwareRequirements(RepositoryAnalysisContext context, DocumentationSectionType sectionType)
    {
        var baseRequirement = GetSectionRequirements(sectionType);
        var businessDomain = context.Purpose.BusinessDomain.ToLowerInvariant();

        // Enhance requirements based on actual project type
        var enhancement = businessDomain switch
        {
            "game" when sectionType == DocumentationSectionType.Overview => 
                " Focus on gameplay mechanics, controls, and what makes this game unique.",
            "game" when sectionType == DocumentationSectionType.Usage => 
                " Explain how to play, control schemes, game objectives, and scoring system.",
            "web api" when sectionType == DocumentationSectionType.Overview => 
                " Focus on what endpoints are available and what data the API provides.",
            "web api" when sectionType == DocumentationSectionType.Usage => 
                " Include API endpoint examples, request/response formats, and authentication.",
            "library" when sectionType == DocumentationSectionType.Overview => 
                " Focus on what problems this library solves and what functionality it provides.",
            "library" when sectionType == DocumentationSectionType.Usage => 
                " Include import/installation instructions and common usage patterns.",
            _ => ""
        };

        return baseRequirement + enhancement;
    }

    /// <summary>
    /// Get domain-specific instructions for documentation generation
    /// </summary>
    private string GetDomainSpecificInstructions(string businessDomain, DocumentationSectionType sectionType)
    {
        return businessDomain.ToLowerInvariant() switch
        {
            "game" => GetGameDocumentationInstructions(sectionType),
            "web api" => GetWebApiDocumentationInstructions(sectionType),
            "library" => GetLibraryDocumentationInstructions(sectionType),
            "web application" => GetWebAppDocumentationInstructions(sectionType),
            "cli tool" => GetCliToolDocumentationInstructions(sectionType),
            _ => ""
        };
    }

    private string GetGameDocumentationInstructions(DocumentationSectionType sectionType)
    {
        return sectionType switch
        {
            DocumentationSectionType.Overview => 
                "Explain the game genre, objective, key mechanics, and what makes it enjoyable to play. Mention any special features like AI difficulty adjustment or power-ups.",
            DocumentationSectionType.Usage => 
                "Detail how to start playing, control schemes, game rules, scoring system, and any special gameplay features. Include what keys/controls to use.",
            DocumentationSectionType.GettingStarted => 
                "Explain how to open/run the game, what browser requirements exist, and how to start playing immediately.",
            _ => "Focus on the interactive and entertainment aspects of this game application."
        };
    }

    private string GetWebApiDocumentationInstructions(DocumentationSectionType sectionType)
    {
        return sectionType switch
        {
            DocumentationSectionType.Overview => 
                "Explain what data or services this API provides, who would use it, and what business problems it solves.",
            DocumentationSectionType.Usage => 
                "Include example API calls, request/response formats, authentication methods, and common use cases.",
            DocumentationSectionType.GettingStarted => 
                "Explain how to access the API, get API keys if needed, and make the first successful request.",
            _ => "Focus on the data services and endpoints provided by this API."
        };
    }

    private string GetLibraryDocumentationInstructions(DocumentationSectionType sectionType)
    {
        return sectionType switch
        {
            DocumentationSectionType.Overview => 
                "Explain what problems this library solves, what functionality it provides, and who would benefit from using it.",
            DocumentationSectionType.Usage => 
                "Include installation instructions, import statements, and common usage patterns with code examples.",
            DocumentationSectionType.GettingStarted => 
                "Show how to install the library and write a simple 'Hello World' example using its main features.",
            _ => "Focus on the reusable functionality and developer benefits of this library."
        };
    }

    private string GetWebAppDocumentationInstructions(DocumentationSectionType sectionType)
    {
        return sectionType switch
        {
            DocumentationSectionType.Overview => 
                "Explain what tasks users can accomplish with this web application and what makes it useful or unique.",
            DocumentationSectionType.Usage => 
                "Detail the main features, user workflows, and how to accomplish common tasks within the application.",
            DocumentationSectionType.GettingStarted => 
                "Explain how to access the application, create an account if needed, and complete initial setup.",
            _ => "Focus on user tasks and workflows within this web application."
        };
    }

    private string GetCliToolDocumentationInstructions(DocumentationSectionType sectionType)
    {
        return sectionType switch
        {
            DocumentationSectionType.Overview => 
                "Explain what command-line tasks this tool helps with and what problems it solves for users.",
            DocumentationSectionType.Usage => 
                "Include command syntax, common flags/options, and examples of typical workflows.",
            DocumentationSectionType.GettingStarted => 
                "Show installation steps and a simple example command to demonstrate basic functionality.",
            _ => "Focus on command-line workflows and automation provided by this tool."
        };
    }

    #endregion

    #region Azure Search Integration Methods

    /// <summary>
    /// Build repository analysis context from indexed Azure Search documents instead of fresh GitHub API calls
    /// </summary>
    private async Task<RepositoryAnalysisContext> BuildAnalysisContextFromIndexedDocumentsAsync(
        Repository repository, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Building analysis context from indexed documents for repository: {RepositoryId}", repository.Id);

        // Search for all documents in this repository
        var searchQuery = SearchQuery.Create("*")
            .WithFilters(SearchFilter.Equal("repository_id", repository.Id.ToString()))
            .WithPaging(1000); // Get all documents

        var searchResults = await _azureSearchService.SearchAsync(searchQuery, cancellationToken);
        
        if (searchResults?.Results == null || !searchResults.Results.Any())
        {
            _logger.LogWarning("No indexed documents found for repository: {RepositoryId}", repository.Id);
            throw new InvalidOperationException($"No indexed documents found for repository {repository.Id}. Ensure repository has been indexed first.");
        }

        var documents = searchResults.Results.Select(r => r.Document).ToList();
        
        _logger.LogInformation("Found {DocumentCount} indexed documents for repository: {RepositoryId}", 
            documents.Count, repository.Id);

        // Build analysis context from indexed documents
        var context = new RepositoryAnalysisContext
        {
            RepositoryId = repository.Id,
            RepositoryName = repository.Name,
            RepositoryUrl = repository.Url,
            PrimaryLanguage = repository.Language,
            AnalyzedAt = DateTime.UtcNow
        };

        // Convert SearchableDocuments to FileAnalysis objects
        context.ImportantFiles = documents
            .Where(doc => !string.IsNullOrWhiteSpace(doc.Content))
            .OrderByDescending(doc => doc.SizeInBytes) // Prioritize larger files
            .Take(50) // Limit to most important files
            .Select(doc => new FileAnalysis
            {
                FilePath = doc.FilePath,
                FileType = doc.FileExtension,
                Language = doc.Language,
                LineCount = doc.LineCount,
                Content = doc.Content,
                KeyConcepts = ExtractKeyConceptsFromContent(doc.Content, doc.Language),
                Purpose = DetermineFunctionPurpose(doc.Content, doc.FilePath),
                ImportanceScore = CalculateFileImportanceScore(doc.FilePath, doc.Content)
            })
            .ToList();

        // Extract languages from indexed documents
        context.Languages = documents
            .Where(doc => !string.IsNullOrWhiteSpace(doc.Language))
            .Select(doc => doc.Language)
            .Distinct()
            .ToList();

        // Build project structure analysis from file paths
        context.Structure = BuildProjectStructureFromFilePaths(documents);

        // Extract dependencies from content (simplified)
        context.Dependencies = ExtractDependenciesFromContent(documents);

        // Set basic architectural patterns (simplified)
        context.Patterns = new ArchitecturalPatterns();

        // Enhanced content analysis properties (simplified from indexed content)
        context.Purpose = ExtractProjectPurposeFromIndexedContent(documents);
        context.ComponentMap = new ComponentRelationshipMap();
        context.ContentSummaries = CreateContentSummariesFromIndexedDocuments(documents);

        _logger.LogInformation("Built analysis context with {FileCount} files, {LanguageCount} languages for repository: {RepositoryId}", 
            context.ImportantFiles.Count, context.Languages.Count, repository.Id);

        return context;
    }

    /// <summary>
    /// Extract key concepts from file content using simple heuristics
    /// </summary>
    private List<string> ExtractKeyConceptsFromContent(string content, string language)
    {
        var concepts = new List<string>();
        
        if (string.IsNullOrWhiteSpace(content)) return concepts;

        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines.Take(20)) // Only check first 20 lines
        {
            var trimmedLine = line.Trim();
            
            // Language-specific concept extraction
            if (language.Equals("csharp", StringComparison.OrdinalIgnoreCase))
            {
                if (trimmedLine.StartsWith("public class ") || trimmedLine.StartsWith("class "))
                    concepts.Add("Class Definition");
                if (trimmedLine.StartsWith("public interface ") || trimmedLine.StartsWith("interface "))
                    concepts.Add("Interface Definition");
                if (trimmedLine.Contains("async ") || trimmedLine.Contains("Task"))
                    concepts.Add("Async Programming");
            }
            else if (language.Equals("javascript", StringComparison.OrdinalIgnoreCase) || 
                     language.Equals("typescript", StringComparison.OrdinalIgnoreCase))
            {
                if (trimmedLine.StartsWith("function ") || trimmedLine.Contains(" => "))
                    concepts.Add("Function Definition");
                if (trimmedLine.Contains("async ") || trimmedLine.Contains("await "))
                    concepts.Add("Async Programming");
            }
        }

        return concepts.Distinct().ToList();
    }

    /// <summary>
    /// Determine file purpose from content and path
    /// </summary>
    private string DetermineFunctionPurpose(string content, string filePath)
    {
        if (string.IsNullOrWhiteSpace(content)) return "Configuration or data file";

        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        
        // Determine purpose from filename patterns
        if (fileName.Contains("test") || fileName.Contains("spec"))
            return "Test file";
        if (fileName.Contains("config") || fileName.Contains("setting"))
            return "Configuration file";
        if (fileName.Equals("readme.md") || fileName.Equals("readme.txt"))
            return "Documentation file";
        if (fileName.Contains("service") || fileName.Contains("repository"))
            return "Service layer implementation";
        if (fileName.Contains("controller") || fileName.Contains("api"))
            return "API layer implementation";
        if (fileName.Contains("model") || fileName.Contains("entity"))
            return "Data model definition";

        // Determine from content patterns
        if (content.Contains("class ") || content.Contains("interface "))
            return "Core application logic";
        if (content.Contains("function ") || content.Contains("def "))
            return "Utility functions";

        return "Application component";
    }

    /// <summary>
    /// Calculate file importance score based on path and content
    /// </summary>
    private double CalculateFileImportanceScore(string filePath, string content)
    {
        double score = 0.5; // Base score
        
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        var directory = Path.GetDirectoryName(filePath)?.ToLowerInvariant() ?? "";
        
        // Boost score for important files
        if (fileName.Equals("readme.md") || fileName.Equals("readme.txt")) score += 0.4;
        if (fileName.Contains("main") || fileName.Contains("index") || fileName.Contains("app")) score += 0.3;
        if (directory.Contains("src") || directory.Contains("lib")) score += 0.2;
        if (fileName.EndsWith(".cs") || fileName.EndsWith(".js") || fileName.EndsWith(".ts")) score += 0.1;
        
        // Reduce score for less important files
        if (fileName.Contains("test") || fileName.Contains("spec")) score -= 0.2;
        if (directory.Contains("node_modules") || directory.Contains("bin") || directory.Contains("obj")) score -= 0.4;
        
        // Content-based scoring
        if (!string.IsNullOrWhiteSpace(content))
        {
            var lineCount = content.Split('\n').Length;
            if (lineCount > 100) score += 0.1;
            if (lineCount > 500) score += 0.1;
        }
        
        return Math.Max(0.0, Math.Min(1.0, score)); // Clamp between 0 and 1
    }

    /// <summary>
    /// Build project structure analysis from file paths
    /// </summary>
    private ProjectStructureAnalysis BuildProjectStructureFromFilePaths(List<SearchableDocument> documents)
    {
        var structure = new ProjectStructureAnalysis();
        
        var allPaths = documents.Select(d => d.FilePath).ToList();
        var extensions = documents.Select(d => d.FileExtension).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList();
        var languages = documents.Select(d => d.Language).Where(l => !string.IsNullOrWhiteSpace(l)).Distinct().ToList();
        
        // Determine project type from file patterns
        if (allPaths.Any(p => p.Contains("package.json"))) structure.ProjectType = "Node.js Application";
        else if (extensions.Any(e => e.Equals(".csproj") || e.Equals(".sln"))) structure.ProjectType = ".NET Application";
        else if (extensions.Contains(".py") && allPaths.Any(p => p.Contains("requirements.txt"))) structure.ProjectType = "Python Application";
        else if (extensions.Contains(".java") && allPaths.Any(p => p.Contains("pom.xml"))) structure.ProjectType = "Java Application";
        else structure.ProjectType = "Application";
        
        // Extract frameworks from file contents (simplified)
        structure.Frameworks = ExtractFrameworksFromDocuments(documents);
        
        return structure;
    }

    /// <summary>
    /// Extract frameworks from document contents
    /// </summary>
    private List<string> ExtractFrameworksFromDocuments(List<SearchableDocument> documents)
    {
        var frameworks = new HashSet<string>();
        
        foreach (var doc in documents.Take(10)) // Check first 10 documents
        {
            if (string.IsNullOrWhiteSpace(doc.Content)) continue;
            
            var content = doc.Content.ToLowerInvariant();
            
            // .NET frameworks
            if (content.Contains("asp.net") || content.Contains("aspnetcore")) frameworks.Add("ASP.NET Core");
            if (content.Contains("entity framework") || content.Contains("entityframework")) frameworks.Add("Entity Framework");
            if (content.Contains("blazor")) frameworks.Add("Blazor");
            
            // JavaScript frameworks
            if (content.Contains("react")) frameworks.Add("React");
            if (content.Contains("angular")) frameworks.Add("Angular");
            if (content.Contains("vue")) frameworks.Add("Vue.js");
            if (content.Contains("express")) frameworks.Add("Express.js");
            if (content.Contains("next.js") || content.Contains("nextjs")) frameworks.Add("Next.js");
            
            // Python frameworks
            if (content.Contains("django")) frameworks.Add("Django");
            if (content.Contains("flask")) frameworks.Add("Flask");
            if (content.Contains("fastapi")) frameworks.Add("FastAPI");
        }
        
        return frameworks.ToList();
    }

    /// <summary>
    /// Extract dependencies from document contents (simplified)
    /// </summary>
    private List<DependencyInfo> ExtractDependenciesFromContent(List<SearchableDocument> documents)
    {
        var dependencies = new List<DependencyInfo>();
        
        // Look for package.json, requirements.txt, .csproj files
        var packageFiles = documents.Where(d => 
            d.FileName.Equals("package.json", StringComparison.OrdinalIgnoreCase) ||
            d.FileName.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase) ||
            d.FileExtension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
        ).ToList();
        
        foreach (var packageFile in packageFiles)
        {
            if (string.IsNullOrWhiteSpace(packageFile.Content)) continue;
            
            // Simple dependency extraction (could be enhanced)
            var lines = packageFile.Content.Split('\n');
            foreach (var line in lines.Take(50)) // Limit to prevent processing large files
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.Contains('"') && trimmedLine.Contains(':'))
                {
                    // Extract package name (simplified)
                    var packageName = trimmedLine.Split('"').FirstOrDefault(s => !string.IsNullOrWhiteSpace(s) && !s.Contains(':'));
                    if (!string.IsNullOrWhiteSpace(packageName) && packageName.Length < 50)
                    {
                        dependencies.Add(new DependencyInfo 
                        { 
                            Name = packageName, 
                            Version = "Unknown",
                            Type = "Package"
                        });
                    }
                }
            }
        }
        
        return dependencies.Take(20).ToList(); // Limit to prevent too many dependencies
    }

    /// <summary>
    /// Extract project purpose from indexed content (simplified)
    /// </summary>
    private ProjectPurpose ExtractProjectPurposeFromIndexedContent(List<SearchableDocument> documents)
    {
        var purpose = new ProjectPurpose();
        
        // Look for README file
        var readmeDoc = documents.FirstOrDefault(d => 
            d.FileName.Equals("readme.md", StringComparison.OrdinalIgnoreCase) ||
            d.FileName.Equals("readme.txt", StringComparison.OrdinalIgnoreCase));
        
        if (readmeDoc != null && !string.IsNullOrWhiteSpace(readmeDoc.Content))
        {
            // Extract first paragraph as description
            var lines = readmeDoc.Content.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l.Trim())).ToArray();
            if (lines.Any())
            {
                purpose.Description = lines.First().Trim().Replace("#", "").Trim();
            }
        }
        
        // Set default values if not found
        if (string.IsNullOrWhiteSpace(purpose.Description))
        {
            purpose.Description = "Software application with multiple components";
        }
        
        return purpose;
    }

    /// <summary>
    /// Create content summaries from indexed documents
    /// </summary>
    private List<ContentSummary> CreateContentSummariesFromIndexedDocuments(List<SearchableDocument> documents)
    {
        var summaries = new List<ContentSummary>();
        
        // Create summaries for important files
        var importantDocs = documents
            .Where(d => !string.IsNullOrWhiteSpace(d.Content) && d.SizeInBytes > 100)
            .OrderByDescending(d => d.SizeInBytes)
            .Take(10)
            .ToList();
        
        foreach (var doc in importantDocs)
        {
            var summary = new ContentSummary
            {
                FilePath = doc.FilePath,
                Language = doc.Language,
                FunctionalityDescription = GenerateSimpleSummary(doc.Content, doc.Language),
                LinesOfCode = doc.LineCount,
                ComplexityScore = CalculateFileImportanceScore(doc.FilePath, doc.Content),
                ContentType = DetermineContentType(doc.FileExtension),
                CodeSnippet = ExtractCodeSnippet(doc.Content)
            };
            
            // Add key concepts as key functions
            var keyConcepts = ExtractKeyConceptsFromContent(doc.Content, doc.Language);
            foreach (var concept in keyConcepts)
            {
                summary.AddKeyFunction(concept);
            }
            
            summaries.Add(summary);
        }
        
        return summaries;
    }

    /// <summary>
    /// Generate a simple summary from content
    /// </summary>
    private string GenerateSimpleSummary(string content, string language)
    {
        if (string.IsNullOrWhiteSpace(content)) return "Empty file";
        
        var lines = content.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l.Trim())).ToArray();
        var lineCount = lines.Length;
        
        var summary = $"{language} file with {lineCount} lines";
        
        // Add specific details based on content patterns
        if (content.Contains("class ")) summary += ", contains class definitions";
        if (content.Contains("function ") || content.Contains("def ")) summary += ", contains function definitions";
        if (content.Contains("import ") || content.Contains("using ")) summary += ", includes dependencies";
        
        return summary;
    }

    /// <summary>
    /// Determine content type from file extension
    /// </summary>
    private ContentType DetermineContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".cs" or ".js" or ".ts" or ".py" or ".java" or ".cpp" or ".c" or ".go" or ".rs" => ContentType.SourceCode,
            ".json" or ".xml" or ".yml" or ".yaml" or ".config" or ".ini" => ContentType.Configuration,
            ".md" or ".txt" or ".rst" => ContentType.Documentation,
            ".test.js" or ".test.ts" or ".spec.js" or ".spec.ts" => ContentType.Test,
            ".html" or ".htm" => ContentType.Markup,
            ".css" or ".scss" or ".less" => ContentType.Style,
            ".sql" or ".csv" => ContentType.Data,
            ".sh" or ".ps1" or ".bat" => ContentType.Script,
            _ => ContentType.SourceCode
        };
    }

    /// <summary>
    /// Extract a representative code snippet from content
    /// </summary>
    private string ExtractCodeSnippet(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return "";
        
        var lines = content.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l.Trim())).ToArray();
        if (!lines.Any()) return "";
        
        // Find the most interesting lines (class definitions, functions, etc.)
        var interestingLines = lines.Where(l => 
            l.Trim().StartsWith("class ") ||
            l.Trim().StartsWith("public class ") ||
            l.Trim().StartsWith("function ") ||
            l.Trim().StartsWith("public ") ||
            l.Trim().StartsWith("private ") ||
            l.Trim().StartsWith("def ") ||
            l.Trim().StartsWith("interface ") ||
            l.Trim().StartsWith("export ")
        ).Take(3).ToArray();
        
        if (interestingLines.Any())
        {
            return string.Join("\n", interestingLines);
        }
        
        // Fall back to first few non-empty lines
        return string.Join("\n", lines.Take(3));
    }

    #endregion

    public void Dispose()
    {
        _rateLimitSemaphore?.Dispose();
    }
}