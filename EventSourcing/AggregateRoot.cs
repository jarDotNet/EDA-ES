namespace EventSourcing;

public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
    public long Version { get; protected set; }

    private List<AggregateChange> _changes = [];
    public IReadOnlyList<AggregateChange> UncommittedEvents => _changes.AsReadOnly();

    public long ExpectedVersion => Version - UncommittedEvents.Count;

    internal void Initialize(Guid id)
    {
        Id = id;
        _changes = [];
    }

    protected void RaiseEvent<T>(T domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent, nameof(domainEvent));

        ApplyChange(domainEvent);

        Version++;

        var change = new AggregateChange(
            domainEvent,
            Id,
            domainEvent.GetType(),
            Version,
            DateTime.UtcNow
        );
        _changes.Add(change);
    }

    public void MarkEventsAsCommitted()
    {
        _changes.Clear();
    }

    public void LoadFromHistory(IList<AggregateChange> history)
    {
        if (!history.Any())
        {
            return;
        }

        foreach (var change in history)
        {
            ApplyChange(change.Content);
        }

        Version = history.Last().Version;
    }

    private void ApplyChange<T>(T domainEvent)
    {
        if (domainEvent is not null)
        {
            dynamic me = this;
            dynamic eventObj = domainEvent;
            me.Apply(eventObj);
        }
    }
}
