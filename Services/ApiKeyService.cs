using Microsoft.EntityFrameworkCore;
using active_directory_rest_api.Data;

namespace active_directory_rest_api.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly LoggingDbContext _context;
    private readonly ILogger<ApiKeyService> _logger;

    public ApiKeyService(LoggingDbContext context, ILogger<ApiKeyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ApiKey>> GetAllApiKeysAsync()
    {
        try
        {
            return await _context.ApiKeys
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all API keys");
            throw;
        }
    }

    public async Task<ApiKey?> GetApiKeyByIdAsync(int id)
    {
        try
        {
            return await _context.ApiKeys.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API key by ID {Id}", id);
            throw;
        }
    }

    public async Task<ApiKey?> GetApiKeyByKeyAsync(string key)
    {
        try
        {
            return await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.Key == key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API key by key");
            throw;
        }
    }

    public async Task<ApiKey> CreateApiKeyAsync(ApiKey apiKey)
    {
        try
        {
            // Generate a unique key if not provided
            if (string.IsNullOrEmpty(apiKey.Key))
            {
                apiKey.Key = GenerateApiKey();
            }

            apiKey.CreatedAt = DateTime.UtcNow;
            _context.ApiKeys.Add(apiKey);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("API key created: {Name}", apiKey.Name);
            return apiKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API key {Name}", apiKey.Name);
            throw;
        }
    }

    public async Task<bool> UpdateApiKeyAsync(ApiKey apiKey)
    {
        try
        {
            var existingKey = await _context.ApiKeys.FindAsync(apiKey.Id);
            if (existingKey == null)
                return false;

            existingKey.Name = apiKey.Name;
            existingKey.Description = apiKey.Description;
            existingKey.IsActive = apiKey.IsActive;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("API key updated: {Name}", apiKey.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating API key {Id}", apiKey.Id);
            throw;
        }
    }

    public async Task<bool> DeleteApiKeyAsync(int id)
    {
        try
        {
            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey == null)
                return false;

            _context.ApiKeys.Remove(apiKey);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("API key deleted: {Name}", apiKey.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting API key {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeactivateApiKeyAsync(int id)
    {
        try
        {
            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey == null)
                return false;

            apiKey.IsActive = false;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("API key deactivated: {Name}", apiKey.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating API key {Id}", id);
            throw;
        }
    }

    public async Task<bool> ActivateApiKeyAsync(int id)
    {
        try
        {
            var apiKey = await _context.ApiKeys.FindAsync(id);
            if (apiKey == null)
                return false;

            apiKey.IsActive = true;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("API key activated: {Name}", apiKey.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating API key {Id}", id);
            throw;
        }
    }

    public async Task UpdateLastUsedAsync(string key)
    {
        try
        {
            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.Key == key);
            
            if (apiKey != null)
            {
                apiKey.LastUsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last used for API key");
        }
    }

    public async Task<bool> IsValidApiKeyAsync(string key)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
                return false;

            var apiKey = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.Key == key && k.IsActive);
            
            return apiKey != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key");
            return false;
        }
    }

    private string GenerateApiKey()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 32)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
