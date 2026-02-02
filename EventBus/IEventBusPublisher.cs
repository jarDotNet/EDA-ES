using EventBus.Events;

namespace EventBus;

public interface IEventBusPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent;
    Task PublishAsync<TEvent>(TEvent[] integrationEvents, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent;
}
