namespace Projections.Banking.Domain.Balances;

public interface IBalanceRepository
{   
    Task<IEnumerable<Balance>> GetBalancesAsync(CancellationToken cancellationToken = default);
    Task CreateInitialBalanceAsync(Balance balance, CancellationToken cancellationToken = default);
    Task UpdateBalanceAsync(Guid accountId, decimal newBalance, DateTime updatedAt, CancellationToken cancellationToken = default);
}
