using EventSourcing.Abstractions;

namespace EventSourcing.UnitTests;

public class InMemoryEventStore : IEventStore, IDisposable
{
    private readonly Dictionary<Guid, List<AggregateChange>> _store = [];

    public Task<IEnumerable<AggregateChange>> GetEventsAsync(Guid streamId, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(streamId, out var events);
        return Task.FromResult(events?.AsEnumerable() ?? []);
    }

    public Task<IEnumerable<AggregateChange>> GetEventsAsync(Guid streamId, long fromVersion, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(streamId, out var events);
        return Task.FromResult(events?.Skip((int)fromVersion).AsEnumerable() ?? []);
    }

    public Task SaveEventsAsync(Guid streamId, IEnumerable<AggregateChange> events, long expectedVersion, CancellationToken cancellationToken = default)
    {
        if (events == null || !events.Any())
        {
            return Task.CompletedTask;
        }

        if (!_store.TryGetValue(streamId, out List<AggregateChange>? value))
        {
            value = [];
            _store[streamId] = value;
        }

        value.AddRange(events);
        return Task.CompletedTask;
    }
    public Task SaveEventsBatchAsync(IEnumerable<AggregateRoot> aggregates, CancellationToken cancellationToken = default)
    {
       foreach (var aggregate in aggregates)
        {
            SaveEventsAsync(aggregate.Id, aggregate.UncommittedEvents, aggregate.ExpectedVersion, cancellationToken);
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _store.Clear();
    }
}

