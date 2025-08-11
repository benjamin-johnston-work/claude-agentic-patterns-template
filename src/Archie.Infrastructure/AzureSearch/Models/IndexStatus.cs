namespace Archie.Infrastructure.AzureSearch.Models;

public class IndexStatus
{
    public IndexingStatus Status { get; set; } = IndexingStatus.NOT_STARTED;
    public int DocumentsIndexed { get; set; }
    public int TotalDocuments { get; set; }
    public DateTime? LastIndexed { get; set; }
    public DateTime? EstimatedCompletion { get; set; }
    public string? ErrorMessage { get; set; }
    public double ProgressPercentage => TotalDocuments > 0 ? (double)DocumentsIndexed / TotalDocuments * 100 : 0;
}

public enum IndexingStatus
{
    NOT_STARTED,
    IN_PROGRESS,
    COMPLETED,
    ERROR,
    REFRESHING
}