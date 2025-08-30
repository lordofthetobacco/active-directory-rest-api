using active_directory_rest_api.Models;

namespace active_directory_rest_api.Services;

public interface IActiveDirectoryService
{
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserAsync(string username);
    Task<bool> UserExistsAsync(string username);
    Task<bool> CreateUserAsync(CreateUserDto userDto);
    Task<bool> UpdateUserAsync(string username, UpdateUserDto userDto);
    Task<bool> DeleteUserAsync(string username);
    Task<bool> AuthenticateUserAsync(string username, string password);
    Task<bool> ChangePasswordAsync(string username, string newPassword);
    Task<bool> SetPasswordNeverExpiresAsync(string username, bool neverExpires);
    Task<bool> SetPasswordExpiresAsync(string username, bool expires);
    Task<bool> EnableUserAsync(string username, bool enabled);
    Task<bool> MoveUserAsync(string username, string newOu);
    Task<bool> UnlockUserAsync(string username);
    Task<bool> IsUserMemberOfGroupAsync(string username, string groupName);

    Task<List<QueryableResponseDto>> GetUsersQueryableAsync(List<string>? attributes = null);
    Task<QueryableResponseDto?> GetUserQueryableAsync(string username, List<string>? attributes = null);
    Task<List<QueryableResponseDto>> GetGroupsQueryableAsync(List<string>? attributes = null);
    Task<QueryableResponseDto?> GetGroupQueryableAsync(string groupName, List<string>? attributes = null);
    Task<List<QueryableResponseDto>> GetOrganizationalUnitsQueryableAsync(List<string>? attributes = null);
    Task<QueryableResponseDto?> GetOrganizationalUnitQueryableAsync(string ouName, List<string>? attributes = null);

    Task<List<GroupDto>> GetGroupsAsync();
    Task<GroupDto?> GetGroupAsync(string groupName);
    Task<bool> GroupExistsAsync(string groupName);
    Task<bool> CreateGroupAsync(CreateGroupDto groupDto);
    Task<bool> AddUserToGroupAsync(string groupName, string username);
    Task<bool> RemoveUserFromGroupAsync(string groupName, string username);
    
    Task<List<OrganizationalUnitDto>> GetOrganizationalUnitsAsync();
    Task<OrganizationalUnitDto?> GetOrganizationalUnitAsync(string ouName);
    Task<bool> OrganizationalUnitExistsAsync(string ouName);
    Task<bool> CreateOrganizationalUnitAsync(CreateOrganizationalUnitDto ouDto);
    
    Task<object> GetAllAsync();
    Task<object> FindAsync(string filter);
    Task<object> GetStatusAsync();
}
