using ESsample.Banking.API.Domain.Events;
using EventSourcing;

namespace ESsample.Banking.API.Domain.Aggregates;

public class Account : AggregateRoot
{
    public string AccountNumber { get; private set; } = string.Empty;
    public string AccountName { get; private set; } = string.Empty;
    public string OwnerName { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    private List<Transaction> _transactions = [];

    // Parameterless constructor for reconstruction
    public Account() { }

    // Constructor for creating new account
    public Account(string accountNumber, string accountName, string ownerName, decimal initialBalance)
    {
        if (string.IsNullOrEmpty(accountNumber))
            throw new ArgumentException("Account number cannot be empty");

        if (string.IsNullOrEmpty(accountName))
            throw new ArgumentException("Account name cannot be empty");

        if (string.IsNullOrEmpty(ownerName))
            throw new ArgumentException("Owner name cannot be empty");

        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative");

        Id = Guid.NewGuid();

        RaiseEvent(new AccountCreated
        {
            AccountNumber = accountNumber,
            AccountName = accountName,
            OwnerName = ownerName,
            InitialBalance = initialBalance,
            Transaction = new Transaction(initialBalance, openingBalance: 0, "Initial deposit")
        });
    }

    public void Deposit(decimal amount, string description = "")
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive");

        if (!IsActive)
            throw new InvalidOperationException("Cannot deposit to inactive account");

        RaiseEvent(new MoneyDeposited
        {
            Amount = amount,
            Transaction = new Transaction(amount, openingBalance: Balance, description)
        });
    }

    public void Withdraw(decimal amount, string description = "")
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive");

        if (!IsActive)
            throw new InvalidOperationException("Cannot withdraw from inactive account");

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient funds");

        RaiseEvent(new MoneyWithdrawn
        {
            Amount = amount,
            Transaction = new Transaction(-amount, openingBalance: Balance, description)
        });
    }

    public void Apply(AccountCreated created)
    {
        AccountNumber = created.AccountNumber;
        AccountName = created.AccountName;
        OwnerName = created.OwnerName;
        Balance = created.InitialBalance;
        IsActive = true;

        _transactions.Add(created.Transaction);
    }

    public void Apply(MoneyDeposited deposited)
    {
       Balance += deposited.Amount;

        _transactions.Add(deposited.Transaction);
    }

    public void Apply(MoneyWithdrawn withdrawn)
    {
        Balance -= withdrawn.Amount;

        _transactions.Add(withdrawn.Transaction);
    }
}
