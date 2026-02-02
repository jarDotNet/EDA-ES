using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace EventSourcing.Postgres.IntegrationTests;

public class PostgresEventStoreIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private EventStoreDbContext _dbContext = null!;
    private PostgresEventStore _eventStore = null!;

    public PostgresEventStoreIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("eventstoredb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<EventStoreDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _dbContext = new EventStoreDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _eventStore = new PostgresEventStore(_dbContext);
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task SaveAndRetrieveEvents_ShouldPersistAndReturnEvents()
    {
        // Arrange
        var version = 1;
        var streamId = Guid.NewGuid();
        var testEvent = new TestDomainEvent("Hello World");
        var change = new AggregateChange(
            testEvent,
            streamId,
            typeof(TestDomainEvent),
            version,
            DateTime.UtcNow
        );
        var events = new List<AggregateChange> { change };

        // Act
        await _eventStore.SaveEventsAsync(streamId, events, expectedVersion: 0, CancellationToken.None);
        var loadedEvents = await _eventStore.GetEventsAsync(streamId, CancellationToken.None);

        // Assert
        Assert.Single(loadedEvents);
        var loadedAggregateChange = loadedEvents.First();
        var loadedEvent = Assert.IsType<TestDomainEvent>(loadedAggregateChange.Content);
        Assert.Equal(testEvent.Message, loadedEvent.Message);
        Assert.Equal(version, loadedAggregateChange.Version);
    }

    [Fact]
    public async Task SaveAndRetrieveBatchEvents_ShouldPersistAndReturnEvents()
    {
        // Arrange
        var aggregate1 = new TestAggregate(Guid.NewGuid());
        var aggregate2 = new TestAggregate(Guid.NewGuid());

        aggregate1.DoSomething("Hello");
        aggregate2.DoSomething("Bye");

        // Act
        await _eventStore.SaveEventsBatchAsync([aggregate1, aggregate2], CancellationToken.None);

        // Assert
        var events1 = await _eventStore.GetEventsAsync(aggregate1.Id);
        var events2 = await _eventStore.GetEventsAsync(aggregate2.Id);
        Assert.Single(events1);
        Assert.Single(events2);

        var aggregateChange1 = events1.First();
        var aggregateChange2 = events2.First();
        var loadedEvent1 = Assert.IsType<TestDomainEvent>(aggregateChange1.Content);
        var loadedEvent2 = Assert.IsType<TestDomainEvent>(aggregateChange2.Content);
        Assert.Equal(aggregate1.Data, loadedEvent1.Message);
        Assert.Equal(aggregate2.Data, loadedEvent2.Message);
        Assert.Equal(aggregate1.Version, aggregateChange1.Version);
        Assert.Equal(aggregate2.Version, aggregateChange2.Version);
    }
}

