namespace ActiveDirectory_API.Models;

public class ActiveDirectoryUser
{
    public string DistinguishedName { get; set; } = string.Empty;
    public string SamAccountName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserPrincipalName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public DateTime? LastLogon { get; set; }
    public DateTime? PasswordLastSet { get; set; }
    public DateTime? AccountExpires { get; set; }
    public string[] MemberOf { get; set; } = Array.Empty<string>();
    public string Department { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
}
