namespace Archie.Domain.ValueObjects;

public enum GraphStatus
{
    NotBuilt,
    Building,
    Analyzing,
    Complete,
    Error,
    UpdateRequired
}