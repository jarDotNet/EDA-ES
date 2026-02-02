using EventBus.Events;

namespace ESsample.Banking.Shared.Events;

public record AccountBalanceUpdatedEvent(
    Guid AccountId,
    string AccountNumber,
    string AccountName,
    decimal PreviousBalance,
    decimal NewBalance,
    TransactionInfo Transaction
) : IntegrationEvent;
