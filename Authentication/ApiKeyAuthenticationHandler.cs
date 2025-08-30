using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using active_directory_rest_api.Models;

namespace active_directory_rest_api.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ApiKeyConfig _apiKeyConfig;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<ApiKeyConfig> apiKeyConfig)
        : base(options, logger, encoder)
    {
        _apiKeyConfig = apiKeyConfig.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("X-API-Key"))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key header not found."));
        }

        var providedApiKey = Request.Headers["X-API-Key"].ToString();

        if (string.IsNullOrEmpty(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key is empty."));
        }

        if (!_apiKeyConfig.ValidKeys.Contains(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "API User"),
            new Claim("APIKey", providedApiKey)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.ContentType = "application/json";
        
        var errorResponse = new ApiResponse<object>
        {
            Success = false,
            Message = "Authentication required. Please provide a valid API key in the X-API-Key header.",
            Data = null
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(errorResponse);
        await Response.WriteAsync(jsonResponse);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        Response.ContentType = "application/json";
        
        var errorResponse = new ApiResponse<object>
        {
            Success = false,
            Message = "Access forbidden. Your API key does not have permission to access this resource.",
            Data = null
        };

        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(errorResponse);
        await Response.WriteAsync(jsonResponse);
    }
}
