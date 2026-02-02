using Microsoft.EntityFrameworkCore;
using Projections.Banking.Domain.Transactions;

namespace Projections.Banking.Postgres.Repositories;

internal class TransactionRepository : ITransactionRepository
{
    private readonly BankingDbContext _context;

    public TransactionRepository(BankingDbContext context)
    {
         _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateTransactionAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(transaction, nameof(transaction));

        // Check if the transaction already exists
        var exists = await _context.Transactions
            .AnyAsync(t => t.TransactionId.ToString() == transaction.TransactionId.ToString(), cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"A transaction '{transaction.TransactionId}' for account ID '{transaction.AccountId}' already exists.");
        }

        // Add the initial transaction
        await _context.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
