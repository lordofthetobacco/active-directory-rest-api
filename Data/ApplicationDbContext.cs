using ActiveDirectory_API.Models;
using Microsoft.EntityFrameworkCore;

namespace ActiveDirectory_API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AuditLog entity
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Create indexes for better query performance
            entity.HasIndex(e => e.Timestamp).HasDatabaseName("ix_audit_logs_timestamp");
            entity.HasIndex(e => e.CorrelationId).HasDatabaseName("ix_audit_logs_correlation_id");
            entity.HasIndex(e => e.LogType).HasDatabaseName("ix_audit_logs_log_type");
            entity.HasIndex(e => e.Action).HasDatabaseName("ix_audit_logs_action");
            entity.HasIndex(e => e.Resource).HasDatabaseName("ix_audit_logs_resource");
            entity.HasIndex(e => e.UserContext).HasDatabaseName("ix_audit_logs_user_context");
            entity.HasIndex(e => e.StatusCode).HasDatabaseName("ix_audit_logs_status_code");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_audit_logs_created_at");
            
            // Composite indexes for common query patterns
            entity.HasIndex(e => new { e.Timestamp, e.LogType }).HasDatabaseName("ix_audit_logs_timestamp_log_type");
            entity.HasIndex(e => new { e.CorrelationId, e.Timestamp }).HasDatabaseName("ix_audit_logs_correlation_timestamp");
            entity.HasIndex(e => new { e.Action, e.Timestamp }).HasDatabaseName("ix_audit_logs_action_timestamp");
            
            // Configure timestamp column to use UTC
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
            
            // Configure created_at and updated_at columns
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
            
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
            
            // Configure JSONB columns for PostgreSQL
            entity.Property(e => e.RequestData)
                .HasColumnType("jsonb");
            
            entity.Property(e => e.ResponseData)
                .HasColumnType("jsonb");
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<AuditLog>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
