using System.ComponentModel.DataAnnotations;

namespace Archie.Application.DTOs;

public record AddRepositoryInput
{
    [Required(ErrorMessage = "Repository URL is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    public string Url { get; init; } = string.Empty;
    
    public string? AccessToken { get; init; }
    
    public string? Branch { get; init; }
}

public record RepositoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public IEnumerable<BranchDto> Branches { get; init; } = Array.Empty<BranchDto>();
    public RepositoryStatisticsDto? Statistics { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record BranchDto
{
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public CommitDto? LastCommit { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CommitDto
{
    public string Hash { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

public record RepositoryStatisticsDto
{
    public int FileCount { get; init; }
    public int LineCount { get; init; }
    public IEnumerable<LanguageStatsDto> LanguageBreakdown { get; init; } = Array.Empty<LanguageStatsDto>();
}

public record LanguageStatsDto
{
    public string Language { get; init; } = string.Empty;
    public int FileCount { get; init; }
    public int LineCount { get; init; }
    public double Percentage { get; init; }
}