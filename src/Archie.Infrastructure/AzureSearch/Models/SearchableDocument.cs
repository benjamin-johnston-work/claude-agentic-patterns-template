namespace Archie.Infrastructure.AzureSearch.Models;

public class SearchableDocument
{
    public string Id { get; private set; } = string.Empty;
    public Guid RepositoryId { get; private set; }
    public string FilePath { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string FileExtension { get; private set; } = string.Empty;
    public string Language { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public float[] ContentVector { get; private set; } = Array.Empty<float>();
    public int LineCount { get; private set; }
    public long SizeInBytes { get; private set; }
    public DateTime LastModified { get; private set; }
    public string BranchName { get; private set; } = string.Empty;
    public string DocumentType { get; private set; } = "File";
    public DocumentMetadata Metadata { get; private set; } = new();

    public static SearchableDocument Create(
        Guid repositoryId,
        string filePath,
        string content,
        float[] contentVector,
        string branchName,
        DocumentMetadata metadata)
    {
        return Create(repositoryId, filePath, content, contentVector, branchName, "File", metadata);
    }
    
    public static SearchableDocument Create(
        Guid repositoryId,
        string filePath,
        string content,
        float[] contentVector,
        string branchName,
        string documentType,
        DocumentMetadata metadata)
    {
        var fileName = Path.GetFileName(filePath);
        var fileExtension = Path.GetExtension(filePath);
        var language = DetectLanguageFromExtension(fileExtension);
        
        return new SearchableDocument
        {
            Id = GenerateDocumentId(repositoryId, filePath),
            RepositoryId = repositoryId,
            FilePath = filePath,
            FileName = fileName,
            FileExtension = fileExtension,
            Language = language,
            Content = content,
            ContentVector = contentVector,
            LineCount = content.Split('\n').Length,
            SizeInBytes = System.Text.Encoding.UTF8.GetByteCount(content),
            LastModified = DateTime.UtcNow,
            BranchName = branchName,
            DocumentType = documentType,
            Metadata = metadata
        };
    }

    private static string GenerateDocumentId(Guid repositoryId, string filePath)
    {
        var combined = $"{repositoryId}:{filePath}";
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(combined))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string DetectLanguageFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".cs" => "csharp",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".py" => "python",
            ".java" => "java",
            ".cpp" or ".c" => "cpp",
            ".go" => "go",
            ".rs" => "rust",
            ".php" => "php",
            ".rb" => "ruby",
            ".swift" => "swift",
            ".kt" => "kotlin",
            ".scala" => "scala",
            ".html" => "html",
            ".css" => "css",
            ".sql" => "sql",
            ".md" => "markdown",
            ".json" => "json",
            ".xml" => "xml",
            ".yml" or ".yaml" => "yaml",
            ".sh" => "bash",
            ".ps1" => "powershell",
            _ => "text"
        };
    }
}

public class DocumentMetadata
{
    public string RepositoryName { get; set; } = string.Empty;
    public string RepositoryOwner { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public List<string> CodeSymbols { get; set; } = new();
    public Dictionary<string, string> CustomFields { get; set; } = new();
}