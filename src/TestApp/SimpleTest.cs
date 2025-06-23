using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi;
using ProxmoxApi.Models;
using ProxmoxApi.Exceptions;

class SimpleTest
{
    public static async Task TestWithConfig()
    {
        Console.WriteLine("=================================");
        Console.WriteLine("Proxmox API - Configuration Test");
        Console.WriteLine("=================================");

        try
        {
            // Load configuration
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"❌ Configuration file not found: {configPath}");
                Console.WriteLine("Please create appsettings.json with your Proxmox server details.");
                return;
            }

            var configJson = await File.ReadAllTextAsync(configPath);
            var config = JsonSerializer.Deserialize<ConfigRoot>(configJson, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (config?.ProxmoxConnection == null)
            {
                Console.WriteLine("❌ Invalid configuration structure");
                return;
            }

            var connectionInfo = config.ProxmoxConnection;
            
            // Validate required fields
            if (string.IsNullOrEmpty(connectionInfo.Host) || string.IsNullOrEmpty(connectionInfo.Username))
            {
                Console.WriteLine("❌ Host and Username are required in configuration");
                return;
            }

            if (string.IsNullOrEmpty(connectionInfo.Password) && string.IsNullOrEmpty(connectionInfo.ApiToken))
            {
                Console.WriteLine("❌ Either Password or ApiToken must be provided in configuration");
                return;
            }

            Console.WriteLine($"🌐 Connecting to: {connectionInfo.BaseUrl}");
            Console.WriteLine($"👤 Username: {connectionInfo.Username}@{connectionInfo.Realm}");
            Console.WriteLine($"🔐 Auth Method: {(string.IsNullOrEmpty(connectionInfo.ApiToken) ? "Password" : "API Token")}");
            Console.WriteLine($"🔒 SSL Validation: {(connectionInfo.IgnoreSslErrors ? "Disabled" : "Enabled")}");
            Console.WriteLine();

            // Configure logging
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            
            var logger = loggerFactory.CreateLogger<ProxmoxClient>();

            using var client = new ProxmoxClient(connectionInfo, logger);
            
            // Test connection
            Console.WriteLine("📡 Testing connection...");
            var isConnected = await client.TestConnectionAsync();
            
            if (!isConnected)
            {
                Console.WriteLine("❌ Connection failed!");
                return;
            }
            Console.WriteLine("✅ Connection successful!");

            // Test authentication
            Console.WriteLine("\n🔐 Testing authentication...");
            var isAuthenticated = await client.AuthenticateAsync();
            
            if (!isAuthenticated)
            {
                Console.WriteLine("❌ Authentication failed!");
                return;
            }
            Console.WriteLine("✅ Authentication successful!");

            // Get version info
            Console.WriteLine("\n📋 Getting server information...");
            var version = await client.GetVersionAsync();
            
            Console.WriteLine("✅ Server Information:");
            foreach (var kvp in version)
            {
                Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
            }

            Console.WriteLine("\n🎉 All tests passed! The library is working correctly.");

            // Test node management features
            Console.WriteLine("\n🔧 Testing Node Management Features...");
            try
            {
                var nodes = await client.Nodes.GetNodesAsync();
                Console.WriteLine($"✅ Found {nodes.Count} nodes in the cluster");
                
                foreach (var node in nodes.Take(3)) // Limit to first 3 nodes
                {
                    var status = node.IsOnline ? "🟢 Online" : "🔴 Offline";
                    Console.WriteLine($"   {status} {node.Node} ({node.Type})");
                    
                    if (node.IsOnline)
                    {
                        var nodeStatus = await client.Nodes.GetNodeStatusAsync(node.Node);
                        Console.WriteLine($"      Uptime: {nodeStatus.UptimeSpan.Days}d {nodeStatus.UptimeSpan.Hours}h");
                        
                        if (nodeStatus.Memory != null)
                        {
                            var memGB = nodeStatus.Memory.Total / (1024.0 * 1024.0 * 1024.0);
                            Console.WriteLine($"      Memory: {memGB:F1} GB total");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Node management test failed: {ex.Message}");
            }
        }
        catch (ProxmoxAuthenticationException ex)
        {
            Console.WriteLine($"❌ Authentication failed: {ex.Message}");
        }
        catch (ProxmoxApiException ex)
        {
            Console.WriteLine($"❌ API Error: {ex.Message}");
            if (ex.StatusCode.HasValue)
                Console.WriteLine($"   Status Code: {ex.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}

public class ConfigRoot
{
    public ProxmoxConnectionInfo? ProxmoxConnection { get; set; }
}
