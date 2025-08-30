using Microsoft.EntityFrameworkCore;
using active_directory_rest_api.Models;

namespace active_directory_rest_api.Data;

public class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options) : base(options)
    {
    }

    public DbSet<ApiLog> ApiLogs { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Endpoint).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ApiKey).HasMaxLength(100);
            entity.Property(e => e.StatusCode).IsRequired();
            entity.Property(e => e.ResponseTime).IsRequired();
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            
            // Make the Key unique
            entity.HasIndex(e => e.Key).IsUnique();
        });
    }
}

public class ApiLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public int StatusCode { get; set; }
    public long ResponseTime { get; set; }
    public string? ErrorMessage { get; set; }
}
