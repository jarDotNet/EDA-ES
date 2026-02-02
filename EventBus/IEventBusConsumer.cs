using EventBus.Events;

namespace EventBus;

public interface IEventBusConsumer
{
    void Subscribe();
    void Unsubscribe();

    Task ConsumeAsync(CancellationToken cancellationToken = default);

    Task EmitIntegrationEvent<TEvent>(TEvent notification, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
