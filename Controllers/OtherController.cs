using Microsoft.AspNetCore.Mvc;
using active_directory_rest_api.Services;
using active_directory_rest_api.Attributes;

namespace active_directory_rest_api.Controllers
{
    [ApiController]
    public class OtherController : ControllerBase
    {
        private readonly IActiveDirectoryService _adService;
        private readonly ILogger<OtherController> _logger;

        public OtherController(IActiveDirectoryService adService, ILogger<OtherController> logger)
        {
            _adService = adService;
            _logger = logger;
        }

        /// <summary>
        /// Get other Active Directory information
        /// </summary>
        [HttpGet("other")]
        [RequireScope("other:read")]
        public async Task<ActionResult<object>> GetOther()
        {
            try
            {
                var result = await _adService.GetOtherAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting other information");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all Active Directory information summary
        /// </summary>
        [HttpGet("all")]
        [RequireScope("all:read")]
        public async Task<ActionResult<object>> GetAll()
        {
            try
            {
                var result = await _adService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all information");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Find items using a custom filter
        /// </summary>
        [HttpGet("find/{filter}")]
        [RequireScope("find:read")]
        public async Task<ActionResult<object>> Find(string filter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filter))
                {
                    return BadRequest(new { error = "Filter cannot be empty" });
                }

                var result = await _adService.FindAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding items with filter {Filter}", filter);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get Active Directory status
        /// </summary>
        [HttpGet("status")]
        [RequireScope("status:read")]
        public async Task<ActionResult<object>> GetStatus()
        {
            try
            {
                var result = await _adService.GetStatusAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
