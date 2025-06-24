using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi;
using ProxmoxApi.Models;

namespace ProxmoxApi.Examples;

/// <summary>
/// Example usage of the ProxmoxApi library
/// </summary>
public class BasicUsageExample
{
    public static async Task RunAsync()
    {
        // Configure logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        var logger = loggerFactory.CreateLogger<ProxmoxClient>();

        // Configure connection
        var connectionInfo = new ProxmoxConnectionInfo
        {
            Host = "your-proxmox-server.local",
            Username = "root",
            Password = "your-password", // Or use ApiToken instead
            IgnoreSslErrors = true // Only for testing/development
        };

        // Alternative: Using API token
        // var connectionInfo = new ProxmoxConnectionInfo
        // {
        //     Host = "your-proxmox-server.local",
        //     Username = "root",
        //     ApiToken = "root@pam!mytoken=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        //     IgnoreSslErrors = true
        // };

        try
        {
            using var client = new ProxmoxClient(connectionInfo, logger);

            // Test connection
            Console.WriteLine("Testing connection...");
            var isConnected = await client.TestConnectionAsync();

            if (!isConnected)
            {
                Console.WriteLine("Failed to connect to Proxmox server");
                return;
            }

            Console.WriteLine("Connection successful!");

            // Authenticate
            Console.WriteLine("Authenticating...");
            var isAuthenticated = await client.AuthenticateAsync();

            if (!isAuthenticated)
            {
                Console.WriteLine("Authentication failed");
                return;
            }

            Console.WriteLine("Authentication successful!");

            // Get version information
            Console.WriteLine("Getting server version...");
            var version = await client.GetVersionAsync();

            if (version != null)
            {
                Console.WriteLine("Proxmox VE Version Information:");
                foreach (var kvp in version)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}
