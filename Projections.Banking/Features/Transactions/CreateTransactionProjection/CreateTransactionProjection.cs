using Projections.Banking.Domain.Transactions;

namespace Projections.Banking.Features.Transactions.CreateTransactionProjection;

// Request
public record CreateTransactionProjectionRequest(
    Guid AccountId,
    string AccountNumber,
    string AccountName,
    decimal Amount,
    decimal InitialBalance,
    decimal NewBalance,
    Guid TransactionId,
    string Description,
    DateTime CreatedAt
);

// Handler
public interface ICreateTransactionProjectionHandler
{
    Task HandleAsync(CreateTransactionProjectionRequest request, CancellationToken cancellationToken = default);
}

public class CreateTransactionProjectionHandler : ICreateTransactionProjectionHandler
{
    private readonly ITransactionRepository _transactionRepository;

    public CreateTransactionProjectionHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
    }

    public async Task HandleAsync(CreateTransactionProjectionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = new Transaction
        {
            TransactionId = request.TransactionId,
            AccountId = request.AccountId,
            AccountName = request.AccountName,
            AccountNumber = request.AccountNumber,
            Amount = request.Amount,
            OpeningBalance = request.InitialBalance,
            ClosingBalance = request.NewBalance,
            Description = request.Description,
            Date = request.CreatedAt
        };

        await _transactionRepository.CreateTransactionAsync(transaction, cancellationToken);
    }
}
