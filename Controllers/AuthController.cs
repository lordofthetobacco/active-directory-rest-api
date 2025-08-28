using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ActiveDirectory_API.Services;
using System.Diagnostics;

namespace ActiveDirectory_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    public AuthController(IAuditLoggingService auditLogger, ILogger<AuthController> logger) 
        : base(auditLogger, logger)
    {
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetCurrentUser";
        var resource = "Auth:CurrentUser";
        var requestData = (object?)null;

        try
        {
            LogRequest(action, resource, requestData);

            var userInfo = new UserInfo
            {
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                Name = User.Identity?.Name ?? string.Empty,
                Claims = User.Claims.Select(c => new ClaimInfo
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList()
            };

            stopwatch.Stop();
            LogResponse(action, resource, userInfo, 200, stopwatch.Elapsed);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            return StatusCode(500, "Error retrieving user information");
        }
    }

    [HttpGet("roles")]
    [Authorize]
    public ActionResult<UserRoles> GetUserRoles()
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUserRoles";
        var resource = "Auth:UserRoles";
        var requestData = (object?)null;

        try
        {
            LogRequest(action, resource, requestData);

            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "roles")
                .Select(c => c.Value)
                .ToList();

            var userRoles = new UserRoles
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                Username = User.Identity?.Name ?? string.Empty,
                Roles = roles,
                IsAdmin = roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                                       r.Equals("Global Administrator", StringComparison.OrdinalIgnoreCase))
            };

            stopwatch.Stop();
            LogResponse(action, resource, userRoles, 200, stopwatch.Elapsed);
            return Ok(userRoles);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            return StatusCode(500, "Error retrieving user roles");
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "Logout";
        var resource = "Auth:Logout";
        var requestData = (object?)null;

        try
        {
            LogRequest(action, resource, requestData);

            // Log successful logout
            _auditLogger.LogAuthenticationSuccess("Logout", User);
            
            // In a real application, you might want to implement proper logout logic
            // such as invalidating tokens, clearing cookies, etc.
            var result = new { message = "Logout successful" };
            
            stopwatch.Stop();
            LogResponse(action, resource, result, 200, stopwatch.Elapsed);
            return Ok(result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }
}

public class UserInfo
{
    public bool IsAuthenticated { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ClaimInfo> Claims { get; set; } = new();
}

public class ClaimInfo
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class UserRoles
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsAdmin { get; set; }
}
