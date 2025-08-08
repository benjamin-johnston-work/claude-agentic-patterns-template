namespace Archie.Domain.Common;

public interface IEntity<T>
{
    T Id { get; }
}

public interface IEntity : IEntity<Guid>
{
}