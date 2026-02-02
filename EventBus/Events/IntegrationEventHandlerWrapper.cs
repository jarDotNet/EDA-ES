using Microsoft.Extensions.DependencyInjection;

namespace EventBus.Events;

internal abstract class IntegrationEventHandlerWrapper
{
    public abstract Task Handle(IIntegrationEvent integrationEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}

internal class IntegrationEventHandlerWrapperImpl<TEvent> : IntegrationEventHandlerWrapper
    where TEvent : IIntegrationEvent
{
    public override Task Handle(IIntegrationEvent integrationEvent, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var scope = serviceProvider.CreateScope();
        var subscribedHandlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler<TEvent>>();
        if (!subscribedHandlers.Any())
        {
            return Task.CompletedTask;
        }

        var allHandlers = subscribedHandlers.Select(handler =>
        {
            handler.HandleAsync((TEvent)integrationEvent);
            return Task.CompletedTask;
        });

        return Task.WhenAll(allHandlers);
    }
}
