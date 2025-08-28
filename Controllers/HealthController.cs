using ActiveDirectory_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace ActiveDirectory_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Allow unauthenticated access for health checks
public class HealthController : BaseController
{
    private readonly IActiveDirectoryService _adService;

    public HealthController(IActiveDirectoryService adService, IAuditLoggingService auditLogger, ILogger<HealthController> logger) 
        : base(auditLogger, logger)
    {
        _adService = adService;
    }

    [HttpGet]
    public async Task<ActionResult<HealthStatus>> GetHealth()
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetHealth";
        var resource = "Health:Overall";
        var requestData = (object?)null;

        try
        {
            LogRequest(action, resource, requestData);

            var adStatus = await _adService.ValidateConnectionAsync();
            stopwatch.Stop();
            
            var result = new HealthStatus
            {
                Status = adStatus ? "Healthy" : "Unhealthy",
                Timestamp = DateTime.UtcNow,
                ActiveDirectory = adStatus ? "Connected" : "Disconnected",
                Version = "1.0.0"
            };

            LogResponse(action, resource, result, 200, stopwatch.Elapsed);
            return Ok(result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            
            var errorResult = new HealthStatus
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                ActiveDirectory = "Error",
                Version = "1.0.0",
                Error = ex.Message
            };

            LogResponse(action, resource, errorResult, 500, stopwatch.Elapsed);
            return StatusCode(500, errorResult);
        }
    }

    [HttpGet("ad")]
    public async Task<ActionResult<ActiveDirectoryHealth>> GetActiveDirectoryHealth()
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetActiveDirectoryHealth";
        var resource = "Health:ActiveDirectory";
        var requestData = (object?)null;

        try
        {
            LogRequest(action, resource, requestData);

            var startTime = DateTime.UtcNow;
            var adStatus = await _adService.ValidateConnectionAsync();
            var responseTime = DateTime.UtcNow - startTime;
            stopwatch.Stop();
            
            var result = new ActiveDirectoryHealth
            {
                Status = adStatus ? "Connected" : "Disconnected",
                ResponseTime = responseTime.TotalMilliseconds,
                Timestamp = DateTime.UtcNow
            };

            LogResponse(action, resource, result, 200, stopwatch.Elapsed);
            return Ok(result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            
            var errorResult = new ActiveDirectoryHealth
            {
                Status = "Disconnected",
                ResponseTime = 0,
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            };

            LogResponse(action, resource, errorResult, 500, stopwatch.Elapsed);
            return StatusCode(500, errorResult);
        }
    }
}

public class HealthStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string ActiveDirectory { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Error { get; set; }
}

public class ActiveDirectoryHealth
{
    public string Status { get; set; } = string.Empty;
    public double ResponseTime { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Error { get; set; }
}
