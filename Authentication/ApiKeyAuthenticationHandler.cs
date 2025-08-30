using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using active_directory_rest_api.Models;
using active_directory_rest_api.Services;

namespace active_directory_rest_api.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApiKeyService apiKeyService)
        : base(options, logger, encoder)
    {
        _apiKeyService = apiKeyService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("X-API-Key"))
        {
            return AuthenticateResult.Fail("API Key header not found.");
        }

        var providedApiKey = Request.Headers["X-API-Key"].ToString();

        if (string.IsNullOrEmpty(providedApiKey))
        {
            return AuthenticateResult.Fail("API Key is empty.");
        }

        var isValid = await _apiKeyService.IsValidApiKeyAsync(providedApiKey);
        if (!isValid)
        {
            return AuthenticateResult.Fail("Invalid API Key.");
        }

        // Update last used timestamp
        await _apiKeyService.UpdateLastUsedAsync(providedApiKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "API User"),
            new Claim("APIKey", providedApiKey)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
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
