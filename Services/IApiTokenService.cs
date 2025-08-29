using active_directory_rest_api.Models;

namespace active_directory_rest_api.Services
{
    public interface IApiTokenService
    {
        Task<ApiToken?> ValidateTokenAsync(string token);
        Task<bool> HasScopeAsync(string token, string scope);
        Task<bool> HasAnyScopeAsync(string token, params string[] scopes);
        Task<bool> HasAllScopesAsync(string token, params string[] scopes);
        Task UpdateLastUsedAsync(string token);
        Task<ApiToken> CreateTokenAsync(string name, string description, string[] scopes, DateTime? expiresAt = null);
        Task<bool> RevokeTokenAsync(string token);
        Task<IEnumerable<ApiToken>> GetAllTokensAsync();
        Task<ApiToken?> GetTokenByIdAsync(int id);
    }
}
