using Projections.Banking.Domain.Transactions;

namespace Projections.Banking.Features.Transactions.GetAccountTransactions;

internal class GetAccountTransactions
{
}
// Request/Response
public record GetAccountTransactionsRequest(
    Guid AccountId
);

public record GetAccountTransactionsResponse(
    bool Success,
    IList<TransactionDto>? Transactions = null,
    string? ErrorMessage = null
);

// DTO
public record TransactionDto(
    Guid AccountId,
    string AccountNumber,
    string AccountName,
    Guid TransactionId,
    decimal Amount,
    decimal OpeningBalance,
    decimal ClosingBalance,
    string Description,
    DateTime Date
);

// Handler
public interface IGetAccountTransactionsHandler
{
    Task<GetAccountTransactionsResponse> HandleAsync(GetAccountTransactionsRequest request,CancellationToken cancellationToken = default);
}

public class GetAccountTransactionsHandler : IGetAccountTransactionsHandler
{
    private readonly ITransactionRepository _transactionRepository;

    public GetAccountTransactionsHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<GetAccountTransactionsResponse> HandleAsync(GetAccountTransactionsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var transactions = await _transactionRepository.GetTransactionsAsync(request.AccountId, cancellationToken);

            var transactionDtos = transactions.Select(t => new TransactionDto(
                t.AccountId,
                t.AccountNumber,
                t.AccountName,
                t.TransactionId,
                t.Amount,
                t.OpeningBalance,
                t.ClosingBalance,
                t.Description,
                t.Date
            )).ToList();

            return new GetAccountTransactionsResponse(true, Transactions: transactionDtos);
        }
        catch (Exception ex)
        {
            return new GetAccountTransactionsResponse(false, ErrorMessage: $"An error occurred: {ex.Message}");
        }
    }
}
