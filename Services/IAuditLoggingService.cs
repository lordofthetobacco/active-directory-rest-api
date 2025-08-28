using System.Security.Claims;

namespace ActiveDirectory_API.Services;

public interface IAuditLoggingService
{
    void LogApiRequest(string action, string resource, object? requestData, ClaimsPrincipal? user, string? correlationId = null);
    void LogApiResponse(string action, string resource, object? responseData, int statusCode, TimeSpan duration, ClaimsPrincipal? user, string? correlationId = null);
    void LogApiError(string action, string resource, Exception exception, object? requestData, ClaimsPrincipal? user, string? correlationId = null);
    void LogAuthenticationSuccess(string action, ClaimsPrincipal user, string? correlationId = null);
    void LogAuthenticationFailure(string action, string reason, string? correlationId = null);
    void LogAuthorizationSuccess(string action, string resource, ClaimsPrincipal user, string? correlationId = null);
    void LogAuthorizationFailure(string action, string resource, ClaimsPrincipal user, string reason, string? correlationId = null);
    void LogActiveDirectoryOperation(string operation, string target, bool success, TimeSpan duration, string? errorMessage = null, string? correlationId = null);
    string GenerateCorrelationId();
}
