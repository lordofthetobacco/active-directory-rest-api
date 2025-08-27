namespace ActiveDirectory_API.Models;

public class EntraIdConfiguration
{
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string Domain { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackPath { get; set; } = "/signin-oidc";
    public string SignedOutCallbackPath { get; set; } = "/signout-oidc";
    public string[] Scopes { get; set; } = { "openid", "profile", "email" };
    public string Authority => $"{Instance}{TenantId}";
}
