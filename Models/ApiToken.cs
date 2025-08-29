using System.ComponentModel.DataAnnotations;

namespace active_directory_rest_api.Models
{
    public class ApiToken
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string TokenHash { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public string[] Scopes { get; set; } = Array.Empty<string>();
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiresAt { get; set; }
        
        public DateTime? LastUsedAt { get; set; }
        
        // Navigation property
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
