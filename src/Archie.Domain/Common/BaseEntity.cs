namespace Archie.Domain.Common;

public abstract class BaseEntity : IEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other || GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !Equals(left, right);
    }
}