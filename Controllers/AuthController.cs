using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace ActiveDirectory_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        try
        {
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

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user information");
            return StatusCode(500, "Error retrieving user information");
        }
    }

    [HttpGet("roles")]
    [Authorize]
    public ActionResult<UserRoles> GetUserRoles()
    {
        try
        {
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

            return Ok(userRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user roles");
            return StatusCode(500, "Error retrieving user roles");
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        // In a real application, you might want to implement proper logout logic
        // such as invalidating tokens, clearing cookies, etc.
        return Ok(new { message = "Logout successful" });
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
