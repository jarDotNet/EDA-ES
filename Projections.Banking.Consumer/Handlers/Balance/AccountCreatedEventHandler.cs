using ESsample.Banking.Shared.Events;
using EventBus;
using Projections.Banking.Features.Balances.AccountCreatedProjection;

namespace Projections.Banking.Consumer.Handlers.Balance;

public class AccountCreatedEventHandler : IIntegrationEventHandler<AccountCreatedEvent>
{
    private readonly IAccountCreatedProjectionHandler _projectionHandler;
    private readonly ILogger<AccountCreatedEventHandler> _logger;

    public AccountCreatedEventHandler(
        IAccountCreatedProjectionHandler projectionHandler,
        ILogger<AccountCreatedEventHandler> logger)
    {
        _projectionHandler = projectionHandler;
        _logger = logger;
    }

    public async Task HandleAsync(AccountCreatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== PROCESSING AccountCreatedEvent - Balance ===");
        _logger.LogInformation("Account ID: {AccountId}", integrationEvent.AccountId);
        _logger.LogInformation("Account Number: {AccountNumber}", integrationEvent.AccountNumber);
        _logger.LogInformation("Account Name: {AccountName}", integrationEvent.AccountName);
        _logger.LogInformation("Owner Name: {OwnerName}", integrationEvent.OwnerName);
        _logger.LogInformation("Initial Balance: {Balance}", integrationEvent.InitialBalance);
        _logger.LogInformation("Event ID: {EventId}", integrationEvent.Id);
        _logger.LogInformation("Created At: {CreatedAt}", integrationEvent.CreatedAt);

        var projectionRequest = new AccountCreatedProjectionRequest(
            integrationEvent.AccountId,
            integrationEvent.AccountNumber,
            integrationEvent.AccountName,
            integrationEvent.OwnerName,
            integrationEvent.InitialBalance,
            integrationEvent.Transaction.CreatedAt
        );
        await _projectionHandler.HandleAsync(projectionRequest, cancellationToken);

        _logger.LogInformation("=== SUCCESSFULLY PROCESSED AccountCreatedEvent for Account {AccountId} ===", integrationEvent.AccountId);
    }
}
