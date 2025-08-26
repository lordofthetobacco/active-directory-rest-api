namespace ActiveDirectory_API.Models;

public class ActiveDirectoryConfiguration
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 389;
    public bool UseSSL { get; set; } = false;
    public string BindDN { get; set; } = string.Empty;
    public string BindPassword { get; set; } = string.Empty;
    public string SearchBase { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30;
    public bool UseIntegratedSecurity { get; set; } = false;
}
