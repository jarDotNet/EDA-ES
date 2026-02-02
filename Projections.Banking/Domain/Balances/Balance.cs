namespace Projections.Banking.Domain.Balances;

public class Balance
{
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; } = string.Empty;
    public string AccountName { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public decimal CurrentBalance { get; private set; }
    public DateTime LastUpdated { get; private set; }

    // Parameterless constructor for reconstruction
    public Balance() { }

    public Balance(Guid accountId, string accountNumber, string accountName, string ownerName, decimal initialBalance, DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(accountNumber, nameof(accountNumber));
        ArgumentNullException.ThrowIfNullOrEmpty(accountName, nameof(accountName));
        ArgumentNullException.ThrowIfNullOrEmpty(ownerName, nameof(ownerName));

        if (initialBalance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial balance cannot be less than or equal to zero");
        }

        AccountId = accountId;
        AccountNumber = accountNumber;
        AccountName = accountName;
        OwnerName = ownerName;

        UpdateBalance(initialBalance, createdAt);
    }

    public void UpdateBalance(decimal newBalance, DateTime updatedAt)
    {      
        CurrentBalance = newBalance;
        LastUpdated = updatedAt;
    }
}
