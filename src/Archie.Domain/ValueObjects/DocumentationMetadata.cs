namespace Archie.Domain.ValueObjects;

public class DocumentationMetadata
{
    public string RepositoryName { get; private set; }
    public string RepositoryUrl { get; private set; }
    public string PrimaryLanguage { get; private set; }
    public List<string> Languages { get; private set; }
    public List<string> Frameworks { get; private set; }
    public List<string> Dependencies { get; private set; }
    public string ProjectType { get; private set; } // Library, Application, Framework
    public Dictionary<string, object> CustomProperties { get; private set; }

    protected DocumentationMetadata() // EF Constructor
    {
        RepositoryName = string.Empty;
        RepositoryUrl = string.Empty;
        PrimaryLanguage = string.Empty;
        Languages = new List<string>();
        Frameworks = new List<string>();
        Dependencies = new List<string>();
        ProjectType = string.Empty;
        CustomProperties = new Dictionary<string, object>();
    }

    public DocumentationMetadata(
        string repositoryName, 
        string repositoryUrl, 
        string primaryLanguage, 
        string projectType)
    {
        if (string.IsNullOrWhiteSpace(repositoryName))
            throw new ArgumentException("Repository name cannot be null or empty", nameof(repositoryName));
        
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            throw new ArgumentException("Repository URL cannot be null or empty", nameof(repositoryUrl));
        
        if (string.IsNullOrWhiteSpace(primaryLanguage))
            throw new ArgumentException("Primary language cannot be null or empty", nameof(primaryLanguage));
        
        if (string.IsNullOrWhiteSpace(projectType))
            throw new ArgumentException("Project type cannot be null or empty", nameof(projectType));

        RepositoryName = repositoryName;
        RepositoryUrl = repositoryUrl;
        PrimaryLanguage = primaryLanguage;
        ProjectType = projectType;
        Languages = new List<string> { primaryLanguage };
        Frameworks = new List<string>();
        Dependencies = new List<string>();
        CustomProperties = new Dictionary<string, object>();
    }

    public void AddLanguage(string language)
    {
        if (!string.IsNullOrWhiteSpace(language) && !Languages.Contains(language, StringComparer.OrdinalIgnoreCase))
        {
            Languages.Add(language);
        }
    }

    public void AddFramework(string framework)
    {
        if (!string.IsNullOrWhiteSpace(framework) && !Frameworks.Contains(framework, StringComparer.OrdinalIgnoreCase))
        {
            Frameworks.Add(framework);
        }
    }

    public void AddDependency(string dependency)
    {
        if (!string.IsNullOrWhiteSpace(dependency) && !Dependencies.Contains(dependency, StringComparer.OrdinalIgnoreCase))
        {
            Dependencies.Add(dependency);
        }
    }

    public void AddCustomProperty(string key, object value)
    {
        if (!string.IsNullOrWhiteSpace(key) && value != null)
        {
            CustomProperties[key] = value;
        }
    }
}