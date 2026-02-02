using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EventSourcing.Postgres;

internal class EventStoreDbContext : DbContext
{
    public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options) : base(options)
    {
    }

    public DbSet<EventEntity> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("mt_events_seq_id_seq", schema: "public")
            .StartsAt(1)
            .IncrementsBy(1)
            .HasMax(9223372036854775807);

        modelBuilder.Entity<EventEntity>(entity =>
        {
            entity.ToTable("mt_events");
            
            entity.HasKey(e => e.SeqId);
            entity.Property(e => e.SeqId).HasColumnName("seq_id").HasDefaultValueSql("nextval('\"mt_events_seq_id_seq\"'::regclass)");
            
            entity.Property(e => e.Id).HasColumnName("id").IsRequired();
            entity.Property(e => e.StreamId).HasColumnName("stream_id");
            entity.Property(e => e.Version).HasColumnName("version").IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Timestamp).HasColumnName("timestamp").IsRequired();
            entity.Property(e => e.TenantId).HasColumnName("tenant_id").HasDefaultValue("*DEFAULT*");
            entity.Property(e => e.MtDotnetType).HasColumnName("mt_dotnet_type");
            entity.Property(e => e.IsArchived).HasColumnName("is_archived").HasDefaultValue(false);
            
            // Configure JSONB column
            entity.Property(e => e.Data)
                .HasColumnName("data")
                .HasColumnType("jsonb")
                .IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<JsonDocument>(v, (JsonSerializerOptions?)null)!
                );

            // Indexes for performance
            entity.HasIndex(e => e.StreamId).HasDatabaseName("ix_mt_events_stream_id");
            entity.HasIndex(e => new { e.StreamId, e.Version }).HasDatabaseName("ix_mt_events_stream_version");
            entity.HasIndex(e => e.Timestamp).HasDatabaseName("ix_mt_events_timestamp");
        });

        base.OnModelCreating(modelBuilder);
    }
}
