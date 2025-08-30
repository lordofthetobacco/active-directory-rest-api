using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using active_directory_rest_api.Data;

namespace active_directory_rest_api.Middleware;

public class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiLoggingMiddleware> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger, IServiceProvider serviceProvider)
    {
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            stopwatch.Stop();
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);

            await LogApiCallAsync(context, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error in API logging middleware");
            
            // Restore the original response body
            context.Response.Body = originalBodyStream;
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogApiCallAsync(HttpContext context, long responseTime, int statusCode)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();

            var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            var endpoint = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            var apiLog = new ApiLog
            {
                Timestamp = DateTime.UtcNow,
                Endpoint = endpoint,
                Method = method,
                ApiKey = apiKey,
                StatusCode = statusCode,
                ResponseTime = responseTime,
                ErrorMessage = statusCode >= 400 ? "Error occurred" : null
            };

            dbContext.ApiLogs.Add(apiLog);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging API call to database");
        }
    }
}
