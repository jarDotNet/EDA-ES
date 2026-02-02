using ESsample.Banking.API.Domain.Aggregates;
using ESsample.Banking.Shared.Events;
using EventBus;
using EventSourcing.Abstractions;

namespace ESsample.Banking.API.Features.DepositMoney;

// Request/Response
public record DepositMoneyRequest(
    Guid AccountId,
    decimal Amount,
    string Description = ""
);

public record DepositMoneyResponse(
    bool Success,
    decimal NewBalance = 0,
    long Version = 0,
    string? ErrorMessage = null
);

// Endpoint
public static class DepositMoneyEndpoint
{
    public record DepositRequest(decimal Amount, string? Description = null);

    public static void MapDepositMoneyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/accounts/{id:guid}/deposit", async (
            Guid id,
            DepositRequest request,
            IDepositMoneyHandler handler) =>
        {
            var depositRequest = new DepositMoneyRequest(
                id,
                request.Amount,
                request.Description ?? ""
            );

            var result = await handler.HandleAsync(depositRequest);

            return result.Success
                ? Results.Ok(new { Balance = result.NewBalance, Version = result.Version })
                : Results.BadRequest(result.ErrorMessage);
        })
        .WithName("DepositMoney")
        .WithTags("Accounts")
        .WithOpenApi();
    }
}

// Handler
public interface IDepositMoneyHandler
{
    Task<DepositMoneyResponse> HandleAsync(DepositMoneyRequest request, CancellationToken cancellationToken = default);
}

public class DepositMoneyHandler : IDepositMoneyHandler
{
    private readonly IAggregateRepository<Account> _accountRepository;
    private readonly IEventBusPublisher _eventBusPublisher;

    public DepositMoneyHandler(IAggregateRepository<Account> accountRepository, IEventBusPublisher eventBusPublisher)
    {
        _accountRepository = accountRepository;
        _eventBusPublisher = eventBusPublisher;
    }

    public async Task<DepositMoneyResponse> HandleAsync(DepositMoneyRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (request.Amount <= 0)
            {
                return new DepositMoneyResponse(false, ErrorMessage: "Deposit amount must be positive");
            }

            // Get account
            var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
            if (account is null)
            {
                return new DepositMoneyResponse(false, ErrorMessage: "Account not found");
            }

            var previousBalance = account.Balance;

            // Perform deposit
            account.Deposit(request.Amount, request.Description);
            await _accountRepository.SaveAsync(account, cancellationToken);

            // Publish deposit event
            await PublishDepositEvent(account, previousBalance, cancellationToken);

            return new DepositMoneyResponse(
                true,
                NewBalance: account.Balance,
                Version: account.Version
            );
        }
        catch (InvalidOperationException ex)
        {
            return new DepositMoneyResponse(false, ErrorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            return new DepositMoneyResponse(false, ErrorMessage: $"An error occurred: {ex.Message}");
        }
    }

    private async Task PublishDepositEvent(Account account, decimal previousBalance, CancellationToken cancellationToken = default)
    {
        var transaction = account.Transactions.Last();

        var balanceUpdatedEvent = new AccountBalanceUpdatedEvent(
            account.Id,
            account.AccountNumber,
            account.AccountName,
            previousBalance,
            NewBalance: account.Balance,
            new TransactionInfo(
                transaction.Id,
                transaction.Amount,
                transaction.Description,
                transaction.Date
            )
        );

        await _eventBusPublisher.PublishAsync(balanceUpdatedEvent, cancellationToken);
    }
}
