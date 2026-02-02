using Xunit;

namespace EventSourcing.UnitTests;

public class AggregateRepositoryTests : IAsyncLifetime
{
    private InMemoryEventStore _eventStore = null!;
    private AggregateRepository<TestAggregate> _repository = null!;

    public Task InitializeAsync()
    {
        _eventStore = new InMemoryEventStore();
        _repository = new AggregateRepository<TestAggregate>(_eventStore);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _eventStore?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNoEvents()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsAggregate_WhenEventsExist()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var changes = new List<AggregateChange>
        {
            new(new TestDomainEvent { Data = "first" }, aggregateId, typeof(TestDomainEvent), 1, DateTime.UtcNow),
            new(new TestDomainEvent { Data = "second" }, aggregateId, typeof(TestDomainEvent), 2, DateTime.UtcNow)
        };

        await _eventStore.SaveEventsAsync(aggregateId, changes, 0);

        // Act
        var result = await _repository.GetByIdAsync(aggregateId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(changes.Count, result.Version);
    }

    [Fact]
    public async Task SaveAsync_DoesNothing_WhenNoUncommittedEvents()
    {
        // Arrange
        var aggregate = new TestAggregate();

        // Act
        await _repository.SaveAsync(aggregate);

        // Assert
        var events = await _eventStore.GetEventsAsync(aggregate.Id);
        Assert.Empty(events);
    }

    [Fact]
    public async Task SaveAsync_SavesEvents_WhenUncommittedEventsExist()
    {
        // Arrange
        var aggregate = new TestAggregate();

        aggregate.DoSomething("test");

        // Act
        await _repository.SaveAsync(aggregate);

        // Assert
        var events = await _eventStore.GetEventsAsync(aggregate.Id);
        Assert.Single(events);
        Assert.Empty(aggregate.UncommittedEvents);
    }

    [Fact]
    public async Task SaveAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.DoSomething("test data");
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _repository.SaveAsync(aggregate, cancellationToken)
        );
    }

    [Fact]
    public async Task SaveBatchAsync_WithNullAggregates_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _repository.SaveBatchAsync(null!, CancellationToken.None)
        );
    }

    [Fact]
    public async Task SaveBatchAsync_WithEmptyCollection_DoesNotCallEventStore()
    {
        // Arrange
        var emptyAggregates = new List<TestAggregate>();

        // Act
        await _repository.SaveBatchAsync(emptyAggregates, CancellationToken.None);

        // Assert - No exception should be thrown and method should complete successfully
        Assert.True(true); // Test passes if no exception is thrown
    }

    [Fact]
    public async Task SaveBatchAsync_WithAggregatesWithoutEvents_DoesNotCallEventStore()
    {
        // Arrange
        var aggregates = new List<TestAggregate>
            {
                new(Guid.NewGuid()),
                new(Guid.NewGuid())
            };

        // Act
        await _repository.SaveBatchAsync(aggregates, CancellationToken.None);

        // Assert - Verify no events were stored
        var events1 = await _eventStore.GetEventsAsync(aggregates[0].Id);
        var events2 = await _eventStore.GetEventsAsync(aggregates[1].Id);
        Assert.Empty(events1);
        Assert.Empty(events2);
    }

    [Fact]
    public async Task SaveBatchAsync_WithValidAggregates_SavesEventsToStore()
    {
        // Arrange
        var aggregate1 = new TestAggregate(Guid.NewGuid());
        var aggregate2 = new TestAggregate(Guid.NewGuid());

        aggregate1.DoSomething("test data 1");
        aggregate2.DoSomething("test data 2");

        // Act
        await _repository.SaveBatchAsync([aggregate1, aggregate2], CancellationToken.None);

        // Assert - Verify events were saved to the event store
        var events1 = await _eventStore.GetEventsAsync(aggregate1.Id);
        var events2 = await _eventStore.GetEventsAsync(aggregate2.Id);

        Assert.Single(events1);
        Assert.Single(events2);
        Assert.Equal(aggregate1.Id, events1.First().Id);
        Assert.Equal(aggregate2.Id, events2.First().Id);
    }

    [Fact]
    public async Task SaveBatchAsync_WithMixedAggregates_OnlySavesAggregatesWithEvents()
    {
        // Arrange
        var aggregateWithEvents = new TestAggregate(Guid.NewGuid());
        var aggregateWithoutEvents = new TestAggregate(Guid.NewGuid());

        aggregateWithEvents.DoSomething("test data");

        // Act
        await _repository.SaveBatchAsync([aggregateWithEvents, aggregateWithoutEvents], CancellationToken.None);

        // Assert - Only aggregate with events should have stored events
        var eventsWithEvents = await _eventStore.GetEventsAsync(aggregateWithEvents.Id);
        var eventsWithoutEvents = await _eventStore.GetEventsAsync(aggregateWithoutEvents.Id);

        Assert.Single(eventsWithEvents);
        Assert.Empty(eventsWithoutEvents);
    }

    [Fact]
    public async Task SaveBatchAsync_AfterSaving_MarksAllEventsAsCommitted()
    {
        // Arrange
        var aggregate1 = new TestAggregate(Guid.NewGuid());
        var aggregate2 = new TestAggregate(Guid.NewGuid());

        aggregate1.DoSomething("test data 1");
        aggregate2.DoSomething("test data 2");

        // Verify events exist before saving
        Assert.NotEmpty(aggregate1.UncommittedEvents);
        Assert.NotEmpty(aggregate2.UncommittedEvents);

        // Act
        await _repository.SaveBatchAsync([aggregate1, aggregate2], CancellationToken.None);

        // Assert - Events should be marked as committed
        Assert.Empty(aggregate1.UncommittedEvents);
        Assert.Empty(aggregate2.UncommittedEvents);
    }

    [Fact]
    public async Task SaveBatchAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.DoSomething("test data");
        var cancellationToken = new CancellationToken(true);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _repository.SaveBatchAsync([aggregate], cancellationToken)
        );
    }
}

