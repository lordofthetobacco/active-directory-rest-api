using Microsoft.AspNetCore.Mvc;
using active_directory_rest_api.Services;
using active_directory_rest_api.Models.DTOs;
using active_directory_rest_api.Attributes;

namespace active_directory_rest_api.Controllers
{
    [ApiController]
    [Route("user")]
    public class UsersController : ControllerBase
    {
        private readonly IActiveDirectoryService _adService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IActiveDirectoryService adService, ILogger<UsersController> logger)
        {
            _adService = adService;
            _logger = logger;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        [RequireScope("users:read")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                var users = await _adService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [RequireScope("users:write")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _adService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUser), new { user = user.SamAccountName }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", createUserDto.SamAccountName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get a specific user
        /// </summary>
        [HttpGet("{user}")]
        [RequireScope("users:read")]
        public async Task<ActionResult<UserDto>> GetUser(string user)
        {
            try
            {
                var userDto = await _adService.GetUserAsync(user);
                if (userDto == null)
                {
                    return NotFound(new { error = $"User {user} not found" });
                }

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Update a user
        /// </summary>
        [HttpPut("{user}")]
        [RequireScope("users:write")]
        public async Task<ActionResult<UserDto>> UpdateUser(string user, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedUser = await _adService.UpdateUserAsync(user, updateUserDto);
                return Ok(updatedUser);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if a user exists
        /// </summary>
        [HttpGet("{user}/exists")]
        [RequireScope("users:read")]
        public async Task<ActionResult<bool>> UserExists(string user)
        {
            try
            {
                var exists = await _adService.UserExistsAsync(user);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {Username} exists", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if a user is a member of a specific group
        /// </summary>
        [HttpGet("{user}/member-of/{group}")]
        [RequireScope("users:read")]
        public async Task<ActionResult<bool>> IsUserMemberOfGroup(string user, string group)
        {
            try
            {
                var isMember = await _adService.IsUserMemberOfGroupAsync(user, group);
                return Ok(isMember);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {Username} is member of group {GroupName}", user, group);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Authenticate a user
        /// </summary>
        [HttpPost("{user}/authenticate")]
        [RequireScope("users:read")]
        public async Task<ActionResult<bool>> AuthenticateUser(string user, [FromBody] AuthenticateUserDto authenticateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var isAuthenticated = await _adService.AuthenticateUserAsync(authenticateDto.Username, authenticateDto.Password);
                return Ok(isAuthenticated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPut("{user}/password")]
        [RequireScope("users:write")]
        public async Task<ActionResult<bool>> ChangePassword(string user, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _adService.ChangePasswordAsync(user, changePasswordDto.NewPassword, changePasswordDto.ForceChangeAtNextLogon);
                if (success)
                {
                    return Ok(new { message = "Password changed successfully" });
                }
                else
                {
                    return NotFound(new { error = $"User {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Set password to never expire
        /// </summary>
        [HttpPut("{user}/password-never-expires")]
        [RequireScope("users:write")]
        public async Task<ActionResult<bool>> SetPasswordNeverExpires(string user)
        {
            try
            {
                var success = await _adService.SetPasswordNeverExpiresAsync(user);
                if (success)
                {
                    return Ok(new { message = "Password set to never expire" });
                }
                else
                {
                    return NotFound(new { error = $"User {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting password never expires for user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Set password to expire
        /// </summary>
        [HttpPut("{user}/password-expires")]
        [RequireScope("users:write")]
        public async Task<ActionResult<bool>> SetPasswordExpires(string user)
        {
            try
            {
                var success = await _adService.SetPasswordExpiresAsync(user);
                if (success)
                {
                    return Ok(new { message = "Password set to expire" });
                }
                else
                {
                    return NotFound(new { error = $"User {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting password expires for user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Enable a user
        /// </summary>
        [HttpPut("{user}/enable")]
        [RequireScope("users:write")]
        public async Task<ActionResult<bool>> EnableUser(string user)
        {
            try
            {
                var success = await _adService.EnableUserAsync(user);
                if (success)
                {
                    return Ok(new { message = "User enabled successfully" });
                }
                else
                {
                    return NotFound(new { error = $"User {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Disable a user
        /// </summary>
        [HttpPut("{user}/disable")]
        [RequireScope("users:write")]
        public async Task<ActionResult<bool>> DisableUser(string user)
        {
            try
            {
                var success = await _adService.DisableUserAsync(user);
                if (success)
                {
                    return Ok(new { message = "User disabled successfully" });
                }
                else
                {
                    return NotFound(new { error = $"User {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Move a user to a different organizational unit
        /// </summary>
        [HttpPut("{user}/move")]
        [RequireScope("users:write")]
        public async Task<ActionResult<bool>> MoveUser(string user, [FromBody] MoveUserDto moveUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _adService.MoveUserAsync(user, moveUserDto.NewOrganizationalUnit);
                if (success)
                {
                    return Ok(new { message = "User moved successfully" });
                }
                else
                {
                    return NotFound(new { error = $"User {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Unlock a user account
        /// </summary>
        [HttpPut("{user}/unlock")]
        [RequireScope("users:write")]
        public async Task<ActionResult<bool>> UnlockUser(string user)
        {
            try
            {
                var success = await _adService.UnlockUserAsync(user);
                if (success)
                {
                    return Ok(new { message = "User unlocked successfully" });
                }
                else
                {
                    return NotFound(new { error = $"User {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{user}")]
        [RequireScope("users:delete")]
        public async Task<ActionResult<bool>> DeleteUser(string user)
        {
            try
            {
                var success = await _adService.DeleteUserAsync(user);
                if (success)
                {
                    return Ok(new { message = "User deleted successfully" });
                }
                else
                {
                    return NotFound(new { error = $"User {user} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Username}", user);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
