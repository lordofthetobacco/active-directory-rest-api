using Microsoft.AspNetCore.Mvc;
using active_directory_rest_api.Services;
using active_directory_rest_api.Models.DTOs;
using active_directory_rest_api.Attributes;

namespace active_directory_rest_api.Controllers
{
    [ApiController]
    [Route("ou")]
    public class OrganizationalUnitsController : ControllerBase
    {
        private readonly IActiveDirectoryService _adService;
        private readonly ILogger<OrganizationalUnitsController> _logger;

        public OrganizationalUnitsController(IActiveDirectoryService adService, ILogger<OrganizationalUnitsController> logger)
        {
            _adService = adService;
            _logger = logger;
        }

        /// <summary>
        /// Get all organizational units
        /// </summary>
        [HttpGet]
        [RequireScope("ous:read")]
        public async Task<ActionResult<IEnumerable<OrganizationalUnitDto>>> GetOrganizationalUnits()
        {
            try
            {
                var ous = await _adService.GetOrganizationalUnitsAsync();
                return Ok(ous);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizational units");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new organizational unit
        /// </summary>
        [HttpPost]
        [RequireScope("ous:write")]
        public async Task<ActionResult<OrganizationalUnitDto>> CreateOrganizationalUnit([FromBody] CreateOrganizationalUnitDto createOuDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ou = await _adService.CreateOrganizationalUnitAsync(createOuDto);
                return CreatedAtAction(nameof(GetOrganizationalUnit), new { ou = ou.Name }, ou);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organizational unit {OUName}", createOuDto.Name);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get a specific organizational unit
        /// </summary>
        [HttpGet("{ou}")]
        [RequireScope("ous:read")]
        public async Task<ActionResult<OrganizationalUnitDto>> GetOrganizationalUnit(string ou)
        {
            try
            {
                var ouDto = await _adService.GetOrganizationalUnitAsync(ou);
                if (ouDto == null)
                {
                    return NotFound(new { error = $"Organizational unit {ou} not found" });
                }

                return Ok(ouDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizational unit {OUName}", ou);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if an organizational unit exists
        /// </summary>
        [HttpGet("{ou}/exists")]
        [RequireScope("ous:read")]
        public async Task<ActionResult<bool>> OrganizationalUnitExists(string ou)
        {
            try
            {
                var exists = await _adService.OrganizationalUnitExistsAsync(ou);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if organizational unit {OUName} exists", ou);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an organizational unit
        /// </summary>
        [HttpPut("{ou}")]
        [RequireScope("ous:write")]
        public async Task<ActionResult<OrganizationalUnitDto>> UpdateOrganizationalUnit(string ou, [FromBody] UpdateOrganizationalUnitDto updateOuDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedOu = await _adService.UpdateOrganizationalUnitAsync(ou, updateOuDto);
                return Ok(updatedOu);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organizational unit {OUName}", ou);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete an organizational unit
        /// </summary>
        [HttpDelete("{ou}")]
        [RequireScope("ous:delete")]
        public async Task<ActionResult<bool>> DeleteOrganizationalUnit(string ou)
        {
            try
            {
                var success = await _adService.DeleteOrganizationalUnitAsync(ou);
                if (success)
                {
                    return Ok(new { message = $"Organizational unit {ou} deleted successfully" });
                }
                else
                {
                    return NotFound(new { error = $"Organizational unit {ou} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organizational unit {OUName}", ou);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
