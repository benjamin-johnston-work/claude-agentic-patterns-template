using System.ComponentModel.DataAnnotations;

namespace Archie.Infrastructure.Configuration;

public class IndexingOptions  
{
    public const string SectionName = "Indexing";
    
    [Range(1, 100)]
    public int MaxConcurrentIndexingOperations { get; set; } = 5;
    
    [Range(1000, 100000)]
    public int MaxFileContentLength { get; set; } = 32768; // 32KB limit for embedding
    
    public List<string> IndexableFileExtensions { get; set; } = new()
    {
        ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".go", ".rs",
        ".php", ".rb", ".swift", ".kt", ".scala", ".html", ".css", ".sql",
        ".md", ".txt", ".json", ".xml", ".yml", ".yaml", ".sh", ".ps1"
    };
    
    public List<string> IgnoredDirectories { get; set; } = new()
    {
        ".git", ".vs", ".vscode", "node_modules", "bin", "obj", "packages", ".nuget", "target", "build"
    };
    
    [Range(3600, 86400)]
    public int IndexRefreshIntervalSeconds { get; set; } = 21600; // 6 hours
    
    public bool ExtractCodeSymbols { get; set; } = true;
    
    public bool EnableIncrementalIndexing { get; set; } = true;
}