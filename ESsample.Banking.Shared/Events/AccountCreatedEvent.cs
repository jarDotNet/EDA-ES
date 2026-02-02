using EventBus.Events;

namespace ESsample.Banking.Shared.Events;

public record AccountCreatedEvent(
        Guid AccountId,
        string AccountNumber,
        string AccountName,
        string OwnerName,
        decimal InitialBalance,
        TransactionInfo Transaction
) : IntegrationEvent;
