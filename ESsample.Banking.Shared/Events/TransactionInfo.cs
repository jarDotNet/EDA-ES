namespace ESsample.Banking.Shared.Events;

public record TransactionInfo(
    Guid TransactionId,
    decimal Amount,
    string Description,
    DateTime CreatedAt
);
