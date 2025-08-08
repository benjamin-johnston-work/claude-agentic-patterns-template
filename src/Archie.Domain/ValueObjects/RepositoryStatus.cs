namespace Archie.Domain.ValueObjects;

public enum RepositoryStatus
{
    Connecting,
    Connected,
    Analyzing,
    Ready,
    Error,
    Disconnected
}