using ESsample.Banking.API.Domain.Aggregates;
using EventSourcing.Abstractions;

namespace ESsample.Banking.API.Features.GetAccount;

// Request/Response
public record GetAccountRequest(Guid AccountId);

public record GetAccountResponse(
    bool Success,
    AccountDto? Account = null,
    string? ErrorMessage = null
);

// DTO
public record AccountDto(
    Guid Id,
    string AccountNumber,
    string OwnerName,
    decimal Balance,
    long Version,
    bool IsActive
);

// Endpoint
public static class GetAccountEndpoint
{
    public static void MapGetAccountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/accounts/{id:guid}", async (
            Guid id,
            IGetAccountHandler handler) =>
        {
            var request = new GetAccountRequest(id);
            var result = await handler.HandleAsync(request);

            return result.Success
                ? Results.Ok(result.Account)
                : Results.NotFound(result.ErrorMessage);
        })
        .WithName("GetAccount")
        .WithTags("Accounts")
        .WithOpenApi();
    }
}

// Handler
public interface IGetAccountHandler
{
    Task<GetAccountResponse> HandleAsync(GetAccountRequest request, CancellationToken cancellationToken = default);
}

public class GetAccountHandler : IGetAccountHandler
{
    private readonly IAggregateRepository<Account> _accountRepository;

    public GetAccountHandler(IAggregateRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<GetAccountResponse> HandleAsync(GetAccountRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
            if (account is null)
            {
                return new GetAccountResponse(false, ErrorMessage: "Account not found");
            }

            var accountDto = new AccountDto(
                account.Id,
                account.AccountNumber,
                account.OwnerName,
                account.Balance,
                account.Version,
                account.IsActive
            );

            return new GetAccountResponse(true, Account: accountDto);
        }
        catch (Exception ex)
        {
            return new GetAccountResponse(false, ErrorMessage: $"An error occurred: {ex.Message}");
        }
    }
}
