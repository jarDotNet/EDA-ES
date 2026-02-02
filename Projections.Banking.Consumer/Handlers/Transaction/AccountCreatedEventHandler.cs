using ESsample.Banking.Shared.Events;
using EventBus;
using Projections.Banking.Features.Transactions.CreateTransactionProjection;

namespace Projections.Banking.Consumer.Handlers.Transaction;

public class AccountCreatedEventHandler : IIntegrationEventHandler<AccountCreatedEvent>
{
    private readonly ICreateTransactionProjectionHandler _projectionHandler;
    private readonly ILogger<AccountCreatedEventHandler> _logger;

    public AccountCreatedEventHandler(
        ICreateTransactionProjectionHandler projectionHandler,
        ILogger<AccountCreatedEventHandler> logger)
    {
        _projectionHandler = projectionHandler;
        _logger = logger;
    }

    public async Task HandleAsync(AccountCreatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== PROCESSING AccountCreatedEvent - Transaction ===");
        _logger.LogInformation("Account ID: {AccountId}", integrationEvent.AccountId);
        _logger.LogInformation("Account Number: {AccountNumber}", integrationEvent.AccountNumber);
        _logger.LogInformation("Account Name: {AccountName}", integrationEvent.AccountName);
        _logger.LogInformation("Owner Name: {OwnerName}", integrationEvent.OwnerName);
        _logger.LogInformation("Initial Balance: {Balance}", integrationEvent.InitialBalance);
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
            InitialBalance: 0,
            NewBalance: integrationEvent.InitialBalance,
            integrationEvent.Transaction.TransactionId,
            integrationEvent.Transaction.Description,
            integrationEvent.Transaction.CreatedAt
        );
        await _projectionHandler.HandleAsync(projectionRequest, cancellationToken);

        _logger.LogInformation("=== SUCCESSFULLY PROCESSED AccountCreatedEvent for Account {AccountId} ===", integrationEvent.AccountId);
    }
}
