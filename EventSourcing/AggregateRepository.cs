using EventSourcing.Abstractions;

namespace EventSourcing;

public class AggregateRepository<TAggregate> : IAggregateRepository<TAggregate> where TAggregate : AggregateRoot, new()
{
    private readonly IEventStore _eventStore;

    public AggregateRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var events = (await _eventStore.GetEventsAsync(id, cancellationToken)).ToList();
        if (events.Count == 0)
        {
            return null;
        }

        var aggregate = new TAggregate();
        aggregate.Initialize(id);
        aggregate.LoadFromHistory(events);
        return aggregate;
    }

    public async Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var uncommittedEvents = aggregate.UncommittedEvents;

        if (!uncommittedEvents.Any())
        {
            return;
        }

        await _eventStore.SaveEventsAsync(aggregate.Id, uncommittedEvents, aggregate.ExpectedVersion, cancellationToken);
        aggregate.MarkEventsAsCommitted();
    }

    public async Task SaveBatchAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var aggregatesToSave = aggregates.Where(a => a.UncommittedEvents.Any());

        if (!aggregatesToSave.Any())
        {
            return;
        }

        await _eventStore.SaveEventsBatchAsync(aggregatesToSave, cancellationToken);

        foreach (var aggregate in aggregatesToSave)
        {
            aggregate.MarkEventsAsCommitted();
        }
    }
}
