using Xunit;

namespace EventSourcing.UnitTests;

public class AggregateRootTests
{
    [Fact]
    public void Initialize_SetsIdAndClearsChanges()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var aggregate = new TestAggregate(id);

        // Assert
        Assert.Equal(id, aggregate.Id);
        Assert.Empty(aggregate.UncommittedEvents);
    }

    [Fact]
    public void RaiseEvent_AddsEventAndIncrementsVersion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregate = new TestAggregate(id);

        // Act
        aggregate.DoSomething("test");

        // Assert
        Assert.Single(aggregate.UncommittedEvents);
        Assert.Equal(1, aggregate.Version);
        Assert.Equal("test", aggregate.State);
    }

    [Fact]
    public void MarkEventsAsCommitted_ClearsUncommittedEvents()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregate = new TestAggregate(id);

        // Act
        aggregate.DoSomething("test");
        aggregate.MarkEventsAsCommitted();

        // Assert
        Assert.Empty(aggregate.UncommittedEvents);
    }

    [Fact]
    public void ExpectedVersion_IsZero_ForNewAggregate()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());

        // Act
        aggregate.DoSomething("test1");

        // Assert
        Assert.Equal(0, aggregate.ExpectedVersion);

        // Act
        aggregate.DoSomething("test2");

        // Assert
        Assert.Equal(0, aggregate.ExpectedVersion);
    }

    [Fact]
    public void ExpectedVersion_RemainsTheSame_AfterAddingNewEvents()
    {
        // Arrange
        var id = Guid.NewGuid();
        var history = new List<AggregateChange>
        {
            new(new TestDomainEvent { Data = "first" }, id, typeof(TestDomainEvent), 1, DateTime.UtcNow),
            new(new TestDomainEvent { Data = "second" }, id, typeof(TestDomainEvent), 2, DateTime.UtcNow)
        };

        // Act
        var aggregate = new TestAggregate(id);
        aggregate.LoadFromHistory(history);

        // Assert
        Assert.Equal(2, aggregate.ExpectedVersion);

        // Act
        aggregate.DoSomething("third");
        
        // Assert
        Assert.Equal(2, aggregate.ExpectedVersion);
    }

    [Fact]
    public void LoadFromHistory_EmptyHistory_DoesNothing()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregate = new TestAggregate(id);

        // Act
        aggregate.LoadFromHistory([]);

        // Assert
        Assert.Equal(0, aggregate.Version);
        Assert.Empty(aggregate.UncommittedEvents);
    }

    [Fact]
    public void LoadFromHistory_AppliesEventsAndSetsVersion()
    {
        // Arrange
        var id = Guid.NewGuid();
        var history = new List<AggregateChange>
        {
            new(new TestDomainEvent { Data = "first" }, id, typeof(TestDomainEvent), 1, DateTime.UtcNow),
            new(new TestDomainEvent { Data = "second" }, id, typeof(TestDomainEvent), 2, DateTime.UtcNow)
        };

        // Act
        var aggregate = new TestAggregate(id);
        aggregate.LoadFromHistory(history);

        // Assert
        Assert.Equal("second", aggregate.State);
        Assert.Equal(2, aggregate.Version);
        Assert.Equal(2, aggregate.ExpectedVersion);
    }
}
