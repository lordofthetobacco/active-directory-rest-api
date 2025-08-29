using active_directory_rest_api.Models.DTOs;

namespace active_directory_rest_api.Services
{
    public interface IActiveDirectoryService
    {
        // User operations
        Task<IEnumerable<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserAsync(string username);
        Task<bool> UserExistsAsync(string username);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(string username, UpdateUserDto updateUserDto);
        Task<bool> AuthenticateUserAsync(string username, string password);
        Task<bool> ChangePasswordAsync(string username, string newPassword, bool forceChangeAtNextLogon = true);
        Task<bool> SetPasswordNeverExpiresAsync(string username);
        Task<bool> SetPasswordExpiresAsync(string username);
        Task<bool> EnableUserAsync(string username);
        Task<bool> DisableUserAsync(string username);
        Task<bool> MoveUserAsync(string username, string newOrganizationalUnit);
        Task<bool> UnlockUserAsync(string username);
        Task<bool> DeleteUserAsync(string username);
        Task<bool> IsUserMemberOfGroupAsync(string username, string groupName);

        // Group operations
        Task<IEnumerable<GroupDto>> GetGroupsAsync();
        Task<GroupDto?> GetGroupAsync(string groupName);
        Task<bool> GroupExistsAsync(string groupName);
        Task<GroupDto> CreateGroupAsync(CreateGroupDto createGroupDto);
        Task<GroupDto> UpdateGroupAsync(string groupName, UpdateGroupDto updateGroupDto);
        Task<bool> AddUserToGroupAsync(string groupName, string username);
        Task<bool> RemoveUserFromGroupAsync(string groupName, string username);
        Task<bool> DeleteGroupAsync(string groupName);

        // Organizational Unit operations
        Task<IEnumerable<OrganizationalUnitDto>> GetOrganizationalUnitsAsync();
        Task<OrganizationalUnitDto?> GetOrganizationalUnitAsync(string ouName);
        Task<bool> OrganizationalUnitExistsAsync(string ouName);
        Task<OrganizationalUnitDto> CreateOrganizationalUnitAsync(CreateOrganizationalUnitDto createOuDto);
        Task<OrganizationalUnitDto> UpdateOrganizationalUnitAsync(string ouName, UpdateOrganizationalUnitDto updateOuDto);
        Task<bool> DeleteOrganizationalUnitAsync(string ouName);

        // Other operations
        Task<object> GetOtherAsync();
        Task<object> GetAllAsync();
        Task<object> FindAsync(string filter);
        Task<object> GetStatusAsync();
    }
}
