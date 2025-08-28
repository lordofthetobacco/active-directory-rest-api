using ActiveDirectory_API.Models;
using ActiveDirectory_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace ActiveDirectory_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class UsersController : BaseController
{
    private readonly IActiveDirectoryService _adService;

    public UsersController(IActiveDirectoryService adService, IAuditLoggingService auditLogger, ILogger<UsersController> logger) 
        : base(auditLogger, logger)
    {
        _adService = adService;
    }

    [HttpGet("{samAccountName}")]
    public async Task<ActionResult<ActiveDirectoryUser>> GetUser(string samAccountName)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUser";
        var resource = $"User:{samAccountName}";
        var requestData = new { samAccountName };

        try
        {
            LogRequest(action, resource, requestData);

            var user = await _adService.GetUserAsync(samAccountName);
            stopwatch.Stop();

            if (user == null)
            {
                LogResponse(action, resource, null, 404, stopwatch.Elapsed);
                return NotFound();
            }

            LogResponse(action, resource, user, 200, stopwatch.Elapsed);
            return Ok(user);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("{samAccountName}/properties")]
    public async Task<ActionResult<ActiveDirectoryUser>> GetUserWithProperties(
        string samAccountName, 
        [FromQuery] string[]? properties = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUserWithProperties";
        var resource = $"User:{samAccountName}";
        var requestData = new { samAccountName, properties };

        try
        {
            LogRequest(action, resource, requestData);

            var user = await _adService.GetUserAsync(samAccountName, properties);
            stopwatch.Stop();

            if (user == null)
            {
                LogResponse(action, resource, null, 404, stopwatch.Elapsed);
                return NotFound();
            }

            LogResponse(action, resource, user, 200, stopwatch.Elapsed);
            return Ok(user);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<ActiveDirectoryUser>> GetUserByEmail(string email)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUserByEmail";
        var resource = $"User:Email:{email}";
        var requestData = new { email };

        try
        {
            LogRequest(action, resource, requestData);

            var user = await _adService.GetUserByEmailAsync(email);
            stopwatch.Stop();

            if (user == null)
            {
                LogResponse(action, resource, null, 404, stopwatch.Elapsed);
                return NotFound();
            }

            LogResponse(action, resource, user, 200, stopwatch.Elapsed);
            return Ok(user);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("email/{email}/properties")]
    public async Task<ActionResult<ActiveDirectoryUser>> GetUserByEmailWithProperties(
        string email, 
        [FromQuery] string[]? properties = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUserByEmailWithProperties";
        var resource = $"User:Email:{email}";
        var requestData = new { email, properties };

        try
        {
            LogRequest(action, resource, requestData);

            var user = await _adService.GetUserByEmailAsync(email, properties);
            stopwatch.Stop();

            if (user == null)
            {
                LogResponse(action, resource, null, 404, stopwatch.Elapsed);
                return NotFound();
            }

            LogResponse(action, resource, user, 200, stopwatch.Elapsed);
            return Ok(user);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("dn/{distinguishedName}")]
    public async Task<ActionResult<ActiveDirectoryUser>> GetUserByDN(string distinguishedName)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUserByDN";
        var resource = $"User:DN:{distinguishedName}";
        var requestData = new { distinguishedName };

        try
        {
            LogRequest(action, resource, requestData);

            var user = await _adService.GetUserByDNAsync(distinguishedName);
            stopwatch.Stop();

            if (user == null)
            {
                LogResponse(action, resource, null, 404, stopwatch.Elapsed);
                return NotFound();
            }

            LogResponse(action, resource, user, 200, stopwatch.Elapsed);
            return Ok(user);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("dn/{distinguishedName}/properties")]
    public async Task<ActionResult<ActiveDirectoryUser>> GetUserByDNWithProperties(
        string distinguishedName, 
        [FromQuery] string[]? properties = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUserByDNWithProperties";
        var resource = $"User:DN:{distinguishedName}";
        var requestData = new { distinguishedName, properties };

        try
        {
            LogRequest(action, resource, requestData);

            var user = await _adService.GetUserByDNAsync(distinguishedName, properties);
            stopwatch.Stop();

            if (user == null)
            {
                LogResponse(action, resource, null, 404, stopwatch.Elapsed);
                return NotFound();
            }

            LogResponse(action, resource, user, 200, stopwatch.Elapsed);
            return Ok(user);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryUser>>> SearchUsers([FromBody] ActiveDirectorySearchRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "SearchUsers";
        var resource = "Users:Search";
        var requestData = request;

        try
        {
            LogRequest(action, resource, requestData);

            var users = await _adService.SearchUsersAsync(request);
            stopwatch.Stop();

            LogResponse(action, resource, users, 200, stopwatch.Elapsed);
            return Ok(users);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("authenticate")]
    public async Task<ActionResult<bool>> AuthenticateUser([FromBody] AuthenticationRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "AuthenticateUser";
        var resource = $"User:Auth:{request.Username}";
        var requestData = new { request.Username, password = "***MASKED***" };

        try
        {
            LogRequest(action, resource, requestData);

            var isAuthenticated = await _adService.AuthenticateUserAsync(request.Username, request.Password);
            stopwatch.Stop();

            LogResponse(action, resource, isAuthenticated, 200, stopwatch.Elapsed);
            return Ok(isAuthenticated);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("{username}/groups")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserGroups(string username)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUserGroups";
        var resource = $"User:{username}:Groups";
        var requestData = new { username };

        try
        {
            LogRequest(action, resource, requestData);

            var groups = await _adService.GetUserGroupsAsync(username);
            stopwatch.Stop();

            LogResponse(action, resource, groups, 200, stopwatch.Elapsed);
            return Ok(groups);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> CreateUser([FromBody] CreateUserRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "CreateUser";
        var resource = $"User:{request.User.SamAccountName}";
        var requestData = new { request.User, password = "***MASKED***" };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.CreateUserAsync(request.User, request.Password);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to create user {request.User.SamAccountName}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPut("{samAccountName}")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> UpdateUser(string samAccountName, [FromBody] ActiveDirectoryUser user)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "UpdateUser";
        var resource = $"User:{samAccountName}";
        var requestData = user;

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.UpdateUserAsync(user);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to update user {samAccountName}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("{samAccountName}/enable")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> EnableUser(string samAccountName)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "EnableUser";
        var resource = $"User:{samAccountName}";
        var requestData = new { samAccountName };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.EnableUserAsync(samAccountName);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to enable user {samAccountName}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("upn/{upn}/enable")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> EnableUserByUPN(string upn)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "EnableUserByUPN";
        var resource = $"User:UPN:{upn}";
        var requestData = new { upn };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.EnableUserByUPNAsync(upn);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to enable user {upn}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("{samAccountName}/reset-password")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> ResetPassword(string samAccountName, [FromBody] ResetPasswordRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "ResetPassword";
        var resource = $"User:{samAccountName}";
        var requestData = new { samAccountName, newPassword = "***MASKED***" };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.ResetPasswordAsync(samAccountName, request.NewPassword);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to reset password for user {samAccountName}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("upn/{upn}/reset-password")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> ResetPasswordByUPN(string upn, [FromBody] ResetPasswordRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "ResetPasswordByUPN";
        var resource = $"User:UPN:{upn}";
        var requestData = new { upn, newPassword = "***MASKED***" };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.ResetPasswordByUPNAsync(upn, request.NewPassword);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to reset password for user {upn}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("{samAccountName}/unlock")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> UnlockUser(string samAccountName)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "UnlockUser";
        var resource = $"User:{samAccountName}";
        var requestData = new { samAccountName };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.UnlockUserAsync(samAccountName);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to unlock user {samAccountName}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("upn/{upn}/unlock")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> UnlockUserByUPN(string upn)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "UnlockUserByUPN";
        var resource = $"User:UPN:{upn}";
        var requestData = new { upn };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.UnlockUserByUPNAsync(upn);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to unlock user {upn}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("{samAccountName}/disable")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> DisableUser(string samAccountName)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "DisableUser";
        var resource = $"User:{samAccountName}";
        var requestData = new { samAccountName };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.DisableUserAsync(samAccountName);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to disable user {samAccountName}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("upn/{upn}/disable")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> DisableUserByUPN(string upn)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "DisableUserByUPN";
        var resource = $"User:UPN:{upn}";
        var requestData = new { upn };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.DisableUserByUPNAsync(upn);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to disable user {upn}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpDelete("{samAccountName}")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> DeleteUser(string samAccountName)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "DeleteUser";
        var resource = $"User:{samAccountName}";
        var requestData = new { samAccountName };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.DeleteUserAsync(samAccountName);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to delete user {samAccountName}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpDelete("upn/{upn}")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> DeleteUserByUPN(string upn)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "DeleteUserByUPN";
        var resource = $"User:UPN:{upn}";
        var requestData = new { upn };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.DeleteUserByUPNAsync(upn);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to delete user {upn}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("{samAccountName}/extension-attributes")]
    public async Task<ActionResult<Dictionary<string, object>>> GetUserExtensionAttributes(
        string samAccountName, 
        [FromQuery] string[]? attributes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUserExtensionAttributes";
        var resource = $"User:{samAccountName}:ExtensionAttributes";
        var requestData = new { samAccountName, attributes };

        try
        {
            LogRequest(action, resource, requestData);

            var extensionAttributes = await _adService.GetUserExtensionAttributesAsync(samAccountName, attributes);
            stopwatch.Stop();

            if (extensionAttributes.Count == 0)
            {
                LogResponse(action, resource, null, 404, stopwatch.Elapsed);
                return NotFound($"No extension attributes found for user {samAccountName}");
            }

            LogResponse(action, resource, extensionAttributes, 200, stopwatch.Elapsed);
            return Ok(extensionAttributes);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("upn/{upn}/extension-attributes")]
    public async Task<ActionResult<Dictionary<string, object>>> GetUserExtensionAttributesByUPN(
        string upn, 
        [FromQuery] string[]? attributes = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetUserExtensionAttributesByUPN";
        var resource = $"User:UPN:{upn}:ExtensionAttributes";
        var requestData = new { upn, attributes };

        try
        {
            LogRequest(action, resource, requestData);

            var extensionAttributes = await _adService.GetUserExtensionAttributesByUPNAsync(upn, attributes);
            stopwatch.Stop();

            if (extensionAttributes.Count == 0)
            {
                LogResponse(action, resource, null, 404, stopwatch.Elapsed);
                return NotFound($"No extension attributes found for user {upn}");
            }

            LogResponse(action, resource, extensionAttributes, 200, stopwatch.Elapsed);
            return Ok(extensionAttributes);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("search/fullname")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryUser>>> SearchUsersByFullName(
        [FromQuery] string q, 
        [FromQuery] int maxResults = 10)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "SearchUsersByFullName";
        var resource = "Users:Search:FullName";
        var requestData = new { q, maxResults };

        try
        {
            LogRequest(action, resource, requestData);

            if (string.IsNullOrWhiteSpace(q))
            {
                stopwatch.Stop();
                LogResponse(action, resource, null, 400, stopwatch.Elapsed);
                return BadRequest("Search query 'q' parameter is required");
            }

            if (maxResults < 1 || maxResults > 100)
            {
                stopwatch.Stop();
                LogResponse(action, resource, null, 400, stopwatch.Elapsed);
                return BadRequest("maxResults must be between 1 and 100");
            }

            var users = await _adService.SearchUsersByFullNameAsync(q, maxResults);
            stopwatch.Stop();

            LogResponse(action, resource, users, 200, stopwatch.Elapsed);
            return Ok(users);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("search/upn")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryUser>>> SearchUsersByUPN(
        [FromQuery] string q, 
        [FromQuery] int maxResults = 10)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "SearchUsersByUPN";
        var resource = "Users:Search:UPN";
        var requestData = new { q, maxResults };

        try
        {
            LogRequest(action, resource, requestData);

            if (string.IsNullOrWhiteSpace(q))
            {
                stopwatch.Stop();
                LogResponse(action, resource, null, 400, stopwatch.Elapsed);
                return BadRequest("Search query 'q' parameter is required");
            }

            if (maxResults < 1 || maxResults > 100)
            {
                stopwatch.Stop();
                LogResponse(action, resource, null, 400, stopwatch.Elapsed);
                return BadRequest("maxResults must be between 1 and 100");
            }

            var users = await _adService.SearchUsersByUPNAsync(q, maxResults);
            stopwatch.Stop();

            LogResponse(action, resource, users, 200, stopwatch.Elapsed);
            return Ok(users);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }
}

public class AuthenticationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateUserRequest
{
    public ActiveDirectoryUser User { get; set; } = new();
    public string Password { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
