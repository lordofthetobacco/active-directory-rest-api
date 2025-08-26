namespace ActiveDirectory_API.Models;

public class ActiveDirectorySearchRequest
{
    public string SearchTerm { get; set; } = string.Empty;
    public string SearchBase { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;
    public string[] Attributes { get; set; } = Array.Empty<string>();
    public int MaxResults { get; set; } = 100;
    public string SortBy { get; set; } = string.Empty;
    public bool SortAscending { get; set; } = true;
}
