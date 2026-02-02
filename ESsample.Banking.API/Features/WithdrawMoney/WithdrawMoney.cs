using ESsample.Banking.API.Domain.Aggregates;
using ESsample.Banking.Shared.Events;
using EventBus;
using EventSourcing.Abstractions;

namespace ESsample.Banking.API.Features.WithdrawMoney;

// Request/Response
public record WithdrawMoneyRequest(
    Guid AccountId,
    decimal Amount,
    string Description = ""
);

public record WithdrawMoneyResponse(
    bool Success,
    decimal NewBalance = 0,
    long Version = 0,
    string? ErrorMessage = null
);

// Endpoint
public static class WithdrawMoneyEndpoint
{
    public record WithdrawRequest(decimal Amount, string? Description = null);

    public static void MapWithdrawMoneyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/accounts/{id:guid}/withdraw", async (
            Guid id,
            WithdrawRequest request,
            IWithdrawMoneyHandler handler) =>
        {
            var withdrawRequest = new WithdrawMoneyRequest(
                id,
                request.Amount,
                request.Description ?? ""
            );

            var result = await handler.HandleAsync(withdrawRequest);

            return result.Success
                ? Results.Ok(new { Balance = result.NewBalance, Version = result.Version })
                : Results.BadRequest(result.ErrorMessage);
        })
        .WithName("WithdrawMoney")
        .WithTags("Accounts")
        .WithOpenApi();
    }
}

// Handler
public interface IWithdrawMoneyHandler
{
    Task<WithdrawMoneyResponse> HandleAsync(WithdrawMoneyRequest request, CancellationToken cancellationToken = default);
}

public class WithdrawMoneyHandler : IWithdrawMoneyHandler
{
    private readonly IAggregateRepository<Account> _accountRepository;
    private readonly IEventBusPublisher _eventBusPublisher;

    public WithdrawMoneyHandler(IAggregateRepository<Account> accountRepository, IEventBusPublisher eventBusPublisher)
    {
        _accountRepository = accountRepository;
        _eventBusPublisher = eventBusPublisher;
    }

    public async Task<WithdrawMoneyResponse> HandleAsync(WithdrawMoneyRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (request.Amount <= 0)
            {
                return new WithdrawMoneyResponse(false, ErrorMessage: "Withdrawal amount must be positive");
            }

            // Get account
            var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
            if (account is null)
            {
                return new WithdrawMoneyResponse(false, ErrorMessage: "Account not found");
            }

            // Check sufficient funds
            if (account.Balance < request.Amount)
            {
                return new WithdrawMoneyResponse(
                    false,
                    NewBalance: account.Balance,
                    ErrorMessage: "Insufficient funds"
                );
            }

            var previousBalance = account.Balance;

            // Perform withdrawal
            account.Withdraw(request.Amount, request.Description);
            await _accountRepository.SaveAsync(account, cancellationToken);

            // Publish withdrawal event
            await PublishWithdrawalEvent(account, previousBalance, cancellationToken);

            return new WithdrawMoneyResponse(
                true,
                NewBalance: account.Balance,
                Version: account.Version
            );
        }
        catch (InvalidOperationException ex)
        {
            return new WithdrawMoneyResponse(false, ErrorMessage: ex.Message);
        }
        catch (Exception ex)
        {
            return new WithdrawMoneyResponse(false, ErrorMessage: $"An error occurred: {ex.Message}");
        }
    }

    private async Task PublishWithdrawalEvent(Account account, decimal previousBalance, CancellationToken cancellationToken = default)
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
