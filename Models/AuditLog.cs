using System.ComponentModel.DataAnnotations;

namespace active_directory_rest_api.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public int? ApiTokenId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Endpoint { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(10)]
        public string Method { get; set; } = string.Empty;
        
        public string? UserAgent { get; set; }
        
        public string? IpAddress { get; set; }
        
        public string? RequestBody { get; set; }
        
        public int? ResponseStatus { get; set; }
        
        public string? ResponseBody { get; set; }
        
        public int? ExecutionTimeMs { get; set; }
        
        public string? ErrorMessage { get; set; }
        
        public string? AdditionalData { get; set; }
        
        // Navigation property
        public virtual ApiToken? ApiToken { get; set; }
    }
}
