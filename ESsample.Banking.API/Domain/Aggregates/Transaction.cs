namespace ESsample.Banking.API.Domain.Aggregates;

public class Transaction
{
    public Guid Id { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal ClosingBalance { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime Date { get; init; }

    public decimal Amount => ClosingBalance - OpeningBalance;

    // Parameterless constructor for reconstruction
    public Transaction() { }

    // Constructor for creating new transaction

    public Transaction(decimal amount, decimal openingBalance, string description)
    {
        if (amount == 0)
            throw new ArgumentException("Amount cannot be zero");

        if (string.IsNullOrEmpty(description))
            throw new ArgumentException("Description cannot be empty");

        Id = Guid.NewGuid();
        OpeningBalance = openingBalance;
        ClosingBalance = openingBalance + amount;
        Description = description;
        Date = DateTime.UtcNow;
    }
}
