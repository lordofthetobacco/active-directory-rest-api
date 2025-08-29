using System.ComponentModel.DataAnnotations;

namespace active_directory_rest_api.Models.DTOs
{
    public class GroupDto
    {
        public string? DistinguishedName { get; set; }
        public string? SamAccountName { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public string? GroupType { get; set; }
        public string? Scope { get; set; }
        public string? Category { get; set; }
        public string? Email { get; set; }
        public string? ManagedBy { get; set; }
        public string[]? Members { get; set; }
        public string[]? MemberOf { get; set; }
    }

    public class CreateGroupDto
    {
        [Required]
        public string SamAccountName { get; set; } = string.Empty;
        
        [Required]
        public string DisplayName { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        public string? GroupType { get; set; }
        public string? Scope { get; set; }
        public string? Category { get; set; }
        public string? Email { get; set; }
        public string? ManagedBy { get; set; }
        public string? OrganizationalUnit { get; set; }
    }

    public class UpdateGroupDto
    {
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public string? Email { get; set; }
        public string? ManagedBy { get; set; }
    }

    public class AddUserToGroupDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }

    public class RemoveUserFromGroupDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
    }
}
