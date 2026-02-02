namespace EventSourcing.Abstractions;

public interface IEventStore
{
    Task SaveEventsAsync(Guid streamId, IEnumerable<AggregateChange> events, long expectedVersion, CancellationToken cancellationToken = default);
    Task SaveEventsBatchAsync(IEnumerable<AggregateRoot> aggregates, CancellationToken cancellationToken = default);
    Task<IEnumerable<AggregateChange>> GetEventsAsync(Guid streamId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AggregateChange>> GetEventsAsync(Guid streamId, long fromVersion, CancellationToken cancellationToken = default);
}
