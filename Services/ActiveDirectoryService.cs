using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using active_directory_rest_api.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace active_directory_rest_api.Services
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ActiveDirectoryService> _logger;
        private readonly string _domain;
        private readonly string _container;
        private readonly string _username;
        private readonly string _password;

        public ActiveDirectoryService(IConfiguration configuration, ILogger<ActiveDirectoryService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _domain = _configuration["ActiveDirectory:Domain"] ?? throw new InvalidOperationException("Active Directory domain not configured");
            _container = _configuration["ActiveDirectory:Container"] ?? throw new InvalidOperationException("Active Directory container not configured");
            _username = _configuration["ActiveDirectory:Username"] ?? throw new InvalidOperationException("Active Directory username not configured");
            _password = _configuration["ActiveDirectory:Password"] ?? throw new InvalidOperationException("Active Directory password not configured");
        }

        private DirectoryEntry GetDirectoryEntry()
        {
            return new DirectoryEntry($"LDAP://{_container}", _username, _password);
        }

        private DirectorySearcher GetDirectorySearcher()
        {
            var entry = GetDirectoryEntry();
            return new DirectorySearcher(entry);
        }

        // User operations
        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            try
            {
                var users = new List<UserDto>();
                var searcher = GetDirectorySearcher();
                searcher.Filter = "(&(objectClass=user)(objectCategory=person))";
                searcher.PropertiesToLoad.AddRange(new[] { "samAccountName", "displayName", "userPrincipalName", "mail", "department", "title", "company", "physicalDeliveryOfficeName", "telephoneNumber", "mobile", "enabled", "lastLogon", "pwdLastSet", "accountExpires", "manager", "memberOf" });

                var results = searcher.FindAll();
                foreach (SearchResult result in results)
                {
                    users.Add(MapSearchResultToUserDto(result));
                }

                return await Task.FromResult(users);
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
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(|(samAccountName={username})(userPrincipalName={username})))";
                searcher.PropertiesToLoad.AddRange(new[] { "samAccountName", "displayName", "userPrincipalName", "mail", "department", "title", "company", "physicalDeliveryOfficeName", "telephoneNumber", "mobile", "enabled", "lastLogon", "pwdLastSet", "accountExpires", "manager", "memberOf" });

                var result = searcher.FindOne();
                return result != null ? await Task.FromResult(MapSearchResultToUserDto(result)) : null;
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
                var user = await GetUserAsync(username);
                return user != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {Username} exists", username);
                throw;
            }
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            try
            {
                var entry = GetDirectoryEntry();
                var newUser = entry.Children.Add($"CN={createUserDto.DisplayName}", "user");
                
                newUser.Properties["samAccountName"].Add(createUserDto.SamAccountName);
                newUser.Properties["userPrincipalName"].Add(createUserDto.UserPrincipalName);
                newUser.Properties["givenName"].Add(createUserDto.GivenName);
                newUser.Properties["sn"].Add(createUserDto.Surname);
                newUser.Properties["displayName"].Add(createUserDto.DisplayName);
                
                if (!string.IsNullOrEmpty(createUserDto.Department))
                    newUser.Properties["department"].Add(createUserDto.Department);
                if (!string.IsNullOrEmpty(createUserDto.Title))
                    newUser.Properties["title"].Add(createUserDto.Title);
                if (!string.IsNullOrEmpty(createUserDto.Company))
                    newUser.Properties["company"].Add(createUserDto.Company);
                if (!string.IsNullOrEmpty(createUserDto.Office))
                    newUser.Properties["physicalDeliveryOfficeName"].Add(createUserDto.Office);
                if (!string.IsNullOrEmpty(createUserDto.Phone))
                    newUser.Properties["telephoneNumber"].Add(createUserDto.Phone);
                if (!string.IsNullOrEmpty(createUserDto.Mobile))
                    newUser.Properties["mobile"].Add(createUserDto.Mobile);
                if (!string.IsNullOrEmpty(createUserDto.Manager))
                    newUser.Properties["manager"].Add(createUserDto.Manager);

                newUser.CommitChanges();

                // Set initial password
                newUser.Invoke("SetPassword", "TempPassword123!");
                newUser.Properties["pwdLastSet"].Value = -1; // Force password change at next logon
                newUser.CommitChanges();

                return await GetUserAsync(createUserDto.SamAccountName) ?? throw new InvalidOperationException("Failed to retrieve created user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", createUserDto.SamAccountName);
                throw;
            }
        }

        public async Task<UserDto> UpdateUserAsync(string username, UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await GetUserAsync(username);
                if (user == null)
                    throw new InvalidOperationException($"User {username} not found");

                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(samAccountName={username}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    throw new InvalidOperationException($"User {username} not found");

                var entry = result.GetDirectoryEntry();
                
                if (!string.IsNullOrEmpty(updateUserDto.DisplayName))
                    entry.Properties["displayName"].Value = updateUserDto.DisplayName;
                if (!string.IsNullOrEmpty(updateUserDto.GivenName))
                    entry.Properties["givenName"].Value = updateUserDto.GivenName;
                if (!string.IsNullOrEmpty(updateUserDto.Surname))
                    entry.Properties["sn"].Value = updateUserDto.Surname;
                if (!string.IsNullOrEmpty(updateUserDto.Department))
                    entry.Properties["department"].Value = updateUserDto.Department;
                if (!string.IsNullOrEmpty(updateUserDto.Title))
                    entry.Properties["title"].Value = updateUserDto.Title;
                if (!string.IsNullOrEmpty(updateUserDto.Company))
                    entry.Properties["company"].Value = updateUserDto.Company;
                if (!string.IsNullOrEmpty(updateUserDto.Office))
                    entry.Properties["physicalDeliveryOfficeName"].Value = updateUserDto.Office;
                if (!string.IsNullOrEmpty(updateUserDto.Phone))
                    entry.Properties["telephoneNumber"].Value = updateUserDto.Phone;
                if (!string.IsNullOrEmpty(updateUserDto.Mobile))
                    entry.Properties["mobile"].Value = updateUserDto.Mobile;
                if (!string.IsNullOrEmpty(updateUserDto.Manager))
                    entry.Properties["manager"].Value = updateUserDto.Manager;

                entry.CommitChanges();
                
                return await GetUserAsync(username) ?? throw new InvalidOperationException("Failed to retrieve updated user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Username}", username);
                throw;
            }
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, _domain);
                return await Task.FromResult(context.ValidateCredentials(username, password));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user {Username}", username);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(string username, string newPassword, bool forceChangeAtNextLogon = true)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(samAccountName={username}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.Invoke("SetPassword", newPassword);
                
                if (forceChangeAtNextLogon)
                {
                    entry.Properties["pwdLastSet"].Value = 0; // Force password change at next logon
                }
                else
                {
                    entry.Properties["pwdLastSet"].Value = -1; // Password never expires
                }
                
                entry.CommitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {Username}", username);
                throw;
            }
        }

        public async Task<bool> SetPasswordNeverExpiresAsync(string username)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(samAccountName={username}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.Properties["userAccountControl"].Value = 0x10000; // DONT_EXPIRE_PASSWORD
                entry.CommitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting password never expires for user {Username}", username);
                throw;
            }
        }

        public async Task<bool> SetPasswordExpiresAsync(string username)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(samAccountName={username}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.Properties["userAccountControl"].Value = 0x800000; // NORMAL_ACCOUNT
                entry.CommitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting password expires for user {Username}", username);
                throw;
            }
        }

        public async Task<bool> EnableUserAsync(string username)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(samAccountName={username}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.Properties["userAccountControl"].Value = 0x200; // NORMAL_ACCOUNT
                entry.CommitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling user {Username}", username);
                throw;
            }
        }

        public async Task<bool> DisableUserAsync(string username)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(samAccountName={username}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.Properties["userAccountControl"].Value = 0x2; // ACCOUNTDISABLE
                entry.CommitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling user {Username}", username);
                throw;
            }
        }

        public async Task<bool> MoveUserAsync(string username, string newOrganizationalUnit)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(samAccountName={username}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                var newOu = GetDirectoryEntry();
                newOu.Path = $"LDAP://{newOrganizationalUnit}";
                
                entry.MoveTo(newOu);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving user {Username} to {NewOU}", username, newOrganizationalUnit);
                throw;
            }
        }

        public async Task<bool> UnlockUserAsync(string username)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(samAccountName={username}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.Properties["lockoutTime"].Value = 0;
                entry.CommitChanges();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user {Username}", username);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=user)(objectCategory=person)(samAccountName={username}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.DeleteTree();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {Username}", username);
                throw;
            }
        }

        public async Task<bool> IsUserMemberOfGroupAsync(string username, string groupName)
        {
            try
            {
                var user = await GetUserAsync(username);
                if (user?.MemberOf == null)
                    return false;

                return user.MemberOf.Any(g => g.Contains(groupName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {Username} is member of group {GroupName}", username, groupName);
                throw;
            }
        }

        // Group operations
        public async Task<IEnumerable<GroupDto>> GetGroupsAsync()
        {
            try
            {
                var groups = new List<GroupDto>();
                var searcher = GetDirectorySearcher();
                searcher.Filter = "(&(objectClass=group)(objectCategory=group))";
                searcher.PropertiesToLoad.AddRange(new[] { "samAccountName", "displayName", "description", "groupType", "scope", "category", "mail", "managedBy", "member", "memberOf" });

                var results = searcher.FindAll();
                foreach (SearchResult result in results)
                {
                    groups.Add(MapSearchResultToGroupDto(result));
                }

                return await Task.FromResult(groups);
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
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=group)(objectCategory=group)(samAccountName={groupName}))";
                searcher.PropertiesToLoad.AddRange(new[] { "samAccountName", "displayName", "description", "groupType", "scope", "category", "mail", "managedBy", "member", "memberOf" });

                var result = searcher.FindOne();
                return result != null ? await Task.FromResult(MapSearchResultToGroupDto(result)) : null;
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
                var group = await GetGroupAsync(groupName);
                return group != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if group {GroupName} exists", groupName);
                throw;
            }
        }

        public async Task<GroupDto> CreateGroupAsync(CreateGroupDto createGroupDto)
        {
            try
            {
                var entry = GetDirectoryEntry();
                var newGroup = entry.Children.Add($"CN={createGroupDto.DisplayName}", "group");
                
                newGroup.Properties["samAccountName"].Add(createGroupDto.SamAccountName);
                newGroup.Properties["displayName"].Add(createGroupDto.DisplayName);
                
                if (!string.IsNullOrEmpty(createGroupDto.Description))
                    newGroup.Properties["description"].Add(createGroupDto.Description);
                if (!string.IsNullOrEmpty(createGroupDto.GroupType))
                    newGroup.Properties["groupType"].Add(createGroupDto.GroupType);
                if (!string.IsNullOrEmpty(createGroupDto.Scope))
                    newGroup.Properties["scope"].Add(createGroupDto.Scope);
                if (!string.IsNullOrEmpty(createGroupDto.Category))
                    newGroup.Properties["category"].Add(createGroupDto.Category);
                if (!string.IsNullOrEmpty(createGroupDto.Email))
                    newGroup.Properties["mail"].Add(createGroupDto.Email);
                if (!string.IsNullOrEmpty(createGroupDto.ManagedBy))
                    newGroup.Properties["managedBy"].Add(createGroupDto.ManagedBy);

                newGroup.CommitChanges();
                
                return await GetGroupAsync(createGroupDto.SamAccountName) ?? throw new InvalidOperationException("Failed to retrieve created group");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group {GroupName}", createGroupDto.SamAccountName);
                throw;
            }
        }

        public async Task<GroupDto> UpdateGroupAsync(string groupName, UpdateGroupDto updateGroupDto)
        {
            try
            {
                var group = await GetGroupAsync(groupName);
                if (group == null)
                    throw new InvalidOperationException($"Group {groupName} not found");

                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=group)(objectCategory=group)(samAccountName={groupName}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    throw new InvalidOperationException($"Group {groupName} not found");

                var entry = result.GetDirectoryEntry();
                
                if (!string.IsNullOrEmpty(updateGroupDto.DisplayName))
                    entry.Properties["displayName"].Value = updateGroupDto.DisplayName;
                if (!string.IsNullOrEmpty(updateGroupDto.Description))
                    entry.Properties["description"].Value = updateGroupDto.Description;
                if (!string.IsNullOrEmpty(updateGroupDto.Email))
                    entry.Properties["mail"].Value = updateGroupDto.Email;
                if (!string.IsNullOrEmpty(updateGroupDto.ManagedBy))
                    entry.Properties["managedBy"].Value = updateGroupDto.ManagedBy;

                entry.CommitChanges();
                
                return await GetGroupAsync(groupName) ?? throw new InvalidOperationException("Failed to retrieve updated group");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating group {GroupName}", groupName);
                throw;
            }
        }

        public async Task<bool> AddUserToGroupAsync(string groupName, string username)
        {
            try
            {
                var group = await GetGroupAsync(groupName);
                if (group == null)
                    return false;

                var user = await GetUserAsync(username);
                if (user == null)
                    return false;

                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=group)(objectCategory=group)(samAccountName={groupName}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.Properties["member"].Add($"CN={user.DisplayName},{_container}");
                entry.CommitChanges();
                
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
                var group = await GetGroupAsync(groupName);
                if (group == null)
                    return false;

                var user = await GetUserAsync(username);
                if (user == null)
                    return false;

                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=group)(objectCategory=group)(samAccountName={groupName}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.Properties["member"].Remove($"CN={user.DisplayName},{_container}");
                entry.CommitChanges();
                
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {Username} from group {GroupName}", username, groupName);
                throw;
            }
        }

        public async Task<bool> DeleteGroupAsync(string groupName)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(&(objectClass=group)(objectCategory=group)(samAccountName={groupName}))";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.DeleteTree();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group {GroupName}", groupName);
                throw;
            }
        }

        // Organizational Unit operations
        public async Task<IEnumerable<OrganizationalUnitDto>> GetOrganizationalUnitsAsync()
        {
            try
            {
                var ous = new List<OrganizationalUnitDto>();
                var searcher = GetDirectorySearcher();
                searcher.Filter = "(objectClass=organizationalUnit)";
                searcher.PropertiesToLoad.AddRange(new[] { "name", "displayName", "description", "managedBy", "street", "l", "st", "postalCode", "c", "telephoneNumber", "facsimileTelephoneNumber", "wWWHomePage", "mail" });

                var results = searcher.FindAll();
                foreach (SearchResult result in results)
                {
                    ous.Add(MapSearchResultToOrganizationalUnitDto(result));
                }

                return await Task.FromResult(ous);
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
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(objectClass=organizationalUnit)(name={ouName})";
                searcher.PropertiesToLoad.AddRange(new[] { "name", "displayName", "description", "managedBy", "street", "l", "st", "postalCode", "c", "telephoneNumber", "facsimileTelephoneNumber", "wWWHomePage", "mail" });

                var result = searcher.FindOne();
                return result != null ? await Task.FromResult(MapSearchResultToOrganizationalUnitDto(result)) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizational unit {OUName} from Active Directory", ouName);
                throw;
            }
        }

        public async Task<bool> OrganizationalUnitExistsAsync(string ouName)
        {
            try
            {
                var ou = await GetOrganizationalUnitAsync(ouName);
                return ou != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if organizational unit {OUName} exists", ouName);
                throw;
            }
        }

        public async Task<OrganizationalUnitDto> CreateOrganizationalUnitAsync(CreateOrganizationalUnitDto createOuDto)
        {
            try
            {
                var entry = GetDirectoryEntry();
                var newOu = entry.Children.Add($"OU={createOuDto.Name}", "organizationalUnit");
                
                if (!string.IsNullOrEmpty(createOuDto.DisplayName))
                    newOu.Properties["displayName"].Add(createOuDto.DisplayName);
                if (!string.IsNullOrEmpty(createOuDto.Description))
                    newOu.Properties["description"].Add(createOuDto.Description);
                if (!string.IsNullOrEmpty(createOuDto.ManagedBy))
                    newOu.Properties["managedBy"].Add(createOuDto.ManagedBy);
                if (!string.IsNullOrEmpty(createOuDto.Street))
                    newOu.Properties["street"].Add(createOuDto.Street);
                if (!string.IsNullOrEmpty(createOuDto.City))
                    newOu.Properties["l"].Add(createOuDto.City);
                if (!string.IsNullOrEmpty(createOuDto.State))
                    newOu.Properties["st"].Add(createOuDto.State);
                if (!string.IsNullOrEmpty(createOuDto.PostalCode))
                    newOu.Properties["postalCode"].Add(createOuDto.PostalCode);
                if (!string.IsNullOrEmpty(createOuDto.Country))
                    newOu.Properties["c"].Add(createOuDto.Country);
                if (!string.IsNullOrEmpty(createOuDto.Telephone))
                    newOu.Properties["telephoneNumber"].Add(createOuDto.Telephone);
                if (!string.IsNullOrEmpty(createOuDto.Fax))
                    newOu.Properties["facsimileTelephoneNumber"].Add(createOuDto.Fax);
                if (!string.IsNullOrEmpty(createOuDto.Website))
                    newOu.Properties["wWWHomePage"].Add(createOuDto.Website);
                if (!string.IsNullOrEmpty(createOuDto.Email))
                    newOu.Properties["mail"].Add(createOuDto.Email);

                newOu.CommitChanges();
                
                return await GetOrganizationalUnitAsync(createOuDto.Name) ?? throw new InvalidOperationException("Failed to retrieve created organizational unit");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organizational unit {OUName}", createOuDto.Name);
                throw;
            }
        }

        public async Task<OrganizationalUnitDto> UpdateOrganizationalUnitAsync(string ouName, UpdateOrganizationalUnitDto updateOuDto)
        {
            try
            {
                var ou = await GetOrganizationalUnitAsync(ouName);
                if (ou == null)
                    throw new InvalidOperationException($"Organizational unit {ouName} not found");

                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(objectClass=organizationalUnit)(name={ouName})";
                var result = searcher.FindOne();
                
                if (result == null)
                    throw new InvalidOperationException($"Organizational unit {ouName} not found");

                var entry = result.GetDirectoryEntry();
                
                if (!string.IsNullOrEmpty(updateOuDto.DisplayName))
                    entry.Properties["displayName"].Value = updateOuDto.DisplayName;
                if (!string.IsNullOrEmpty(updateOuDto.Description))
                    entry.Properties["description"].Value = updateOuDto.Description;
                if (!string.IsNullOrEmpty(updateOuDto.ManagedBy))
                    entry.Properties["managedBy"].Value = updateOuDto.ManagedBy;
                if (!string.IsNullOrEmpty(updateOuDto.Street))
                    entry.Properties["street"].Value = updateOuDto.Street;
                if (!string.IsNullOrEmpty(updateOuDto.City))
                    entry.Properties["l"].Value = updateOuDto.City;
                if (!string.IsNullOrEmpty(updateOuDto.State))
                    entry.Properties["st"].Value = updateOuDto.State;
                if (!string.IsNullOrEmpty(updateOuDto.PostalCode))
                    entry.Properties["postalCode"].Value = updateOuDto.PostalCode;
                if (!string.IsNullOrEmpty(updateOuDto.Country))
                    entry.Properties["c"].Value = updateOuDto.Country;
                if (!string.IsNullOrEmpty(updateOuDto.Telephone))
                    entry.Properties["telephoneNumber"].Value = updateOuDto.Telephone;
                if (!string.IsNullOrEmpty(updateOuDto.Fax))
                    entry.Properties["facsimileTelephoneNumber"].Value = updateOuDto.Fax;
                if (!string.IsNullOrEmpty(updateOuDto.Website))
                    entry.Properties["wWWHomePage"].Value = updateOuDto.Website;
                if (!string.IsNullOrEmpty(updateOuDto.Email))
                    entry.Properties["mail"].Value = updateOuDto.Email;

                entry.CommitChanges();
                
                return await GetOrganizationalUnitAsync(ouName) ?? throw new InvalidOperationException("Failed to retrieve updated organizational unit");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organizational unit {OUName}", ouName);
                throw;
            }
        }

        public async Task<bool> DeleteOrganizationalUnitAsync(string ouName)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = $"(objectClass=organizationalUnit)(name={ouName})";
                var result = searcher.FindOne();
                
                if (result == null)
                    return false;

                var entry = result.GetDirectoryEntry();
                entry.DeleteTree();
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organizational unit {OUName}", ouName);
                throw;
            }
        }

        // Other operations
        public async Task<object> GetOtherAsync()
        {
            try
            {
                var result = new
                {
                    timestamp = DateTime.UtcNow,
                    message = "Other Active Directory information",
                    domain = _domain,
                    container = _container
                };
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting other information");
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

                var result = new
                {
                    timestamp = DateTime.UtcNow,
                    users = users.Count(),
                    groups = groups.Count(),
                    organizationalUnits = ous.Count(),
                    domain = _domain,
                    container = _container
                };
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all information");
                throw;
            }
        }

        public async Task<object> FindAsync(string filter)
        {
            try
            {
                var searcher = GetDirectorySearcher();
                searcher.Filter = filter;
                searcher.PropertiesToLoad.AddRange(new[] { "objectClass", "samAccountName", "displayName", "userPrincipalName", "mail", "description" });

                var results = searcher.FindAll();
                var items = new List<object>();

                foreach (SearchResult result in results)
                {
                    var item = new
                    {
                        objectClass = GetPropertyValue(result, "objectClass"),
                        samAccountName = GetPropertyValue(result, "samAccountName"),
                        displayName = GetPropertyValue(result, "displayName"),
                        userPrincipalName = GetPropertyValue(result, "userPrincipalName"),
                        mail = GetPropertyValue(result, "mail"),
                        description = GetPropertyValue(result, "description"),
                        distinguishedName = result.Path
                    };
                    items.Add(item);
                }

                var result = new
                {
                    timestamp = DateTime.UtcNow,
                    filter = filter,
                    count = items.Count,
                    items = items
                };
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding items with filter {Filter}", filter);
                throw;
            }
        }

        public async Task<object> GetStatusAsync()
        {
            try
            {
                var entry = GetDirectoryEntry();
                var result = new
                {
                    timestamp = DateTime.UtcNow,
                    status = "Connected",
                    domain = _domain,
                    container = _container,
                    connectionString = $"{_domain}:{_container}",
                    isConnected = true
                };
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status");
                var result = new
                {
                    timestamp = DateTime.UtcNow,
                    status = "Disconnected",
                    domain = _domain,
                    container = _container,
                    error = ex.Message,
                    isConnected = false
                };
                return await Task.FromResult(result);
            }
        }

        // Helper methods
        private UserDto MapSearchResultToUserDto(SearchResult result)
        {
            return new UserDto
            {
                DistinguishedName = result.Path,
                SamAccountName = GetPropertyValue(result, "samAccountName"),
                UserPrincipalName = GetPropertyValue(result, "userPrincipalName"),
                DisplayName = GetPropertyValue(result, "displayName"),
                GivenName = GetPropertyValue(result, "givenName"),
                Surname = GetPropertyValue(result, "sn"),
                Email = GetPropertyValue(result, "mail"),
                Department = GetPropertyValue(result, "department"),
                Title = GetPropertyValue(result, "title"),
                Company = GetPropertyValue(result, "company"),
                Office = GetPropertyValue(result, "physicalDeliveryOfficeName"),
                Phone = GetPropertyValue(result, "telephoneNumber"),
                Mobile = GetPropertyValue(result, "mobile"),
                Enabled = GetPropertyValue(result, "userAccountControl") != "0x2",
                LastLogon = GetDateTimePropertyValue(result, "lastLogon"),
                PasswordLastSet = GetDateTimePropertyValue(result, "pwdLastSet"),
                AccountExpires = GetDateTimePropertyValue(result, "accountExpires"),
                Manager = GetPropertyValue(result, "manager"),
                MemberOf = GetPropertyValues(result, "memberOf")
            };
        }

        private GroupDto MapSearchResultToGroupDto(SearchResult result)
        {
            return new GroupDto
            {
                DistinguishedName = result.Path,
                SamAccountName = GetPropertyValue(result, "samAccountName"),
                DisplayName = GetPropertyValue(result, "displayName"),
                Description = GetPropertyValue(result, "description"),
                GroupType = GetPropertyValue(result, "groupType"),
                Scope = GetPropertyValue(result, "scope"),
                Category = GetPropertyValue(result, "category"),
                Email = GetPropertyValue(result, "mail"),
                ManagedBy = GetPropertyValue(result, "managedBy"),
                Members = GetPropertyValues(result, "member"),
                MemberOf = GetPropertyValues(result, "memberOf")
            };
        }

        private OrganizationalUnitDto MapSearchResultToOrganizationalUnitDto(SearchResult result)
        {
            return new OrganizationalUnitDto
            {
                DistinguishedName = result.Path,
                Name = GetPropertyValue(result, "name"),
                DisplayName = GetPropertyValue(result, "displayName"),
                Description = GetPropertyValue(result, "description"),
                ManagedBy = GetPropertyValue(result, "managedBy"),
                Street = GetPropertyValue(result, "street"),
                City = GetPropertyValue(result, "l"),
                State = GetPropertyValue(result, "st"),
                PostalCode = GetPropertyValue(result, "postalCode"),
                Country = GetPropertyValue(result, "c"),
                Telephone = GetPropertyValue(result, "telephoneNumber"),
                Fax = GetPropertyValue(result, "facsimileTelephoneNumber"),
                Website = GetPropertyValue(result, "wWWHomePage"),
                Email = GetPropertyValue(result, "mail")
            };
        }

        private string? GetPropertyValue(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) && result.Properties[propertyName].Count > 0)
            {
                return result.Properties[propertyName][0]?.ToString();
            }
            return null;
        }

        private string[] GetPropertyValues(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) && result.Properties[propertyName].Count > 0)
            {
                var values = new string[result.Properties[propertyName].Count];
                for (int i = 0; i < result.Properties[propertyName].Count; i++)
                {
                    values[i] = result.Properties[propertyName][i]?.ToString() ?? string.Empty;
                }
                return values;
            }
            return Array.Empty<string>();
        }

        private DateTime? GetDateTimePropertyValue(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName) && result.Properties[propertyName].Count > 0)
            {
                var value = result.Properties[propertyName][0];
                if (value is long longValue && longValue > 0)
                {
                    return DateTime.FromFileTime(longValue);
                }
            }
            return null;
        }
    }
}
