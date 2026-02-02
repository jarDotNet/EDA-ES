using ESsample.Banking.API.Domain.Aggregates;
using ESsample.Banking.Shared.Events;
using EventBus;
using EventSourcing.Abstractions;

namespace ESsample.Banking.API.Features.TransferMoney;

// Request/Response
public record TransferMoneyRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Description = ""
);

public record TransferMoneyResponse(
    bool Success,
    decimal FromAccountNewBalance = 0,
    decimal ToAccountNewBalance = 0,
    string? ErrorMessage = null
);

// Endpoint
public static class TransferMoneyEndpoint
{
    public record TransferRequest(Guid ToAccountId, decimal Amount, string? Description = null);

    public static void MapTransferMoneyEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/accounts/{id:guid}/transfer", async (
            Guid id,
            TransferRequest request,
            ITransferMoneyHandler handler) =>
        {
            var transferRequest = new TransferMoneyRequest(
                id,
                request.ToAccountId,
                request.Amount,
                request.Description ?? ""
            );

            var result = await handler.HandleAsync(transferRequest);

            return result.Success
                ? Results.Ok(new { result.FromAccountNewBalance, result.ToAccountNewBalance })
                : Results.BadRequest(result.ErrorMessage);
        })
        .WithName("TransferMoney")
        .WithTags("Accounts")
        .WithOpenApi();
    }
}

// Handler
public interface ITransferMoneyHandler
{
    Task<TransferMoneyResponse> HandleAsync(TransferMoneyRequest request, CancellationToken cancellationToken = default);
}

public class TransferMoneyHandler : ITransferMoneyHandler
{
    private readonly IAggregateRepository<Account> _accountRepository;
    private readonly IEventBusPublisher _eventBusPublisher;

    public TransferMoneyHandler(IAggregateRepository<Account> accountRepository, IEventBusPublisher eventBusPublisher)
    {
        _accountRepository = accountRepository;
        _eventBusPublisher = eventBusPublisher;
    }

    public async Task<TransferMoneyResponse> HandleAsync(TransferMoneyRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (request.Amount < 0)
            {
                return new TransferMoneyResponse(false, ErrorMessage: "Amount cannot be negative");
            }

            // Get accounts
            var fromAccount = await _accountRepository.GetByIdAsync(request.FromAccountId, cancellationToken);
            if (fromAccount is null)
            {
                return new TransferMoneyResponse(false, ErrorMessage: "Source Account not found");
            }

            var toAccount = await _accountRepository.GetByIdAsync(request.ToAccountId, cancellationToken);
            if (toAccount is null)
            {
                return new TransferMoneyResponse(false, ErrorMessage: "Destination Account not found");
            }

            // Perform transfer
            var widthdrawDescription = string.IsNullOrEmpty(request.Description)
                ? $"Transfer to {toAccount.AccountNumber}"
                : request.Description;
            var depositDescription = string.IsNullOrEmpty(request.Description)
                ? $"Transfer from {fromAccount.AccountNumber}"
                : request.Description;

            fromAccount.Withdraw(request.Amount, widthdrawDescription);
            toAccount.Deposit(request.Amount, depositDescription);

            await _accountRepository.SaveBatchAsync([fromAccount, toAccount], cancellationToken);   

            // Publish balance updates events
            await PublishMoneyTransferlEvent(fromAccount, toAccount, cancellationToken);

            return new TransferMoneyResponse(true, fromAccount.Balance, toAccount.Balance);
        }
        catch (Exception ex)
        {
            return new TransferMoneyResponse(false, ErrorMessage: ex.Message);
        }
    }

    private async Task PublishMoneyTransferlEvent(Account fromAccount, Account toAccount, CancellationToken cancellationToken = default)
    {
        var fromTransaction = fromAccount.Transactions.Last();
        var toTransaction = toAccount.Transactions.Last();

        var fromBalanceUpdatedEvent = new AccountBalanceUpdatedEvent(
            fromAccount.Id,
            fromAccount.AccountNumber,
            fromAccount.AccountName,
            fromTransaction.OpeningBalance,
            NewBalance: fromAccount.Balance,
            new TransactionInfo(
                fromTransaction.Id,
                fromTransaction.Amount,
                fromTransaction.Description,
                fromTransaction.Date
            )
        );

        var toBalanceUpdatedEvent = new AccountBalanceUpdatedEvent(
            toAccount.Id,
            toAccount.AccountNumber,
            toAccount.AccountName,
            toTransaction.OpeningBalance,
            NewBalance: toAccount.Balance,
            new TransactionInfo(
                toTransaction.Id,
                toTransaction.Amount,
                toTransaction.Description,
                toTransaction.Date
            )
        );

        await _eventBusPublisher.PublishAsync([fromBalanceUpdatedEvent, toBalanceUpdatedEvent], cancellationToken);
    }
}
