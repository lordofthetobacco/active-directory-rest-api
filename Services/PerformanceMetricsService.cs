using ActiveDirectory_API.Data;
using ActiveDirectory_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ActiveDirectory_API.Services;

public class PerformanceMetricsService : IPerformanceMetricsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PerformanceMetricsService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PerformanceMetricsService(ApplicationDbContext context, ILogger<PerformanceMetricsService> logger, IHttpContextAccessor httpContextAccessor)
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

    public async Task StoreMetricAsync(PerformanceMetric metric)
    {
        try
        {
            // Categorize performance based on response time
            metric.PerformanceCategory = CategorizePerformance(metric.ResponseTimeMs);
            
            _context.PerformanceMetrics.Add(metric);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store performance metric for endpoint: {Endpoint}", metric.Endpoint);
            throw;
        }
    }

    public async Task<IEnumerable<PerformanceMetric>> GetMetricsByEndpointAsync(string endpoint, int limit = 100)
    {
        return await _context.PerformanceMetrics
            .Where(m => m.Endpoint == endpoint)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceMetric>> GetMetricsByTimeRangeAsync(DateTime startTime, DateTime endTime, int limit = 100)
    {
        return await _context.PerformanceMetrics
            .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceMetric>> GetMetricsByActionAsync(string action, int limit = 100)
    {
        return await _context.PerformanceMetrics
            .Where(m => m.Action == action)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceMetric>> GetMetricsByPerformanceCategoryAsync(string category, int limit = 100)
    {
        return await _context.PerformanceMetrics
            .Where(m => m.PerformanceCategory == category)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<PerformanceMetric>> GetLatestMetricsAsync(int limit = 100)
    {
        return await _context.PerformanceMetrics
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<object> GetEndpointPerformanceSummaryAsync(string endpoint, DateTime startTime, DateTime endTime)
    {
        var metrics = await _context.PerformanceMetrics
            .Where(m => m.Endpoint == endpoint && m.Timestamp >= startTime && m.Timestamp <= endTime)
            .ToListAsync();

        if (!metrics.Any())
            return new { endpoint, message = "No metrics found for the specified time range" };

        var successfulRequests = metrics.Where(m => m.IsSuccess).ToList();
        var failedRequests = metrics.Where(m => !m.IsSuccess).ToList();

        return new
        {
            endpoint,
            timeRange = new { startTime, endTime },
            totalRequests = metrics.Count,
            successfulRequests = successfulRequests.Count,
            failedRequests = failedRequests.Count,
            successRate = metrics.Count > 0 ? (double)successfulRequests.Count / metrics.Count * 100 : 0,
            averageResponseTime = metrics.Average(m => m.ResponseTimeMs),
            minResponseTime = metrics.Min(m => m.ResponseTimeMs),
            maxResponseTime = metrics.Max(m => m.ResponseTimeMs),
            p95ResponseTime = CalculatePercentile(metrics.Select(m => m.ResponseTimeMs).ToList(), 95),
            p99ResponseTime = CalculatePercentile(metrics.Select(m => m.ResponseTimeMs).ToList(), 99),
            performanceBreakdown = new
            {
                fast = metrics.Count(m => m.PerformanceCategory == "FAST"),
                normal = metrics.Count(m => m.PerformanceCategory == "NORMAL"),
                slow = metrics.Count(m => m.PerformanceCategory == "SLOW"),
                verySlow = metrics.Count(m => m.PerformanceCategory == "VERY_SLOW")
            },
            statusCodeBreakdown = metrics.GroupBy(m => m.StatusCode)
                .Select(g => new { statusCode = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
        };
    }

    public async Task<object> GetOverallPerformanceSummaryAsync(DateTime startTime, DateTime endTime)
    {
        var metrics = await _context.PerformanceMetrics
            .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
            .ToListAsync();

        if (!metrics.Any())
            return new { message = "No metrics found for the specified time range" };

        var endpoints = metrics.GroupBy(m => m.Endpoint).ToList();

        return new
        {
            timeRange = new { startTime, endTime },
            totalRequests = metrics.Count,
            uniqueEndpoints = endpoints.Count,
            overallSuccessRate = metrics.Count > 0 ? (double)metrics.Count(m => m.IsSuccess) / metrics.Count * 100 : 0,
            overallAverageResponseTime = metrics.Average(m => m.ResponseTimeMs),
            overallP95ResponseTime = CalculatePercentile(metrics.Select(m => m.ResponseTimeMs).ToList(), 95),
            overallP99ResponseTime = CalculatePercentile(metrics.Select(m => m.ResponseTimeMs).ToList(), 99),
            endpointBreakdown = endpoints.Select(g => new
            {
                endpoint = g.Key,
                requestCount = g.Count(),
                averageResponseTime = g.Average(m => m.ResponseTimeMs),
                successRate = g.Count() > 0 ? (double)g.Count(m => m.IsSuccess) / g.Count() * 100 : 0
            }).OrderByDescending(x => x.requestCount)
        };
    }

    public async Task<object> GetSlowestEndpointsAsync(DateTime startTime, DateTime endTime, int limit = 10)
    {
        var slowestEndpoints = await _context.PerformanceMetrics
            .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
            .GroupBy(m => m.Endpoint)
            .Select(g => new
            {
                endpoint = g.Key,
                averageResponseTime = g.Average(m => m.ResponseTimeMs),
                p95ResponseTime = CalculatePercentile(g.Select(m => m.ResponseTimeMs).ToList(), 95),
                p99ResponseTime = CalculatePercentile(g.Select(m => m.ResponseTimeMs).ToList(), 99),
                requestCount = g.Count(),
                slowRequestCount = g.Count(m => m.PerformanceCategory == "SLOW" || m.PerformanceCategory == "VERY_SLOW")
            })
            .OrderByDescending(x => x.averageResponseTime)
            .Take(limit)
            .ToListAsync();

        return new
        {
            timeRange = new { startTime, endTime },
            slowestEndpoints
        };
    }

    public async Task<object> GetErrorRateByEndpointAsync(DateTime startTime, DateTime endTime)
    {
        var errorRates = await _context.PerformanceMetrics
            .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
            .GroupBy(m => m.Endpoint)
            .Select(g => new
            {
                endpoint = g.Key,
                totalRequests = g.Count(),
                errorRequests = g.Count(m => !m.IsSuccess),
                errorRate = g.Count() > 0 ? (double)g.Count(m => !m.IsSuccess) / g.Count() * 100 : 0,
                statusCodeBreakdown = g.GroupBy(m => m.StatusCode)
                    .Select(sg => new { statusCode = sg.Key, count = sg.Count() })
                    .OrderByDescending(x => x.count)
            })
            .OrderByDescending(x => x.errorRate)
            .ToListAsync();

        return new
        {
            timeRange = new { startTime, endTime },
            errorRates
        };
    }

    public async Task<bool> CleanupOldMetricsAsync(DateTime cutoffDate)
    {
        try
        {
            var oldMetrics = await _context.PerformanceMetrics
                .Where(m => m.Timestamp < cutoffDate)
                .ToListAsync();

            _context.PerformanceMetrics.RemoveRange(oldMetrics);
            var deletedCount = await _context.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} old performance metrics older than {CutoffDate}", deletedCount, cutoffDate);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup old performance metrics");
            return false;
        }
    }

    public async Task<long> GetMetricsCountAsync(DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = _context.PerformanceMetrics.AsQueryable();
        
        if (startTime.HasValue)
            query = query.Where(m => m.Timestamp >= startTime.Value);
        
        if (endTime.HasValue)
            query = query.Where(m => m.Timestamp <= endTime.Value);
        
        return await query.CountAsync();
    }

    private string CategorizePerformance(double responseTimeMs)
    {
        return responseTimeMs switch
        {
            < 100 => "FAST",
            < 500 => "NORMAL",
            < 2000 => "SLOW",
            _ => "VERY_SLOW"
        };
    }

    private double CalculatePercentile(List<double> values, int percentile)
    {
        if (!values.Any()) return 0;
        
        values.Sort();
        var index = (int)Math.Ceiling((percentile / 100.0) * values.Count) - 1;
        return values[Math.Max(0, Math.Min(index, values.Count - 1))];
    }
}
