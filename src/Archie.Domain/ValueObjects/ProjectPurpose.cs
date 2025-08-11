namespace Archie.Domain.ValueObjects;

/// <summary>
/// Represents the core purpose and business value of a software project
/// Extracted from README, documentation, and source code analysis
/// </summary>
public class ProjectPurpose
{
    public string Description { get; set; } = string.Empty;
    public List<string> KeyFeatures { get; set; } = new();
    public string BusinessDomain { get; set; } = string.Empty;
    public string UserValue { get; set; } = string.Empty;
    public string PrimaryUseCase { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public Dictionary<string, string> TechnicalHighlights { get; set; } = new();

    public ProjectPurpose()
    {
    }

    public ProjectPurpose(
        string description, 
        string businessDomain, 
        string userValue,
        string primaryUseCase = "",
        string targetAudience = "")
    {
        Description = description;
        BusinessDomain = businessDomain;
        UserValue = userValue;
        PrimaryUseCase = primaryUseCase;
        TargetAudience = targetAudience;
    }

    public void AddFeature(string feature)
    {
        if (!string.IsNullOrWhiteSpace(feature) && !KeyFeatures.Contains(feature))
        {
            KeyFeatures.Add(feature);
        }
    }

    public void AddTechnicalHighlight(string aspect, string description)
    {
        if (!string.IsNullOrWhiteSpace(aspect) && !string.IsNullOrWhiteSpace(description))
        {
            TechnicalHighlights[aspect] = description;
        }
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Description) && 
                          string.IsNullOrWhiteSpace(BusinessDomain) && 
                          !KeyFeatures.Any();

    public string GetSummary()
    {
        if (IsEmpty) return "Purpose not determined from repository analysis";
        
        var features = KeyFeatures.Any() 
            ? $" Key features: {string.Join(", ", KeyFeatures.Take(3))}" 
            : "";
            
        return $"{Description}{features}";
    }
}