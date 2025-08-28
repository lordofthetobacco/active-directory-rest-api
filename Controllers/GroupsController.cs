using ActiveDirectory_API.Models;
using ActiveDirectory_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace ActiveDirectory_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class GroupsController : BaseController
{
    private readonly IActiveDirectoryService _adService;

    public GroupsController(IActiveDirectoryService adService, IAuditLoggingService auditLogger, ILogger<GroupsController> logger) 
        : base(auditLogger, logger)
    {
        _adService = adService;
    }

    [HttpGet("{groupName}")]
    public async Task<ActionResult<ActiveDirectoryGroup>> GetGroup(string groupName)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "GetGroup";
        var resource = $"Group:{groupName}";
        var requestData = new { groupName };

        try
        {
            LogRequest(action, resource, requestData);

            var group = await _adService.GetGroupAsync(groupName);
            stopwatch.Stop();

            if (group == null)
            {
                LogResponse(action, resource, null, 404, stopwatch.Elapsed);
                return NotFound();
            }

            LogResponse(action, resource, group, 200, stopwatch.Elapsed);
            return Ok(group);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryGroup>>> SearchGroups([FromBody] ActiveDirectorySearchRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "SearchGroups";
        var resource = "Groups:Search";
        var requestData = request;

        try
        {
            LogRequest(action, resource, requestData);

            var groups = await _adService.SearchGroupsAsync(request);
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

    [HttpGet("{groupName}/members/{username}/check")]
    public async Task<ActionResult<bool>> IsUserInGroup(string groupName, string username)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "IsUserInGroup";
        var resource = $"Group:{groupName}:Member:{username}";
        var requestData = new { groupName, username };

        try
        {
            LogRequest(action, resource, requestData);

            var isMember = await _adService.IsUserInGroupAsync(username, groupName);
            stopwatch.Stop();

            LogResponse(action, resource, isMember, 200, stopwatch.Elapsed);
            return Ok(isMember);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpPost("{groupName}/members/{username}")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> AddUserToGroup(string groupName, string username)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "AddUserToGroup";
        var resource = $"Group:{groupName}:Member:{username}";
        var requestData = new { groupName, username };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.AddUserToGroupAsync(username, groupName);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to add user {username} to group {groupName}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpDelete("{groupName}/members/{username}")]
    [Authorize(Policy = "RequireAdminPermissions")]
    public async Task<ActionResult<bool>> RemoveUserFromGroup(string groupName, string username)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "RemoveUserFromGroup";
        var resource = $"Group:{groupName}:Member:{username}";
        var requestData = new { groupName, username };

        LogAuthorizationSuccess(action, resource);

        try
        {
            LogRequest(action, resource, requestData);

            var success = await _adService.RemoveUserFromGroupAsync(username, groupName);
            stopwatch.Stop();

            if (success)
            {
                LogResponse(action, resource, success, 200, stopwatch.Elapsed);
                return Ok(success);
            }

            LogResponse(action, resource, null, 400, stopwatch.Elapsed);
            return BadRequest($"Failed to remove user {username} from group {groupName}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogError(action, resource, ex, requestData);
            throw;
        }
    }

    [HttpGet("search/name")]
    public async Task<ActionResult<IEnumerable<ActiveDirectoryGroup>>> SearchGroupsByName(
        [FromQuery] string q, 
        [FromQuery] int maxResults = 10)
    {
        var stopwatch = Stopwatch.StartNew();
        var action = "SearchGroupsByName";
        var resource = "Groups:Search:Name";
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

            var groups = await _adService.SearchGroupsByNameAsync(q, maxResults);
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
}
