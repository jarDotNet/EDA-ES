using Microsoft.EntityFrameworkCore;
using Projections.Banking.Domain.Balances;
using Projections.Banking.Domain.Transactions;

namespace Projections.Banking.Postgres;

internal class BankingDbContext : DbContext
{
    public DbSet<Balance> Balances { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    public BankingDbContext(DbContextOptions<BankingDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the Balance entity
        modelBuilder.Entity<Balance>(entity =>
        {
            entity.ToTable("Balances");

            // Primary key
            entity.HasKey(b => b.AccountId);

            // Properties (with required fields)
            entity.Property(b => b.AccountId)
                .IsRequired();

            entity.Property(b => b.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(b => b.AccountName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(b => b.OwnerName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(b => b.CurrentBalance)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(b => b.LastUpdated)
                .IsRequired();
        });

        // Configure the Transaction entity
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions"); // Table name in the database

            // Composite primary key (TransactionId should be unique, with AccountId for context)
            entity.HasKey(t => t.TransactionId);

            // Properties (with required fields)
            entity.Property(t => t.TransactionId)
                .IsRequired();

            entity.Property(t => t.AccountId)
                .IsRequired();

            entity.Property(t => t.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(t => t.AccountName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(t => t.OpeningBalance)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(t => t.ClosingBalance)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(t => t.Date)
                .IsRequired();
        });

        /*
        // This is a possible relationships between Balance and Transaction
        // Assuming that a `Transaction` may belong to a `Balance`
        modelBuilder.Entity<Transaction>()
            .HasOne<Balance>()
            .WithMany() // A Balance may optionally have many related Transactions
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict); // Avoid cascade deletes
        */
    }
}
