namespace EventSourcing.UnitTests;

public record TestDomainEvent
{
    public required string Data { get; init; }
}
