using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi;
using ProxmoxApi.Models;
using ProxmoxApi.Exceptions;
using ProxmoxApi.Examples;

class Program
{
    static async Task Main(string[] args)
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
            await RunInteractiveTest();
        }
    }
    private static async Task RunInteractiveTest()
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
            Console.WriteLine("\n🎉 Basic tests completed successfully!");

            // Advanced Features Testing
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("Advanced Feature Testing");
            Console.WriteLine(new string('=', 50)); Console.WriteLine("\nChoose advanced test to run:");
            Console.WriteLine("1. Node Management Test");
            Console.WriteLine("2. VM Management Test");
            Console.WriteLine("3. Container Management Test");
            Console.WriteLine("4. Storage Management Test");
            Console.WriteLine("5. Network Management Test");
            Console.WriteLine("6. All Advanced Tests");
            Console.WriteLine("7. Skip advanced tests");
            Console.Write("Enter choice (1-7): ");

            var advancedChoice = Console.ReadLine()?.Trim(); switch (advancedChoice)
            {
                case "1":
                    await RunNodeManagementTest(client, logger);
                    break;
                case "2":
                    await RunVmManagementTest(client, logger);
                    break;
                case "3":
                    await RunContainerManagementTest(client, logger);
                    break;
                case "4":
                    await RunStorageManagementTest(client, logger);
                    break;
                case "5":
                    await RunNetworkManagementTest(client, logger);
                    break;
                case "6":
                    await RunNodeManagementTest(client, logger);
                    await RunVmManagementTest(client, logger);
                    await RunContainerManagementTest(client, logger);
                    await RunStorageManagementTest(client, logger);
                    await RunNetworkManagementTest(client, logger);
                    break;
                case "7":
                default:
                    Console.WriteLine("Skipping advanced tests.");
                    break;
            }

            Console.WriteLine("\n🎉 All selected tests completed!");
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
        }
        Console.WriteLine("\nPress any key to exit...");
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
    private static async Task RunNodeManagementTest(ProxmoxClient client, ILogger logger)
    {
        try
        {
            Console.WriteLine("\n" + new string('-', 40));
            Console.WriteLine("Node Management Test");
            Console.WriteLine(new string('-', 40));

            // Get all nodes in the cluster
            Console.WriteLine("\n📋 Getting cluster nodes...");
            var nodes = await client.Nodes.GetNodesAsync();
            Console.WriteLine($"Found {nodes.Count} nodes:");
            foreach (var node in nodes)
            {
                Console.WriteLine($"  📍 {node.Node} - Status: {node.Status} - Type: {node.Type}");
                if (node.Status == "online" && node.CpuUsage.HasValue && node.MemoryUsed.HasValue && node.MemoryTotal.HasValue)
                {
                    Console.WriteLine($"     CPU: {node.CpuUsage:P2}, Memory: {node.MemoryUsed / (1024.0 * 1024 * 1024):F1}/{node.MemoryTotal / (1024.0 * 1024 * 1024):F1} GB");
                }
            }

            if (nodes.Count > 0)
            {
                // Demonstrate detailed node operations with the first node
                var firstNode = nodes.First();
                Console.WriteLine($"\n🔍 Getting detailed information for node '{firstNode.Node}':");
                var nodeStatus = await client.Nodes.GetNodeStatusAsync(firstNode.Node);
                if (nodeStatus != null)
                {
                    Console.WriteLine($"  Uptime: {nodeStatus.Uptime}s");
                    if (nodeStatus.LoadAverage != null && nodeStatus.LoadAverage.Length > 0)
                    {
                        Console.WriteLine($"  Load Average: {string.Join(", ", nodeStatus.LoadAverage)}");
                    }
                    Console.WriteLine($"  CPU Info: {nodeStatus.CpuInfo?.Model}");
                    Console.WriteLine($"  Memory: {nodeStatus.Memory?.Total / (1024.0 * 1024 * 1024):F1} GB total");
                }

                var nodeStats = await client.Nodes.GetNodeStatisticsAsync(firstNode.Node);
                if (nodeStats != null && nodeStats.Count > 0)
                {
                    Console.WriteLine($"  Statistics data points: {nodeStats.Count}");
                    Console.WriteLine("  (Raw statistics available for advanced processing)");
                }
            }

            Console.WriteLine("✅ Node Management Test completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Node Management Test Failed: {ex.Message}");
            logger.LogError(ex, "Node management test failed");
        }
    }

    private static async Task RunVmManagementTest(ProxmoxClient client, ILogger logger)
    {
        try
        {
            Console.WriteLine("\n" + new string('-', 40));
            Console.WriteLine("VM Management Test");
            Console.WriteLine(new string('-', 40));

            await VmManagementExample.RunExampleAsync(client, logger);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ VM Management Test Failed: {ex.Message}");
            logger.LogError(ex, "VM management test failed");
        }
    }
    private static async Task RunContainerManagementTest(ProxmoxClient client, ILogger logger)
    {
        try
        {
            Console.WriteLine("\n" + new string('-', 40));
            Console.WriteLine("Container Management Test");
            Console.WriteLine(new string('-', 40));

            var containerLogger = LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<ContainerManagementExample>();

            var containerExample = new ContainerManagementExample(client, containerLogger);
            await containerExample.RunExample();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Container Management Test Failed: {ex.Message}");
            logger.LogError(ex, "Container management test failed");
        }
    }

    private static async Task RunStorageManagementTest(ProxmoxClient client, ILogger logger)
    {
        try
        {
            Console.WriteLine("\n" + new string('-', 40));
            Console.WriteLine("Storage Management Test");
            Console.WriteLine(new string('-', 40));

            // Get first available node for storage operations
            var nodes = await client.Nodes.GetNodesAsync();
            var nodeName = nodes.FirstOrDefault()?.Node ?? "pve";

            Console.WriteLine($"Using node '{nodeName}' for storage testing");

            await StorageManagementExample.RunStorageManagementExampleAsync(client, nodeName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Storage Management Test Failed: {ex.Message}");
            logger.LogError(ex, "Storage management test failed");
        }
    }
    private static async Task RunNetworkManagementTest(ProxmoxClient client, ILogger logger)
    {
        try
        {
            Console.WriteLine("\n" + new string('-', 40));
            Console.WriteLine("Network Management Test");
            Console.WriteLine(new string('-', 40));

            // Get first available node for network operations
            var nodes = await client.Nodes.GetNodesAsync();
            var nodeName = nodes.FirstOrDefault()?.Node ?? "pve";

            Console.WriteLine($"Using node '{nodeName}' for network testing");

            await NetworkManagementExample.RunNetworkManagementExampleAsync(client, nodeName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Network Management Test Failed: {ex.Message}");
            logger.LogError(ex, "Network management test failed");
        }
    }
}
