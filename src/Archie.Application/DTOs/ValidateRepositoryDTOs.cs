using System.ComponentModel.DataAnnotations;

namespace Archie.Application.DTOs;

public record ValidateRepositoryInput
{
    [Required(ErrorMessage = "Repository URL is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    public string Url { get; init; } = string.Empty;
    
    public string? AccessToken { get; init; }
}

public record ValidateRepositoryResult
{
    public bool IsValid { get; init; }
    
    public RepositoryInfo? Repository { get; init; }
    
    public string? ErrorMessage { get; init; }
}

public record RepositoryInfo
{
    public string Name { get; init; } = string.Empty;
    
    public string? Description { get; init; }
    
    public string Language { get; init; } = string.Empty;
    
    public bool IsPrivate { get; init; }
    
    public IEnumerable<string> Branches { get; init; } = Array.Empty<string>();
}