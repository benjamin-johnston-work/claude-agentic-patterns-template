using Archie.Domain.Common;
using Archie.Domain.ValueObjects;

namespace Archie.Domain.Entities;

public class KnowledgeGraph : BaseEntity, IAggregateRoot
{
    private readonly List<Guid> _repositoryIds = new();

    public List<Guid> RepositoryIds => _repositoryIds;
    public GraphStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public GraphStatistics Statistics { get; private set; }
    public GraphMetadata Metadata { get; private set; }

    protected KnowledgeGraph() // EF Constructor
    {
        Statistics = GraphStatistics.Empty;
        Metadata = GraphMetadata.Default;
    }

    public KnowledgeGraph(List<Guid> repositoryIds, GraphMetadata metadata)
    {
        if (repositoryIds == null || repositoryIds.Count == 0)
            throw new ArgumentException("Repository IDs cannot be null or empty", nameof(repositoryIds));

        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        _repositoryIds.AddRange(repositoryIds.Distinct());
        Status = GraphStatus.NotBuilt;
        CreatedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
        Statistics = GraphStatistics.Empty;
        Metadata = metadata;
    }

    public static KnowledgeGraph Create(List<Guid> repositoryIds, GraphMetadata metadata)
    {
        return new KnowledgeGraph(repositoryIds, metadata);
    }

    public void UpdateStatus(GraphStatus status)
    {
        if (Status == status)
            return;

        var validTransitions = GetValidStatusTransitions(Status);
        if (!validTransitions.Contains(status))
            throw new InvalidOperationException($"Cannot transition from {Status} to {status}");

        Status = status;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatistics(GraphStatistics statistics)
    {
        Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMetadata(GraphMetadata metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void AddRepository(Guid repositoryId)
    {
        if (repositoryId == Guid.Empty)
            throw new ArgumentException("Repository ID cannot be empty", nameof(repositoryId));

        if (!_repositoryIds.Contains(repositoryId))
        {
            _repositoryIds.Add(repositoryId);
            UpdateStatus(GraphStatus.UpdateRequired);
        }
    }

    public void RemoveRepository(Guid repositoryId)
    {
        if (_repositoryIds.Remove(repositoryId))
        {
            UpdateStatus(GraphStatus.UpdateRequired);
        }
    }

    public bool RequiresRebuild(List<DateTime> repositoryLastModified)
    {
        if (repositoryLastModified == null || repositoryLastModified.Count == 0)
            return Status == GraphStatus.NotBuilt;

        if (Status == GraphStatus.NotBuilt || Status == GraphStatus.Error || Status == GraphStatus.UpdateRequired)
            return true;

        // Check if any repository has been modified since the last graph analysis
        var lastAnalysis = Statistics.LastAnalysis == DateTime.MinValue ? CreatedAt : Statistics.LastAnalysis;
        return repositoryLastModified.Any(modified => modified > lastAnalysis);
    }

    public bool IsReady() => Status == GraphStatus.Complete;

    public bool IsBuilding() => Status == GraphStatus.Building || Status == GraphStatus.Analyzing;

    public bool HasError() => Status == GraphStatus.Error;

    public bool ContainsRepository(Guid repositoryId) => _repositoryIds.Contains(repositoryId);

    public int RepositoryCount => _repositoryIds.Count;

    private static List<GraphStatus> GetValidStatusTransitions(GraphStatus currentStatus)
    {
        return currentStatus switch
        {
            GraphStatus.NotBuilt => new List<GraphStatus> { GraphStatus.Building, GraphStatus.Error },
            GraphStatus.Building => new List<GraphStatus> { GraphStatus.Analyzing, GraphStatus.Error, GraphStatus.NotBuilt },
            GraphStatus.Analyzing => new List<GraphStatus> { GraphStatus.Complete, GraphStatus.Error, GraphStatus.NotBuilt },
            GraphStatus.Complete => new List<GraphStatus> { GraphStatus.Building, GraphStatus.UpdateRequired, GraphStatus.Error },
            GraphStatus.Error => new List<GraphStatus> { GraphStatus.Building, GraphStatus.NotBuilt },
            GraphStatus.UpdateRequired => new List<GraphStatus> { GraphStatus.Building, GraphStatus.Error },
            _ => new List<GraphStatus>()
        };
    }
}