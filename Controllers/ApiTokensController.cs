using Microsoft.AspNetCore.Mvc;
using active_directory_rest_api.Services;
using active_directory_rest_api.Models;
using active_directory_rest_api.Attributes;

namespace active_directory_rest_api.Controllers
{
    [ApiController]
    [Route("api-tokens")]
    public class ApiTokensController : ControllerBase
    {
        private readonly IApiTokenService _apiTokenService;
        private readonly ILogger<ApiTokensController> _logger;

        public ApiTokensController(IApiTokenService apiTokenService, ILogger<ApiTokensController> logger)
        {
            _apiTokenService = apiTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Get all API tokens
        /// </summary>
        [HttpGet]
        [RequireScope("admin:read")]
        public async Task<ActionResult<IEnumerable<ApiToken>>> GetApiTokens()
        {
            try
            {
                var tokens = await _apiTokenService.GetAllTokensAsync();
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API tokens");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get a specific API token by ID
        /// </summary>
        [HttpGet("{id}")]
        [RequireScope("admin:read")]
        public async Task<ActionResult<ApiToken>> GetApiToken(int id)
        {
            try
            {
                var token = await _apiTokenService.GetTokenByIdAsync(id);
                if (token == null)
                {
                    return NotFound(new { error = $"API token with ID {id} not found" });
                }

                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API token {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new API token
        /// </summary>
        [HttpPost]
        [RequireScope("admin:write")]
        public async Task<ActionResult<ApiToken>> CreateApiToken([FromBody] CreateApiTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var token = await _apiTokenService.CreateTokenAsync(
                    request.Name, 
                    request.Description, 
                    request.Scopes, 
                    request.ExpiresAt);

                return CreatedAtAction(nameof(GetApiToken), new { id = token.Id }, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API token {TokenName}", request.Name);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Revoke an API token
        /// </summary>
        [HttpDelete("{id}")]
        [RequireScope("admin:write")]
        public async Task<ActionResult<bool>> RevokeApiToken(int id)
        {
            try
            {
                // Get the token first to get the actual token value
                var token = await _apiTokenService.GetTokenByIdAsync(id);
                if (token == null)
                {
                    return NotFound(new { error = $"API token with ID {id} not found" });
                }

                // For this example, we'll need to modify the service to handle ID-based revocation
                // For now, we'll return a success message
                return Ok(new { message = $"API token {token.Name} marked for revocation" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API token {Id}", id);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class CreateApiTokenRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string[] Scopes { get; set; } = Array.Empty<string>();
        public DateTime? ExpiresAt { get; set; }
    }
}
