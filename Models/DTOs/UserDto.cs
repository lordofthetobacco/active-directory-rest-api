using System.ComponentModel.DataAnnotations;

namespace active_directory_rest_api.Models.DTOs
{
    public class UserDto
    {
        public string? DistinguishedName { get; set; }
        public string? SamAccountName { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? DisplayName { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? Email { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Office { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public bool? Enabled { get; set; }
        public DateTime? LastLogon { get; set; }
        public DateTime? PasswordLastSet { get; set; }
        public DateTime? AccountExpires { get; set; }
        public string? Manager { get; set; }
        public string[]? MemberOf { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        public string SamAccountName { get; set; } = string.Empty;
        
        [Required]
        public string DisplayName { get; set; } = string.Empty;
        
        [Required]
        public string GivenName { get; set; } = string.Empty;
        
        [Required]
        public string Surname { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string UserPrincipalName { get; set; } = string.Empty;
        
        public string? Department { get; set; }
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Office { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Manager { get; set; }
        public string? OrganizationalUnit { get; set; }
    }

    public class UpdateUserDto
    {
        public string? DisplayName { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Office { get; set; }
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Manager { get; set; }
    }

    public class AuthenticateUserDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        [Required]
        public string NewPassword { get; set; } = string.Empty;
        
        public bool ForceChangeAtNextLogon { get; set; } = true;
    }

    public class MoveUserDto
    {
        [Required]
        public string NewOrganizationalUnit { get; set; } = string.Empty;
    }
}
