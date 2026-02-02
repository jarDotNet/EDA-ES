using EventBus.Events;
using System.Collections.Concurrent;

namespace EventBus;

public abstract class EventBusConsumer : IEventBusConsumer
{
    private static readonly ConcurrentDictionary<Type, IntegrationEventHandlerWrapper> _eventHandlers = new();

    protected readonly IServiceProvider _serviceProvider;

    protected EventBusConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task EmitIntegrationEvent<TEvent>(TEvent integrationEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent, nameof(integrationEvent));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        var eventType = integrationEvent.GetType();

        var handler = _eventHandlers.GetOrAdd(eventType,
            factory =>
            {
                var instance = Activator.CreateInstance(typeof(IntegrationEventHandlerWrapperImpl<>).MakeGenericType(eventType));
                return instance is null
                    ? throw new InvalidOperationException($"Could not create an instance of EventRequestHandler<{eventType.Name}>.")
                    : (IntegrationEventHandlerWrapper)instance;
            });

        return handler.Handle(integrationEvent, serviceProvider, cancellationToken);
    }

    public abstract void Subscribe();

    public abstract void Unsubscribe();

    public abstract Task ConsumeAsync(CancellationToken cancellationToken = default);
}
