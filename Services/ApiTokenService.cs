using System.Security.Cryptography;
using System.Text;
using active_directory_rest_api.Data;
using active_directory_rest_api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace active_directory_rest_api.Services
{
    public class ApiTokenService : IApiTokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiTokenService> _logger;

        public ApiTokenService(ApplicationDbContext context, ILogger<ApiTokenService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiToken?> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHash = HashToken(token);
                var apiToken = await _context.ApiTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.IsActive);

                if (apiToken != null)
                {
                    // Check if token has expired
                    if (apiToken.ExpiresAt.HasValue && apiToken.ExpiresAt.Value < DateTime.UtcNow)
                    {
                        _logger.LogWarning("Token {TokenName} has expired", apiToken.Name);
                        return null;
                    }

                    return apiToken;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return null;
            }
        }

        public async Task<bool> HasScopeAsync(string token, string scope)
        {
            try
            {
                var apiToken = await ValidateTokenAsync(token);
                if (apiToken == null)
                    return false;

                return apiToken.Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking scope {Scope} for token", scope);
                return false;
            }
        }

        public async Task<bool> HasAnyScopeAsync(string token, params string[] scopes)
        {
            try
            {
                var apiToken = await ValidateTokenAsync(token);
                if (apiToken == null)
                    return false;

                return scopes.Any(scope => apiToken.Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking any scope for token");
                return false;
            }
        }

        public async Task<bool> HasAllScopesAsync(string token, params string[] scopes)
        {
            try
            {
                var apiToken = await ValidateTokenAsync(token);
                if (apiToken == null)
                    return false;

                return scopes.All(scope => apiToken.Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking all scopes for token");
                return false;
            }
        }

        public async Task UpdateLastUsedAsync(string token)
        {
            try
            {
                var tokenHash = HashToken(token);
                var apiToken = await _context.ApiTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

                if (apiToken != null)
                {
                    apiToken.LastUsedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last used for token");
            }
        }

        public async Task<ApiToken> CreateTokenAsync(string name, string description, string[] scopes, DateTime? expiresAt = null)
        {
            try
            {
                var token = Guid.NewGuid().ToString("N");
                var tokenHash = HashToken(token);

                var apiToken = new ApiToken
                {
                    TokenHash = tokenHash,
                    Name = name,
                    Description = description,
                    Scopes = scopes,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                };

                _context.ApiTokens.Add(apiToken);
                await _context.SaveChangesAsync();

                // Return the token with the actual token value (not hash)
                apiToken.TokenHash = token; // Temporarily set to actual token for return
                return apiToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token {TokenName}", name);
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                var tokenHash = HashToken(token);
                var apiToken = await _context.ApiTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

                if (apiToken != null)
                {
                    apiToken.IsActive = false;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return false;
            }
        }

        public async Task<IEnumerable<ApiToken>> GetAllTokensAsync()
        {
            try
            {
                return await _context.ApiTokens
                    .OrderBy(t => t.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tokens");
                throw;
            }
        }

        public async Task<ApiToken?> GetTokenByIdAsync(int id)
        {
            try
            {
                return await _context.ApiTokens
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token by ID {Id}", id);
                throw;
            }
        }

        private string HashToken(string token)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
