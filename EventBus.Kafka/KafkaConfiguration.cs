namespace EventBus.Kafka;

public class KafkaConfiguration
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = "default-group";
    public string TopicPrefix { get; set; } = "integration-events";
}
