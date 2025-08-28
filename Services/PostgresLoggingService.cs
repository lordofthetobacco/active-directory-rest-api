using ActiveDirectory_API.Data;
using ActiveDirectory_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ActiveDirectory_API.Services;

public class PostgresLoggingService : IDatabaseLoggingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PostgresLoggingService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PostgresLoggingService(ApplicationDbContext context, ILogger<PostgresLoggingService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connection test failed");
            return false;
        }
    }

    public async Task LogApiRequestAsync(string action, string resource, object? requestData, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                LogType = "API_REQUEST",
                Action = action,
                Resource = resource,
                UserContext = userContext,
                RequestData = SerializeToJson(requestData),
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = userAgent ?? GetUserAgent(),
                HttpMethod = httpMethod ?? GetHttpMethod(),
                Endpoint = endpoint ?? GetEndpoint()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log API request to database: {Action} on {Resource}", action, resource);
            // Fallback to file logging
            _logger.LogInformation("API Request: {Action} on {Resource} | User: {UserContext} | CorrelationId: {CorrelationId} | Request: {RequestData}", 
                action, resource, userContext, correlationId, SerializeToJson(requestData));
        }
    }

    public async Task LogApiResponseAsync(string action, string resource, object? responseData, int statusCode, TimeSpan duration, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                LogType = "API_RESPONSE",
                Action = action,
                Resource = resource,
                UserContext = userContext,
                ResponseData = SerializeToJson(responseData),
                StatusCode = statusCode,
                DurationMs = duration.TotalMilliseconds,
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = userAgent ?? GetUserAgent(),
                HttpMethod = httpMethod ?? GetHttpMethod(),
                Endpoint = endpoint ?? GetEndpoint()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log API response to database: {Action} on {Resource}", action, resource);
            // Fallback to file logging
            _logger.LogInformation("API Response: {Action} on {Resource} | Status: {StatusCode} | Duration: {Duration}ms | User: {UserContext} | CorrelationId: {CorrelationId}", 
                action, resource, statusCode, duration.TotalMilliseconds, userContext, correlationId);
        }
    }

    public async Task LogApiErrorAsync(string action, string resource, Exception exception, object? requestData, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                LogType = "API_ERROR",
                Action = action,
                Resource = resource,
                UserContext = userContext,
                RequestData = SerializeToJson(requestData),
                ErrorMessage = exception.Message,
                ExceptionDetails = exception.ToString(),
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = userAgent ?? GetUserAgent(),
                HttpMethod = httpMethod ?? GetHttpMethod(),
                Endpoint = endpoint ?? GetEndpoint()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log API error to database: {Action} on {Resource}", action, resource);
            // Fallback to file logging
            _logger.LogError(exception, "API Error: {Action} on {Resource} | User: {UserContext} | CorrelationId: {CorrelationId} | Request: {RequestData}", 
                action, resource, userContext, correlationId, SerializeToJson(requestData));
        }
    }

    public async Task LogAuthenticationSuccessAsync(string action, string resource, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                LogType = "AUTH_SUCCESS",
                Action = action,
                Resource = resource,
                UserContext = userContext,
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = userAgent ?? GetUserAgent(),
                HttpMethod = httpMethod ?? GetHttpMethod(),
                Endpoint = endpoint ?? GetEndpoint()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log authentication success to database: {Action} on {Resource}", action, resource);
            // Fallback to file logging
            _logger.LogInformation("Authentication Success: {Action} on {Resource} | User: {UserContext} | CorrelationId: {CorrelationId}", 
                action, resource, userContext, correlationId);
        }
    }

    public async Task LogAuthenticationFailureAsync(string action, string resource, string reason, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                LogType = "AUTH_FAILURE",
                Action = action,
                Resource = resource,
                UserContext = userContext,
                ErrorMessage = reason,
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = userAgent ?? GetUserAgent(),
                HttpMethod = httpMethod ?? GetHttpMethod(),
                Endpoint = endpoint ?? GetEndpoint()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log authentication failure to database: {Action} on {Resource}", action, resource);
            // Fallback to file logging
            _logger.LogWarning("Authentication Failure: {Action} on {Resource} | User: {UserContext} | Reason: {Reason} | CorrelationId: {CorrelationId}", 
                action, resource, userContext, reason, correlationId);
        }
    }

    public async Task LogActiveDirectoryOperationAsync(string operation, string target, bool success, TimeSpan duration, string? errorMessage, string? userContext, string correlationId, string? ipAddress = null, string? userAgent = null, string? httpMethod = null, string? endpoint = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                LogType = "AD_OPERATION",
                Action = operation,
                Resource = target,
                UserContext = userContext,
                DurationMs = duration.TotalMilliseconds,
                ErrorMessage = errorMessage,
                AdOperation = operation,
                AdTarget = target,
                AdSuccess = success,
                IpAddress = ipAddress ?? GetClientIpAddress(),
                UserAgent = userAgent ?? GetUserAgent(),
                HttpMethod = httpMethod ?? GetHttpMethod(),
                Endpoint = endpoint ?? GetEndpoint()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log AD operation to database: {Operation} on {Target}", operation, target);
            // Fallback to file logging
            if (success)
            {
                _logger.LogInformation("Active Directory Operation: {Operation} on {Target} | Success | Duration: {Duration}ms | CorrelationId: {CorrelationId}", 
                    operation, target, duration.TotalMilliseconds, correlationId);
            }
            else
            {
                _logger.LogError("Active Directory Operation: {Operation} on {Target} | Failed | Duration: {Duration}ms | Error: {ErrorMessage} | CorrelationId: {CorrelationId}", 
                    operation, target, duration.TotalMilliseconds, errorMessage, correlationId);
            }
        }
    }

    // Query methods implementation
    public async Task<IEnumerable<AuditLog>> GetLogsByCorrelationIdAsync(string correlationId)
    {
        try
        {
            return await _context.AuditLogs
                .Where(l => l.CorrelationId == correlationId)
                .OrderBy(l => l.Timestamp)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get logs by correlation ID: {CorrelationId}", correlationId);
            return Enumerable.Empty<AuditLog>();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
        try
        {
            return await _context.AuditLogs
                .Where(l => l.Timestamp >= startTime && l.Timestamp <= endTime)
                .OrderByDescending(l => l.Timestamp)
                .Take(1000)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get logs by time range: {StartTime} to {EndTime}", startTime, endTime);
            return Enumerable.Empty<AuditLog>();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByActionAsync(string action, int limit = 100)
    {
        try
        {
            return await _context.AuditLogs
                .Where(l => l.Action == action)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get logs by action: {Action}", action);
            return Enumerable.Empty<AuditLog>();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByUserContextAsync(string userContext, int limit = 100)
    {
        try
        {
            return await _context.AuditLogs
                .Where(l => l.UserContext == userContext)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get logs by user context: {UserContext}", userContext);
            return Enumerable.Empty<AuditLog>();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetErrorLogsAsync(DateTime startTime, DateTime endTime, int limit = 100)
    {
        try
        {
            return await _context.AuditLogs
                .Where(l => l.Timestamp >= startTime && l.Timestamp <= endTime && l.LogType == "API_ERROR")
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get error logs: {StartTime} to {EndTime}", startTime, endTime);
            return Enumerable.Empty<AuditLog>();
        }
    }

    public async Task<long> GetLogCountAsync(DateTime? startTime = null, DateTime? endTime = null)
    {
        try
        {
            var query = _context.AuditLogs.AsQueryable();
            
            if (startTime.HasValue)
                query = query.Where(l => l.Timestamp >= startTime.Value);
            
            if (endTime.HasValue)
                query = query.Where(l => l.Timestamp <= endTime.Value);
            
            return await query.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get log count");
            return 0;
        }
    }

    public async Task<bool> CleanupOldLogsAsync(DateTime cutoffDate)
    {
        try
        {
            var oldLogs = await _context.AuditLogs
                .Where(l => l.Timestamp < cutoffDate)
                .ToListAsync();
            
            _context.AuditLogs.RemoveRange(oldLogs);
            var deletedCount = await _context.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} old audit logs older than {CutoffDate}", deletedCount, cutoffDate);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old logs older than {CutoffDate}", cutoffDate);
            return false;
        }
    }

    public async Task<bool> OptimizeTableAsync()
    {
        try
        {
            // PostgreSQL VACUUM and ANALYZE
            await _context.Database.ExecuteSqlRawAsync("VACUUM ANALYZE audit_logs");
            _logger.LogInformation("Successfully optimized audit_logs table");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to optimize audit_logs table");
            return false;
        }
    }

    // Helper methods
    private string? SerializeToJson(object? obj)
    {
        if (obj == null) return null;
        
        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return obj.ToString();
        }
    }

    private string? GetClientIpAddress()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Connection?.RemoteIpAddress != null)
            {
                return httpContext.Connection.RemoteIpAddress.ToString();
            }
            
            var forwardedHeader = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',')[0].Trim();
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }

    private string? GetUserAgent()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private string? GetHttpMethod()
    {
        try
        {
            return _httpContextAccessor.HttpContext?.Request.Method;
        }
        catch
        {
            return null;
        }
    }

    private string? GetEndpoint()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request?.Path.HasValue == true)
            {
                return httpContext.Request.Path.Value;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
