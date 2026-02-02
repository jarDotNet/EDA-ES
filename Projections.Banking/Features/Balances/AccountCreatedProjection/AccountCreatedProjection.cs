using Projections.Banking.Domain.Balances;

namespace Projections.Banking.Features.Balances.AccountCreatedProjection;

// Request
public record AccountCreatedProjectionRequest(
    Guid AccountId,
    string AccountNumber,
    string AccountName,
    string OwnerName,
    decimal InitialBalance,
    DateTime CreatedAt
);

// Handler
public interface IAccountCreatedProjectionHandler
{
    Task HandleAsync(AccountCreatedProjectionRequest request, CancellationToken cancellationToken = default);
}

public class AccountCreatedProjectionHandler : IAccountCreatedProjectionHandler
{
    private readonly IBalanceRepository _balanceRepository;

    public AccountCreatedProjectionHandler(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository ?? throw new ArgumentNullException(nameof(balanceRepository));
    }

    public async Task HandleAsync(AccountCreatedProjectionRequest request, CancellationToken cancellationToken = default)
    {
        var balance = new Balance(
            request.AccountId,
            request.AccountNumber,
            request.AccountName,
            request.OwnerName,
            request.InitialBalance,
            request.CreatedAt
        );

        await _balanceRepository.CreateInitialBalanceAsync(balance, cancellationToken);
    }
}
