using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Archie.Application.Interfaces;
using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;
using Archie.Infrastructure.Configuration;

namespace Archie.Infrastructure.Services;

/// <summary>
/// Azure OpenAI service implementation for conversational query processing using GPT models.
/// Handles intent analysis, response generation, and follow-up suggestions.
/// </summary>
public class ConversationalAIService : IConversationalAIService
{
    private readonly AzureOpenAIClient _openAIClient;
    private readonly ConversationalAIOptions _options;
    private readonly ILogger<ConversationalAIService> _logger;
    private readonly SemaphoreSlim _rateLimitSemaphore;

    public ConversationalAIService(
        IOptions<ConversationalAIOptions> options,
        ILogger<ConversationalAIService> logger)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var endpoint = new Uri(_options.AzureOpenAIEndpoint);
        var credential = new AzureKeyCredential(_options.AzureOpenAIApiKey);
        
        // Configure client options with timeout
        var clientOptions = new AzureOpenAIClientOptions();
        clientOptions.NetworkTimeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);
        
        _openAIClient = new AzureOpenAIClient(endpoint, credential, clientOptions);
        _rateLimitSemaphore = new SemaphoreSlim(5, 5); // Limit concurrent requests
    }

    public async Task<QueryResponse> ProcessQueryAsync(
        string query,
        ConversationContext context,
        IReadOnlyList<ConversationMessage> messageHistory,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty", nameof(query));

        var startTime = DateTime.UtcNow;

        try
        {
            await _rateLimitSemaphore.WaitAsync(cancellationToken);

            _logger.LogInformation("Processing conversational query with {MessageCount} history messages", 
                messageHistory.Count);

            // Build the conversation prompt
            var systemPrompt = BuildSystemPrompt(context);
            var conversationPrompt = BuildConversationPrompt(query, messageHistory, context);

            // Get chat completion client
            var chatClient = _openAIClient.GetChatClient(_options.GPTDeploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(conversationPrompt)
            };

            // Add recent conversation history
            AddConversationHistory(messages, messageHistory);

            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = _options.MaxTokensPerResponse,
                Temperature = (float)_options.Temperature,
                TopP = (float)_options.TopP,
                FrequencyPenalty = 0.1f,
                PresencePenalty = 0.1f
            };

            var response = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            var responseTime = (DateTime.UtcNow - startTime).TotalSeconds;

            var content = response.Value.Content[0].Text;
            var confidence = CalculateConfidence(content, query);

            _logger.LogInformation("Generated AI response in {ResponseTime:F2}s with confidence {Confidence:F2}", 
                responseTime, confidence);

            return new QueryResponse
            {
                Query = query,
                Response = content,
                Confidence = confidence,
                ResponseTimeSeconds = responseTime,
                Attachments = ExtractCodeReferences(content),
                Metadata = new Dictionary<string, object>
                {
                    ["model"] = _options.GPTDeploymentName,
                    ["temperature"] = _options.Temperature,
                    ["maxTokens"] = _options.MaxTokensPerResponse,
                    ["historyMessageCount"] = messageHistory.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing conversational query");
            
            return new QueryResponse
            {
                Query = query,
                Response = "I apologize, but I encountered an error processing your question. Please try rephrasing your query.",
                Confidence = 0.0,
                ResponseTimeSeconds = (DateTime.UtcNow - startTime).TotalSeconds,
                Attachments = Array.Empty<MessageAttachment>(),
                Metadata = new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["errorType"] = ex.GetType().Name
                }
            };
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    public async Task<QueryIntent> AnalyzeQueryIntentAsync(
        string query,
        ConversationContext? context = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return QueryIntent.Unknown;

        if (!_options.EnableIntentAnalysis)
        {
            return QueryIntent.Create(IntentType.Unknown, confidence: 0.0);
        }

        try
        {
            await _rateLimitSemaphore.WaitAsync(cancellationToken);

            _logger.LogDebug("Analyzing query intent for: {Query}", query);

            var intentPrompt = BuildIntentAnalysisPrompt(query, context);
            var chatClient = _openAIClient.GetChatClient(_options.GPTDeploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert at analyzing technical queries and determining their intent. Respond only in the specified JSON format."),
                new UserChatMessage(intentPrompt)
            };

            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 500,
                Temperature = 0.3f, // Lower temperature for more consistent intent analysis
                TopP = 0.9f
            };

            var response = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            var content = response.Value.Content[0].Text;

            return ParseIntentFromResponse(content, query);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing query intent, falling back to heuristic analysis");
            return AnalyzeIntentHeuristically(query);
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    public async Task<IReadOnlyList<string>> GenerateFollowUpQuestionsAsync(
        string query,
        string response,
        ConversationContext context,
        int count = 3,
        CancellationToken cancellationToken = default)
    {
        if (!_options.EnableFollowUpGeneration)
        {
            return Array.Empty<string>();
        }

        try
        {
            await _rateLimitSemaphore.WaitAsync(cancellationToken);

            var followUpPrompt = BuildFollowUpPrompt(query, response, context, count);
            var chatClient = _openAIClient.GetChatClient(_options.GPTDeploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant that generates relevant follow-up questions based on conversations about code repositories."),
                new UserChatMessage(followUpPrompt)
            };

            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 300,
                Temperature = 0.8f, // Higher temperature for more creative questions
                TopP = 0.95f
            };

            var followUpResponse = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            var content = followUpResponse.Value.Content[0].Text;

            return ParseFollowUpQuestions(content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating follow-up questions");
            return Array.Empty<string>();
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    public async Task<IReadOnlyList<string>> SuggestQuestionsForRepositoryAsync(
        Guid repositoryId,
        string? domain = null,
        int count = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _rateLimitSemaphore.WaitAsync(cancellationToken);

            var suggestionsPrompt = BuildSuggestionsPrompt(repositoryId, domain, count);
            var chatClient = _openAIClient.GetChatClient(_options.GPTDeploymentName);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant that suggests relevant questions developers might ask about code repositories."),
                new UserChatMessage(suggestionsPrompt)
            };

            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 400,
                Temperature = 0.9f,
                TopP = 0.95f
            };

            var response = await chatClient.CompleteChatAsync(messages, chatOptions, cancellationToken);
            var content = response.Value.Content[0].Text;

            return ParseSuggestedQuestions(content);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating repository question suggestions");
            return GetDefaultSuggestions(domain);
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    public async Task<IReadOnlyList<string>> ExtractEntitiesAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = new List<string>();
            
            // Simple regex-based entity extraction for now
            var patterns = new[]
            {
                @"\b(class|interface|method|function|variable|property)\s+(\w+)",
                @"\b(\w+)(Service|Controller|Repository|Manager|Factory|Builder)\b",
                @"\b(HTTP|REST|API|SQL|JSON|XML|JWT|OAuth)\b",
                @"\b(\w+\.cs|\w+\.js|\w+\.ts|\w+\.py|\w+\.java)\b"
            };

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    var entity = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                    if (!entities.Contains(entity, StringComparer.OrdinalIgnoreCase))
                    {
                        entities.Add(entity);
                    }
                }
            }

            return entities.Take(10).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting entities from query");
            return Array.Empty<string>();
        }
    }

    public async Task<string> SummarizeConversationAsync(
        IReadOnlyList<ConversationMessage> messages,
        int maxSummaryLength = 500,
        CancellationToken cancellationToken = default)
    {
        if (!messages.Any())
            return "Empty conversation";

        try
        {
            await _rateLimitSemaphore.WaitAsync(cancellationToken);

            var conversationText = BuildConversationText(messages);
            var summaryPrompt = $"Summarize this technical conversation in {maxSummaryLength} characters or less:\n\n{conversationText}";

            var chatClient = _openAIClient.GetChatClient(_options.GPTDeploymentName);

            var chatMessages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a helpful assistant that creates concise summaries of technical conversations."),
                new UserChatMessage(summaryPrompt)
            };

            var chatOptions = new ChatCompletionOptions
            {
                MaxOutputTokenCount = maxSummaryLength / 3, // Rough token estimate
                Temperature = 0.3f,
                TopP = 0.9f
            };

            var response = await chatClient.CompleteChatAsync(chatMessages, chatOptions, cancellationToken);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error summarizing conversation");
            return $"Conversation with {messages.Count} messages about technical topics";
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    private string BuildSystemPrompt(ConversationContext context)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("You are Archie, an AI assistant specialized in helping developers understand and work with code repositories.");
        prompt.AppendLine("You have access to repository content and can provide accurate, helpful responses about code architecture, implementation details, and best practices.");
        prompt.AppendLine();
        
        if (context.HasRepositories())
        {
            prompt.AppendLine($"You are currently analyzing {context.RepositoryIds.Count} repository(ies): {string.Join(", ", context.RepositoryNames)}");
        }

        if (context.HasDomain())
        {
            prompt.AppendLine($"Focus area: {context.Domain}");
        }

        var preferences = context.Preferences;
        prompt.AppendLine($"Response style: {preferences.ResponseStyle}");
        
        if (preferences.IncludeCodeExamples)
        {
            prompt.AppendLine("Include relevant code examples when helpful.");
        }

        if (preferences.IncludeReferences)
        {
            prompt.AppendLine("Include file references and line numbers when mentioning specific code.");
        }

        prompt.AppendLine($"Keep responses under {preferences.MaxResponseLength} characters.");
        prompt.AppendLine();
        prompt.AppendLine("Always be accurate, helpful, and provide actionable information.");

        return prompt.ToString();
    }

    private string BuildConversationPrompt(string query, IReadOnlyList<ConversationMessage> messageHistory, ConversationContext context)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine($"User question: {query}");
        
        if (context.HasTechnicalTags())
        {
            prompt.AppendLine($"Relevant tags: {string.Join(", ", context.TechnicalTags)}");
        }

        return prompt.ToString();
    }

    private void AddConversationHistory(List<ChatMessage> messages, IReadOnlyList<ConversationMessage> messageHistory)
    {
        var recentMessages = messageHistory.TakeLast(_options.MaxConversationHistory);
        
        foreach (var message in recentMessages)
        {
            if (message.Type == MessageType.UserQuery)
            {
                messages.Add(new UserChatMessage(message.Content));
            }
            else if (message.Type == MessageType.AIResponse)
            {
                messages.Add(new AssistantChatMessage(message.Content));
            }
        }
    }

    private double CalculateConfidence(string response, string query)
    {
        // Simple confidence calculation based on response characteristics
        var confidence = 0.5; // Base confidence
        
        if (response.Length > 50) confidence += 0.1;
        if (response.Contains("```")) confidence += 0.1; // Has code examples
        if (response.Split('.').Length > 3) confidence += 0.1; // Multiple sentences
        if (!response.Contains("I don't know") && !response.Contains("I'm not sure")) confidence += 0.2;
        
        return Math.Min(1.0, confidence);
    }

    private IReadOnlyList<MessageAttachment> ExtractCodeReferences(string content)
    {
        var attachments = new List<MessageAttachment>();
        
        // Extract code blocks
        var codeBlockPattern = @"```(?:(\w+)\s+)?([^`]+)```";
        var matches = Regex.Matches(content, codeBlockPattern, RegexOptions.Singleline);
        
        foreach (Match match in matches)
        {
            var language = match.Groups[1].Value;
            var code = match.Groups[2].Value.Trim();
            
            if (!string.IsNullOrWhiteSpace(code))
            {
                var attachment = MessageAttachment.CreateCodeReference(
                    $"example.{language ?? "txt"}", 
                    code, 
                    1);
                attachments.Add(attachment);
            }
        }
        
        return attachments.AsReadOnly();
    }

    private string BuildIntentAnalysisPrompt(string query, ConversationContext? context)
    {
        return $@"Analyze this technical query and determine its intent. Respond only with JSON in this exact format:
{{
    ""intent"": ""ExplainConcept|FindImplementation|CompareApproaches|Troubleshoot|ProvideExample|ArchitecturalQuery|CodeReview|Documentation|Testing|Unknown"",
    ""confidence"": 0.0-1.0,
    ""domain"": ""domain or topic"",
    ""entities"": [""entity1"", ""entity2""]
}}

Query: {query}";
    }

    private QueryIntent ParseIntentFromResponse(string response, string originalQuery)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            var intentString = root.GetProperty("intent").GetString() ?? "Unknown";
            var confidence = root.GetProperty("confidence").GetDouble();
            var domain = root.GetProperty("domain").GetString() ?? "";
            
            var entities = new List<string>();
            if (root.TryGetProperty("entities", out var entitiesArray))
            {
                foreach (var entity in entitiesArray.EnumerateArray())
                {
                    var entityValue = entity.GetString();
                    if (!string.IsNullOrWhiteSpace(entityValue))
                        entities.Add(entityValue);
                }
            }

            if (Enum.TryParse<IntentType>(intentString, out var intentType))
            {
                return QueryIntent.Create(intentType, domain, entities, confidence);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse intent response, falling back to heuristic analysis");
        }

        return AnalyzeIntentHeuristically(originalQuery);
    }

    private QueryIntent AnalyzeIntentHeuristically(string query)
    {
        var lowercaseQuery = query.ToLowerInvariant();
        
        if (lowercaseQuery.Contains("how does") || lowercaseQuery.Contains("what is") || lowercaseQuery.Contains("explain"))
            return QueryIntent.Create(IntentType.ExplainConcept, confidence: 0.7);
            
        if (lowercaseQuery.Contains("where is") || lowercaseQuery.Contains("find") || lowercaseQuery.Contains("locate"))
            return QueryIntent.Create(IntentType.FindImplementation, confidence: 0.7);
            
        if (lowercaseQuery.Contains("compare") || lowercaseQuery.Contains("difference") || lowercaseQuery.Contains("vs"))
            return QueryIntent.Create(IntentType.CompareApproaches, confidence: 0.7);
            
        if (lowercaseQuery.Contains("error") || lowercaseQuery.Contains("issue") || lowercaseQuery.Contains("problem"))
            return QueryIntent.Create(IntentType.Troubleshoot, confidence: 0.7);
            
        if (lowercaseQuery.Contains("example") || lowercaseQuery.Contains("show me"))
            return QueryIntent.Create(IntentType.ProvideExample, confidence: 0.7);
        
        return QueryIntent.Create(IntentType.Unknown, confidence: 0.3);
    }

    private string BuildFollowUpPrompt(string query, string response, ConversationContext context, int count)
    {
        return $@"Based on this conversation about a code repository, generate {count} relevant follow-up questions:

Original Question: {query}
Response: {response}

Generate {count} follow-up questions that would naturally come next in this conversation. Return only the questions, one per line.";
    }

    private IReadOnlyList<string> ParseFollowUpQuestions(string content)
    {
        var questions = content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(q => q.Trim().TrimStart('-', '*', '1', '2', '3', '4', '5', '.', ' '))
            .Where(q => !string.IsNullOrWhiteSpace(q) && q.EndsWith('?'))
            .Take(5)
            .ToList();

        return questions.AsReadOnly();
    }

    private string BuildSuggestionsPrompt(Guid repositoryId, string? domain, int count)
    {
        var domainPart = !string.IsNullOrWhiteSpace(domain) ? $" related to {domain}" : "";
        
        return $@"Generate {count} helpful questions that developers typically ask about code repositories{domainPart}.

Focus on practical questions about:
- Code structure and architecture
- Implementation details
- Best practices
- Testing approaches
- Debugging and troubleshooting

Return only the questions, one per line.";
    }

    private IReadOnlyList<string> ParseSuggestedQuestions(string content)
    {
        var questions = content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(q => q.Trim().TrimStart('-', '*', '1', '2', '3', '4', '5', '.', ' '))
            .Where(q => !string.IsNullOrWhiteSpace(q) && q.EndsWith('?'))
            .Take(10)
            .ToList();

        return questions.AsReadOnly();
    }

    private IReadOnlyList<string> GetDefaultSuggestions(string? domain)
    {
        var suggestions = domain?.ToLowerInvariant() switch
        {
            "testing" => new[]
            {
                "How should I write unit tests for this functionality?",
                "What testing frameworks are used in this project?",
                "How is test data set up and managed?",
                "What is the test coverage for critical components?"
            },
            "architecture" => new[]
            {
                "What design patterns are used in this codebase?",
                "How is dependency injection implemented?",
                "What is the overall system architecture?",
                "How are different layers separated?"
            },
            _ => new[]
            {
                "How does the authentication system work?",
                "What are the main components of this application?",
                "How is error handling implemented?",
                "What dependencies does this project have?",
                "How is the database access layer organized?"
            }
        };

        return suggestions.ToList().AsReadOnly();
    }

    private string BuildConversationText(IReadOnlyList<ConversationMessage> messages)
    {
        var text = new StringBuilder();
        
        foreach (var message in messages.Take(10)) // Limit to recent messages
        {
            var rolePrefix = message.Type switch
            {
                MessageType.UserQuery => "User: ",
                MessageType.AIResponse => "Assistant: ",
                MessageType.SystemMessage => "System: ",
                _ => ""
            };
            
            text.AppendLine($"{rolePrefix}{message.Content}");
        }
        
        return text.ToString();
    }
}