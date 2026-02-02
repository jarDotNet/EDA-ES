namespace EventSourcing.Postgres.IntegrationTests;

public class TestAggregate : AggregateRoot
{
    public string Data { get; private set; } = string.Empty;

    public TestAggregate()
    {

    }

    public TestAggregate(Guid id)
    {
        Initialize(id);
    }

    public void DoSomething(string message)
    {
        RaiseEvent(new TestDomainEvent(message));
    }

    public void Apply(TestDomainEvent domainEvent)
    {
        Data = domainEvent.Message;
    }
}

