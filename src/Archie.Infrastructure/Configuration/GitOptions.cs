using System.ComponentModel.DataAnnotations;

namespace Archie.Infrastructure.Configuration;

public class GitOptions
{
    public const string SectionName = "Git";

    public int TimeoutSeconds { get; set; } = 30;
    public int MaxConcurrentRequests { get; set; } = 5;
    public int RateLimitDelaySeconds { get; set; } = 60;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
    public string UserAgent { get; set; } = "Archie-Documentation-Platform/1.0";
}

public class GitHubOptions
{
    public const string SectionName = "GitHub";

    public string? DefaultAccessToken { get; set; }
    public int ApiTimeoutSeconds { get; set; } = 30;
    public int RateLimitBuffer { get; set; } = 100; // Reserve buffer for rate limit
    public bool EnableRateLimitProtection { get; set; } = true;
    public string UserAgent { get; set; } = "Archie-Documentation-Platform/1.0";
    public int MaxTreeDepthForRecursion { get; set; } = 10;
}

