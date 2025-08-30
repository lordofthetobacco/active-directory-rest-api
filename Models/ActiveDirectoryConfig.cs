namespace active_directory_rest_api.Models;

public class ActiveDirectoryConfig
{
    public string Domain { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SearchBase { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
}

public class ApiKeyConfig
{
    public List<string> ValidKeys { get; set; } = new();
}

public class LoggingDbConfig
{
    public bool Enabled { get; set; }
    public string LogLevel { get; set; } = "Information";
}
