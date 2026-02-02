using Projections.Banking.Domain.Balances;

namespace Projections.Banking.Features.Balances.AccountBalanceUpdatedProjection;

// Request
public record AccountBalanceUpdatedProjectionRequest(
    Guid AccountId,
    decimal NewBalance,
    DateTime UpdatedAt
);

// Handler
public interface IAccountBalanceUpdatedProjectionHandler
{
    Task HandleAsync(AccountBalanceUpdatedProjectionRequest request, CancellationToken cancellationToken = default);
}

public class AccountBalanceUpdatedProjectionHandler : IAccountBalanceUpdatedProjectionHandler
{
    private readonly IBalanceRepository _balanceRepository;

    public AccountBalanceUpdatedProjectionHandler(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository ?? throw new ArgumentNullException(nameof(balanceRepository));
    }

    public async Task HandleAsync(AccountBalanceUpdatedProjectionRequest request, CancellationToken cancellationToken = default)
    {
        await _balanceRepository.UpdateBalanceAsync(request.AccountId, request.NewBalance, request.UpdatedAt,cancellationToken);
    }
}
