
namespace EventBus.Events;

public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime CreatedAt { get; }
    string EventType { get; }
    string DotNetType { get; }
}