using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ActiveDirectory_API.Models;

[Table("performance_metrics")]
public class PerformanceMetric
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("endpoint")]
    [StringLength(200)]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    [Column("http_method")]
    [StringLength(10)]
    public string HttpMethod { get; set; } = string.Empty;

    [Required]
    [Column("action")]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    [Required]
    [Column("response_time_ms")]
    public double ResponseTimeMs { get; set; }

    [Required]
    [Column("status_code")]
    public int StatusCode { get; set; }

    [Column("request_size_bytes")]
    public long? RequestSizeBytes { get; set; }

    [Column("response_size_bytes")]
    public long? ResponseSizeBytes { get; set; }

    [Column("correlation_id")]
    [StringLength(50)]
    public string? CorrelationId { get; set; }

    [Column("user_context")]
    [StringLength(500)]
    public string? UserContext { get; set; }

    [Column("ip_address")]
    [StringLength(45)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [StringLength(500)]
    public string? UserAgent { get; set; }

    [Column("is_success")]
    public bool IsSuccess { get; set; }

    [Column("error_message")]
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    // Performance categorization
    [Column("performance_category")]
    [StringLength(20)]
    public string PerformanceCategory { get; set; } = string.Empty; // FAST, NORMAL, SLOW, VERY_SLOW

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
