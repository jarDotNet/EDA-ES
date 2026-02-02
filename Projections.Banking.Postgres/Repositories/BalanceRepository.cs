using Microsoft.EntityFrameworkCore;
using Projections.Banking.Domain.Balances;

namespace Projections.Banking.Postgres.Repositories;

internal class BalanceRepository : IBalanceRepository
{
    private readonly BankingDbContext _context;

    public BalanceRepository(BankingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Balance>> GetBalancesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Balances
            .AsNoTracking()
            .OrderBy(b => b.AccountName)
            .ToListAsync(cancellationToken);
    }

    public async Task CreateInitialBalanceAsync(Balance balance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(balance, nameof(balance));

        // Check if the account already exists
        var accountExists = await _context.Balances
            .AsNoTracking()
            .AnyAsync(b => b.AccountId == balance.AccountId, cancellationToken);

        if (accountExists)
        {
            throw new InvalidOperationException($"A balance for account ID '{balance.AccountId}' already exists.");
        }

        // Check if the account number already exists
        var accountNumberExists = await _context.Balances
            .AsNoTracking()
            .AnyAsync(b => b.AccountNumber == balance.AccountNumber, cancellationToken);

        if (accountNumberExists)
        {
            throw new InvalidOperationException($"A balance for account number '{balance.AccountNumber}' already exists.");
        }

        // Add the initial balance
        await _context.Balances.AddAsync(balance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBalanceAsync(Guid accountId, decimal newBalance, DateTime updatedAt, CancellationToken cancellationToken = default)
    {
        // Retrieve the existing balance record
        var balance = await _context.Balances
            .FirstOrDefaultAsync(b => b.AccountId == accountId, cancellationToken) 
            ?? throw new KeyNotFoundException($"No balance found for account ID '{accountId}'.");

        // Update the balance amount
        balance.UpdateBalance(newBalance, updatedAt);

        // Save changes to the database
        _context.Balances.Update(balance);
        await _context.SaveChangesAsync(cancellationToken);
    }
}