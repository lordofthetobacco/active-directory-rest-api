using ActiveDirectory_API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ActiveDirectory_API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected readonly IAuditLoggingService _auditLogger;
    protected readonly ILogger _logger;

    protected BaseController(IAuditLoggingService auditLogger, ILogger logger)
    {
        _auditLogger = auditLogger;
        _logger = logger;
    }

    protected void LogRequest(string action, string resource, object? requestData)
    {
        _auditLogger.LogApiRequest(action, resource, requestData, User);
    }

    protected void LogResponse(string action, string resource, object? responseData, int statusCode, TimeSpan duration)
    {
        _auditLogger.LogApiResponse(action, resource, responseData, statusCode, duration, User);
    }

    protected void LogError(string action, string resource, Exception exception, object? requestData)
    {
        _auditLogger.LogApiError(action, resource, exception, requestData, User);
    }

    protected void LogAuthorizationSuccess(string action, string resource)
    {
        _auditLogger.LogAuthorizationSuccess(action, resource, User);
    }

    protected void LogAuthorizationFailure(string action, string resource, string reason)
    {
        _auditLogger.LogAuthorizationFailure(action, resource, User, reason);
    }

    protected void LogActiveDirectoryOperation(string operation, string target, bool success, TimeSpan duration, string? errorMessage = null)
    {
        _auditLogger.LogActiveDirectoryOperation(operation, target, success, duration, errorMessage);
    }
}
