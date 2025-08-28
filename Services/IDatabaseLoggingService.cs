using ActiveDirectory_API.Models;

namespace ActiveDirectory_API.Services;

public interface IDatabaseLoggingService
{
    Task<bool> IsAvailableAsync();
    Task LogApiRequestAsync(string action, string resource, object? requestData, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null);
    Task LogApiResponseAsync(string action, string resource, object? responseData, int statusCode, TimeSpan duration, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null);
    Task LogApiErrorAsync(string action, string resource, Exception exception, object? requestData, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null);
    Task LogAuthenticationSuccessAsync(string action, string resource, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null);
    Task LogAuthenticationFailureAsync(string action, string resource, string reason, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null);
    Task LogActiveDirectoryOperationAsync(string operation, string target, bool success, TimeSpan duration, string? errorMessage, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null);
    
    // Query methods for log analysis
    Task<IEnumerable<AuditLog>> GetLogsByCorrelationIdAsync(string correlationId);
    Task<IEnumerable<AuditLog>> GetLogsByTimeRangeAsync(DateTime startTime, DateTime endTime);
    Task<IEnumerable<AuditLog>> GetLogsByActionAsync(string action, int limit = 100);
    Task<IEnumerable<AuditLog>> GetLogsByUserContextAsync(string userContext, int limit = 100);
    Task<IEnumerable<AuditLog>> GetErrorLogsAsync(DateTime startTime, DateTime endTime, int limit = 100);
    Task<long> GetLogCountAsync(DateTime? startTime = null, DateTime? endTime = null);
    
    // Maintenance methods
    Task<bool> CleanupOldLogsAsync(DateTime cutoffDate);
    Task<bool> OptimizeTableAsync();
}
