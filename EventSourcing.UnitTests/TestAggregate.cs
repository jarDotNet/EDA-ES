namespace EventSourcing.UnitTests;

public class TestAggregate : AggregateRoot
{
    public string State { get; private set; } = string.Empty;

    public TestAggregate()
    {
            
    }

    public TestAggregate(Guid id)
    {
        Initialize(id);
    }

    public void DoSomething(string data)
    {
        RaiseEvent(new TestDomainEvent { Data = data });
    }

    public void Apply(TestDomainEvent domainEvent)
    {
        State = domainEvent.Data;
    }
}
