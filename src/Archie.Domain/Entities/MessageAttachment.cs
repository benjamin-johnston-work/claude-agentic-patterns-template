using Archie.Domain.Common;

namespace Archie.Domain.Entities;

public class MessageAttachment : BaseEntity
{
    private readonly Dictionary<string, object> _properties = new();

    public Guid MessageId { get; private set; }
    public AttachmentType Type { get; private set; }
    public string Content { get; private set; }
    public string Title { get; private set; }
    public IReadOnlyDictionary<string, object> Properties => _properties.AsReadOnly();
    public DateTime CreatedAt { get; private set; }

    protected MessageAttachment() // EF Constructor
    {
        Content = string.Empty;
        Title = string.Empty;
    }

    private MessageAttachment(Guid messageId, AttachmentType type, string content, string title)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentException("Message ID cannot be empty", nameof(messageId));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));
        
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        MessageId = messageId;
        Type = type;
        Content = content;
        Title = title;
        CreatedAt = DateTime.UtcNow;
    }

    public static MessageAttachment CreateCodeReference(string filePath, string code, int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be null or empty", nameof(code));

        var attachment = new MessageAttachment(Guid.Empty, AttachmentType.CodeReference, code, $"Code from {Path.GetFileName(filePath)}");
        attachment.SetProperty("FilePath", filePath);
        attachment.SetProperty("LineNumber", lineNumber);
        attachment.SetProperty("Language", DetermineLanguageFromFilePath(filePath));
        
        return attachment;
    }

    public static MessageAttachment CreateDocumentationReference(string title, string content, string? url = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));

        var attachment = new MessageAttachment(Guid.Empty, AttachmentType.DocumentationReference, content, title);
        
        if (!string.IsNullOrWhiteSpace(url))
        {
            attachment.SetProperty("Url", url);
        }
        
        return attachment;
    }

    public static MessageAttachment CreateSearchResult(string query, List<object> results)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or empty", nameof(query));
        
        if (results == null)
            throw new ArgumentNullException(nameof(results));

        var content = $"Found {results.Count} result(s)";
        var attachment = new MessageAttachment(Guid.Empty, AttachmentType.SearchResult, content, $"Search: {query}");
        attachment.SetProperty("Query", query);
        attachment.SetProperty("ResultCount", results.Count);
        attachment.SetProperty("Results", results);
        
        return attachment;
    }

    public static MessageAttachment CreateDiagramReference(string title, string diagramData, string diagramType = "mermaid")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(diagramData))
            throw new ArgumentException("Diagram data cannot be null or empty", nameof(diagramData));

        var attachment = new MessageAttachment(Guid.Empty, AttachmentType.DiagramReference, diagramData, title);
        attachment.SetProperty("DiagramType", diagramType);
        
        return attachment;
    }

    public static MessageAttachment CreateFileReference(string filePath, string? content = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        var fileName = Path.GetFileName(filePath);
        var fileContent = content ?? $"Reference to file: {fileName}";
        
        var attachment = new MessageAttachment(Guid.Empty, AttachmentType.FileReference, fileContent, fileName);
        attachment.SetProperty("FilePath", filePath);
        attachment.SetProperty("FileSize", content?.Length ?? 0);
        attachment.SetProperty("FileExtension", Path.GetExtension(filePath));
        
        return attachment;
    }

    public void SetMessageId(Guid messageId)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentException("Message ID cannot be empty", nameof(messageId));
        
        MessageId = messageId;
    }

    public void SetProperty(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Property key cannot be null or empty", nameof(key));

        _properties[key] = value;
    }

    public T? GetProperty<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !_properties.ContainsKey(key))
            return default;

        try
        {
            return (T)_properties[key];
        }
        catch
        {
            return default;
        }
    }

    public bool HasProperty(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && _properties.ContainsKey(key);
    }

    public string GetDisplayText()
    {
        return Type switch
        {
            AttachmentType.CodeReference => $"Code from {GetProperty<string>("FilePath") ?? "unknown file"}",
            AttachmentType.DocumentationReference => Title,
            AttachmentType.SearchResult => $"Search results for '{GetProperty<string>("Query")}'",
            AttachmentType.DiagramReference => $"Diagram: {Title}",
            AttachmentType.FileReference => $"File: {GetProperty<string>("FilePath") ?? Title}",
            _ => Title
        };
    }

    public string GetSummary()
    {
        return Type switch
        {
            AttachmentType.CodeReference => $"Code reference at line {GetProperty<int>("LineNumber")}",
            AttachmentType.DocumentationReference => $"Documentation: {Title}",
            AttachmentType.SearchResult => $"{GetProperty<int>("ResultCount")} search results",
            AttachmentType.DiagramReference => $"{GetProperty<string>("DiagramType")} diagram",
            AttachmentType.FileReference => $"File reference: {Path.GetFileName(GetProperty<string>("FilePath") ?? Title)}",
            _ => Title
        };
    }

    private static string DetermineLanguageFromFilePath(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return extension switch
        {
            ".cs" => "csharp",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".py" => "python",
            ".java" => "java",
            ".cpp" or ".cc" or ".cxx" => "cpp",
            ".c" => "c",
            ".h" => "c",
            ".hpp" => "cpp",
            ".go" => "go",
            ".rs" => "rust",
            ".rb" => "ruby",
            ".php" => "php",
            ".swift" => "swift",
            ".kt" => "kotlin",
            ".scala" => "scala",
            ".clj" => "clojure",
            ".fs" => "fsharp",
            ".vb" => "vbnet",
            ".sql" => "sql",
            ".html" => "html",
            ".css" => "css",
            ".scss" => "scss",
            ".less" => "less",
            ".xml" => "xml",
            ".json" => "json",
            ".yaml" or ".yml" => "yaml",
            ".toml" => "toml",
            ".md" => "markdown",
            ".sh" => "bash",
            ".ps1" => "powershell",
            ".dockerfile" => "dockerfile",
            _ => "text"
        };
    }
}

public enum AttachmentType
{
    CodeReference,
    DocumentationReference,
    SearchResult,
    DiagramReference,
    FileReference
}