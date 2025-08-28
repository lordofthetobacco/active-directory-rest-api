using System.Security.Claims;
using System.Text.Json;
using ActiveDirectory_API.Models;

namespace ActiveDirectory_API.Services;

public class AuditLoggingService : IAuditLoggingService
{
    private readonly ILogger<AuditLoggingService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDatabaseLoggingService _databaseLogger;
    private readonly IPerformanceMetricsService _performanceMetricsService;

    public AuditLoggingService(ILogger<AuditLoggingService> logger, IHttpContextAccessor httpContextAccessor, IDatabaseLoggingService databaseLogger, IPerformanceMetricsService performanceMetricsService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _databaseLogger = databaseLogger;
        _performanceMetricsService = performanceMetricsService;
    }

    public void LogApiRequest(string action, string resource, object? requestData, ClaimsPrincipal? user, string? correlationId = null)
    {
        var correlationIdValue = correlationId ?? GetCorrelationId();
        var userInfo = ExtractUserInfo(user);
        var requestInfo = SerializeRequestData(requestData);

        // Log to database (async, fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _databaseLogger.LogApiRequestAsync(action, resource, requestData, userInfo, correlationIdValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log API request to database");
            }
        });

        // Fallback to file logging
        _logger.LogInformation(
            "API Request: {Action} on {Resource} | User: {UserInfo} | CorrelationId: {CorrelationId} | Request: {RequestInfo}",
            action, resource, userInfo, correlationIdValue, requestInfo);
    }

    public void LogApiResponse(string action, string resource, object? responseData, int statusCode, TimeSpan duration, ClaimsPrincipal? user, string? correlationId = null)
    {
        var correlationIdValue = correlationId ?? GetCorrelationId();
        var userInfo = ExtractUserInfo(user);
        var responseInfo = SerializeResponseData(responseData, statusCode);
        var durationMs = duration.TotalMilliseconds;

        // Log to database (async, fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _databaseLogger.LogApiResponseAsync(action, resource, responseData, statusCode, duration, userInfo, correlationIdValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log API response to database");
            }
        });

        // Store performance metrics (async, fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var metric = new PerformanceMetric
                {
                    Endpoint = httpContext?.Request.Path.Value ?? "unknown",
                    HttpMethod = httpContext?.Request.Method ?? "unknown",
                    Action = action,
                    Timestamp = DateTime.UtcNow,
                    ResponseTimeMs = duration.TotalMilliseconds,
                    StatusCode = statusCode,
                    RequestSizeBytes = httpContext?.Request.ContentLength,
                    ResponseSizeBytes = responseData != null ? JsonSerializer.Serialize(responseData).Length : null,
                    CorrelationId = correlationIdValue,
                    UserContext = userInfo,
                    IpAddress = GetClientIpAddress(httpContext),
                    UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                    IsSuccess = statusCode >= 200 && statusCode < 300,
                    ErrorMessage = statusCode >= 400 ? $"HTTP {statusCode}" : null
                };

                await _performanceMetricsService.StoreMetricAsync(metric);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store performance metric");
            }
        });

        if (statusCode >= 200 && statusCode < 300)
        {
            _logger.LogInformation(
                "API Response: {Action} on {Resource} | Status: {StatusCode} | Duration: {Duration}ms | User: {UserInfo} | CorrelationId: {CorrelationId} | Response: {ResponseInfo}",
                action, resource, statusCode, durationMs, userInfo, correlationIdValue, responseInfo);
        }
        else
        {
            _logger.LogWarning(
                "API Response: {Action} on {Resource} | Status: {StatusCode} | Duration: {Duration}ms | User: {UserInfo} | CorrelationId: {CorrelationId} | Response: {ResponseInfo}",
                action, resource, statusCode, durationMs, userInfo, correlationIdValue, responseInfo);
        }
    }

    public void LogApiError(string action, string resource, Exception exception, object? requestData, ClaimsPrincipal? user, string? correlationId = null)
    {
        var correlationIdValue = correlationId ?? GetCorrelationId();
        var userInfo = ExtractUserInfo(user);
        var requestInfo = SerializeRequestData(requestData);

        // Log to database (async, fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _databaseLogger.LogApiErrorAsync(action, resource, exception, requestData, userInfo, correlationIdValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log API error to database");
            }
        });

        _logger.LogError(
            exception,
            "API Error: {Action} on {Resource} | User: {UserInfo} | CorrelationId: {CorrelationId} | Request: {RequestInfo} | Error: {ErrorMessage}",
            action, resource, userInfo, correlationIdValue, requestInfo, exception.Message);
    }

    public void LogAuthenticationSuccess(string action, ClaimsPrincipal user, string? correlationId = null)
    {
        var correlationIdValue = correlationId ?? GetCorrelationId();
        var userInfo = ExtractUserInfo(user);

        // Log to database (async, fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _databaseLogger.LogAuthenticationSuccessAsync(action, "Authentication", userInfo, correlationIdValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log authentication success to database");
            }
        });

        _logger.LogInformation(
            "Authentication Success: {Action} | User: {UserInfo} | CorrelationId: {CorrelationId}",
            action, userInfo, correlationIdValue);
    }

    public void LogAuthenticationFailure(string action, string reason, string? correlationId = null)
    {
        var correlationIdValue = correlationId ?? GetCorrelationId();

        // Log to database (async, fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _databaseLogger.LogAuthenticationFailureAsync(action, "Authentication", reason, null, correlationIdValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log authentication failure to database");
            }
        });

        _logger.LogWarning(
            "Authentication Failure: {Action} | Reason: {Reason} | CorrelationId: {CorrelationId}",
            action, reason, correlationIdValue);
    }

    public void LogAuthorizationSuccess(string action, string resource, ClaimsPrincipal user, string? correlationId = null)
    {
        var correlationIdValue = correlationId ?? GetCorrelationId();
        var userInfo = ExtractUserInfo(user);

        // Log to database (async, fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _databaseLogger.LogAuthenticationSuccessAsync(action, resource, userInfo, correlationIdValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log authorization success to database");
            }
        });

        _logger.LogInformation(
            "Authorization Success: {Action} on {Resource} | User: {UserInfo} | CorrelationId: {CorrelationId}",
            action, resource, userInfo, correlationIdValue);
    }

    public void LogAuthorizationFailure(string action, string resource, ClaimsPrincipal user, string reason, string? correlationId = null)
    {
        var correlationIdValue = correlationId ?? GetCorrelationId();
        var userInfo = ExtractUserInfo(user);

        // Log to database (async, fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _databaseLogger.LogAuthenticationFailureAsync(action, resource, reason, userInfo, correlationIdValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log authorization failure to database");
            }
        });

        _logger.LogWarning(
            "Authorization Failure: {Action} on {Resource} | User: {UserInfo} | Reason: {Reason} | CorrelationId: {CorrelationId}",
            action, resource, userInfo, reason, correlationIdValue);
    }

    public void LogActiveDirectoryOperation(string operation, string target, bool success, TimeSpan duration, string? errorMessage = null, string? correlationId = null)
    {
        var correlationIdValue = correlationId ?? GetCorrelationId();
        var durationMs = duration.TotalMilliseconds;

        // Log to database (async, fire-and-forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _databaseLogger.LogActiveDirectoryOperationAsync(operation, target, success, duration, errorMessage, null, correlationIdValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log AD operation to database");
            }
        });

        if (success)
        {
            _logger.LogInformation(
                "Active Directory Operation: {Operation} on {Target} | Success | Duration: {Duration}ms | CorrelationId: {CorrelationId}",
                operation, target, durationMs, correlationIdValue);
        }
        else
        {
            _logger.LogError(
                "Active Directory Operation: {Operation} on {Target} | Failed | Duration: {Duration}ms | Error: {ErrorMessage} | CorrelationId: {CorrelationId}",
                operation, target, durationMs, errorMessage, correlationIdValue);
        }
    }

    public string GenerateCorrelationId()
    {
        return Guid.NewGuid().ToString("N");
    }

    private string GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId) == true)
        {
            return correlationId.ToString();
        }

        if (httpContext?.Items.TryGetValue("CorrelationId", out var item) == true && item is string id)
        {
            return id;
        }

        return "unknown";
    }

    private string ExtractUserInfo(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return "unauthenticated";
        }

        var appId = user.FindFirst("appid")?.Value;
        var name = user.Identity?.Name;
        var roles = user.FindAll("roles").Select(c => c.Value).ToList();

        if (!string.IsNullOrEmpty(appId))
        {
            return $"app:{appId}";
        }

        if (!string.IsNullOrEmpty(name))
        {
            var roleInfo = roles.Any() ? $" (roles: {string.Join(",", roles)})" : "";
            return $"user:{name}{roleInfo}";
        }

        return "unknown";
    }

    private string SerializeRequestData(object? requestData)
    {
        if (requestData == null)
        {
            return "null";
        }

        try
        {
            // For sensitive data like passwords, we might want to mask certain fields
            var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Mask sensitive fields
            json = MaskSensitiveData(json);

            return json.Length > 1000 ? json[..1000] + "..." : json;
        }
        catch
        {
            return requestData.ToString() ?? "serialization_error";
        }
    }

    private string SerializeResponseData(object? responseData, int statusCode)
    {
        if (responseData == null)
        {
            return "null";
        }

        try
        {
            var json = JsonSerializer.Serialize(responseData, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // For large responses, truncate to avoid log bloat
            return json.Length > 1000 ? json[..1000] + "..." : json;
        }
        catch
        {
            return responseData.ToString() ?? "serialization_error";
        }
    }

    private string MaskSensitiveData(string json)
    {
        // Mask common sensitive fields
        var sensitiveFields = new[] { "password", "clientSecret", "secret", "token" };
        
        foreach (var field in sensitiveFields)
        {
            json = System.Text.RegularExpressions.Regex.Replace(
                json,
                $@"""{field}""\s*:\s*""[^""]*""",
                $@"""{field}"" : ""***MASKED***""",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return json;
    }

    private string? GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null) return null;

        // Try to get the real IP address from various headers
        var forwardedHeader = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one
            var firstIp = forwardedHeader.Split(',')[0].Trim();
            return firstIp;
        }

        var realIpHeader = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIpHeader))
        {
            return realIpHeader;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
