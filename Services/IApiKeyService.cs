using active_directory_rest_api.Data;

namespace active_directory_rest_api.Services;

public interface IApiKeyService
{
    Task<List<ApiKey>> GetAllApiKeysAsync();
    Task<ApiKey?> GetApiKeyByIdAsync(int id);
    Task<ApiKey?> GetApiKeyByKeyAsync(string key);
    Task<ApiKey> CreateApiKeyAsync(ApiKey apiKey);
    Task<bool> UpdateApiKeyAsync(ApiKey apiKey);
    Task<bool> DeleteApiKeyAsync(int id);
    Task<bool> DeactivateApiKeyAsync(int id);
    Task<bool> ActivateApiKeyAsync(int id);
    Task UpdateLastUsedAsync(string key);
    Task<bool> IsValidApiKeyAsync(string key);
}
