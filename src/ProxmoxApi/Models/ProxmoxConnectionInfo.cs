namespace ProxmoxApi.Models;

/// <summary>
/// Connection information for Proxmox VE server
/// </summary>
public class ProxmoxConnectionInfo
{
    /// <summary>
    /// Proxmox server host (IP or FQDN)
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// Port number (default: 8006)
    /// </summary>
    public int Port { get; set; } = 8006;

    /// <summary>
    /// Username for authentication
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Password for authentication
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// API token for authentication (alternative to password)
    /// </summary>
    public string? ApiToken { get; set; }

    /// <summary>
    /// Authentication realm (default: pam)
    /// </summary>
    public string Realm { get; set; } = "pam";

    /// <summary>
    /// Whether to use HTTPS (default: true)
    /// </summary>
    public bool UseHttps { get; set; } = true;

    /// <summary>
    /// Whether to ignore SSL certificate errors (default: false)
    /// </summary>
    public bool IgnoreSslErrors { get; set; } = false;

    /// <summary>
    /// Connection timeout in seconds (default: 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the base URL for the Proxmox API
    /// </summary>
    public string BaseUrl => $"{(UseHttps ? "https" : "http")}://{Host}:{Port}/api2/json";
}
