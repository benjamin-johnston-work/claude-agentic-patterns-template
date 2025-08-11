namespace Archie.Domain.ValueObjects;

public class CodeReference
{
    public string FilePath { get; private set; }
    public int? StartLine { get; private set; }
    public int? EndLine { get; private set; }
    public string CodeSnippet { get; private set; }
    public string Description { get; private set; }
    public string ReferenceType { get; private set; } // Class, Method, Interface, etc.

    protected CodeReference() // EF Constructor
    {
        FilePath = string.Empty;
        CodeSnippet = string.Empty;
        Description = string.Empty;
        ReferenceType = string.Empty;
    }

    public CodeReference(
        string filePath, 
        string codeSnippet, 
        string description, 
        string referenceType,
        int? startLine = null, 
        int? endLine = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        
        if (string.IsNullOrWhiteSpace(codeSnippet))
            throw new ArgumentException("Code snippet cannot be null or empty", nameof(codeSnippet));
        
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));
        
        if (string.IsNullOrWhiteSpace(referenceType))
            throw new ArgumentException("Reference type cannot be null or empty", nameof(referenceType));

        if (startLine.HasValue && startLine.Value < 1)
            throw new ArgumentOutOfRangeException(nameof(startLine), "Start line must be greater than 0");
        
        if (endLine.HasValue && endLine.Value < 1)
            throw new ArgumentOutOfRangeException(nameof(endLine), "End line must be greater than 0");
        
        if (startLine.HasValue && endLine.HasValue && endLine.Value < startLine.Value)
            throw new ArgumentException("End line cannot be before start line");

        FilePath = filePath;
        StartLine = startLine;
        EndLine = endLine;
        CodeSnippet = codeSnippet;
        Description = description;
        ReferenceType = referenceType;
    }

    public bool HasLineNumbers => StartLine.HasValue;
    
    public bool HasRange => StartLine.HasValue && EndLine.HasValue;

    public string GetDisplayLocation()
    {
        if (HasRange)
            return $"{FilePath}:{StartLine}-{EndLine}";
        if (HasLineNumbers)
            return $"{FilePath}:{StartLine}";
        return FilePath;
    }
}