using ActiveDirectory_API.Models;
using ActiveDirectory_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ActiveDirectory_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IActiveDirectoryService adService, ILogger<UsersController> logger)
    {
        _adService = adService;
        _logger = logger;
    }

    [HttpGet("{samAccountName}")]
    public async Task<ActionResult<ActiveDirectoryUser>> GetUser(string samAccountName)
    {
        var user = await _adService.GetUserAsync(samAccountName);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<ActiveDirectoryUser>> GetUserByEmail(string email)
    {
        var user = await _adService.GetUserByEmailAsync(email);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }

    [HttpGet("dn/{distinguishedName}")]
    public async Task<ActionResult<ActiveDirectoryUser>> GetUserByDN(string distinguishedName)
    {
        var user = await _adService.GetUserByDNAsync(distinguishedName);
        if (user == null)
            return NotFound();
        
        return Ok(user);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryUser>>> SearchUsers([FromBody] ActiveDirectorySearchRequest request)
    {
        var users = await _adService.SearchUsersAsync(request);
        return Ok(users);
    }

    [HttpPost("authenticate")]
    public async Task<ActionResult<bool>> AuthenticateUser([FromBody] AuthenticationRequest request)
    {
        var isAuthenticated = await _adService.AuthenticateUserAsync(request.Username, request.Password);
        return Ok(isAuthenticated);
    }

    [HttpGet("{username}/groups")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserGroups(string username)
    {
        var groups = await _adService.GetUserGroupsAsync(username);
        return Ok(groups);
    }

    [HttpGet("{username}/groups/{groupName}/ismember")]
    public async Task<ActionResult<bool>> IsUserInGroup(string username, string groupName)
    {
        var isMember = await _adService.IsUserInGroupAsync(username, groupName);
        return Ok(isMember);
    }

    [HttpPost]
    public async Task<ActionResult<bool>> CreateUser([FromBody] CreateUserRequest request)
    {
        var success = await _adService.CreateUserAsync(request.User, request.Password);
        if (success)
            return CreatedAtAction(nameof(GetUser), new { samAccountName = request.User.SamAccountName }, success);
        
        return BadRequest("Failed to create user");
    }

    [HttpPut("{samAccountName}")]
    public async Task<ActionResult<bool>> UpdateUser(string samAccountName, [FromBody] ActiveDirectoryUser user)
    {
        if (samAccountName != user.SamAccountName)
            return BadRequest("SamAccountName mismatch");
        
        var success = await _adService.UpdateUserAsync(user);
        if (success)
            return Ok(success);
        
        return BadRequest("Failed to update user");
    }

    [HttpPost("{samAccountName}/enable")]
    public async Task<ActionResult<bool>> EnableUser(string samAccountName)
    {
        var success = await _adService.EnableUserAsync(samAccountName);
        if (success)
            return Ok(success);
        
        return BadRequest("Failed to enable user");
    }

    [HttpPost("{samAccountName}/disable")]
    public async Task<ActionResult<bool>> DisableUser(string samAccountName)
    {
        var success = await _adService.DisableUserAsync(samAccountName);
        if (success)
            return Ok(success);
        
        return BadRequest("Failed to disable user");
    }

    [HttpPost("{samAccountName}/reset-password")]
    public async Task<ActionResult<bool>> ResetPassword(string samAccountName, [FromBody] ResetPasswordRequest request)
    {
        var success = await _adService.ResetPasswordAsync(samAccountName, request.NewPassword);
        if (success)
            return Ok(success);
        
        return BadRequest("Failed to reset password");
    }

    [HttpPost("{samAccountName}/unlock")]
    public async Task<ActionResult<bool>> UnlockUser(string samAccountName)
    {
        var success = await _adService.UnlockUserAsync(samAccountName);
        if (success)
            return Ok(success);
        
        return BadRequest("Failed to unlock user");
    }

    [HttpGet("{samAccountName}/extension-attributes")]
    public async Task<ActionResult<Dictionary<string, object>>> GetUserExtensionAttributes(
        string samAccountName, 
        [FromQuery] string[]? attributes = null)
    {
        var extensionAttributes = await _adService.GetUserExtensionAttributesAsync(samAccountName, attributes);
        if (extensionAttributes.Count == 0)
            return NotFound($"No extension attributes found for user {samAccountName}");
        
        return Ok(extensionAttributes);
    }

    [HttpGet("search/fullname")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryUser>>> SearchUsersByFullName(
        [FromQuery] string q, 
        [FromQuery] int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search query 'q' parameter is required");
        
        if (maxResults < 1 || maxResults > 100)
            return BadRequest("maxResults must be between 1 and 100");
        
        var users = await _adService.SearchUsersByFullNameAsync(q, maxResults);
        return Ok(users);
    }

    [HttpGet("search/upn")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryUser>>> SearchUsersByUPN(
        [FromQuery] string q, 
        [FromQuery] int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search query 'q' parameter is required");
        
        if (maxResults < 1 || maxResults > 100)
            return BadRequest("maxResults must be between 1 and 100");
        
        var users = await _adService.SearchUsersByUPNAsync(q, maxResults);
        return Ok(users);
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
