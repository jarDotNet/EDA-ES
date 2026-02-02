using Confluent.Kafka;
using EventBus.Events;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace EventBus.Kafka
{
    public class KafkaEventBusConsumer : EventBusConsumer, IDisposable
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<KafkaEventBusConsumer> _logger;
        private readonly string _defaultTopic;

        public KafkaEventBusConsumer(
            IConsumer<string, string> consumer,
            IServiceProvider serviceProvider,
            ILogger<KafkaEventBusConsumer> logger,
            string defaultTopic = "integration-events") : base(serviceProvider)
        {
            _consumer = consumer;
            _logger = logger;
            _defaultTopic = defaultTopic;
        }

        public override void Subscribe()
        {
            try
            {
                _consumer.Subscribe(_defaultTopic);
                _logger.LogInformation("✅ Successfully subscribed to topic: {Topic}", _defaultTopic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to subscribe to topic: {Topic}", _defaultTopic);
                throw;
            }
        }

        public override void Unsubscribe()
        {
            try
            {
                _consumer.Unsubscribe();
                _logger.LogInformation("✅ Successfully unsubscribed to topic: {Topic}", _defaultTopic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to unsubscribe from topic: {Topic}", _defaultTopic);
                throw;
            }
        }

        public override async Task ConsumeAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting Kafka consumer loop for topic: {Topic}", _defaultTopic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Use a shorter timeout to make the consumer more responsive
                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                    if (consumeResult?.Message != null)
                    {
                        _logger.LogInformation("📨 Received message - Key: {Key}, Partition: {Partition}, Offset: {Offset}",
                            consumeResult.Message.Key, consumeResult.Partition, consumeResult.Offset);

                        var message = consumeResult.Message;
                        var eventName = message.Key;
                        var eventData = message.Value;

                        _logger.LogInformation("🔄 Processing event {EventName}", eventName);
                        _logger.LogDebug("Event data: {EventData}", eventData);

                        message.Headers.TryGetLastBytes("EventType", out var eventTypeBytes);
                        if (eventTypeBytes is null || eventTypeBytes.Length == 0)
                        {
                            _logger.LogError("❌ EventType header not found in message");
                            continue;
                        }

                        var assemblyQualifiedName = Encoding.UTF8.GetString([.. eventTypeBytes]);
                        var eventType = Type.GetType(assemblyQualifiedName);
                        if (eventType is not null)
                        {
                            var integrationEvent = JsonSerializer.Deserialize(eventData, eventType) as IntegrationEvent;
                            if (integrationEvent is null)
                            {
                                _logger.LogError("❌ Failed to deserialize event {EventName}", eventName);
                                continue;
                            }

                            await EmitIntegrationEvent(integrationEvent, _serviceProvider, cancellationToken);
                        }

                        try
                        {
                            _consumer.Commit(consumeResult);
                            _logger.LogDebug("✅ Committed offset {Offset} for partition {Partition}",
                                consumeResult.Offset, consumeResult.Partition);
                        }
                        catch (Exception commitEx)
                        {
                            _logger.LogError(commitEx, "❌ Failed to commit offset");
                        }
                    }
                    else
                    {
                        // No message received within timeout - this is normal
                        _logger.LogTrace("No message received within timeout");
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "❌ Kafka consume error: {Error}", ex.Error.Reason);
                    await Task.Delay(1000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("❌ Consumer operation cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Unexpected error in consumer loop");
                    await Task.Delay(1000, cancellationToken);
                }
            }

            _logger.LogInformation("🛑 Kafka consumer loop ended");
        }

        public void Dispose()
        {
            _logger.LogInformation("🛑 Disposing Kafka consumer...");
            _consumer?.Dispose();
        }
    }
}
