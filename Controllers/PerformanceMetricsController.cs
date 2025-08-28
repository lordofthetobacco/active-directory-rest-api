using ActiveDirectory_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ActiveDirectory_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireReadPermissions")]
public class PerformanceMetricsController : BaseController
{
    private readonly IPerformanceMetricsService _performanceMetricsService;

    public PerformanceMetricsController(
        IPerformanceMetricsService performanceMetricsService,
        IAuditLoggingService auditLogger,
        ILogger<PerformanceMetricsController> logger) : base(auditLogger, logger)
    {
        _performanceMetricsService = performanceMetricsService;
    }

    /// <summary>
    /// Get latest performance metrics
    /// </summary>
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestMetrics([FromQuery] int limit = 100)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetLatestMetrics", "PerformanceMetrics", new { limit });
            
            var metrics = await _performanceMetricsService.GetLatestMetricsAsync(limit);
            
            stopwatch.Stop();
            LogResponse("GetLatestMetrics", "PerformanceMetrics", metrics, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    count = metrics.Count(),
                    limit,
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetLatestMetrics", "PerformanceMetrics", ex, new { limit });
            return StatusCode(500, new { success = false, error = "Failed to retrieve latest metrics" });
        }
    }

    /// <summary>
    /// Get performance metrics for a specific endpoint
    /// </summary>
    [HttpGet("endpoint/{endpoint}")]
    public async Task<IActionResult> GetMetricsByEndpoint(
        string endpoint,
        [FromQuery] int limit = 100)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetMetricsByEndpoint", "PerformanceMetrics", new { endpoint, limit });
            
            var metrics = await _performanceMetricsService.GetMetricsByEndpointAsync(endpoint, limit);
            
            stopwatch.Stop();
            LogResponse("GetMetricsByEndpoint", "PerformanceMetrics", metrics, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    endpoint,
                    count = metrics.Count(),
                    limit,
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetMetricsByEndpoint", "PerformanceMetrics", ex, new { endpoint, limit });
            return StatusCode(500, new { success = false, error = "Failed to retrieve endpoint metrics" });
        }
    }

    /// <summary>
    /// Get performance metrics by time range
    /// </summary>
    [HttpGet("timerange")]
    public async Task<IActionResult> GetMetricsByTimeRange(
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        [FromQuery] int limit = 100)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetMetricsByTimeRange", "PerformanceMetrics", new { startTime, endTime, limit });
            
            var metrics = await _performanceMetricsService.GetMetricsByTimeRangeAsync(startTime, endTime, limit);
            
            stopwatch.Stop();
            LogResponse("GetMetricsByTimeRange", "PerformanceMetrics", metrics, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    timeRange = new { startTime, endTime },
                    count = metrics.Count(),
                    limit,
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetMetricsByTimeRange", "PerformanceMetrics", ex, new { startTime, endTime, limit });
            return StatusCode(500, new { success = false, error = "Failed to retrieve time range metrics" });
        }
    }

    /// <summary>
    /// Get performance metrics by action
    /// </summary>
    [HttpGet("action/{action}")]
    public async Task<IActionResult> GetMetricsByAction(
        string action,
        [FromQuery] int limit = 100)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetMetricsByAction", "PerformanceMetrics", new { action, limit });
            
            var metrics = await _performanceMetricsService.GetMetricsByActionAsync(action, limit);
            
            stopwatch.Stop();
            LogResponse("GetMetricsByAction", "PerformanceMetrics", metrics, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    action,
                    count = metrics.Count(),
                    limit,
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetMetricsByAction", "PerformanceMetrics", ex, new { action, limit });
            return StatusCode(500, new { success = false, error = "Failed to retrieve action metrics" });
        }
    }

    /// <summary>
    /// Get performance metrics by performance category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetMetricsByCategory(
        string category,
        [FromQuery] int limit = 100)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetMetricsByCategory", "PerformanceMetrics", new { category, limit });
            
            var metrics = await _performanceMetricsService.GetMetricsByPerformanceCategoryAsync(category, limit);
            
            stopwatch.Stop();
            LogResponse("GetMetricsByCategory", "PerformanceMetrics", metrics, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    category,
                    count = metrics.Count(),
                    limit,
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetMetricsByCategory", "PerformanceMetrics", ex, new { category, limit });
            return StatusCode(500, new { success = false, error = "Failed to retrieve category metrics" });
        }
    }

    /// <summary>
    /// Get performance summary for a specific endpoint
    /// </summary>
    [HttpGet("summary/endpoint/{endpoint}")]
    public async Task<IActionResult> GetEndpointSummary(
        string endpoint,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetEndpointSummary", "PerformanceMetrics", new { endpoint, startTime, endTime });
            
            var summary = await _performanceMetricsService.GetEndpointPerformanceSummaryAsync(endpoint, startTime, endTime);
            
            stopwatch.Stop();
            LogResponse("GetEndpointSummary", "PerformanceMetrics", summary, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = summary,
                metadata = new
                {
                    endpoint,
                    timeRange = new { startTime, endTime },
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetEndpointSummary", "PerformanceMetrics", ex, new { endpoint, startTime, endTime });
            return StatusCode(500, new { success = false, error = "Failed to retrieve endpoint summary" });
        }
    }

    /// <summary>
    /// Get overall performance summary
    /// </summary>
    [HttpGet("summary/overall")]
    public async Task<IActionResult> GetOverallSummary(
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetOverallSummary", "PerformanceMetrics", new { startTime, endTime });
            
            var summary = await _performanceMetricsService.GetOverallPerformanceSummaryAsync(startTime, endTime);
            
            stopwatch.Stop();
            LogResponse("GetOverallSummary", "PerformanceMetrics", summary, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = summary,
                metadata = new
                {
                    timeRange = new { startTime, endTime },
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetOverallSummary", "PerformanceMetrics", ex, new { startTime, endTime });
            return StatusCode(500, new { success = false, error = "Failed to retrieve overall summary" });
        }
    }

    /// <summary>
    /// Get slowest endpoints
    /// </summary>
    [HttpGet("slowest")]
    public async Task<IActionResult> GetSlowestEndpoints(
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        [FromQuery] int limit = 10)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetSlowestEndpoints", "PerformanceMetrics", new { startTime, endTime, limit });
            
            var slowestEndpoints = await _performanceMetricsService.GetSlowestEndpointsAsync(startTime, endTime, limit);
            
            stopwatch.Stop();
            LogResponse("GetSlowestEndpoints", "PerformanceMetrics", slowestEndpoints, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = slowestEndpoints,
                metadata = new
                {
                    timeRange = new { startTime, endTime },
                    limit,
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetSlowestEndpoints", "PerformanceMetrics", ex, new { startTime, endTime, limit });
            return StatusCode(500, new { success = false, error = "Failed to retrieve slowest endpoints" });
        }
    }

    /// <summary>
    /// Get error rates by endpoint
    /// </summary>
    [HttpGet("errors")]
    public async Task<IActionResult> GetErrorRates(
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetErrorRates", "PerformanceMetrics", new { startTime, endTime });
            
            var errorRates = await _performanceMetricsService.GetErrorRateByEndpointAsync(startTime, endTime);
            
            stopwatch.Stop();
            LogResponse("GetErrorRates", "PerformanceMetrics", errorRates, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = errorRates,
                metadata = new
                {
                    timeRange = new { startTime, endTime },
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetErrorRates", "PerformanceMetrics", ex, new { startTime, endTime });
            return StatusCode(500, new { success = false, error = "Failed to retrieve error rates" });
        }
    }

    /// <summary>
    /// Get metrics count
    /// </summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetMetricsCount(
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            LogRequest("GetMetricsCount", "PerformanceMetrics", new { startTime, endTime });
            
            var count = await _performanceMetricsService.GetMetricsCountAsync(startTime, endTime);
            
            stopwatch.Stop();
            LogResponse("GetMetricsCount", "PerformanceMetrics", new { count }, 200, stopwatch.Elapsed);
            
            return Ok(new
            {
                success = true,
                data = new { count },
                metadata = new
                {
                    timeRange = startTime.HasValue && endTime.HasValue ? new { startTime, endTime } : null,
                    timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError("GetMetricsCount", "PerformanceMetrics", ex, new { startTime, endTime });
            return StatusCode(500, new { success = false, error = "Failed to retrieve metrics count" });
        }
    }
}
