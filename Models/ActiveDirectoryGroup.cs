namespace ActiveDirectory_API.Models;

public class ActiveDirectoryGroup
{
    public string DistinguishedName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SamAccountName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string GroupType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string[] Members { get; set; } = Array.Empty<string>();
    public string[] MemberOf { get; set; } = Array.Empty<string>();
    public string Manager { get; set; } = string.Empty;
    public DateTime? WhenCreated { get; set; }
    public DateTime? WhenChanged { get; set; }
}
