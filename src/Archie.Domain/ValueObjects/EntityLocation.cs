namespace Archie.Domain.ValueObjects;

public record EntityLocation
{
    public int StartLine { get; init; }
    public int EndLine { get; init; }
    public int StartColumn { get; init; }
    public int EndColumn { get; init; }

    public static EntityLocation Create(int startLine, int endLine, int startColumn = 0, int endColumn = 0)
    {
        if (startLine < 0)
            throw new ArgumentException("Start line must be non-negative", nameof(startLine));
        if (endLine < startLine)
            throw new ArgumentException("End line must be greater than or equal to start line", nameof(endLine));
        if (startColumn < 0)
            throw new ArgumentException("Start column must be non-negative", nameof(startColumn));
        if (endColumn < 0)
            throw new ArgumentException("End column must be non-negative", nameof(endColumn));

        return new EntityLocation
        {
            StartLine = startLine,
            EndLine = endLine,
            StartColumn = startColumn,
            EndColumn = endColumn
        };
    }

    public static EntityLocation Unknown => new()
    {
        StartLine = 0,
        EndLine = 0,
        StartColumn = 0,
        EndColumn = 0
    };

    public bool IsValid => StartLine > 0 && EndLine >= StartLine;

    public int LineCount => Math.Max(0, EndLine - StartLine + 1);
}