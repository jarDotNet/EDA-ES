using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventSourcing.Postgres.Migrations;

[DbContext(typeof(EventStoreDbContext))]
[Migration("20240101000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create sequence for seq_id
        migrationBuilder.CreateSequence(
            name: "mt_events_seq_id_seq",
            schema: "public",
            startValue: 1L,
            incrementBy: 1,
            minValue: 1L,
            maxValue: 9223372036854775807L,
            cyclic: false);

        migrationBuilder.CreateTable(
            name: "mt_events",
            columns: table => new
            {
                seq_id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.SequenceHiLo),
                id = table.Column<Guid>(type: "uuid", nullable: false),
                stream_id = table.Column<Guid>(type: "uuid", nullable: true),
                version = table.Column<long>(type: "bigint", nullable: false),
                data = table.Column<string>(type: "jsonb", nullable: false),
                type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                tenant_id = table.Column<string>(type: "text", nullable: true, defaultValue: "*DEFAULT*"),
                mt_dotnet_type = table.Column<string>(type: "text", nullable: true),
                is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_mt_events", x => x.seq_id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_mt_events_stream_id",
            table: "mt_events",
            column: "stream_id");

        migrationBuilder.CreateIndex(
            name: "ix_mt_events_stream_version",
            table: "mt_events",
            columns: new[] { "stream_id", "version" });

        migrationBuilder.CreateIndex(
            name: "ix_mt_events_timestamp",
            table: "mt_events",
            column: "timestamp");

        // Set the default value for seq_id to use the sequence
        migrationBuilder.Sql("ALTER TABLE \"mt_events\" ALTER COLUMN \"seq_id\" SET DEFAULT nextval('mt_events_seq_id_seq'::regclass);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "mt_events");
        migrationBuilder.DropSequence(name: "mt_events_seq_id_seq", schema: "public");
    }
}
