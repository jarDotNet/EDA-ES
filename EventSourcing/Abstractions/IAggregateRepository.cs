namespace EventSourcing.Abstractions;

public interface IAggregateRepository<TAggregate> where TAggregate : AggregateRoot, new()
{
    Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    Task SaveBatchAsync(IEnumerable<TAggregate> aggregates, CancellationToken cancellationToken = default);
}
