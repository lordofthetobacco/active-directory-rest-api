using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using active_directory_rest_api.Models;
using active_directory_rest_api.Services;

namespace active_directory_rest_api.Controllers;

[ApiController]
[Route("ou")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public class OrganizationalUnitsController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly ILogger<OrganizationalUnitsController> _logger;

    public OrganizationalUnitsController(IActiveDirectoryService adService, ILogger<OrganizationalUnitsController> logger)
    {
        _adService = adService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrganizationalUnitDto>>>> GetOrganizationalUnits()
    {
        try
        {
            var ous = await _adService.GetOrganizationalUnitsAsync();
            return Ok(new ApiResponse<List<OrganizationalUnitDto>>
            {
                Success = true,
                Message = "Organizational units retrieved successfully",
                Data = ous
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizational units");
            return StatusCode(500, new ApiResponse<List<OrganizationalUnitDto>>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("queryable")]
    public async Task<ActionResult<ApiResponse<List<QueryableResponseDto>>>> GetOrganizationalUnitsQueryable([FromQuery] string? attributes)
    {
        try
        {
            var attributeList = !string.IsNullOrEmpty(attributes) 
                ? attributes.Split(',').Select(a => a.Trim()).ToList() 
                : null;
                
            var ous = await _adService.GetOrganizationalUnitsQueryableAsync(attributeList);
            return Ok(new ApiResponse<List<QueryableResponseDto>>
            {
                Success = true,
                Message = "Organizational units retrieved successfully with queryable attributes",
                Data = ous
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizational units with queryable attributes");
            return StatusCode(500, new ApiResponse<List<QueryableResponseDto>>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<bool>>> CreateOrganizationalUnit([FromBody] CreateOrganizationalUnitDto ouDto)
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

            var result = await _adService.CreateOrganizationalUnitAsync(ouDto);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Organizational unit created successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organizational unit {OuName}", ouDto.Name);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{ouName}")]
    public async Task<ActionResult<ApiResponse<OrganizationalUnitDto>>> GetOrganizationalUnit(string ouName)
    {
        try
        {
            var ou = await _adService.GetOrganizationalUnitAsync(ouName);
            if (ou == null)
            {
                return NotFound(new ApiResponse<OrganizationalUnitDto>
                {
                    Success = false,
                    Message = "Organizational unit not found"
                });
            }

            return Ok(new ApiResponse<OrganizationalUnitDto>
            {
                Success = true,
                Message = "Organizational unit retrieved successfully",
                Data = ou
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizational unit {OuName}", ouName);
            return StatusCode(500, new ApiResponse<OrganizationalUnitDto>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{ouName}/queryable")]
    public async Task<ActionResult<ApiResponse<QueryableResponseDto>>> GetOrganizationalUnitQueryable(string ouName, [FromQuery] string? attributes)
    {
        try
        {
            var attributeList = !string.IsNullOrEmpty(attributes) 
                ? attributes.Split(',').Select(a => a.Trim()).ToList() 
                : null;
                
            var ou = await _adService.GetOrganizationalUnitQueryableAsync(ouName, attributeList);
            if (ou == null)
            {
                return NotFound(new ApiResponse<QueryableResponseDto>
                {
                    Success = false,
                    Message = "Organizational unit not found"
                });
            }

            return Ok(new ApiResponse<QueryableResponseDto>
            {
                Success = true,
                Message = "Organizational unit retrieved successfully with queryable attributes",
                Data = ou
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizational unit {OuName} with queryable attributes", ouName);
            return StatusCode(500, new ApiResponse<QueryableResponseDto>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{ouName}/exists")]
    public async Task<ActionResult<ApiResponse<bool>>> OrganizationalUnitExists(string ouName)
    {
        try
        {
            var exists = await _adService.OrganizationalUnitExistsAsync(ouName);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Organizational unit existence checked successfully",
                Data = exists
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if organizational unit {OuName} exists", ouName);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }
}
