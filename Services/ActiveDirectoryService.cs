using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using Microsoft.Extensions.Options;
using active_directory_rest_api.Models;
using System.Runtime.Versioning;

namespace active_directory_rest_api.Services;
[SupportedOSPlatform("windows")]
public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly ActiveDirectoryConfig _config;
    private readonly ILogger<ActiveDirectoryService> _logger;

    public ActiveDirectoryService(IOptions<ActiveDirectoryConfig> config, ILogger<ActiveDirectoryService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = new UserPrincipal(context);
            var searcher = new PrincipalSearcher(userPrincipal);
            var users = searcher.FindAll().Cast<UserPrincipal>();

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                userDtos.Add(new UserDto
                {
                    Username = user.SamAccountName ?? string.Empty,
                    DisplayName = user.DisplayName ?? string.Empty,
                    Email = user.EmailAddress ?? string.Empty,
                    Enabled = user.Enabled ?? false,
                    LockedOut = user.IsAccountLockedOut(),
                    LastLogon = user.LastLogon,
                    DistinguishedName = user.DistinguishedName ?? string.Empty
                });
            }

            return await Task.FromResult(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users from Active Directory");
            throw;
        }
    }

    public async Task<UserDto?> GetUserAsync(string username)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return null;

            return await Task.FromResult(new UserDto
            {
                Username = userPrincipal.SamAccountName ?? string.Empty,
                DisplayName = userPrincipal.DisplayName ?? string.Empty,
                Email = userPrincipal.EmailAddress ?? string.Empty,
                Enabled = userPrincipal.Enabled ?? false,
                LockedOut = userPrincipal.IsAccountLockedOut(),
                LastLogon = userPrincipal.LastLogon,
                DistinguishedName = userPrincipal.DistinguishedName ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {Username} from Active Directory", username);
            throw;
        }
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            return await Task.FromResult(userPrincipal != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {Username} exists", username);
            throw;
        }
    }

    public async Task<bool> CreateUserAsync(CreateUserDto userDto)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = new UserPrincipal(context, userDto.Username, userDto.Password, true);
            userPrincipal.DisplayName = userDto.DisplayName;
            userPrincipal.EmailAddress = userDto.Email;
            userPrincipal.Save();

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", userDto.Username);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(string username, UpdateUserDto userDto)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return false;

            if (!string.IsNullOrEmpty(userDto.DisplayName))
                userPrincipal.DisplayName = userDto.DisplayName;
            
            if (!string.IsNullOrEmpty(userDto.Email))
                userPrincipal.EmailAddress = userDto.Email;

            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {Username}", username);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(string username)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return false;

            userPrincipal.Enabled = false;
            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {Username}", username);
            throw;
        }
    }



    public async Task<bool> ChangePasswordAsync(string username, string newPassword)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return false;

            userPrincipal.SetPassword(newPassword);
            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {Username}", username);
            throw;
        }
    }

    public async Task<bool> SetPasswordNeverExpiresAsync(string username, bool neverExpires)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return false;

            userPrincipal.PasswordNeverExpires = neverExpires;
            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting password never expires for user {Username}", username);
            throw;
        }
    }

    public async Task<bool> SetPasswordExpiresAsync(string username, bool expires)
    {
        return await SetPasswordNeverExpiresAsync(username, !expires);
    }

    public async Task<bool> EnableUserAsync(string username, bool enable)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return false;

            userPrincipal.Enabled = enable;
            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling/disabling user {Username}", username);
            throw;
        }
    }

    public async Task<bool> MoveUserAsync(string username, string newOu)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return false;

            // This is a simplified implementation - in practice you'd need to handle the move operation
            // using DirectoryEntry and DirectorySearcher for more complex scenarios
            _logger.LogWarning("Move user operation not fully implemented for {Username} to {NewOu}", username, newOu);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving user {Username}", username);
            throw;
        }
    }

    public async Task<bool> UnlockUserAsync(string username)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return false;

            userPrincipal.UnlockAccount();
            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user {Username}", username);
            throw;
        }
    }

    public async Task<bool> IsUserMemberOfGroupAsync(string username, string groupName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return false;

            var groups = userPrincipal.GetGroups();
            return await Task.FromResult(groups.Any(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {Username} is member of group {GroupName}", username, groupName);
            throw;
        }
    }

    public async Task<List<GroupDto>> GetGroupsAsync()
    {
        try
        {
            using var context = CreatePrincipalContext();
            var groupPrincipal = new GroupPrincipal(context);
            var searcher = new PrincipalSearcher(groupPrincipal);
            var groups = searcher.FindAll().Cast<GroupPrincipal>();

            var groupDtos = new List<GroupDto>();
            foreach (var group in groups)
            {
                var members = group.GetMembers().Select(m => m.SamAccountName ?? string.Empty).ToList();
                groupDtos.Add(new GroupDto
                {
                    Name = group.Name ?? string.Empty,
                    Description = group.Description ?? string.Empty,
                    DistinguishedName = group.DistinguishedName ?? string.Empty,
                    Members = members
                });
            }

            return await Task.FromResult(groupDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups from Active Directory");
            throw;
        }
    }

    public async Task<GroupDto?> GetGroupAsync(string groupName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var groupPrincipal = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
            
            if (groupPrincipal == null) return null;

            var members = groupPrincipal.GetMembers().Select(m => m.SamAccountName ?? string.Empty).ToList();
            return await Task.FromResult(new GroupDto
            {
                Name = groupPrincipal.Name ?? string.Empty,
                Description = groupPrincipal.Description ?? string.Empty,
                DistinguishedName = groupPrincipal.DistinguishedName ?? string.Empty,
                Members = members
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupName} from Active Directory", groupName);
            throw;
        }
    }

    public async Task<bool> GroupExistsAsync(string groupName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var groupPrincipal = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
            return await Task.FromResult(groupPrincipal != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if group {GroupName} exists", groupName);
            throw;
        }
    }

    public async Task<bool> CreateGroupAsync(CreateGroupDto groupDto)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var groupPrincipal = new GroupPrincipal(context, groupDto.Name);
            groupPrincipal.Description = groupDto.Description;
            groupPrincipal.Save();

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating group {GroupName}", groupDto.Name);
            throw;
        }
    }

    public async Task<bool> AddUserToGroupAsync(string groupName, string username)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var groupPrincipal = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (groupPrincipal == null || userPrincipal == null) return false;

            groupPrincipal.Members.Add(userPrincipal);
            groupPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {Username} to group {GroupName}", username, groupName);
            throw;
        }
    }

    public async Task<bool> RemoveUserFromGroupAsync(string groupName, string username)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var groupPrincipal = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (groupPrincipal == null || userPrincipal == null) return false;

            groupPrincipal.Members.Remove(userPrincipal);
            groupPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {Username} from group {GroupName}", username, groupName);
            throw;
        }
    }

    public async Task<List<OrganizationalUnitDto>> GetOrganizationalUnitsAsync()
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Server}/{_config.SearchBase}", _config.Username, _config.Password);
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = "(objectClass=organizationalUnit)",
                PropertiesToLoad = { "name", "description", "distinguishedName" }
            };

            var results = searcher.FindAll();
            var ouDtos = new List<OrganizationalUnitDto>();

            foreach (SearchResult result in results)
            {
                ouDtos.Add(new OrganizationalUnitDto
                {
                    Name = result.Properties["name"][0]?.ToString() ?? string.Empty,
                    Description = result.Properties["description"][0]?.ToString() ?? string.Empty,
                    DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty
                });
            }

            return await Task.FromResult(ouDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizational units from Active Directory");
            throw;
        }
    }

    public async Task<OrganizationalUnitDto?> GetOrganizationalUnitAsync(string ouName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Server}/{_config.SearchBase}", _config.Username, _config.Password);
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = $"(&(objectClass=organizationalUnit)(name={ouName}))",
                PropertiesToLoad = { "name", "description", "distinguishedName" }
            };

            var result = searcher.FindOne();
            if (result == null) return null;

            return await Task.FromResult(new OrganizationalUnitDto
            {
                Name = result.Properties["name"][0]?.ToString() ?? string.Empty,
                Description = result.Properties["description"][0]?.ToString() ?? string.Empty,
                DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizational unit {OuName} from Active Directory", ouName);
            throw;
        }
    }

    public async Task<bool> OrganizationalUnitExistsAsync(string ouName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Server}/{_config.SearchBase}", _config.Username, _config.Password);
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = $"(&(objectClass=organizationalUnit)(name={ouName}))"
            };

            var result = searcher.FindOne();
            return await Task.FromResult(result != null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if organizational unit {OuName} exists", ouName);
            throw;
        }
    }

    public async Task<bool> CreateOrganizationalUnitAsync(CreateOrganizationalUnitDto ouDto)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Server}/{_config.SearchBase}", _config.Username, _config.Password);
            var newOu = directoryEntry.Children.Add($"OU={ouDto.Name}", "organizationalUnit");
            newOu.Properties["description"].Add(ouDto.Description);
            newOu.CommitChanges();

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organizational unit {OuName}", ouDto.Name);
            throw;
        }
    }

    public async Task<object> GetAllAsync()
    {
        try
        {
            var users = await GetUsersAsync();
            var groups = await GetGroupsAsync();
            var ous = await GetOrganizationalUnitsAsync();

            return await Task.FromResult(new
            {
                Users = users,
                Groups = groups,
                OrganizationalUnits = ous
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all Active Directory objects");
            throw;
        }
    }

    public async Task<object> FindAsync(string filter)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Server}/{_config.SearchBase}", _config.Username, _config.Password);
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = filter,
                PropertiesToLoad = { "name", "objectClass", "distinguishedName" }
            };

            var results = searcher.FindAll();
            var foundObjects = new List<object>();

            foreach (SearchResult result in results)
            {
                foundObjects.Add(new
                {
                    Name = result.Properties["name"][0]?.ToString(),
                    ObjectClass = result.Properties["objectClass"][0]?.ToString(),
                    DistinguishedName = result.Properties["distinguishedName"][0]?.ToString()
                });
            }

            return await Task.FromResult(foundObjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Active Directory with filter: {Filter}", filter);
            throw;
        }
    }

    public async Task<object> GetStatusAsync()
    {
        try
        {
            using var context = CreatePrincipalContext();
            var isConnected = context.ValidateCredentials(_config.Username, _config.Password);

            return await Task.FromResult(new
            {
                Connected = isConnected,
                Server = _config.Server,
                Domain = _config.Domain,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Active Directory status");
            throw;
        }
    }

    public async Task<List<QueryableResponseDto>> GetUsersQueryableAsync(List<string>? attributes = null)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Domain}/{_config.SearchBase}", _config.Username, _config.Password);
            
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = "(&(objectClass=user)(objectCategory=person))"
            };
            
            // Always include distinguishedName as it's required for the response
            searcher.PropertiesToLoad.Add("distinguishedName");
            
            if (attributes != null && attributes.Any())
            {
                foreach (var attr in attributes)
                {
                    if (!attr.Equals("distinguishedName", StringComparison.OrdinalIgnoreCase))
                    {
                        searcher.PropertiesToLoad.Add(attr);
                    }
                }
            }
            else
            {
                searcher.PropertiesToLoad.Add("*");
            }

            var results = searcher.FindAll();
            var userDtos = new List<QueryableResponseDto>();

            foreach (SearchResult result in results)
            {
                var userDto = new QueryableResponseDto
                {
                    DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty,
                    ObjectClass = "user"
                };

                foreach (string propertyName in result.Properties.PropertyNames)
                {
                    if (result.Properties[propertyName].Count > 0)
                    {
                        if (result.Properties[propertyName].Count == 1)
                        {
                            userDto.Attributes[propertyName] = result.Properties[propertyName][0];
                        }
                        else
                        {
                            userDto.Attributes[propertyName] = result.Properties[propertyName].Cast<object>().ToArray();
                        }
                    }
                }

                userDtos.Add(userDto);
            }

            return await Task.FromResult(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users from Active Directory");
            throw;
        }
    }

    public async Task<QueryableResponseDto?> GetUserQueryableAsync(string username, List<string>? attributes = null)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Domain}/{_config.SearchBase}", _config.Username, _config.Password);
            
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = $"(&(objectClass=user)(objectCategory=person)(sAMAccountName={username}))"
            };
            
            // Always include distinguishedName as it's required for the response
            searcher.PropertiesToLoad.Add("distinguishedName");
            
            if (attributes != null && attributes.Any())
            {
                foreach (var attr in attributes)
                {
                    if (!attr.Equals("distinguishedName", StringComparison.OrdinalIgnoreCase))
                    {
                        searcher.PropertiesToLoad.Add(attr);
                    }
                }
            }
            else
            {
                searcher.PropertiesToLoad.Add("*");
            }

            var result = searcher.FindOne();
            if (result == null) return null;

            var userDto = new QueryableResponseDto
            {
                DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty,
                ObjectClass = "user"
            };

            foreach (string propertyName in result.Properties.PropertyNames)
            {
                if (result.Properties[propertyName].Count > 0)
                {
                    if (result.Properties[propertyName].Count == 1)
                    {
                        userDto.Attributes[propertyName] = result.Properties[propertyName][0];
                    }
                    else
                    {
                        userDto.Attributes[propertyName] = result.Properties[propertyName].Cast<object>().ToArray();
                    }
                }
            }

            return await Task.FromResult(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {Username} from Active Directory", username);
            throw;
        }
    }

    public async Task<List<QueryableResponseDto>> GetGroupsQueryableAsync(List<string>? attributes = null)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Domain}/{_config.SearchBase}", _config.Username, _config.Password);
            
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = "(objectClass=group)"
            };
            
            // Always include distinguishedName as it's required for the response
            searcher.PropertiesToLoad.Add("distinguishedName");
            
            if (attributes != null && attributes.Any())
            {
                foreach (var attr in attributes)
                {
                    if (!attr.Equals("distinguishedName", StringComparison.OrdinalIgnoreCase))
                    {
                        searcher.PropertiesToLoad.Add(attr);
                    }
                }
            }
            else
            {
                searcher.PropertiesToLoad.Add("*");
            }

            var results = searcher.FindAll();
            var groupDtos = new List<QueryableResponseDto>();

            foreach (SearchResult result in results)
            {
                var groupDto = new QueryableResponseDto
                {
                    DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty,
                    ObjectClass = "group"
                };

                foreach (string propertyName in result.Properties.PropertyNames)
                {
                    if (result.Properties[propertyName].Count > 0)
                    {
                        if (result.Properties[propertyName].Count == 1)
                        {
                            groupDto.Attributes[propertyName] = result.Properties[propertyName][0];
                        }
                        else
                        {
                            groupDto.Attributes[propertyName] = result.Properties[propertyName].Cast<object>().ToArray();
                        }
                    }
                }

                groupDtos.Add(groupDto);
            }

            return await Task.FromResult(groupDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups from Active Directory");
            throw;
        }
    }

    public async Task<QueryableResponseDto?> GetGroupQueryableAsync(string groupName, List<string>? attributes = null)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Domain}/{_config.SearchBase}", _config.Username, _config.Password);
            
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = $"(&(objectClass=group)(sAMAccountName={groupName}))"
            };
            
            // Always include distinguishedName as it's required for the response
            searcher.PropertiesToLoad.Add("distinguishedName");
            
            if (attributes != null && attributes.Any())
            {
                foreach (var attr in attributes)
                {
                    if (!attr.Equals("distinguishedName", StringComparison.OrdinalIgnoreCase))
                    {
                        searcher.PropertiesToLoad.Add(attr);
                    }
                }
            }
            else
            {
                searcher.PropertiesToLoad.Add("*");
            }

            var result = searcher.FindOne();
            if (result == null) return null;

            var groupDto = new QueryableResponseDto
            {
                DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty,
                ObjectClass = "group"
            };

            foreach (string propertyName in result.Properties.PropertyNames)
            {
                if (result.Properties[propertyName].Count > 0)
                {
                    if (result.Properties[propertyName].Count == 1)
                    {
                        groupDto.Attributes[propertyName] = result.Properties[propertyName][0];
                    }
                    else
                    {
                        groupDto.Attributes[propertyName] = result.Properties[propertyName].Cast<object>().ToArray();
                    }
                }
            }

            return await Task.FromResult(groupDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupName} from Active Directory", groupName);
            throw;
        }
    }

    public async Task<List<QueryableResponseDto>> GetOrganizationalUnitsQueryableAsync(List<string>? attributes = null)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Domain}/{_config.SearchBase}", _config.Username, _config.Password);
            
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = "(objectClass=organizationalUnit)"
            };
            
            // Always include distinguishedName as it's required for the response
            searcher.PropertiesToLoad.Add("distinguishedName");
            
            if (attributes != null && attributes.Any())
            {
                foreach (var attr in attributes)
                {
                    if (!attr.Equals("distinguishedName", StringComparison.OrdinalIgnoreCase))
                    {
                        searcher.PropertiesToLoad.Add(attr);
                    }
                }
            }
            else
            {
                searcher.PropertiesToLoad.Add("*");
            }

            var results = searcher.FindAll();
            var ouDtos = new List<QueryableResponseDto>();

            foreach (SearchResult result in results)
            {
                var ouDto = new QueryableResponseDto
                {
                    DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty,
                    ObjectClass = "organizationalUnit"
                };

                foreach (string propertyName in result.Properties.PropertyNames)
                {
                    if (result.Properties[propertyName].Count > 0)
                    {
                        if (result.Properties[propertyName].Count == 1)
                        {
                            ouDto.Attributes[propertyName] = result.Properties[propertyName][0];
                        }
                        else
                        {
                            ouDto.Attributes[propertyName] = result.Properties[propertyName].Cast<object>().ToArray();
                        }
                    }
                }

                ouDtos.Add(ouDto);
            }

            return await Task.FromResult(ouDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizational units from Active Directory");
            throw;
        }
    }

    public async Task<QueryableResponseDto?> GetOrganizationalUnitQueryableAsync(string ouName, List<string>? attributes = null)
    {
        try
        {
            using var context = CreatePrincipalContext();
            using var directoryEntry = new DirectoryEntry($"LDAP://{_config.Domain}/{_config.SearchBase}", _config.Username, _config.Password);
            
            var searcher = new DirectorySearcher(directoryEntry)
            {
                Filter = $"(&(objectClass=organizationalUnit)(name={ouName}))"
            };
            
            // Always include distinguishedName as it's required for the response
            searcher.PropertiesToLoad.Add("distinguishedName");
            
            if (attributes != null && attributes.Any())
            {
                foreach (var attr in attributes)
                {
                    if (!attr.Equals("distinguishedName", StringComparison.OrdinalIgnoreCase))
                    {
                        searcher.PropertiesToLoad.Add(attr);
                    }
                }
            }
            else
            {
                searcher.PropertiesToLoad.Add("*");
            }

            var result = searcher.FindOne();
            if (result == null) return null;

            var ouDto = new QueryableResponseDto
            {
                DistinguishedName = result.Properties["distinguishedName"][0]?.ToString() ?? string.Empty,
                ObjectClass = "organizationalUnit"
            };

            foreach (string propertyName in result.Properties.PropertyNames)
            {
                if (result.Properties[propertyName].Count > 0)
                {
                    if (result.Properties[propertyName].Count == 1)
                    {
                        ouDto.Attributes[propertyName] = result.Properties[propertyName][0];
                    }
                    else
                    {
                        ouDto.Attributes[propertyName] = result.Properties[propertyName].Cast<object>().ToArray();
                    }
                }
            }

            return await Task.FromResult(ouDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organizational unit {OuName} from Active Directory", ouName);
            throw;
        }
    }

    private PrincipalContext CreatePrincipalContext()
    {
        return new PrincipalContext(ContextType.Domain, _config.Server, _config.SearchBase, _config.Username, _config.Password);
    }
}
