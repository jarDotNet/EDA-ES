namespace Projections.Banking.Domain.Transactions;

public class Transaction
{
    // Parameterless constructor for reconstruction
    public Transaction() { }

    public required Guid TransactionId { get; set; }
    public required Guid AccountId { get; set; }
    public required string AccountNumber { get; set; }
    public required string AccountName { get; set; }
    public required decimal Amount { get; set; }
    public required decimal OpeningBalance { get; set; }
    public required decimal ClosingBalance { get; set; }
    public required string Description { get; set; }
    public DateTime Date { get; set; }
}
