using System.Diagnostics;
using System.Text;
using active_directory_rest_api.Data;
using active_directory_rest_api.Models;
using Microsoft.EntityFrameworkCore;

namespace active_directory_rest_api.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditLoggingMiddleware> _logger;

        public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                // Capture request body
                var requestBody = await CaptureRequestBody(context.Request);

                // Capture response body
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                // Process the request
                await _next(context);

                // Capture response body
                memoryStream.Position = 0;
                var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                // Copy response back to original stream
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);

                // Log the audit entry
                await LogAuditEntry(context, requestBody, responseBody, stopwatch.ElapsedMilliseconds, dbContext);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Log error audit entry
                await LogAuditEntry(context, null, null, stopwatch.ElapsedMilliseconds, dbContext, ex.Message);
                
                // Re-throw the exception
                throw;
            }
            finally
            {
                stopwatch.Stop();
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task<string?> CaptureRequestBody(HttpRequest request)
        {
            try
            {
                if (request.Body.CanSeek)
                {
                    request.Body.Position = 0;
                    using var reader = new StreamReader(request.Body, leaveOpen: true);
                    return await reader.ReadToEndAsync();
                }
                else
                {
                    // For non-seekable streams, we can't read without consuming
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not capture request body");
                return null;
            }
        }

        private async Task LogAuditEntry(HttpContext context, string? requestBody, string? responseBody, long executionTimeMs, ApplicationDbContext dbContext, string? errorMessage = null)
        {
            try
            {
                // Skip logging for certain endpoints
                if (ShouldSkipAuditLogging(context.Request.Path))
                {
                    return;
                }

                // Get token ID from context (set by authentication middleware)
                int? tokenId = null;
                if (context.Items.TryGetValue("TokenId", out var tokenIdObj))
                {
                    tokenId = tokenIdObj as int?;
                }

                var auditLog = new AuditLog
                {
                    ApiTokenId = tokenId,
                    Endpoint = $"{context.Request.Method} {context.Request.Path}",
                    Method = context.Request.Method,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    IpAddress = GetClientIpAddress(context),
                    RequestBody = TruncateString(requestBody, 4000),
                    ResponseStatus = context.Response.StatusCode,
                    ResponseBody = TruncateString(responseBody, 4000),
                    ExecutionTimeMs = (int)executionTimeMs,
                    ErrorMessage = errorMessage,
                    AdditionalData = GetAdditionalData(context)
                };

                dbContext.AuditLogs.Add(auditLog);
                await dbContext.SaveChangesAsync();

                _logger.LogDebug("Audit log created for endpoint {Endpoint} with status {Status}", 
                    auditLog.Endpoint, auditLog.ResponseStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log entry");
            }
        }

        private bool ShouldSkipAuditLogging(PathString path)
        {
            // Skip logging for certain endpoints to avoid noise
            var skipPaths = new[]
            {
                "/swagger",
                "/swagger.json",
                "/swagger/v1/swagger.json",
                "/health",
                "/favicon.ico"
            };

            return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath));
        }

        private string? GetClientIpAddress(HttpContext context)
        {
            // Try to get the real IP address from various headers
            var headers = new[] { "X-Forwarded-For", "X-Real-IP", "X-Client-IP" };
            
            foreach (var header in headers)
            {
                if (context.Request.Headers.TryGetValue(header, out var value))
                {
                    var ip = value.FirstOrDefault();
                    if (!string.IsNullOrEmpty(ip))
                    {
                        // Handle comma-separated values (X-Forwarded-For can have multiple IPs)
                        return ip.Split(',')[0].Trim();
                    }
                }
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetAdditionalData(HttpContext context)
        {
            try
            {
                var additionalData = new
                {
                    queryString = context.Request.QueryString.ToString(),
                    contentType = context.Request.ContentType,
                    contentLength = context.Request.ContentLength,
                    host = context.Request.Host.ToString(),
                    scheme = context.Request.Scheme,
                    protocol = context.Request.Protocol,
                    userAgent = context.Request.Headers.UserAgent.ToString(),
                    referer = context.Request.Headers.Referer.ToString(),
                    userLanguages = context.Request.Headers.AcceptLanguage.ToString()
                };

                return System.Text.Json.JsonSerializer.Serialize(additionalData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not serialize additional data for audit log");
                return null;
            }
        }

        private string? TruncateString(string? input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input.Length <= maxLength ? input : input.Substring(0, maxLength) + "...";
        }
    }

    public static class AuditLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditLoggingMiddleware>();
        }
    }
}
