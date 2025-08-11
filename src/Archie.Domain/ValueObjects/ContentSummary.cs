namespace Archie.Domain.ValueObjects;

/// <summary>
/// Summarized content from file analysis
/// Contains key functionality extracted from source code
/// </summary>
public class ContentSummary
{
    public string FilePath { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string FunctionalityDescription { get; set; } = string.Empty;
    public List<string> KeyFunctions { get; set; } = new();
    public List<string> KeyVariables { get; set; } = new();
    public List<string> ImportedModules { get; set; } = new();
    public List<string> ExportedComponents { get; set; } = new();
    public string CodeSnippet { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public int LinesOfCode { get; set; }
    public double ComplexityScore { get; set; }

    public ContentSummary()
    {
    }

    public ContentSummary(string filePath, string language, string functionality)
    {
        FilePath = filePath;
        Language = language;
        FunctionalityDescription = functionality;
    }

    public void AddKeyFunction(string function)
    {
        if (!string.IsNullOrWhiteSpace(function) && !KeyFunctions.Contains(function))
        {
            KeyFunctions.Add(function);
        }
    }

    public void AddImportedModule(string module)
    {
        if (!string.IsNullOrWhiteSpace(module) && !ImportedModules.Contains(module))
        {
            ImportedModules.Add(module);
        }
    }

    public void AddExportedComponent(string component)
    {
        if (!string.IsNullOrWhiteSpace(component) && !ExportedComponents.Contains(component))
        {
            ExportedComponents.Add(component);
        }
    }

    public bool IsSignificant => !string.IsNullOrWhiteSpace(FunctionalityDescription) || 
                                KeyFunctions.Any() || 
                                ExportedComponents.Any() ||
                                LinesOfCode > 10;

    public string GetBriefDescription()
    {
        if (!string.IsNullOrWhiteSpace(FunctionalityDescription))
            return FunctionalityDescription;

        if (KeyFunctions.Any())
            return $"Contains {string.Join(", ", KeyFunctions.Take(2))}";

        if (ExportedComponents.Any())
            return $"Exports {string.Join(", ", ExportedComponents.Take(2))}";

        return $"{Language} file with {LinesOfCode} lines";
    }
}

public enum ContentType
{
    SourceCode,
    Configuration,
    Documentation,
    Test,
    Asset,
    Markup,
    Style,
    Data,
    Script
}