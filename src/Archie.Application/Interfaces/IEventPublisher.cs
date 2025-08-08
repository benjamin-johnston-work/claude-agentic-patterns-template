using Archie.Domain.Common;

namespace Archie.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent;
    Task PublishAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) where T : IDomainEvent;
}