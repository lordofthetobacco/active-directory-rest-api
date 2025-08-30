using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using active_directory_rest_api.Models;
using active_directory_rest_api.Services;

namespace active_directory_rest_api.Controllers;

[ApiController]
[Route("user")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public class UsersController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IActiveDirectoryService adService, ILogger<UsersController> logger)
    {
        _adService = adService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers()
    {
        try
        {
            var users = await _adService.GetUsersAsync();
            return Ok(new ApiResponse<List<UserDto>>
            {
                Success = true,
                Message = "Users retrieved successfully",
                Data = users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, new ApiResponse<List<UserDto>>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("queryable")]
    public async Task<ActionResult<ApiResponse<List<QueryableResponseDto>>>> GetUsersQueryable([FromQuery] string? attributes)
    {
        try
        {
            var attributeList = !string.IsNullOrEmpty(attributes) 
                ? attributes.Split(',').Select(a => a.Trim()).ToList() 
                : null;
                
            var users = await _adService.GetUsersQueryableAsync(attributeList);
            return Ok(new ApiResponse<List<QueryableResponseDto>>
            {
                Success = true,
                Message = "Users retrieved successfully with queryable attributes",
                Data = users
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users with queryable attributes");
            return StatusCode(500, new ApiResponse<List<QueryableResponseDto>>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateUser([FromBody] CreateUserDto userDto)
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

            var result = await _adService.CreateUserAsync(userDto);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "User created successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", userDto.Username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string username)
    {
        try
        {
            var user = await _adService.GetUserAsync(username);
            if (user == null)
            {
                return NotFound(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<UserDto>
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {Username}", username);
            return StatusCode(500, new ApiResponse<UserDto>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{username}/queryable")]
    public async Task<ActionResult<ApiResponse<QueryableResponseDto>>> GetUserQueryable(string username, [FromQuery] string? attributes)
    {
        try
        {
            var attributeList = !string.IsNullOrEmpty(attributes) 
                ? attributes.Split(',').Select(a => a.Trim()).ToList() 
                : null;
                
            var user = await _adService.GetUserQueryableAsync(username, attributeList);
            if (user == null)
            {
                return NotFound(new ApiResponse<QueryableResponseDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<QueryableResponseDto>
            {
                Success = true,
                Message = "User retrieved successfully with queryable attributes",
                Data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {Username} with queryable attributes", username);
            return StatusCode(500, new ApiResponse<QueryableResponseDto>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPut("{username}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateUser(string username, [FromBody] UpdateUserDto userDto)
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

            var result = await _adService.UpdateUserAsync(username, userDto);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "User updated successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {Username}", username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{username}/exists")]
    public async Task<ActionResult<ApiResponse<bool>>> UserExists(string username)
    {
        try
        {
            var exists = await _adService.UserExistsAsync(username);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "User existence checked successfully",
                Data = exists
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {Username} exists", username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{username}/member-of/{groupName}")]
    public async Task<ActionResult<ApiResponse<bool>>> IsUserMemberOfGroup(string username, string groupName)
    {
        try
        {
            var isMember = await _adService.IsUserMemberOfGroupAsync(username, groupName);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Group membership checked successfully",
                Data = isMember
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {Username} is member of group {GroupName}", username, groupName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }



    [HttpPut("{username}/password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(string username, [FromBody] PasswordChangeRequest request)
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

            var result = await _adService.ChangePasswordAsync(username, request.NewPassword);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Password changed successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {Username}", username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPut("{username}/password-never-expires")]
    public async Task<ActionResult<ApiResponse<bool>>> SetPasswordNeverExpires(string username, [FromBody] bool neverExpires)
    {
        try
        {
            var result = await _adService.SetPasswordNeverExpiresAsync(username, neverExpires);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Password expiration policy updated successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting password never expires for user {Username}", username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPut("{username}/password-expires")]
    public async Task<ActionResult<ApiResponse<bool>>> SetPasswordExpires(string username, [FromBody] bool expires)
    {
        try
        {
            var result = await _adService.SetPasswordExpiresAsync(username, expires);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Password expiration policy updated successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting password expires for user {Username}", username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPut("{username}/enable")]
    public async Task<ActionResult<ApiResponse<bool>>> EnableUser(string username, [FromBody] bool enable)
    {
        try
        {
            var result = await _adService.EnableUserAsync(username, enable);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = $"User {(enable ? "enabled" : "disabled")} successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling/disabling user {Username}", username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPut("{username}/move")]
    public async Task<ActionResult<ApiResponse<bool>>> MoveUser(string username, [FromBody] string newOu)
    {
        try
        {
            var result = await _adService.MoveUserAsync(username, newOu);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "User moved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving user {Username}", username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPut("{username}/unlock")]
    public async Task<ActionResult<ApiResponse<bool>>> UnlockUser(string username)
    {
        try
        {
            var result = await _adService.UnlockUserAsync(username);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "User unlocked successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user {Username}", username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpDelete("{username}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string username)
    {
        try
        {
            var result = await _adService.DeleteUserAsync(username);
            if (!result)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "User disabled successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {Username}", username);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }
}
