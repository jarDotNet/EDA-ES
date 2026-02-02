using Confluent.Kafka;
using EventBus.Events;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace EventBus.Kafka
{
    public class KafkaEventBusPublisher : IEventBusPublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaEventBusPublisher> _logger;
        private readonly string _defaultTopic;

        public KafkaEventBusPublisher(
            IProducer<string, string> producer, 
            ILogger<KafkaEventBusPublisher> logger,
            string defaultTopic = "integration-events")
        {
            _producer = producer;
            _logger = logger;
            _defaultTopic = defaultTopic;
        }

        public async Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
        {
            if (typeof(TEvent).IsAbstract)
            {
                throw new ArgumentException(typeof(TEvent).Name + " must be non-abstract class");
            }

            var eventType = typeof(TEvent);
            var eventName = eventType.Name;
            var assemblyQualifiedName = eventType.AssemblyQualifiedName ?? string.Empty;
            var jsonMessage = JsonSerializer.Serialize(integrationEvent, eventType);

            _logger.LogInformation("📩 Publishing event {EventName} to topic {Topic}. Event data: {EventData}", 
                eventName, _defaultTopic, jsonMessage);

            try
            {
                var message = new Message<string, string>
                {
                    Key = eventName,
                    Value = jsonMessage,
                    Headers = new Headers
                    {
                        { "EventType", Encoding.UTF8.GetBytes(assemblyQualifiedName) },
                        { "EventId", Encoding.UTF8.GetBytes(integrationEvent.Id.ToString()) },
                        { "EventTimestamp", Encoding.UTF8.GetBytes(integrationEvent.CreatedAt.ToString("o")) },
                    }
                };

                var result = await _producer.ProduceAsync(_defaultTopic, message, cancellationToken);
                
                _logger.LogInformation("✅ Successfully published event {EventName} to topic {Topic} at partition {Partition}, offset {Offset}", 
                    eventName, _defaultTopic, result.Partition, result.Offset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error publishing event {EventName} to topic {Topic}: {ErrorMessage}", 
                    eventName, _defaultTopic, ex.Message);
                throw;
            }
        }

        public async Task PublishAsync<TEvent>(TEvent[] integrationEvents, CancellationToken cancellationToken = default) where TEvent : IntegrationEvent
        {
            if (typeof(TEvent).IsAbstract)
            {
                throw new ArgumentException(typeof(TEvent).Name + " must be non-abstract class");
            }

            if (integrationEvents is null || integrationEvents.Length == 0)
            {
                var eventName = typeof(TEvent).Name;
                _logger.LogWarning("⚠️ No integration events of type {EventName} to publish to topic {Topic}.", eventName, _defaultTopic);
                return;
            }
           await Task.WhenAll(integrationEvents.Select(e => PublishAsync(e, cancellationToken)));
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
