using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi;
using ProxmoxApi.Models;
using ProxmoxApi.Exceptions;

class Program
{    static async Task Main(string[] args)
    {
        Console.WriteLine("=================================");
        Console.WriteLine("Proxmox API Library - Test Suite");
        Console.WriteLine("=================================");
        Console.WriteLine();
        Console.WriteLine("Choose test method:");
        Console.WriteLine("1. Interactive input (enter details manually)");
        Console.WriteLine("2. Configuration file (use appsettings.json)");
        Console.WriteLine();
        Console.Write("Enter choice (1 or 2): ");
        
        var choice = Console.ReadLine()?.Trim();
        
        if (choice == "2")
        {
            await SimpleTest.TestWithConfig();
        }
        else
        {
            await RunInteractiveTest();        }
    }    private static async Task RunInteractiveTest()
    {
        Console.WriteLine("=================================");
        Console.WriteLine("Proxmox API Library - Real Server Test");  
        Console.WriteLine("=================================");

        // Get connection details from user
        var connectionInfo = GetConnectionInfo();
        
        if (connectionInfo == null)
        {
            Console.WriteLine("Invalid connection information provided.");
            return;
        }

        // Configure logging
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        
        var logger = loggerFactory.CreateLogger<ProxmoxClient>();

        try
        {
            using var client = new ProxmoxClient(connectionInfo, logger);
            
            Console.WriteLine($"\nConnecting to: {connectionInfo.BaseUrl}");
            Console.WriteLine($"Username: {connectionInfo.Username}@{connectionInfo.Realm}");
            Console.WriteLine($"Auth Method: {(string.IsNullOrEmpty(connectionInfo.ApiToken) ? "Password" : "API Token")}");
            Console.WriteLine();

            // Test 1: Connection Test
            Console.WriteLine("📡 Testing connection...");
            var isConnected = await client.TestConnectionAsync();
            
            if (!isConnected)
            {
                Console.WriteLine("❌ Connection test failed!");
                Console.WriteLine("Please check:");
                Console.WriteLine("- Server hostname/IP is correct");
                Console.WriteLine("- Port 8006 is accessible");
                Console.WriteLine("- SSL certificate settings");
                return;
            }
            
            Console.WriteLine("✅ Connection successful!");

            // Test 2: Authentication
            Console.WriteLine("\n🔐 Testing authentication...");
            var isAuthenticated = await client.AuthenticateAsync();
            
            if (!isAuthenticated)
            {
                Console.WriteLine("❌ Authentication failed!");
                return;
            }
            
            Console.WriteLine("✅ Authentication successful!");

            // Test 3: Get Version Information
            Console.WriteLine("\n📋 Getting server information...");
            var version = await client.GetVersionAsync();
            
            if (version != null && version.Count > 0)
            {
                Console.WriteLine("✅ Server Information:");
                foreach (var kvp in version)
                {
                    Console.WriteLine($"   {kvp.Key}: {kvp.Value}");
                }
            }
            else
            {
                Console.WriteLine("⚠️  Could not retrieve version information");
            }

            Console.WriteLine("\n🎉 All tests completed successfully!");
            Console.WriteLine("The ProxmoxApi library is working correctly with your server.");
        }
        catch (ProxmoxAuthenticationException ex)
        {
            Console.WriteLine($"❌ Authentication Error: {ex.Message}");
            Console.WriteLine("\nTroubleshooting:");
            Console.WriteLine("- Verify username and password/API token are correct");
            Console.WriteLine("- Check if the user has appropriate permissions");
            Console.WriteLine("- Ensure the realm (PAM/PVE) is correct");
        }
        catch (ProxmoxAuthorizationException ex)
        {
            Console.WriteLine($"❌ Authorization Error: {ex.Message}");
            Console.WriteLine("\nTroubleshooting:");
            Console.WriteLine("- Check user permissions in Proxmox");
            Console.WriteLine("- Verify the user has API access rights");
        }
        catch (ProxmoxApiException ex)
        {
            Console.WriteLine($"❌ API Error: {ex.Message}");
            if (ex.StatusCode.HasValue)
            {
                Console.WriteLine($"   HTTP Status Code: {ex.StatusCode}");
            }
            if (ex.ErrorDetails != null && ex.ErrorDetails.Count > 0)
            {
                Console.WriteLine("   Error Details:");
                foreach (var detail in ex.ErrorDetails)
                {
                    Console.WriteLine($"     {detail.Key}: {detail.Value}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected Error: {ex.Message}");
            Console.WriteLine($"   Type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
            }
        }        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static ProxmoxConnectionInfo? GetConnectionInfo()
    {
        Console.WriteLine("Please provide your Proxmox server details:");
        
        // Get host
        Console.Write("Host (IP or FQDN): ");
        var host = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(host))
        {
            Console.WriteLine("Host is required!");
            return null;
        }

        // Get port (optional)
        Console.Write("Port [8006]: ");
        var portStr = Console.ReadLine()?.Trim();
        var port = string.IsNullOrEmpty(portStr) ? 8006 : int.Parse(portStr);

        // Get username
        Console.Write("Username: ");
        var username = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(username))
        {
            Console.WriteLine("Username is required!");
            return null;
        }

        // Get realm
        Console.Write("Realm [pam]: ");
        var realm = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(realm))
        {
            realm = "pam";
        }

        Console.WriteLine("\nAuthentication method:");
        Console.WriteLine("1. Username/Password");
        Console.WriteLine("2. API Token");
        Console.Write("Choose (1 or 2): ");
        var authChoice = Console.ReadLine()?.Trim();

        string? password = null;
        string? apiToken = null;

        if (authChoice == "1")
        {
            Console.Write("Password: ");
            password = ReadPassword();
            if (string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Password is required!");
                return null;
            }
        }
        else if (authChoice == "2")
        {
            Console.Write("API Token (format: user@realm!tokenid=uuid): ");
            apiToken = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(apiToken))
            {
                Console.WriteLine("API Token is required!");
                return null;
            }
        }
        else
        {
            Console.WriteLine("Invalid choice!");
            return null;
        }

        // SSL Options
        Console.Write("Ignore SSL certificate errors? (y/N): ");
        var ignoreSsl = Console.ReadLine()?.Trim().ToLower() == "y";

        return new ProxmoxConnectionInfo
        {
            Host = host,
            Port = port,
            Username = username,
            Password = password,
            ApiToken = apiToken,
            Realm = realm,
            IgnoreSslErrors = ignoreSsl
        };
    }

    private static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo keyInfo;
        
        do
        {
            keyInfo = Console.ReadKey(true);
            
            if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
            {
                password += keyInfo.KeyChar;
                Console.Write("*");
            }
            else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
        }
        while (keyInfo.Key != ConsoleKey.Enter);
        
        Console.WriteLine();
        return password;
    }
}
