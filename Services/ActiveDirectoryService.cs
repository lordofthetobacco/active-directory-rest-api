using ActiveDirectory_API.Models;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;

namespace ActiveDirectory_API.Services;
[SupportedOSPlatform("windows")] 
public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly ActiveDirectoryConfiguration _config;
    private readonly ILogger<ActiveDirectoryService> _logger;

    public ActiveDirectoryService(ActiveDirectoryConfiguration config, ILogger<ActiveDirectoryService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<ActiveDirectoryUser?> GetUserAsync(string samAccountName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);
            
            if (userPrincipal == null) return null;
            
            return await Task.FromResult(MapToActiveDirectoryUser(userPrincipal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {SamAccountName}", samAccountName);
            return null;
        }
    }

    public async Task<ActiveDirectoryUser?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, email);
            
            if (userPrincipal == null) return null;
            
            return await Task.FromResult(MapToActiveDirectoryUser(userPrincipal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", email);
            return null;
        }
    }

    public async Task<ActiveDirectoryUser?> GetUserByDNAsync(string distinguishedName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.DistinguishedName, distinguishedName);
            
            if (userPrincipal == null) return null;
            
            return await Task.FromResult(MapToActiveDirectoryUser(userPrincipal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by DN {DN}", distinguishedName);
            return null;
        }
    }

    public async Task<IEnumerable<ActiveDirectoryUser>> SearchUsersAsync(ActiveDirectorySearchRequest request)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = new UserPrincipal(context);
            
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                userPrincipal.SamAccountName = $"*{request.SearchTerm}*";
            }
            
            var searcher = new PrincipalSearcher(userPrincipal);
            var results = searcher.FindAll().Cast<UserPrincipal>();
            
            if (request.MaxResults > 0)
                results = results.Take(request.MaxResults);
            
            var users = results.Select(MapToActiveDirectoryUser).ToList();
            return await Task.FromResult(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users");
            return Enumerable.Empty<ActiveDirectoryUser>();
        }
    }

    public async Task<ActiveDirectoryGroup?> GetGroupAsync(string groupName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var groupPrincipal = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
            
            if (groupPrincipal == null) return null;
            
            return await Task.FromResult(MapToActiveDirectoryGroup(groupPrincipal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group {GroupName}", groupName);
            return null;
        }
    }

    public async Task<IEnumerable<ActiveDirectoryGroup>> SearchGroupsAsync(ActiveDirectorySearchRequest request)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var groupPrincipal = new GroupPrincipal(context);
            
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                groupPrincipal.Name = $"*{request.SearchTerm}*";
            }
            
            var searcher = new PrincipalSearcher(groupPrincipal);
            var results = searcher.FindAll().Cast<GroupPrincipal>();
            
            if (request.MaxResults > 0)
                results = results.Take(request.MaxResults);
            
            var groups = results.Select(MapToActiveDirectoryGroup).ToList();
            return await Task.FromResult(groups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching groups");
            return Enumerable.Empty<ActiveDirectoryGroup>();
        }
    }

    public async Task<IEnumerable<ActiveDirectoryGroup>> SearchGroupsByNameAsync(string groupName, int maxResults = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                _logger.LogWarning("Empty or whitespace group name provided for search");
                return Enumerable.Empty<ActiveDirectoryGroup>();
            }

            using var context = CreatePrincipalContext();
            var groupPrincipal = new GroupPrincipal(context);
            
            var searchTerms = groupName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var searchFilters = new List<string>();
            
            foreach (var term in searchTerms)
            {
                if (term.Length >= 2)
                {
                    searchFilters.Add($"(|(name=*{term}*)(cn=*{term}*)(samAccountName=*{term}*)(description=*{term}*))");
                }
            }
            
            if (searchFilters.Count == 0)
            {
                _logger.LogWarning("No valid search terms found in '{GroupName}'", groupName);
                return Enumerable.Empty<ActiveDirectoryGroup>();
            }
            
            var combinedFilter = $"(&(objectClass=group)(objectCategory=group){string.Join("", searchFilters)})";
            _logger.LogInformation("Searching groups with filter: {Filter}", combinedFilter);
            
            var searcher = new PrincipalSearcher(groupPrincipal);
            var directoryEntry = searcher.GetUnderlyingSearcher() as DirectorySearcher;
            
            if (directoryEntry != null)
            {
                directoryEntry.Filter = combinedFilter;
                directoryEntry.PropertiesToLoad.AddRange(new[] { "name", "cn", "samAccountName", "description", "distinguishedName" });
                directoryEntry.SizeLimit = maxResults;
                directoryEntry.Sort = new SortOption("name", SortDirection.Ascending);
                
                var results = directoryEntry.FindAll();
                var groups = new List<ActiveDirectoryGroup>();
                
                foreach (SearchResult result in results)
                {
                    try
                    {
                        var group = new ActiveDirectoryGroup
                        {
                            Name = GetSearchResultPropertyValue(result, "name"),
                            DistinguishedName = GetSearchResultPropertyValue(result, "distinguishedName"),
                            SamAccountName = GetSearchResultPropertyValue(result, "samAccountName"),
                            Description = GetSearchResultPropertyValue(result, "description")
                        };
                        
                        if (!string.IsNullOrEmpty(group.Name))
                        {
                            groups.Add(group);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing search result for group");
                    }
                }
                
                _logger.LogInformation("Found {Count} groups matching '{GroupName}'", groups.Count, groupName);
                return await Task.FromResult(groups);
            }
            else
            {
                _logger.LogWarning("Failed to get DirectorySearcher, falling back to PrincipalSearcher");
                var principalResults = searcher.FindAll().Cast<GroupPrincipal>();
                var groups = principalResults
                    .Where(g => !string.IsNullOrEmpty(g.Name))
                    .Select(MapToActiveDirectoryGroup)
                    .Take(maxResults)
                    .ToList();
                
                _logger.LogInformation("Found {Count} groups using fallback method", groups.Count);
                return await Task.FromResult(groups);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching groups by name '{GroupName}'", groupName);
            return Enumerable.Empty<ActiveDirectoryGroup>();
        }
    }

    public async Task<bool> AuthenticateUserAsync(string username, string password)
    {
        try
        {
            using var context = CreatePrincipalContext();
            return await Task.FromResult(context.ValidateCredentials(username, password));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user {Username}", username);
            return false;
        }
    }

    public async Task<bool> IsUserInGroupAsync(string username, string groupName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            var groupPrincipal = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
            
            if (userPrincipal == null || groupPrincipal == null) return false;
            
            return await Task.FromResult(userPrincipal.IsMemberOf(groupPrincipal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {Username} is in group {GroupName}", username, groupName);
            return false;
        }
    }

    public async Task<bool> AddUserToGroupAsync(string username, string groupName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            var groupPrincipal = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
            
            if (userPrincipal == null)
            {
                _logger.LogError("User {Username} not found", username);
                return false;
            }
            
            if (groupPrincipal == null)
            {
                _logger.LogError("Group {GroupName} not found", groupName);
                return false;
            }
            
            if (userPrincipal.IsMemberOf(groupPrincipal))
            {
                _logger.LogInformation("User {Username} is already a member of group {GroupName}", username, groupName);
                return true;
            }
            
            groupPrincipal.Members.Add(userPrincipal);
            groupPrincipal.Save();
            
            _logger.LogInformation("Successfully added user {Username} to group {GroupName}", username, groupName);
            return await Task.FromResult(true);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when trying to add user {Username} to group {GroupName}. Check if the service account has sufficient permissions.", username, groupName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user {Username} to group {GroupName}", username, groupName);
            return false;
        }
    }

    public async Task<bool> RemoveUserFromGroupAsync(string username, string groupName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            var groupPrincipal = GroupPrincipal.FindByIdentity(context, IdentityType.Name, groupName);
            
            if (userPrincipal == null)
            {
                _logger.LogError("User {Username} not found", username);
                return false;
            }
            
            if (groupPrincipal == null)
            {
                _logger.LogError("Group {GroupName} not found", groupName);
                return false;
            }
            
            if (!userPrincipal.IsMemberOf(groupPrincipal))
            {
                _logger.LogInformation("User {Username} is not a member of group {GroupName}", username, groupName);
                return true;
            }
            
            groupPrincipal.Members.Remove(userPrincipal);
            groupPrincipal.Save();
            
            _logger.LogInformation("Successfully removed user {Username} from group {GroupName}", username, groupName);
            return await Task.FromResult(true);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when trying to remove user {Username} from group {GroupName}. Check if the service account has insufficient permissions.", username, groupName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {Username} from group {GroupName}", username, groupName);
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetUserExtensionAttributesAsync(string username, string[]? attributes = null)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null)
            {
                _logger.LogError("User {Username} not found", username);
                return new Dictionary<string, object>();
            }
            
            var directoryEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;
            if (directoryEntry == null)
            {
                _logger.LogError("Failed to get directory entry for user {Username}", username);
                return new Dictionary<string, object>();
            }
            
            var result = new Dictionary<string, object>();
            
            if (attributes == null || attributes.Length == 0)
            {
                foreach (string propertyName in directoryEntry.Properties.PropertyNames)
                {
                    var property = directoryEntry.Properties[propertyName];
                    if (property.Count > 0)
                    {
                        if (property.Count == 1)
                        {
                            result[propertyName] = property.Value!;
                        }
                        else
                        {
                            var values = new List<object>();
                            foreach (var value in property)
                            {
                                values.Add(value);
                            }
                            result[propertyName] = values;
                        }
                    }
                }
            }
            else
            {
                foreach (var attributeName in attributes)
                {
                    if (directoryEntry.Properties.Contains(attributeName))
                    {
                        var property = directoryEntry.Properties[attributeName];
                        if (property.Count > 0)
                        {
                            if (property.Count == 1)
                            {
                                result[attributeName] = property.Value!;
                            }
                            else
                            {
                                var values = new List<object>();
                                foreach (var value in property)
                                {
                                    values.Add(value);
                                }
                                result[attributeName] = values;
                            }
                        }
                    }
                }
            }
            
            _logger.LogInformation("Retrieved {Count} extension attributes for user {Username}", result.Count, username);
            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting extension attributes for user {Username}", username);
            return new Dictionary<string, object>();
        }
    }

    public async Task<IEnumerable<ActiveDirectoryUser>> SearchUsersByFullNameAsync(string fullName, int maxResults = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                _logger.LogWarning("Empty or whitespace full name provided for search");
                return Enumerable.Empty<ActiveDirectoryUser>();
            }

            using var context = CreatePrincipalContext();
            var userPrincipal = new UserPrincipal(context);
            
            var searchTerms = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var searchFilters = new List<string>();
            
            foreach (var term in searchTerms)
            {
                if (term.Length >= 2)
                {
                    searchFilters.Add($"(|(displayName=*{term}*)(givenName=*{term}*)(sn=*{term}*)(cn=*{term}*))");
                }
            }
            
            if (searchFilters.Count == 0)
            {
                _logger.LogWarning("No valid search terms found in '{FullName}'", fullName);
                return Enumerable.Empty<ActiveDirectoryUser>();
            }
            
            var combinedFilter = $"(&(objectClass=user)(objectCategory=person){string.Join("", searchFilters)})";
            _logger.LogInformation("Searching users with filter: {Filter}", combinedFilter);
            
            var searcher = new PrincipalSearcher(userPrincipal);
            var directoryEntry = searcher.GetUnderlyingSearcher() as DirectorySearcher;
            
            if (directoryEntry != null)
            {
                directoryEntry.Filter = combinedFilter;
                directoryEntry.PropertiesToLoad.AddRange(new[] { "samAccountName", "displayName", "givenName", "sn", "cn", "mail", "userPrincipalName", "enabled" });
                directoryEntry.SizeLimit = maxResults;
                directoryEntry.Sort = new SortOption("displayName", SortDirection.Ascending);
                
                var results = directoryEntry.FindAll();
                var users = new List<ActiveDirectoryUser>();
                
                foreach (SearchResult result in results)
                {
                    try
                    {
                        var user = new ActiveDirectoryUser
                        {
                            SamAccountName = GetSearchResultPropertyValue(result, "samAccountName"),
                            DisplayName = GetSearchResultPropertyValue(result, "displayName"),
                            GivenName = GetSearchResultPropertyValue(result, "givenName"),
                            Surname = GetSearchResultPropertyValue(result, "sn"),
                            Email = GetSearchResultPropertyValue(result, "mail"),
                            UserPrincipalName = GetSearchResultPropertyValue(result, "userPrincipalName"),
                            Enabled = GetSearchResultPropertyValue(result, "userAccountControl") != "514" && GetSearchResultPropertyValue(result, "userAccountControl") != "66050"
                        };
                        
                        if (!string.IsNullOrEmpty(user.DisplayName))
                        {
                            users.Add(user);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing search result for user");
                    }
                }
                
                _logger.LogInformation("Found {Count} users matching '{FullName}'", users.Count, fullName);
                return await Task.FromResult(users);
            }
            else
            {
                _logger.LogWarning("Failed to get DirectorySearcher, falling back to PrincipalSearcher");
                var principalResults = searcher.FindAll().Cast<UserPrincipal>();
                var users = principalResults
                    .Where(u => !string.IsNullOrEmpty(u.DisplayName))
                    .Select(MapToActiveDirectoryUser)
                    .Take(maxResults)
                    .ToList();
                
                _logger.LogInformation("Found {Count} users using fallback method", users.Count);
                return await Task.FromResult(users);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users by full name '{FullName}'", fullName);
            return Enumerable.Empty<ActiveDirectoryUser>();
        }
    }

    public async Task<IEnumerable<ActiveDirectoryUser>> SearchUsersByUPNAsync(string upn, int maxResults = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(upn))
            {
                _logger.LogWarning("Empty or whitespace UPN provided for search");
                return Enumerable.Empty<ActiveDirectoryUser>();
            }

            using var context = CreatePrincipalContext();
            var userPrincipal = new UserPrincipal(context);
            
            var searchTerms = upn.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var searchFilters = new List<string>();
            
            foreach (var term in searchTerms)
            {
                if (term.Length >= 2)
                {
                    searchFilters.Add($"(|(userPrincipalName=*{term}*)(mail=*{term}*)(samAccountName=*{term}*))");
                }
            }
            
            if (searchFilters.Count == 0)
            {
                _logger.LogWarning("No valid search terms found in '{UPN}'", upn);
                return Enumerable.Empty<ActiveDirectoryUser>();
            }
            
            var combinedFilter = $"(&(objectClass=user)(objectCategory=person){string.Join("", searchFilters)})";
            _logger.LogInformation("Searching users by UPN with filter: {Filter}", combinedFilter);
            
            var searcher = new PrincipalSearcher(userPrincipal);
            var directoryEntry = searcher.GetUnderlyingSearcher() as DirectorySearcher;
            
            if (directoryEntry != null)
            {
                directoryEntry.Filter = combinedFilter;
                directoryEntry.PropertiesToLoad.AddRange(new[] { "samAccountName", "displayName", "givenName", "sn", "cn", "mail", "userPrincipalName", "enabled" });
                directoryEntry.SizeLimit = maxResults;
                directoryEntry.Sort = new SortOption("displayName", SortDirection.Ascending);
                
                var results = directoryEntry.FindAll();
                var users = new List<ActiveDirectoryUser>();
                
                foreach (SearchResult result in results)
                {
                    try
                    {
                        var user = new ActiveDirectoryUser
                        {
                            SamAccountName = GetSearchResultPropertyValue(result, "samAccountName"),
                            DisplayName = GetSearchResultPropertyValue(result, "displayName"),
                            GivenName = GetSearchResultPropertyValue(result, "givenName"),
                            Surname = GetSearchResultPropertyValue(result, "sn"),
                            Email = GetSearchResultPropertyValue(result, "mail"),
                            UserPrincipalName = GetSearchResultPropertyValue(result, "userPrincipalName"),
                            Enabled = GetSearchResultPropertyValue(result, "userAccountControl") != "514" && GetSearchResultPropertyValue(result, "userAccountControl") != "66050"
                        };
                        
                        if (!string.IsNullOrEmpty(user.DisplayName))
                        {
                            users.Add(user);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing search result for user");
                    }
                }
                
                _logger.LogInformation("Found {Count} users matching UPN '{UPN}'", users.Count, upn);
                return await Task.FromResult(users);
            }
            else
            {
                _logger.LogWarning("Failed to get DirectorySearcher, falling back to PrincipalSearcher");
                var principalResults = searcher.FindAll().Cast<UserPrincipal>();
                var users = principalResults
                    .Where(u => !string.IsNullOrEmpty(u.DisplayName))
                    .Select(MapToActiveDirectoryUser)
                    .Take(maxResults)
                    .ToList();
                
                _logger.LogInformation("Found {Count} users using fallback method", users.Count);
                return await Task.FromResult(users);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users by UPN '{UPN}'", upn);
            return Enumerable.Empty<ActiveDirectoryUser>();
        }
    }

    public async Task<IEnumerable<string>> GetUserGroupsAsync(string username)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            
            if (userPrincipal == null) return Enumerable.Empty<string>();
            
            var memberOf = new List<string>();
            try
            {
                var directoryEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;
                if (directoryEntry?.Properties["memberOf"] != null)
                {
                    foreach (var groupDN in directoryEntry.Properties["memberOf"])
                    {
                        var groupName = ExtractGroupNameFromDN(groupDN.ToString()!);
                        if (!string.IsNullOrEmpty(groupName))
                        {
                            memberOf.Add(groupName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting groups for user {Username} using directory entry, falling back to principal method", username);
                try
                {
                    var groups = userPrincipal.GetGroups().Select(g => g.Name).ToList();
                    memberOf.AddRange(groups);
                }
                catch
                {
                    _logger.LogWarning("Both methods failed for getting groups for user {Username}", username);
                }
            }
            
            return await Task.FromResult(memberOf);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting groups for user {Username}", username);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<bool> CreateUserAsync(ActiveDirectoryUser user, string password)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = new UserPrincipal(context, user.SamAccountName, password, true);
            
            userPrincipal.DisplayName = user.DisplayName;
            userPrincipal.GivenName = user.GivenName;
            userPrincipal.Surname = user.Surname;
            userPrincipal.EmailAddress = user.Email;
            userPrincipal.UserPrincipalName = user.UserPrincipalName;
            userPrincipal.Enabled = user.Enabled;
            
            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {SamAccountName}", user.SamAccountName);
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(ActiveDirectoryUser user)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, user.SamAccountName);
            
            if (userPrincipal == null) return false;
            
            userPrincipal.DisplayName = user.DisplayName;
            userPrincipal.GivenName = user.GivenName;
            userPrincipal.Surname = user.Surname;
            userPrincipal.EmailAddress = user.Email;
            userPrincipal.UserPrincipalName = user.UserPrincipalName;
            userPrincipal.Enabled = user.Enabled;
            
            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {SamAccountName}", user.SamAccountName);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(string samAccountName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);
            
            if (userPrincipal == null) return false;
            
            userPrincipal.Delete();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {SamAccountName}", samAccountName);
            return false;
        }
    }

    public async Task<bool> EnableUserAsync(string samAccountName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);
            
            if (userPrincipal == null) return false;
            
            if (userPrincipal.Enabled == true)
            {
                _logger.LogInformation("User {SamAccountName} is already enabled", samAccountName);
                return true;
            }
            
            userPrincipal.Enabled = true;
            userPrincipal.Save();
            
            _logger.LogInformation("Successfully enabled user {SamAccountName}", samAccountName);
            return await Task.FromResult(true);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when trying to enable user {SamAccountName}. Check if the service account has sufficient permissions.", samAccountName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling user {SamAccountName}", samAccountName);
            return false;
        }
    }

    public async Task<bool> DisableUserAsync(string samAccountName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);
            
            if (userPrincipal == null) return false;
            
            if (userPrincipal.Enabled == false)
            {
                _logger.LogInformation("User {SamAccountName} is already disabled", samAccountName);
                return true;
            }
            
            userPrincipal.Enabled = false;
            userPrincipal.Save();
            
            _logger.LogInformation("Successfully disabled user {SamAccountName}", samAccountName);
            return await Task.FromResult(true);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied when trying to disable user {SamAccountName}. Check if the service account has sufficient permissions.", samAccountName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling user {SamAccountName}", samAccountName);
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string samAccountName, string newPassword)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);
            
            if (userPrincipal == null) return false;
            
            userPrincipal.SetPassword(newPassword);
            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {SamAccountName}", samAccountName);
            return false;
        }
    }

    public async Task<bool> UnlockUserAsync(string samAccountName)
    {
        try
        {
            using var context = CreatePrincipalContext();
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);
            
            if (userPrincipal == null) return false;
            
            userPrincipal.UnlockAccount();
            userPrincipal.Save();
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user {SamAccountName}", samAccountName);
            return false;
        }
    }

    public async Task<bool> ValidateConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Validating Active Directory connection to {Server}", _config.Server);
            
            using var context = CreatePrincipalContext();
            
            if (context.ConnectedServer == null)
            {
                _logger.LogError("Failed to connect to Active Directory server {Server}", _config.Server);
                return false;
            }
            
            _logger.LogInformation("Successfully connected to Active Directory server: {ConnectedServer}", context.ConnectedServer);
            
            var searchBase = context.Name;
            if (string.IsNullOrEmpty(searchBase))
            {
                _logger.LogError("Failed to retrieve search base from Active Directory connection");
                return false;
            }
            
            _logger.LogInformation("Active Directory search base: {SearchBase}", searchBase);
            
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Active Directory connection to {Server}", _config.Server);
            return false;
        }
    }

    private PrincipalContext CreatePrincipalContext()
    {
        if (_config.UseIntegratedSecurity)
        {
            return new PrincipalContext(ContextType.Domain, _config.Server, _config.SearchBase);
        }
        else
        {
            return new PrincipalContext(ContextType.Domain, _config.Server, _config.SearchBase, _config.BindDN, _config.BindPassword);
        }
    }

    private static ActiveDirectoryUser MapToActiveDirectoryUser(UserPrincipal userPrincipal)
    {
        var memberOf = new List<string>();
        try
        {
            var directoryEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;
            if (directoryEntry?.Properties["memberOf"] != null)
            {
                foreach (var groupDN in directoryEntry.Properties["memberOf"])
                {
                    var groupName = ExtractGroupNameFromDN(groupDN.ToString()!);
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        memberOf.Add(groupName);
                    }
                }
            }
        }
        catch
        {
            memberOf = new List<string>();
        }

        return new ActiveDirectoryUser
        {
            DistinguishedName = userPrincipal.DistinguishedName ?? string.Empty,
            SamAccountName = userPrincipal.SamAccountName ?? string.Empty,
            DisplayName = userPrincipal.DisplayName ?? string.Empty,
            GivenName = userPrincipal.GivenName ?? string.Empty,
            Surname = userPrincipal.Surname ?? string.Empty,
            Email = userPrincipal.EmailAddress ?? string.Empty,
            UserPrincipalName = userPrincipal.UserPrincipalName ?? string.Empty,
            Enabled = userPrincipal.Enabled ?? false,
            LastLogon = userPrincipal.LastLogon,
            PasswordLastSet = userPrincipal.LastPasswordSet,
            AccountExpires = userPrincipal.AccountExpirationDate,
            MemberOf = memberOf.ToArray(),
            Department = GetPropertyValue(userPrincipal, "department") ?? string.Empty,
            Title = GetPropertyValue(userPrincipal, "title") ?? string.Empty,
            Office = GetPropertyValue(userPrincipal, "physicalDeliveryOfficeName") ?? string.Empty,
            Phone = GetPropertyValue(userPrincipal, "telephoneNumber") ?? string.Empty,
            Mobile = GetPropertyValue(userPrincipal, "mobile") ?? string.Empty,
            Manager = GetPropertyValue(userPrincipal, "manager") ?? string.Empty
        };
    }

    private static ActiveDirectoryGroup MapToActiveDirectoryGroup(GroupPrincipal groupPrincipal)
    {
        var members = new List<string>();
        var memberOf = new List<string>();
        
        try
        {
            var directoryEntry = groupPrincipal.GetUnderlyingObject() as DirectoryEntry;
            
            if (directoryEntry?.Properties["member"] != null)
            {
                foreach (var memberDN in directoryEntry.Properties["member"])
                {
                    var memberName = ExtractGroupNameFromDN(memberDN.ToString()!);
                    if (!string.IsNullOrEmpty(memberName))
                    {
                        members.Add(memberName);
                    }
                }
            }
            
            if (directoryEntry?.Properties["memberOf"] != null)
            {
                foreach (var groupDN in directoryEntry.Properties["memberOf"])
                {
                    var groupName = ExtractGroupNameFromDN(groupDN.ToString()!);
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        memberOf.Add(groupName);
                    }
                }
            }
        }
        catch
        {
            members = new List<string>();
            memberOf = new List<string>();
        }

        return new ActiveDirectoryGroup
        {
            DistinguishedName = groupPrincipal.DistinguishedName ?? string.Empty,
            Name = groupPrincipal.Name ?? string.Empty,
            SamAccountName = groupPrincipal.SamAccountName ?? string.Empty,
            Description = groupPrincipal.Description ?? string.Empty,
            GroupType = GetPropertyValue(groupPrincipal, "groupType") ?? string.Empty,
            Scope = GetPropertyValue(groupPrincipal, "groupType") ?? string.Empty,
            Members = members.ToArray(),
            MemberOf = memberOf.ToArray(),
            Manager = GetPropertyValue(groupPrincipal, "managedBy") ?? string.Empty,
            WhenCreated = groupPrincipal.Context.ConnectedServer != null ? 
                DateTime.Parse(GetPropertyValue(groupPrincipal, "whenCreated") ?? DateTime.MinValue.ToString()) : null,
            WhenChanged = groupPrincipal.Context.ConnectedServer != null ? 
                DateTime.Parse(GetPropertyValue(groupPrincipal, "whenCreated") ?? DateTime.MinValue.ToString()) : null
        };
    }

    private static string? GetPropertyValue(Principal principal, string propertyName)
    {
        try
        {
            var directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
            return directoryEntry?.Properties[propertyName]?.Value?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractGroupNameFromDN(string distinguishedName)
    {
        try
        {
            if (string.IsNullOrEmpty(distinguishedName))
                return string.Empty;

            var parts = distinguishedName.Split(',');
            if (parts.Length > 0)
            {
                var cnPart = parts[0];
                if (cnPart.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
                {
                    return cnPart.Substring(3);
                }
            }
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetSearchResultPropertyValue(SearchResult searchResult, string propertyName)
    {
        try
        {
            if (searchResult.Properties.Contains(propertyName))
            {
                var property = searchResult.Properties[propertyName];
                if (property.Count > 0)
                {
                    return property[0]?.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
