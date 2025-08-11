using System.ComponentModel.DataAnnotations;
using Archie.Domain.ValueObjects;

namespace Archie.Infrastructure.Configuration;

public class ConversationalAIOptions
{
    public const string SectionName = "ConversationalAI";
    
    [Required]
    public string AzureOpenAIEndpoint { get; set; } = "https://ract-ai-foundry-dev.openai.azure.com/";
    
    [Required]
    public string AzureOpenAIApiKey { get; set; } = string.Empty; // From Azure Key Vault
    
    [Required]
    public string GPTDeploymentName { get; set; } = "gpt-4";
    
    [Required]
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-ada-002";
    
    public string ApiVersion { get; set; } = "2024-02-01";
    
    [Range(100, 8000)]
    public int MaxTokensPerResponse { get; set; } = 3000;
    
    [Range(0.0, 2.0)]
    public double Temperature { get; set; } = 0.7; // Balanced creativity for conversations
    
    [Range(0.0, 2.0)]
    public double TopP { get; set; } = 0.95;
    
    [Range(1, 20)]
    public int MaxContextItems { get; set; } = 10;
    
    [Range(1, 50)]
    public int MaxConversationHistory { get; set; } = 20;
    
    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 60;
    
    [Range(1, 5)]
    public int RetryAttempts { get; set; } = 3;
    
    public bool EnableIntentAnalysis { get; set; } = true;
    public bool EnableFollowUpGeneration { get; set; } = true;
    public bool EnableCodeReferenceExtraction { get; set; } = true;
    
    public Dictionary<IntentType, string> IntentPromptTemplates { get; set; } = new()
    {
        { IntentType.ExplainConcept, "Explain the concept of {concept} in the context of {domain}. Provide clear examples from the codebase." },
        { IntentType.FindImplementation, "Find and explain how {functionality} is implemented in the codebase. Include relevant code references." },
        { IntentType.CompareApproaches, "Compare different approaches to {topic} found in the codebase. Highlight pros and cons of each." },
        { IntentType.Troubleshoot, "Help troubleshoot the issue: {problem}. Analyze potential causes and suggest solutions based on the codebase." },
        { IntentType.ProvideExample, "Provide a practical example of {concept} using code from this repository." },
        { IntentType.ArchitecturalQuery, "Analyze the architectural aspects of {topic} in this codebase. Explain design patterns and principles used." },
        { IntentType.CodeReview, "Review the code related to {functionality}. Provide feedback on quality, best practices, and potential improvements." },
        { IntentType.Documentation, "Generate documentation for {functionality} based on the code implementation and usage patterns." },
        { IntentType.Testing, "Provide testing guidance for {functionality}. Include test strategies, examples, and best practices." }
    };

    public ConversationDefaults Defaults { get; set; } = new();
}

public class ConversationDefaults
{
    public ResponseStyle DefaultResponseStyle { get; set; } = ResponseStyle.Balanced;
    public bool DefaultIncludeCodeExamples { get; set; } = true;
    public bool DefaultIncludeReferences { get; set; } = true;
    public int DefaultMaxResponseLength { get; set; } = 2000;
    public int DefaultFollowUpCount { get; set; } = 3;
    public int DefaultSuggestedQuestionsCount { get; set; } = 5;
    public double MinConfidenceThreshold { get; set; } = 0.3;
    public int MaxContextTokens { get; set; } = 8000;
    public int MaxPromptTokens { get; set; } = 12000;
}