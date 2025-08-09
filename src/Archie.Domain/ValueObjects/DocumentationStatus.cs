namespace Archie.Domain.ValueObjects;

public enum DocumentationStatus
{
    NotStarted,
    Analyzing,
    GeneratingContent,
    Enriching,
    Indexing,
    Completed,
    Error,
    UpdateRequired
}