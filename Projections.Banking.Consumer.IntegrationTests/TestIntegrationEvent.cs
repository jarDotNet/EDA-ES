using EventBus.Events;

namespace Projections.Banking.Consumer.IntegrationTests;

public record TestIntegrationEvent : IntegrationEvent
{
    public string Name { get; set; } = string.Empty;
}

