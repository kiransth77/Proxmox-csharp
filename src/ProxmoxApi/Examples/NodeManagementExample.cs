using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi;
using ProxmoxApi.Models;
using ProxmoxApi.Exceptions;

namespace ProxmoxApi.Examples;

/// <summary>
/// Example usage of the Node Management features
/// </summary>
public class NodeManagementExample
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

        try
        {
            using var client = new ProxmoxClient(connectionInfo, logger);
            
            // Authenticate
            Console.WriteLine("🔐 Authenticating...");
            if (!await client.AuthenticateAsync())
            {
                Console.WriteLine("❌ Authentication failed");
                return;
            }
            Console.WriteLine("✅ Authentication successful!");

            // Get all nodes in the cluster
            Console.WriteLine("\n📋 Getting cluster nodes...");
            var nodes = await client.Nodes.GetNodesAsync();
            
            Console.WriteLine($"Found {nodes.Count} nodes:");
            foreach (var node in nodes)
            {
                var status = node.IsOnline ? "🟢 Online" : "🔴 Offline";
                var cpuPercent = node.CpuUsage.HasValue ? $"{node.CpuUsage * 100:F1}%" : "N/A";
                var memPercent = node.MemoryUsagePercentage.HasValue ? $"{node.MemoryUsagePercentage * 100:F1}%" : "N/A";
                
                Console.WriteLine($"  {status} {node.Node}");
                Console.WriteLine($"    CPU: {cpuPercent}, Memory: {memPercent}");
                Console.WriteLine($"    Type: {node.Type}, Level: {node.Level}");
                
                if (node.UptimeSpan.HasValue)
                {
                    Console.WriteLine($"    Uptime: {node.UptimeSpan.Value.Days}d {node.UptimeSpan.Value.Hours}h {node.UptimeSpan.Value.Minutes}m");
                }
                Console.WriteLine();
            }

            // Get detailed status for the first online node
            var onlineNode = nodes.FirstOrDefault(n => n.IsOnline);
            if (onlineNode != null)
            {
                Console.WriteLine($"📊 Getting detailed status for node '{onlineNode.Node}'...");
                var nodeStatus = await client.Nodes.GetNodeStatusAsync(onlineNode.Node);
                
                Console.WriteLine($"Node Status Details for {onlineNode.Node}:");
                Console.WriteLine($"  Current Time: {nodeStatus.CurrentTime}");
                Console.WriteLine($"  Uptime: {nodeStatus.UptimeSpan.Days}d {nodeStatus.UptimeSpan.Hours}h {nodeStatus.UptimeSpan.Minutes}m");
                Console.WriteLine($"  PVE Version: {nodeStatus.PveVersion}");
                
                if (nodeStatus.LoadAverage != null && nodeStatus.LoadAverage.Length >= 3)
                {
                    Console.WriteLine($"  Load Average: {nodeStatus.LoadAverage[0]:F2}, {nodeStatus.LoadAverage[1]:F2}, {nodeStatus.LoadAverage[2]:F2}");
                }

                if (nodeStatus.CpuInfo != null)
                {
                    Console.WriteLine($"  CPU: {nodeStatus.CpuInfo.Cpus} cores");
                    Console.WriteLine($"       Model: {nodeStatus.CpuInfo.Model}");
                    Console.WriteLine($"       Frequency: {nodeStatus.CpuInfo.Mhz:F0} MHz");
                    if (nodeStatus.CpuInfo.Idle.HasValue)
                    {
                        Console.WriteLine($"       Idle: {nodeStatus.CpuInfo.Idle:F1}%");
                    }
                }

                if (nodeStatus.Memory != null)
                {
                    var memTotalGB = nodeStatus.Memory.Total / (1024.0 * 1024.0 * 1024.0);
                    var memUsedGB = nodeStatus.Memory.Used / (1024.0 * 1024.0 * 1024.0);
                    Console.WriteLine($"  Memory: {memUsedGB:F1} GB / {memTotalGB:F1} GB ({nodeStatus.Memory.UsagePercentage * 100:F1}%)");
                }

                if (nodeStatus.RootFs != null)
                {
                    var diskTotalGB = nodeStatus.RootFs.Total / (1024.0 * 1024.0 * 1024.0);
                    var diskUsedGB = nodeStatus.RootFs.Used / (1024.0 * 1024.0 * 1024.0);
                    Console.WriteLine($"  Root FS: {diskUsedGB:F1} GB / {diskTotalGB:F1} GB ({nodeStatus.RootFs.UsagePercentage * 100:F1}%)");
                }

                // Get version information
                Console.WriteLine($"\n🔧 Getting version info for node '{onlineNode.Node}'...");
                var version = await client.Nodes.GetNodeVersionAsync(onlineNode.Node);
                
                Console.WriteLine("Version Information:");
                foreach (var kvp in version)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }

                // Get subscription information
                Console.WriteLine($"\n📄 Getting subscription info for node '{onlineNode.Node}'...");
                try
                {
                    var subscription = await client.Nodes.GetNodeSubscriptionAsync(onlineNode.Node);
                    
                    Console.WriteLine("Subscription Information:");
                    foreach (var kvp in subscription)
                    {
                        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                    }
                }
                catch (ProxmoxApiException ex) when (ex.StatusCode == 500)
                {
                    Console.WriteLine("  No subscription information available (likely community edition)");
                }

                // Get statistics (optional - commented out as it might be resource intensive)
                /*
                Console.WriteLine($"\n📈 Getting hourly statistics for node '{onlineNode.Node}'...");
                var stats = await client.Nodes.GetNodeStatisticsAsync(onlineNode.Node, "hour");
                
                Console.WriteLine($"Retrieved {stats.Count} statistics entries");
                if (stats.Count > 0)
                {
                    var latest = stats.LastOrDefault();
                    if (latest != null)
                    {
                        Console.WriteLine("Latest statistics:");
                        foreach (var kvp in latest)
                        {
                            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                        }
                    }
                }
                */

                // Check online status
                Console.WriteLine($"\n🔍 Checking if node '{onlineNode.Node}' is online...");
                var isOnline = await client.Nodes.IsNodeOnlineAsync(onlineNode.Node);
                Console.WriteLine($"Node {onlineNode.Node} is {(isOnline ? "🟢 online" : "🔴 offline")}");
            }

            // Get nodes summary
            Console.WriteLine("\n📊 Getting nodes summary...");
            var summary = await client.Nodes.GetNodesSummaryAsync();
            
            Console.WriteLine("Nodes Summary:");
            foreach (var kvp in summary)
            {
                var node = kvp.Value;
                var status = node.IsOnline ? "🟢" : "🔴";
                Console.WriteLine($"  {status} {kvp.Key}: {node.Status}");
            }

            Console.WriteLine("\n🎉 Node management example completed successfully!");
        }
        catch (ProxmoxAuthenticationException ex)
        {
            Console.WriteLine($"❌ Authentication Error: {ex.Message}");
        }
        catch (ProxmoxApiException ex)
        {
            Console.WriteLine($"❌ API Error: {ex.Message}");
            if (ex.StatusCode.HasValue)
            {
                Console.WriteLine($"   Status Code: {ex.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected Error: {ex.Message}");
        }
    }
}
