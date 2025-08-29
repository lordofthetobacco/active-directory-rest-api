using active_directory_rest_api.Services;
using Microsoft.Extensions.Primitives;

namespace active_directory_rest_api.Middleware
{
    public class ApiTokenAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiTokenAuthenticationMiddleware> _logger;

        public ApiTokenAuthenticationMiddleware(RequestDelegate next, ILogger<ApiTokenAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IApiTokenService apiTokenService)
        {
            try
            {
                // Skip authentication for certain endpoints
                if (ShouldSkipAuthentication(context.Request.Path))
                {
                    await _next(context);
                    return;
                }

                // Extract token from Authorization header
                var token = ExtractTokenFromHeader(context.Request.Headers);
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No API token provided for endpoint {Endpoint}", context.Request.Path);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = "API token required" });
                    return;
                }

                // Validate token
                var apiToken = await apiTokenService.ValidateTokenAsync(token);
                if (apiToken == null)
                {
                    _logger.LogWarning("Invalid API token provided for endpoint {Endpoint}", context.Request.Path);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = "Invalid API token" });
                    return;
                }

                // Update last used timestamp
                await apiTokenService.UpdateLastUsedAsync(token);

                // Add token info to context items for controllers to use
                context.Items["ApiToken"] = apiToken;
                context.Items["TokenId"] = apiToken.Id;

                _logger.LogDebug("API token validated for endpoint {Endpoint} with token {TokenName}", 
                    context.Request.Path, apiToken.Name);

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in API token authentication middleware");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new { error = "Internal server error" });
            }
        }

        private bool ShouldSkipAuthentication(PathString path)
        {
            // Add endpoints that don't require authentication
            var skipPaths = new[]
            {
                "/swagger",
                "/swagger.json",
                "/swagger/v1/swagger.json",
                "/health",
                "/status"
            };

            return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath));
        }

        private string? ExtractTokenFromHeader(IHeaderDictionary headers)
        {
            // Check Authorization header
            if (headers.TryGetValue("Authorization", out StringValues authHeader))
            {
                var authValue = authHeader.FirstOrDefault();
                if (!string.IsNullOrEmpty(authValue) && authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return authValue.Substring("Bearer ".Length).Trim();
                }
            }

            // Check X-API-Key header as alternative
            if (headers.TryGetValue("X-API-Key", out StringValues apiKeyHeader))
            {
                return apiKeyHeader.FirstOrDefault();
            }

            return null;
        }
    }

    public static class ApiTokenAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiTokenAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiTokenAuthenticationMiddleware>();
        }
    }
}
