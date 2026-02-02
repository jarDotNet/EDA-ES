namespace EventBus.Events;

public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string EventType { get; }
    public string DotNetType { get; }

    protected IntegrationEvent()
    {
        EventType = GetType().Name;
        DotNetType = GetType().AssemblyQualifiedName ?? string.Empty;
    }
}
