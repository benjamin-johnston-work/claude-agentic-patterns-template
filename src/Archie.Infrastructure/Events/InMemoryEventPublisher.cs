using Archie.Application.Interfaces;
using Archie.Domain.Common;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Archie.Infrastructure.Events;

public class InMemoryEventPublisher : IEventPublisher
{
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        await PublishAsync(new[] { domainEvent }, cancellationToken);
    }

    public async Task PublishAsync<T>(IEnumerable<T> domainEvents, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                var eventData = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogInformation("Publishing domain event: {EventType} - {EventData}", 
                    domainEvent.GetType().Name, eventData);

                // In a real implementation, this would publish to Azure Service Bus
                // For now, we just log the event
                await Task.Delay(1, cancellationToken); // Simulate async operation
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Event publishing was cancelled for: {EventType}", domainEvent.GetType().Name);
                // Don't throw on cancellation - this is expected behavior
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish domain event: {EventType}", domainEvent.GetType().Name);
                throw;
            }
        }
    }
}