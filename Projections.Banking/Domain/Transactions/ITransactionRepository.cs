namespace Projections.Banking.Domain.Transactions;

public interface ITransactionRepository
{   
    Task<IEnumerable<Transaction>> GetTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
