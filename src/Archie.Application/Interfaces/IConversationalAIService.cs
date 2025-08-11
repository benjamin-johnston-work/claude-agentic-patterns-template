using Archie.Domain.Entities;
using Archie.Domain.ValueObjects;

namespace Archie.Application.Interfaces;

/// <summary>
/// Service for AI-powered conversational query processing using Azure OpenAI
/// </summary>
public interface IConversationalAIService
{
    /// <summary>
    /// Process a conversational query and generate an AI response
    /// </summary>
    /// <param name="query">User query text</param>
    /// <param name="context">Conversation context including repositories and preferences</param>
    /// <param name="messageHistory">Previous conversation messages for context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>AI-generated response with confidence and attachments</returns>
    Task<QueryResponse> ProcessQueryAsync(
        string query,
        ConversationContext context,
        IReadOnlyList<ConversationMessage> messageHistory,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analyze query intent to determine the type of question being asked
    /// </summary>
    /// <param name="query">User query text</param>
    /// <param name="context">Optional conversation context for better intent recognition</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query intent with confidence score and extracted entities</returns>
    Task<QueryIntent> AnalyzeQueryIntentAsync(
        string query,
        ConversationContext? context = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate follow-up questions based on the current query and response
    /// </summary>
    /// <param name="query">Original user query</param>
    /// <param name="response">AI-generated response</param>
    /// <param name="context">Conversation context</param>
    /// <param name="count">Maximum number of follow-up questions to generate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant follow-up questions</returns>
    Task<IReadOnlyList<string>> GenerateFollowUpQuestionsAsync(
        string query,
        string response,
        ConversationContext context,
        int count = 3,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate suggested questions for a repository to help users get started
    /// </summary>
    /// <param name="repositoryId">Repository identifier</param>
    /// <param name="domain">Optional domain to focus suggestions on</param>
    /// <param name="count">Maximum number of questions to generate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of suggested questions for the repository</returns>
    Task<IReadOnlyList<string>> SuggestQuestionsForRepositoryAsync(
        Guid repositoryId,
        string? domain = null,
        int count = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract key entities and topics from a query for better context understanding
    /// </summary>
    /// <param name="query">User query text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of extracted entities and technical terms</returns>
    Task<IReadOnlyList<string>> ExtractEntitiesAsync(
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Summarize conversation history for context management
    /// </summary>
    /// <param name="messages">List of conversation messages to summarize</param>
    /// <param name="maxSummaryLength">Maximum length of the summary</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Concise summary of the conversation</returns>
    Task<string> SummarizeConversationAsync(
        IReadOnlyList<ConversationMessage> messages,
        int maxSummaryLength = 500,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Response model for AI query processing
/// </summary>
public class QueryResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Query { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public double ResponseTimeSeconds { get; set; }
    public IReadOnlyList<MessageAttachment> Attachments { get; set; } = Array.Empty<MessageAttachment>();
    public IReadOnlyList<string> SuggestedFollowUps { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> RelatedQueries { get; set; } = Array.Empty<string>();
    public QueryIntent? Intent { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}