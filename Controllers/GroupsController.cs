using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using active_directory_rest_api.Models;
using active_directory_rest_api.Services;

namespace active_directory_rest_api.Controllers;

[ApiController]
[Route("group")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public class GroupsController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(IActiveDirectoryService adService, ILogger<GroupsController> logger)
    {
        _adService = adService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<GroupDto>>>> GetGroups()
    {
        try
        {
            var groups = await _adService.GetGroupsAsync();
            return Ok(new ApiResponse<List<GroupDto>>
            {
                Success = true,
                Message = "Groups retrieved successfully",
                Data = groups
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups");
            return StatusCode(500, new ApiResponse<List<GroupDto>>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("queryable")]
    public async Task<ActionResult<ApiResponse<List<QueryableResponseDto>>>> GetGroupsQueryable([FromQuery] string? attributes)
    {
        try
        {
            var attributeList = !string.IsNullOrEmpty(attributes) 
                ? attributes.Split(',').Select(a => a.Trim()).ToList() 
                : null;
                
            var groups = await _adService.GetGroupsQueryableAsync(attributeList);
            return Ok(new ApiResponse<List<QueryableResponseDto>>
            {
                Success = true,
                Message = "Groups retrieved successfully with queryable attributes",
                Data = groups
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups with queryable attributes");
            return StatusCode(500, new ApiResponse<List<QueryableResponseDto>>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateGroup([FromBody] CreateGroupDto groupDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid model state"
                });
            }

            var result = await _adService.CreateGroupAsync(groupDto);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Group created successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group {GroupName}", groupDto.Name);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{groupName}")]
    public async Task<ActionResult<ApiResponse<GroupDto>>> GetGroup(string groupName)
    {
        try
        {
            var group = await _adService.GetGroupAsync(groupName);
            if (group == null)
            {
                return NotFound(new ApiResponse<GroupDto>
                {
                    Success = false,
                    Message = "Group not found"
                });
            }

            return Ok(new ApiResponse<GroupDto>
            {
                Success = true,
                Message = "Group retrieved successfully",
                Data = group
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupName}", groupName);
            return StatusCode(500, new ApiResponse<GroupDto>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{groupName}/queryable")]
    public async Task<ActionResult<ApiResponse<QueryableResponseDto>>> GetGroupQueryable(string groupName, [FromQuery] string? attributes)
    {
        try
        {
            var attributeList = !string.IsNullOrEmpty(attributes) 
                ? attributes.Split(',').Select(a => a.Trim()).ToList() 
                : null;
                
            var group = await _adService.GetGroupQueryableAsync(groupName, attributeList);
            if (group == null)
            {
                return NotFound(new ApiResponse<QueryableResponseDto>
                {
                    Success = false,
                    Message = "Group not found"
                });
            }

            return Ok(new ApiResponse<QueryableResponseDto>
            {
                Success = true,
                Message = "Group retrieved successfully with queryable attributes",
                Data = group
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupName} with queryable attributes", groupName);
            return StatusCode(500, new ApiResponse<QueryableResponseDto>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{groupName}/exists")]
    public async Task<ActionResult<ApiResponse<bool>>> GroupExists(string groupName)
    {
        try
        {
            var exists = await _adService.GroupExistsAsync(groupName);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Group existence checked successfully",
                Data = exists
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if group {GroupName} exists", groupName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("{groupName}/user/{username}")]
    public async Task<ActionResult<ApiResponse<bool>>> AddUserToGroup(string groupName, string username)
    {
        try
        {
            var result = await _adService.AddUserToGroupAsync(groupName, username);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Group or user not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "User added to group successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {Username} to group {GroupName}", username, groupName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpDelete("{groupName}/user/{username}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveUserFromGroup(string groupName, string username)
    {
        try
        {
            var result = await _adService.RemoveUserFromGroupAsync(groupName, username);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Group or user not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "User removed from group successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {Username} from group {GroupName}", username, groupName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }
}
