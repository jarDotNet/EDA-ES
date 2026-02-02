using ESsample.Banking.Shared.Events;
using EventBus;
using Projections.Banking.Features.Balances.AccountBalanceUpdatedProjection;

namespace Projections.Banking.Consumer.Handlers.Balance;

public class AccountBalanceUpdatedEventHandler : IIntegrationEventHandler<AccountBalanceUpdatedEvent>
{
    private readonly IAccountBalanceUpdatedProjectionHandler _projectionHandler;
    private readonly ILogger<AccountBalanceUpdatedEventHandler> _logger;

    public AccountBalanceUpdatedEventHandler(
        IAccountBalanceUpdatedProjectionHandler projectionHandler,
        ILogger<AccountBalanceUpdatedEventHandler> logger)
    {
        _projectionHandler = projectionHandler;
        _logger = logger;
    }

    public async Task HandleAsync(AccountBalanceUpdatedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("=== PROCESSING AccountBalanceUpdatedEvent - Balance ===");
        _logger.LogInformation("Account ID: {AccountId}", integrationEvent.AccountId);
        _logger.LogInformation("Previous Balance: {PreviousBalance}", integrationEvent.PreviousBalance);
        _logger.LogInformation("New Balance: {NewBalance}", integrationEvent.NewBalance);
        _logger.LogInformation("Event ID: {EventId}", integrationEvent.Id);
        _logger.LogInformation("Created At: {CreatedAt}", integrationEvent.CreatedAt);


        var projectionRequest = new AccountBalanceUpdatedProjectionRequest(
            integrationEvent.AccountId,
            integrationEvent.NewBalance,
            integrationEvent.Transaction.CreatedAt
        );
        await _projectionHandler.HandleAsync(projectionRequest, cancellationToken);

        _logger.LogInformation("=== SUCCESSFULLY PROCESSED AccountBalanceUpdatedEvent for Account {AccountId} ===", integrationEvent.AccountId);
    }
}

