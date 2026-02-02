using ESsample.Banking.API.Domain.Aggregates;

namespace ESsample.Banking.API.Domain.Events;

public record AccountCreated
{
    public required string AccountNumber { get; init; } = string.Empty;
    public required string AccountName { get; init; } = string.Empty;
    public required string OwnerName { get; init; } = string.Empty;
    public required decimal InitialBalance { get; init; }
    public required Transaction Transaction { get; init; }
}

public record MoneyDeposited
{
    public required decimal Amount { get; init; }
    public required Transaction Transaction { get; init; }
}

public record MoneyWithdrawn
{
    public required decimal Amount { get; init; }
    public required Transaction Transaction { get; init; }
}
