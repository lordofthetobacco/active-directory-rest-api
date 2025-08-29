using Microsoft.AspNetCore.Mvc;
using active_directory_rest_api.Services;
using active_directory_rest_api.Models.DTOs;
using active_directory_rest_api.Attributes;

namespace active_directory_rest_api.Controllers
{
    [ApiController]
    [Route("group")]
    public class GroupsController : ControllerBase
    {
        private readonly IActiveDirectoryService _adService;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(IActiveDirectoryService adService, ILogger<GroupsController> logger)
        {
            _adService = adService;
            _logger = logger;
        }

        /// <summary>
        /// Get all groups
        /// </summary>
        [HttpGet]
        [RequireScope("groups:read")]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroups()
        {
            try
            {
                var groups = await _adService.GetGroupsAsync();
                return Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting groups");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new group
        /// </summary>
        [HttpPost]
        [RequireScope("groups:write")]
        public async Task<ActionResult<GroupDto>> CreateGroup([FromBody] CreateGroupDto createGroupDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var group = await _adService.CreateGroupAsync(createGroupDto);
                return CreatedAtAction(nameof(GetGroup), new { group = group.SamAccountName }, group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group {GroupName}", createGroupDto.SamAccountName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get a specific group
        /// </summary>
        [HttpGet("{group}")]
        [RequireScope("groups:read")]
        public async Task<ActionResult<GroupDto>> GetGroup(string group)
        {
            try
            {
                var groupDto = await _adService.GetGroupAsync(group);
                if (groupDto == null)
                {
                    return NotFound(new { error = $"Group {group} not found" });
                }

                return Ok(groupDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group {GroupName}", group);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if a group exists
        /// </summary>
        [HttpGet("{group}/exists")]
        [RequireScope("groups:read")]
        public async Task<ActionResult<bool>> GroupExists(string group)
        {
            try
            {
                var exists = await _adService.GroupExistsAsync(group);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if group {GroupName} exists", group);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Update a group
        /// </summary>
        [HttpPut("{group}")]
        [RequireScope("groups:write")]
        public async Task<ActionResult<GroupDto>> UpdateGroup(string group, [FromBody] UpdateGroupDto updateGroupDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedGroup = await _adService.UpdateGroupAsync(group, updateGroupDto);
                return Ok(updatedGroup);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group {GroupName}", group);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Add a user to a group
        /// </summary>
        [HttpPost("{group}/user/{user}")]
        [RequireScope("groups:write")]
        public async Task<ActionResult<bool>> AddUserToGroup(string group, string user)
        {
            try
            {
                var success = await _adService.AddUserToGroupAsync(group, user);
                if (success)
                {
                    return Ok(new { message = $"User {user} added to group {group} successfully" });
                }
                else
                {
                    return NotFound(new { error = $"Group {group} or user {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {Username} to group {GroupName}", user, group);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Remove a user from a group
        /// </summary>
        [HttpDelete("{group}/user/{user}")]
        [RequireScope("groups:write")]
        public async Task<ActionResult<bool>> RemoveUserFromGroup(string group, string user)
        {
            try
            {
                var success = await _adService.RemoveUserFromGroupAsync(group, user);
                if (success)
                {
                    return Ok(new { message = $"User {user} removed from group {group} successfully" });
                }
                else
                {
                    return NotFound(new { error = $"Group {group} or user {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {Username} from group {GroupName}", user, group);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a group
        /// </summary>
        [HttpDelete("{group}")]
        [RequireScope("groups:delete")]
        public async Task<ActionResult<bool>> DeleteGroup(string group)
        {
            try
            {
                var success = await _adService.DeleteGroupAsync(group);
                if (success)
                {
                    return Ok(new { message = $"Group {group} deleted successfully" });
                }
                else
                {
                    return NotFound(new { error = $"Group {group} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group {GroupName}", group);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
