namespace EventSourcing;

public record AggregateChange(object Content, Guid Id, Type Type, long Version, DateTime CreatedAt);
