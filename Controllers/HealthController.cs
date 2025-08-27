using ActiveDirectory_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ActiveDirectory_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Allow unauthenticated access for health checks
public class HealthController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(IActiveDirectoryService adService, ILogger<HealthController> logger)
    {
        _adService = adService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<HealthStatus>> GetHealth()
    {
        try
        {
            var adStatus = await _adService.ValidateConnectionAsync();
            
            return Ok(new HealthStatus
            {
                Status = adStatus ? "Healthy" : "Unhealthy",
                Timestamp = DateTime.UtcNow,
                ActiveDirectory = adStatus ? "Connected" : "Disconnected",
                Version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, new HealthStatus
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                ActiveDirectory = "Error",
                Version = "1.0.0",
                Error = ex.Message
            });
        }
    }

    [HttpGet("ad")]
    public async Task<ActionResult<ActiveDirectoryHealth>> GetActiveDirectoryHealth()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var adStatus = await _adService.ValidateConnectionAsync();
            var responseTime = DateTime.UtcNow - startTime;
            
            return Ok(new ActiveDirectoryHealth
            {
                Status = adStatus ? "Connected" : "Disconnected",
                ResponseTime = responseTime.TotalMilliseconds,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Active Directory health check failed");
            return StatusCode(500, new ActiveDirectoryHealth
            {
                Status = "Disconnected",
                ResponseTime = 0,
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
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
