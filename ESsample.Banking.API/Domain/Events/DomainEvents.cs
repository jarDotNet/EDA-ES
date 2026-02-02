using ESsample.Banking.API.Domain.Aggregates;

namespace ESsample.Banking.API.Domain.Events;

public record AccountCreated //: DomainEvent
{
    public required string AccountNumber { get; init; } = string.Empty;
    public required string AccountName { get; init; } = string.Empty;
    public required string OwnerName { get; init; } = string.Empty;
    public required decimal InitialBalance { get; init; }
    public required Transaction Transaction { get; init; }
}

public record MoneyDeposited //: DomainEvent
{
    public required decimal Amount { get; init; }
    //public required string Description { get; init; } = string.Empty;
    public required Transaction Transaction { get; init; }
}

public record MoneyWithdrawn //: DomainEvent
{
    public required decimal Amount { get; init; }
    //public required string Description { get; init; } = string.Empty;
    public required Transaction Transaction { get; init; }
}
