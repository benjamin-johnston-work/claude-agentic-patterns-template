namespace Archie.Domain.ValueObjects;

/// <summary>
/// Maps relationships and dependencies between code components
/// Helps understand project architecture and component interactions
/// </summary>
public class ComponentRelationshipMap
{
    public Dictionary<string, List<string>> Dependencies { get; set; } = new();
    public List<string> EntryPoints { get; set; } = new();
    public Dictionary<string, string> ComponentPurposes { get; set; } = new();
    public Dictionary<string, ComponentType> ComponentTypes { get; set; } = new();
    public List<string> CoreModules { get; set; } = new();
    
    public ComponentRelationshipMap()
    {
    }

    public void AddComponentPurpose(string component, string purpose, ComponentType type = ComponentType.Module)
    {
        if (!string.IsNullOrWhiteSpace(component) && !string.IsNullOrWhiteSpace(purpose))
        {
            ComponentPurposes[component] = purpose;
            ComponentTypes[component] = type;
        }
    }

    public void AddDependency(string component, string dependency)
    {
        if (!string.IsNullOrWhiteSpace(component) && !string.IsNullOrWhiteSpace(dependency))
        {
            if (!Dependencies.ContainsKey(component))
            {
                Dependencies[component] = new List<string>();
            }
            
            if (!Dependencies[component].Contains(dependency))
            {
                Dependencies[component].Add(dependency);
            }
        }
    }

    public void AddEntryPoint(string entryPoint)
    {
        if (!string.IsNullOrWhiteSpace(entryPoint) && !EntryPoints.Contains(entryPoint))
        {
            EntryPoints.Add(entryPoint);
        }
    }

    public void AddCoreModule(string module)
    {
        if (!string.IsNullOrWhiteSpace(module) && !CoreModules.Contains(module))
        {
            CoreModules.Add(module);
        }
    }

    public List<string> GetMostConnectedComponents(int count = 5)
    {
        return Dependencies
            .OrderByDescending(kvp => kvp.Value.Count)
            .Take(count)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    public bool HasArchitecture => EntryPoints.Any() || ComponentPurposes.Any() || Dependencies.Any();

    public string GetArchitectureSummary()
    {
        if (!HasArchitecture) return "Architecture not determined from analysis";

        var summary = new List<string>();
        
        if (EntryPoints.Any())
            summary.Add($"Entry points: {string.Join(", ", EntryPoints.Take(3))}");
            
        if (CoreModules.Any())
            summary.Add($"Core modules: {string.Join(", ", CoreModules.Take(3))}");
            
        if (ComponentPurposes.Any())
            summary.Add($"{ComponentPurposes.Count} components mapped");

        return string.Join(". ", summary);
    }
}

public enum ComponentType
{
    EntryPoint,
    Module,
    Utility,
    Configuration,
    Test,
    Documentation,
    Asset,
    Interface,
    Service,
    Controller,
    Model,
    View
}