using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Entities;

public class DocumentationSection : BaseEntity
{
    private readonly List<CodeReference> _codeReferences = new();
    private readonly List<string> _tags = new();

    public string Title { get; private set; }
    public string Content { get; private set; }
    public DocumentationSectionType Type { get; private set; }
    public int Order { get; private set; }
    public IReadOnlyList<CodeReference> CodeReferences => _codeReferences.AsReadOnly();
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public SectionMetadata Metadata { get; private set; }

    protected DocumentationSection() // EF Constructor
    {
        Title = string.Empty;
        Content = string.Empty;
        Metadata = new SectionMetadata();
    }

    public DocumentationSection(
        string title, 
        string content, 
        DocumentationSectionType type, 
        int order,
        SectionMetadata? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Section title cannot be null or empty", nameof(title));
        
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Section content cannot be null or empty", nameof(content));
        
        if (order < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Section order cannot be negative");

        Title = title;
        Content = content;
        Type = type;
        Order = order;
        Metadata = metadata ?? new SectionMetadata();
    }

    public static DocumentationSection Create(
        string title, 
        string content, 
        DocumentationSectionType type, 
        int order)
    {
        return new DocumentationSection(title, content, type, order);
    }

    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));

        Content = content;
        Metadata.UpdateModifiedTime();
    }

    public void UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        Title = title;
        Metadata.UpdateModifiedTime();
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentOutOfRangeException(nameof(order), "Order cannot be negative");

        Order = order;
        Metadata.UpdateModifiedTime();
    }

    public void AddCodeReference(string filePath, string codeSnippet, string description, string referenceType, int? startLine = null, int? endLine = null)
    {
        var codeRef = new CodeReference(filePath, codeSnippet, description, referenceType, startLine, endLine);
        
        // Check if a similar code reference already exists
        if (_codeReferences.Any(cr => cr.FilePath == filePath && 
                                     cr.StartLine == startLine && 
                                     cr.EndLine == endLine))
        {
            return; // Don't add duplicate references
        }

        _codeReferences.Add(codeRef);
        Metadata.UpdateModifiedTime();
    }

    public void RemoveCodeReference(string filePath, int? startLine = null)
    {
        var referencesToRemove = _codeReferences
            .Where(cr => cr.FilePath == filePath && 
                        (startLine == null || cr.StartLine == startLine))
            .ToList();

        foreach (var reference in referencesToRemove)
        {
            _codeReferences.Remove(reference);
        }

        if (referencesToRemove.Any())
        {
            Metadata.UpdateModifiedTime();
        }
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        var normalizedTag = tag.ToLowerInvariant();
        if (!_tags.Contains(normalizedTag))
        {
            _tags.Add(normalizedTag);
            Metadata.UpdateModifiedTime();
        }
    }

    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        var normalizedTag = tag.ToLowerInvariant();
        if (_tags.Remove(normalizedTag))
        {
            Metadata.UpdateModifiedTime();
        }
    }

    public void ClearTags()
    {
        if (_tags.Any())
        {
            _tags.Clear();
            Metadata.UpdateModifiedTime();
        }
    }

    public bool HasTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        return _tags.Contains(tag.ToLowerInvariant());
    }

    public bool HasCodeReferences => _codeReferences.Any();

    public int GetWordCount()
    {
        if (string.IsNullOrWhiteSpace(Content))
            return 0;

        return Content.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}