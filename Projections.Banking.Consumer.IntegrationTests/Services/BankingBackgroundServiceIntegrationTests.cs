using EventBus;
using EventBus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Projections.Banking.Consumer.IntegrationTests.Fixtures;
using Projections.Banking.Consumer.IntegrationTests.Handlers;
using Projections.Banking.Consumer.Services;
using StackExchange.Redis;
using System.Reflection;
using Xunit;

namespace Projections.Banking.Consumer.IntegrationTests.Services;

public class BankingBackgroundServiceIntegrationTests : IClassFixture<KafkaFixture>, IClassFixture<RedisFixture>, IAsyncLifetime
{
    private readonly RedisFixture _redisFixture;
    private readonly KafkaFixture _kafkaFixture;

    private readonly IDatabase _redisDatabase;
    private IServiceProvider _serviceProvider = default!;

    protected static readonly TimeSpan CancellationTokenTimeout = TimeSpan.FromSeconds(5);

    public BankingBackgroundServiceIntegrationTests(KafkaFixture kafkaFixture, RedisFixture redisFixture)
    {
        _kafkaFixture = kafkaFixture;
        _redisFixture = redisFixture;

        _redisDatabase = _redisFixture.Connection.GetDatabase();
    }

    public async Task InitializeAsync()
    {
        // Reset Redis to ensure a clean environment for each test.
        await _redisDatabase.ExecuteAsync("FLUSHALL");

        var services = new ServiceCollection();
        services.AddLogging(config =>
        {
            config.AddDebug();
            config.AddConsole();
        });

        services.AddScoped<IDatabase>(_ => _redisDatabase);

        // Register event handlers
        services.AddEventBusSubscriptions(Assembly.GetExecutingAssembly());

        // Register Kafka dependencies
        _kafkaFixture.RegisterDependencies(services);


        _serviceProvider = services.BuildServiceProvider();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ConsumeAsync_ReceivesAndEmitsIntegrationEvent()
    {
        // Arrange
        var testEvent = new TestIntegrationEvent { Name = "TestEvent" };

        var consumer = _serviceProvider.GetRequiredService<IEventBusConsumer>();
        var logger = _serviceProvider.GetRequiredService<ILogger<BankingBackgroundService>>();

        // Act
        var eventBusPublisher = _serviceProvider.GetRequiredService<IEventBusPublisher>();
        var cancellationToken = new CancellationTokenSource(CancellationTokenTimeout).Token;
        await eventBusPublisher.PublishAsync(testEvent, cancellationToken);

        var consumerService = new BankingBackgroundService(consumer, _serviceProvider, logger);
        await consumerService.StartAsync(cancellationToken);

        // Assert
        await Task.Delay(CancellationTokenTimeout);
        Assert.Equal(testEvent.Name, await _redisDatabase.StringGetAsync(FirstTestIntegrationEventHandler.CacheKey));
        Assert.Equal(testEvent.Name, await _redisDatabase.StringGetAsync(SecondTestIntegrationEventHandler.CacheKey));
    }
}

