using ESsample.Banking.API.Domain.Aggregates;
using ESsample.Banking.Shared.Events;
using EventBus;
using EventSourcing.Abstractions;

namespace ESsample.Banking.API.Features.CreateAccount;

// Request/Response
public record CreateAccountRequest(
    string AccountNumber,
    string AccountName,
    string OwnerName,
    decimal InitialBalance
);

public record CreateAccountResponse(
    bool Success,
    Guid? AccountId = null,
    string? ErrorMessage = null
);

// Endpoint
public static class CreateAccountEndpoint
{
    public static void MapCreateAccountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/accounts", async (
            CreateAccountRequest request,
            ICreateAccountHandler handler) =>
        {
            var result = await handler.HandleAsync(request);

            return result.Success
                ? Results.Ok(new { AccountId = result.AccountId })
                : Results.BadRequest(result.ErrorMessage);
        })
        .WithName("CreateAccount")
        .WithTags("Accounts")
        .WithOpenApi();
    }
}

// Handler
public interface ICreateAccountHandler
{
    Task<CreateAccountResponse> HandleAsync(CreateAccountRequest request, CancellationToken cancellationToken = default);
}

public class CreateAccountHandler : ICreateAccountHandler
{
    private readonly IAggregateRepository<Account> _accountRepository;
    private readonly IEventBusPublisher _eventBusPublisher;

    public CreateAccountHandler(IAggregateRepository<Account> accountRepository, IEventBusPublisher eventBusPublisher)
    {
        _accountRepository = accountRepository;
        _eventBusPublisher = eventBusPublisher;
    }

    public async Task<CreateAccountResponse> HandleAsync(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                return new CreateAccountResponse(false, ErrorMessage: "Account number is required");
            }

            if (string.IsNullOrWhiteSpace(request.AccountName))
            {
                return new CreateAccountResponse(false, ErrorMessage: "Account name is required");
            }

            if (string.IsNullOrWhiteSpace(request.OwnerName))
            {
                return new CreateAccountResponse(false, ErrorMessage: "Owner name is required");
            }

            if (request.InitialBalance < 0)
            {
                return new CreateAccountResponse(false, ErrorMessage: "Initial balance cannot be negative");
            }

            // Create and save account
            var account = new Account(
                request.AccountNumber, 
                request.AccountName, 
                request.OwnerName, 
                request.InitialBalance
            );
            await _accountRepository.SaveAsync(account, cancellationToken);

            // Publish account created event
            await PublishAccountCreatedEventAsync(account, cancellationToken);

            return new CreateAccountResponse(true, AccountId: account.Id);
        }
        catch (Exception ex)
        {
            return new CreateAccountResponse(false, ErrorMessage: ex.Message);
        }
    }

    private async Task PublishAccountCreatedEventAsync(Account account, CancellationToken cancellationToken = default)
    {
        var transaction = account.Transactions.Last();

        var accountCreatedEvent = new AccountCreatedEvent(
            account.Id,
            account.AccountNumber,
            account.AccountName,
            account.OwnerName,
            InitialBalance: transaction.ClosingBalance,
            new TransactionInfo(
                transaction.Id,
                transaction.Amount,
                transaction.Description,
                transaction.Date
            )
        );

        await _eventBusPublisher.PublishAsync(accountCreatedEvent, cancellationToken);
    }
}
