using System.Text.Json;

namespace EventSourcing.Postgres;

internal class EventEntity
{
    public long SeqId { get; set; }
    public Guid Id { get; set; }
    public Guid? StreamId { get; set; }
    public long Version { get; set; }
    public JsonDocument Data { get; set; } = null!;
    public string Type { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string? TenantId { get; set; }
    public string? MtDotnetType { get; set; }
    public bool IsArchived { get; set; }
}
