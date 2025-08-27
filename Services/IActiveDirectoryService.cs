using ActiveDirectory_API.Models;

namespace ActiveDirectory_API.Services;

public interface IActiveDirectoryService
{
    Task<ActiveDirectoryUser?> GetUserAsync(string samAccountName);
    Task<ActiveDirectoryUser?> GetUserAsync(string samAccountName, string[]? properties);
    Task<ActiveDirectoryUser?> GetUserByEmailAsync(string email);
    Task<ActiveDirectoryUser?> GetUserByEmailAsync(string email, string[]? properties);
    Task<ActiveDirectoryUser?> GetUserByDNAsync(string distinguishedName);
    Task<ActiveDirectoryUser?> GetUserByDNAsync(string distinguishedName, string[]? properties);
    Task<IEnumerable<ActiveDirectoryUser>> SearchUsersAsync(ActiveDirectorySearchRequest request);
    Task<ActiveDirectoryGroup?> GetGroupAsync(string groupName);
    Task<IEnumerable<ActiveDirectoryGroup>> SearchGroupsAsync(ActiveDirectorySearchRequest request);
    Task<IEnumerable<ActiveDirectoryGroup>> SearchGroupsByNameAsync(string groupName, int maxResults = 10);
    Task<bool> AuthenticateUserAsync(string username, string password);
    Task<bool> IsUserInGroupAsync(string username, string groupName);
    Task<bool> AddUserToGroupAsync(string username, string groupName);
    Task<bool> RemoveUserFromGroupAsync(string username, string groupName);
    Task<IEnumerable<string>> GetUserGroupsAsync(string username);
    Task<Dictionary<string, object>> GetUserExtensionAttributesAsync(string username, string[]? attributes = null);
    Task<Dictionary<string, object>> GetUserExtensionAttributesByUPNAsync(string upn, string[]? attributes = null);
    Task<IEnumerable<ActiveDirectoryUser>> SearchUsersByFullNameAsync(string fullName, int maxResults = 10);
    Task<IEnumerable<ActiveDirectoryUser>> SearchUsersByUPNAsync(string upn, int maxResults = 10);
    Task<bool> CreateUserAsync(ActiveDirectoryUser user, string password);
    Task<bool> UpdateUserAsync(ActiveDirectoryUser user);
    Task<bool> DeleteUserAsync(string samAccountName);
    Task<bool> DeleteUserByUPNAsync(string upn);
    Task<bool> EnableUserAsync(string samAccountName);
    Task<bool> EnableUserByUPNAsync(string upn);
    Task<bool> DisableUserAsync(string samAccountName);
    Task<bool> DisableUserByUPNAsync(string upn);
    Task<bool> ResetPasswordAsync(string samAccountName, string newPassword);
    Task<bool> ResetPasswordByUPNAsync(string upn, string newPassword);
    Task<bool> UnlockUserAsync(string samAccountName);
    Task<bool> UnlockUserByUPNAsync(string upn);
    
    Task<bool> ValidateConnectionAsync();
}
