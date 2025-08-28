using ActiveDirectory_API.Models;

namespace ActiveDirectory_API.Services;

public interface IPerformanceMetricsService
{
    Task<bool> IsAvailableAsync();
    
    // Store performance metrics
    Task StoreMetricAsync(PerformanceMetric metric);
    
    // Query performance metrics
    Task<IEnumerable<PerformanceMetric>> GetMetricsByEndpointAsync(string endpoint, int limit = 100);
    Task<IEnumerable<PerformanceMetric>> GetMetricsByTimeRangeAsync(DateTime startTime, DateTime endTime, int limit = 100);
    Task<IEnumerable<PerformanceMetric>> GetMetricsByActionAsync(string action, int limit = 100);
    Task<IEnumerable<PerformanceMetric>> GetMetricsByPerformanceCategoryAsync(string category, int limit = 100);
    Task<IEnumerable<PerformanceMetric>> GetLatestMetricsAsync(int limit = 100);
    
    // Aggregated metrics
    Task<object> GetEndpointPerformanceSummaryAsync(string endpoint, DateTime startTime, DateTime endTime);
    Task<object> GetOverallPerformanceSummaryAsync(DateTime startTime, DateTime endTime);
    Task<object> GetSlowestEndpointsAsync(DateTime startTime, DateTime endTime, int limit = 10);
    Task<object> GetErrorRateByEndpointAsync(DateTime startTime, DateTime endTime);
    
    // Maintenance
    Task<bool> CleanupOldMetricsAsync(DateTime cutoffDate);
    Task<long> GetMetricsCountAsync(DateTime? startTime = null, DateTime? endTime = null);
}
