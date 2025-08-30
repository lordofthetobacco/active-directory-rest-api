using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using active_directory_rest_api.Models;
using active_directory_rest_api.Services;

namespace active_directory_rest_api.Controllers;

[ApiController]
[Route("")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public class OtherController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly ILogger<OtherController> _logger;

    public OtherController(IActiveDirectoryService adService, ILogger<OtherController> logger)
    {
        _adService = adService;
        _logger = logger;
    }

    [HttpGet("other")]
    public async Task<ActionResult<ApiResponse<object>>> GetOther()
    {
        try
        {
            // This endpoint can be used for additional Active Directory operations
            // For now, returning a placeholder response
            var result = new
            {
                Message = "Other Active Directory operations endpoint",
                AvailableOperations = new[]
                {
                    "Custom LDAP queries",
                    "Advanced filtering",
                    "Bulk operations"
                }
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Other operations endpoint",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in other operations endpoint");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<object>>> GetAll()
    {
        try
        {
            var result = await _adService.GetAllAsync();
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "All Active Directory objects retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all Active Directory objects");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("find/users")]
    public async Task<ActionResult<ApiResponse<object>>> FindUsers([FromQuery] string? name = null, [FromQuery] string? email = null, [FromQuery] string? ou = null)
    {
        try
        {
            // Build LDAP filter for users
            var filter = "(&(objectClass=user)(objectCategory=person))";
            var additionalFilters = new List<string>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                additionalFilters.Add($"(|(cn={name}*)(sAMAccountName={name}*)(displayName={name}*))");
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                additionalFilters.Add($"(mail={email}*)");
            }

            if (!string.IsNullOrWhiteSpace(ou))
            {
                additionalFilters.Add($"(distinguishedName=*{ou}*)");
            }

            if (additionalFilters.Any())
            {
                filter = $"(&{filter}{string.Join("", additionalFilters)})";
            }

            var result = await _adService.FindAsync(filter);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "User search completed successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for users with filters: Name={Name}, Email={Email}, OU={OU}", name, email, ou);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("find/groups")]
    public async Task<ActionResult<ApiResponse<object>>> FindGroups([FromQuery] string? name = null, [FromQuery] string? description = null, [FromQuery] string? ou = null)
    {
        try
        {
            // Build LDAP filter for groups
            var filter = "(&(objectClass=group)(objectCategory=group))";
            var additionalFilters = new List<string>();

            if (!string.IsNullOrWhiteSpace(name))
            {
                additionalFilters.Add($"(|(cn={name}*)(sAMAccountName={name}*))");
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                additionalFilters.Add($"(description={description}*)");
            }

            if (!string.IsNullOrWhiteSpace(ou))
            {
                additionalFilters.Add($"(distinguishedName=*{ou}*)");
            }

            if (additionalFilters.Any())
            {
                filter = $"(&{filter}{string.Join("", additionalFilters)})";
            }

            var result = await _adService.FindAsync(filter);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Group search completed successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for groups with filters: Name={Name}, Description={Description}, OU={OU}", name, description, ou);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("find/custom")]
    public async Task<ActionResult<ApiResponse<object>>> FindCustom([FromQuery] string filter)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Filter parameter is required for custom search"
                });
            }

            var result = await _adService.FindAsync(filter);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Custom search completed successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in custom search with filter: {Filter}", filter);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<object>>> GetStatus()
    {
        try
        {
            var result = await _adService.GetStatusAsync();
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Status retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Active Directory status");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }
}
