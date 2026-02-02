using ESsample.Banking.Shared.Events;
using EventBus;
using Projections.Banking.Features.Transactions.CreateTransactionProjection;

namespace Projections.Banking.Consumer.Handlers.Transaction;

public class AccountBalanceUpdatedEventHandler : IIntegrationEventHandler<AccountBalanceUpdatedEvent>
{
    private readonly ICreateTransactionProjectionHandler _projectionHandler;
    private readonly ILogger<AccountBalanceUpdatedEventHandler> _logger;

    public AccountBalanceUpdatedEventHandler(
        ICreateTransactionProjectionHandler projectionHandler,
        ILogger<AccountBalanceUpdatedEventHandler> logger)
    {
        _projectionHandler = projectionHandler;
        _logger = logger;
    }

    public async Task HandleAsync(AccountBalanceUpdatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== PROCESSING AccountBalanceUpdatedEvent - Transaction ===");
        _logger.LogInformation("Account ID: {AccountId}", integrationEvent.AccountId);
        _logger.LogInformation("Account Number: {AccountNumber}", integrationEvent.AccountNumber);
        _logger.LogInformation("Account Name: {AccountName}", integrationEvent.AccountName);
        _logger.LogInformation("Previous Balance: {PreviousBalance}", integrationEvent.PreviousBalance);
        _logger.LogInformation("New Balance: {NewBalance}", integrationEvent.NewBalance);
        _logger.LogInformation("Transaction ID: {TransactionId}", integrationEvent.Transaction.TransactionId);
        _logger.LogInformation("Transaction Description: {Description}", integrationEvent.Transaction.Description);
        _logger.LogInformation("Transaction Created At: {CreatedAt}", integrationEvent.Transaction.CreatedAt);
        _logger.LogInformation("Event ID: {EventId}", integrationEvent.Id);
        _logger.LogInformation("Created At: {CreatedAt}", integrationEvent.CreatedAt);

        var projectionRequest = new CreateTransactionProjectionRequest(
            integrationEvent.AccountId,
            integrationEvent.AccountNumber,
            integrationEvent.AccountName,
            integrationEvent.Transaction.Amount,
            integrationEvent.PreviousBalance,
            integrationEvent.NewBalance,
            integrationEvent.Transaction.TransactionId,
            integrationEvent.Transaction.Description,
            integrationEvent.Transaction.CreatedAt
        );
        await _projectionHandler.HandleAsync(projectionRequest, cancellationToken);

        _logger.LogInformation("=== SUCCESSFULLY PROCESSED AccountBalanceUpdatedEvent for Account {AccountId} ===", integrationEvent.AccountId);
    }
}
