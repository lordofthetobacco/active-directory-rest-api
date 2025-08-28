using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActiveDirectory_API.Models;

[Table("audit_logs")]
public class AuditLog
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    [Required]
    [Column("correlation_id")]
    [StringLength(50)]
    public string CorrelationId { get; set; } = string.Empty;

    [Required]
    [Column("log_type")]
    [StringLength(50)]
    public string LogType { get; set; } = string.Empty; // API_REQUEST, API_RESPONSE, API_ERROR, AUTH_SUCCESS, AUTH_FAILURE, AD_OPERATION

    [Required]
    [Column("action")]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [Column("resource")]
    [StringLength(200)]
    public string Resource { get; set; } = string.Empty;

    [Column("user_context")]
    [StringLength(500)]
    public string? UserContext { get; set; }

    [Column("request_data", TypeName = "jsonb")]
    public string? RequestData { get; set; }

    [Column("response_data", TypeName = "jsonb")]
    public string? ResponseData { get; set; }

    [Column("status_code")]
    public int? StatusCode { get; set; }

    [Column("duration_ms")]
    public double? DurationMs { get; set; }

    [Column("error_message")]
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    [Column("exception_details", TypeName = "text")]
    public string? ExceptionDetails { get; set; }

    [Column("ip_address")]
    [StringLength(45)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [StringLength(500)]
    public string? UserAgent { get; set; }

    [Column("http_method")]
    [StringLength(10)]
    public string? HttpMethod { get; set; }

    [Column("endpoint")]
    [StringLength(200)]
    public string? Endpoint { get; set; }

    // Additional fields for Active Directory operations
    [Column("ad_operation")]
    [StringLength(100)]
    public string? AdOperation { get; set; }

    [Column("ad_target")]
    [StringLength(200)]
    public string? AdTarget { get; set; }

    [Column("ad_success")]
    public bool? AdSuccess { get; set; }

    // Indexes will be created via migrations
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
