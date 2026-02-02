using EventBus.Kafka.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Kafka;
using Xunit;

namespace Projections.Banking.Consumer.IntegrationTests.Fixtures;

public class KafkaFixture : IAsyncLifetime
{
    private readonly KafkaContainer KafkaContainer = default!;

    //private const string BootstrapServers = "localhost:9092";
    private const string Topic = "test-integration-events";

    public KafkaFixture()
    {
        KafkaContainer = new KafkaBuilder()
            .WithImage("confluentinc/cp-kafka:7.5.0")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await KafkaContainer.StartAsync();
    }

    public void RegisterDependencies(IServiceCollection services)
    {
        var kafkaConfiguration = new EventBus.Kafka.KafkaConfiguration
        {
            BootstrapServers = KafkaContainer.GetBootstrapAddress(),
            TopicPrefix = Topic,
            GroupId = nameof(KafkaFixture)
        };

        services.AddKafkaEventBus(kafkaConfiguration);
    }

    public async Task DisposeAsync()
    {
        if (KafkaContainer is not null)
        {
            await KafkaContainer.StopAsync();
            await KafkaContainer.DisposeAsync();
        }
    }
}
