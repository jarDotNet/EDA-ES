using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventBus.Kafka.Extensions
{
    public static class KafkaEventBusExtensions
    {
        public static IServiceCollection AddKafkaEventBus(this IServiceCollection services, KafkaConfiguration kafkaConfiguration)
        {
            var bootstrapServers = kafkaConfiguration.BootstrapServers
                ?? throw new ArgumentNullException(nameof(kafkaConfiguration.BootstrapServers), "Bootstrap servers must be configured");

            var defaultTopic = kafkaConfiguration.TopicPrefix
                ?? throw new ArgumentNullException(nameof(kafkaConfiguration.TopicPrefix), "Topic must be configured");

            var groupId = kafkaConfiguration.GroupId
                ?? throw new ArgumentNullException(nameof(kafkaConfiguration.GroupId), "Group ID must be configured");

            // Producer configuration
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,
                MessageSendMaxRetries = 3,
                EnableIdempotence = true,
                MessageTimeoutMs = 30000,
                RequestTimeoutMs = 30000
            };

            // Consumer configuration
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                SessionTimeoutMs = 10000,
                HeartbeatIntervalMs = 3000,
                MaxPollIntervalMs = 300000,
                EnablePartitionEof = false,
                FetchWaitMaxMs = 500,
                FetchMinBytes = 1,
                FetchMaxBytes = 52428800,
                MaxPartitionFetchBytes = 1048576
            };

            services.AddSingleton(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<KafkaEventBusPublisher>>();
                logger.LogInformation("Creating Kafka producer with servers: {BootstrapServers}", bootstrapServers);
                return new ProducerBuilder<string, string>(producerConfig).Build();
            });

            services.AddSingleton(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<KafkaEventBusConsumer>>();
                logger.LogInformation("Creating Kafka consumer with servers: {BootstrapServers}, group: {GroupId}", 
                    bootstrapServers, groupId);
                return new ConsumerBuilder<string, string>(consumerConfig).Build();
            });

            services.AddSingleton<IEventBusPublisher>(provider =>
            {
                var producer = provider.GetRequiredService<IProducer<string, string>>();
                var logger = provider.GetRequiredService<ILogger<KafkaEventBusPublisher>>();
                return new KafkaEventBusPublisher(producer, logger, defaultTopic);
            });

            services.AddSingleton<IEventBusConsumer>(provider =>
            {
                var consumer = provider.GetRequiredService<IConsumer<string, string>>();
                var logger = provider.GetRequiredService<ILogger<KafkaEventBusConsumer>>();
                return new KafkaEventBusConsumer(consumer, serviceProvider: provider, logger, defaultTopic);
            });

            return services;
        }
    }
}
