namespace active_directory_rest_api.Models;

public class UserDto
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public bool LockedOut { get; set; }
    public DateTime? LastLogon { get; set; }
    public string DistinguishedName { get; set; } = string.Empty;
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string OrganizationalUnit { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? OrganizationalUnit { get; set; }
}

public class GroupDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
    public List<string> Members { get; set; } = new();
}

public class CreateGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OrganizationalUnit { get; set; } = string.Empty;
}

public class OrganizationalUnitDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
}

public class CreateOrganizationalUnitDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParentOu { get; set; } = string.Empty;
}

public class AuthenticationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class PasswordChangeRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class QueryableResponseDto
{
    public string DistinguishedName { get; set; } = string.Empty;
    public string ObjectClass { get; set; } = string.Empty;
    public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
}
