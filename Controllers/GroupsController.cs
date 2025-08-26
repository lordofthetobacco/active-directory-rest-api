using ActiveDirectory_API.Models;
using ActiveDirectory_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ActiveDirectory_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IActiveDirectoryService adService, ILogger<GroupsController> logger)
    {
        _adService = adService;
        _logger = logger;
    }

    [HttpGet("{groupName}")]
    public async Task<ActionResult<ActiveDirectoryGroup>> GetGroup(string groupName)
    {
        var group = await _adService.GetGroupAsync(groupName);
        if (group == null)
            return NotFound();
        
        return Ok(group);
    }

    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryGroup>>> SearchGroups([FromBody] ActiveDirectorySearchRequest request)
    {
        var groups = await _adService.SearchGroupsAsync(request);
        return Ok(groups);
    }

    [HttpGet("{groupName}/members/{username}/check")]
    public async Task<ActionResult<bool>> IsUserInGroup(string groupName, string username)
    {
        var isMember = await _adService.IsUserInGroupAsync(username, groupName);
        return Ok(isMember);
    }

    [HttpPost("{groupName}/members/{username}")]
    public async Task<ActionResult<bool>> AddUserToGroup(string groupName, string username)
    {
        var success = await _adService.AddUserToGroupAsync(username, groupName);
        if (success)
            return Ok(success);
        
        return BadRequest($"Failed to add user {username} to group {groupName}");
    }

    [HttpDelete("{groupName}/members/{username}")]
    public async Task<ActionResult<bool>> RemoveUserFromGroup(string groupName, string username)
    {
        var success = await _adService.RemoveUserFromGroupAsync(username, groupName);
        if (success)
            return Ok(success);
        
        return BadRequest($"Failed to remove user {username} from group {groupName}");
    }

    [HttpGet("search/name")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryGroup>>> SearchGroupsByName(
        [FromQuery] string q, 
        [FromQuery] int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search query 'q' parameter is required");
        
        if (maxResults < 1 || maxResults > 100)
            return BadRequest("maxResults must be between 1 and 100");
        
        var groups = await _adService.SearchGroupsByNameAsync(q, maxResults);
        return Ok(groups);
    }
}
