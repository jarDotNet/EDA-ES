using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Projections.Banking.Postgres.Migrations;

[DbContext(typeof(BankingDbContext))]
[Migration("20250101000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create Balances table
        migrationBuilder.CreateTable(
            name: "Balances",
            columns: table => new
            {
                AccountId = table.Column<Guid>(nullable: false),
                AccountNumber = table.Column<string>(maxLength: 50, nullable: false),
                AccountName = table.Column<string>(maxLength: 100, nullable: false),
                OwnerName = table.Column<string>(maxLength: 100, nullable: false),
                CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                LastUpdated = table.Column<DateTime>(nullable: false)

            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Balances", x => x.AccountId);
            });

        // Create Transactions table
        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                TransactionId = table.Column<Guid>(nullable: false),
                AccountId = table.Column<Guid>(nullable: false),
                AccountNumber = table.Column<string>(maxLength: 50, nullable: false),
                AccountName = table.Column<string>(maxLength: 100, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ClosingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Description = table.Column<string>(maxLength: 255, nullable: false),
                Date = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                //table.ForeignKey(
                //    name: "FK_Transactions_Balances_AccountId",
                //    column: x => x.AccountId,
                //    principalTable: "Balances",
                //    principalColumn: "AccountId",
                //    onDelete: ReferentialAction.Restrict);
            });

        // Create an index on AccountId for Transactions (optional for optimization)
        migrationBuilder.CreateIndex(
            name: "IX_Transactions_AccountId",
            table: "Transactions",
            column: "AccountId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop Transactions table
        migrationBuilder.DropTable(
            name: "Transactions");

        // Drop Balances table
        migrationBuilder.DropTable(
            name: "Balances");
    }
}
