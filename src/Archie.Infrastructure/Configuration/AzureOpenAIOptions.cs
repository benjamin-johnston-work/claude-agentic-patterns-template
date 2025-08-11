using System.ComponentModel.DataAnnotations;

namespace Archie.Infrastructure.Configuration;

public class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";
    
    [Required]
    public string Endpoint { get; set; } = "https://ract-ai-foundry-dev.openai.azure.com/";
    
    [Required]
    public string ApiKey { get; set; } = string.Empty;
    
    [Required]
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-ada-002";
    
    public string ApiVersion { get; set; } = "2024-02-01";
    
    [Range(1, 16)]
    public int MaxBatchSize { get; set; } = 8;
    
    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 60;
    
    [Range(1, 10)]
    public int RetryAttempts { get; set; } = 3;
    
    public bool EnableRateLimitProtection { get; set; } = true;
}