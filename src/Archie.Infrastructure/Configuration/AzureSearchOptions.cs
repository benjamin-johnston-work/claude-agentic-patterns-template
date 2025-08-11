using System.ComponentModel.DataAnnotations;

namespace Archie.Infrastructure.Configuration;

public class AzureSearchOptions
{
    public const string SectionName = "AzureSearch";
    
    [Required]
    public string ServiceName { get; set; } = "ract-archie-search-dev";
    
    [Required]
    public string ServiceUrl { get; set; } = "https://ract-archie-search-dev.search.windows.net";
    
    [Required]
    public string AdminKey { get; set; } = string.Empty;
    
    public string QueryKey { get; set; } = string.Empty;
    
    [Required]
    public string IndexName { get; set; } = "archie-repository-documents-v1";
    
    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 120;
    
    [Range(1, 1000)]
    public int MaxBatchSize { get; set; } = 100;
    
    [Range(1, 10)]
    public int RetryAttempts { get; set; } = 3;
    
    public bool EnableDetailedLogging { get; set; } = false;
}