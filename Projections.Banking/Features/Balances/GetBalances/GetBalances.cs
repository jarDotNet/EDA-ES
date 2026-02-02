using Projections.Banking.Domain.Balances;

namespace Projections.Banking.Features.Balances.GetBalances;

// Response
public record GetBalancesResponse(
    bool Success,
    IList<BalanceDto>? Balances = null,
    string? ErrorMessage = null
);

// DTO
public record BalanceDto(
    Guid AccountId,
    string AccountNumber,
    string AccountName,
    string OwnerName,
    decimal Balance,
    DateTime LastUpdated
);

// Handler
public interface IGetBalancesHandler
{
    Task<GetBalancesResponse> HandleAsync(CancellationToken cancellationToken = default);
}

public class GetBalancesHandler : IGetBalancesHandler
{
    private readonly IBalanceRepository _balanceRepository;

    public GetBalancesHandler(IBalanceRepository balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<GetBalancesResponse> HandleAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var balances = await _balanceRepository.GetBalancesAsync(cancellationToken);

            var balanceDtos = balances.Select(b => new BalanceDto(
                b.AccountId,
                b.AccountNumber,
                b.AccountName,
                b.OwnerName,
                b.CurrentBalance,
                b.LastUpdated
            )).ToList();

            return new GetBalancesResponse(true, Balances: balanceDtos);
        }
        catch (Exception ex)
        {
            return new GetBalancesResponse(false, ErrorMessage: $"An error occurred: {ex.Message}");
        }
    }
}

