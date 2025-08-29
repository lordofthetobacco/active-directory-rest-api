using Microsoft.EntityFrameworkCore;
using active_directory_rest_api.Models;

namespace active_directory_rest_api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApiToken> ApiTokens { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApiToken entity
            modelBuilder.Entity<ApiToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Scopes).HasColumnType("text[]");
                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure AuditLog entity
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Endpoint).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Method).IsRequired().HasMaxLength(10);
                entity.Property(e => e.AdditionalData).HasColumnType("jsonb");
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.ApiTokenId);
                entity.HasIndex(e => e.Endpoint);
                
                entity.HasOne(e => e.ApiToken)
                    .WithMany(t => t.AuditLogs)
                    .HasForeignKey(e => e.ApiTokenId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
