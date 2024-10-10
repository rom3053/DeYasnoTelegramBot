using Microsoft.EntityFrameworkCore;

namespace DeYasnoTelegramBot.Infrastructure.Persistence;

public class LogRecord
{
    public string? Id { get; set; }

    public string? Message { get; set; }

    public string? Level { get; set; }

    public DateTime? RaiseDate { get; set; }

    public string? Exception { get; set; }

    public string? Properties { get; set; }

    public string? Props_test { get; set; }
}

public class LoggerDbContext(DbContextOptions<LoggerDbContext> options) : DbContext(options)
{
    public DbSet<LogRecord> LogRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<LogRecord>(o =>
        {
            o.ToTable("_logs");
            o.Property(p => p.Id).HasColumnType("Text").HasColumnName("id");
            o.Property(p => p.Message).HasColumnType("Text").HasColumnName("message");
            o.Property(p => p.Level).HasColumnType("Varchar").HasColumnName("level");
            o.Property(p => p.RaiseDate).HasColumnType("TimestampTz").HasColumnName("raise_date");
            o.Property(p => p.Exception).HasColumnType("Text").HasColumnName("exception");
            o.Property(p => p.Properties).HasColumnType("Jsonb").HasColumnName("properties");
            o.Property(p => p.Props_test).HasColumnType("Jsonb").HasColumnName("props_test");
        });

        base.OnModelCreating(builder);
    }
}