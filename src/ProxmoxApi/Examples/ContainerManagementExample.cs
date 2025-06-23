using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Models;

namespace ProxmoxApi.Examples;

/// <summary>
/// Example demonstrating comprehensive LXC container management capabilities
/// </summary>
public class ContainerManagementExample
{
    private readonly ProxmoxClient _client;
    private readonly ILogger<ContainerManagementExample> _logger;

    public ContainerManagementExample(ProxmoxClient client, ILogger<ContainerManagementExample> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Demonstrates comprehensive container management operations
    /// </summary>
    public async Task RunExample()
    {
        try
        {
            _logger.LogInformation("=== Container Management Example ===");
            
            // List all containers across the cluster
            await ListAllContainers();
            
            // Get containers on a specific node
            await ListNodeContainers();
            
            // Create a new container
            //await CreateNewContainer();
            
            // Manage container lifecycle
            await ManageContainerLifecycle();
            
            // Monitor container resources
            await MonitorContainerResources();
            
            // Manage container snapshots
            await ManageContainerSnapshots();
            
            // Container configuration management
            await ManageContainerConfiguration();
            
            _logger.LogInformation("=== Container Management Example Completed ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running container management example");
            throw;
        }
    }

    /// <summary>
    /// Lists all containers across all nodes in the cluster
    /// </summary>
    private async Task ListAllContainers()
    {
        _logger.LogInformation("--- Listing All Containers ---");
        
        try
        {
            var containers = await _client.Containers.GetAllContainersAsync();
            
            if (containers.Any())
            {
                _logger.LogInformation($"Found {containers.Count} containers:");
                
                foreach (var container in containers)
                {
                    _logger.LogInformation($"Container {container.ContainerId}: {container.Name} " +
                                         $"[{container.Status}] on {container.Node} " +
                                         $"(Memory: {container.Memory / 1024 / 1024} MB, " +
                                         $"Cores: {container.Cores}, " +
                                         $"OS: {container.OsType})");
                    
                    if (!string.IsNullOrEmpty(container.Tags))
                    {
                        _logger.LogInformation($"  Tags: {container.Tags}");
                    }
                    
                    if (container.Uptime.HasValue && container.Uptime > 0)
                    {
                        var uptime = TimeSpan.FromSeconds(container.Uptime.Value);
                        _logger.LogInformation($"  Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");
                    }
                }
            }
            else
            {
                _logger.LogInformation("No containers found in the cluster");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list all containers");
        }
    }

    /// <summary>
    /// Lists containers on a specific node
    /// </summary>
    private async Task ListNodeContainers()
    {
        _logger.LogInformation("--- Listing Node Containers ---");
        
        try
        {
            // Get all nodes first
            var nodes = await _client.Nodes.GetNodesAsync();
            
            if (!nodes.Any())
            {
                _logger.LogWarning("No nodes found in the cluster");
                return;
            }

            var firstNode = nodes.First();
            _logger.LogInformation($"Getting containers for node: {firstNode.Node}");
            
            var containers = await _client.Containers.GetContainersAsync(firstNode.Node);
            
            if (containers.Any())
            {
                _logger.LogInformation($"Found {containers.Count} containers on node {firstNode.Node}:");
                
                foreach (var container in containers)
                {
                    _logger.LogInformation($"  {container.ContainerId}: {container.Name} [{container.Status}]");
                    
                    if (container.IsTemplate)
                        _logger.LogInformation($"    [TEMPLATE]");
                    
                    if (container.IsUnprivileged)
                        _logger.LogInformation($"    [UNPRIVILEGED]");
                }
            }
            else
            {
                _logger.LogInformation($"No containers found on node {firstNode.Node}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list node containers");
        }
    }

    /// <summary>
    /// Creates a new container (commented out to prevent actual creation)
    /// </summary>
    private async Task CreateNewContainer()
    {
        _logger.LogInformation("--- Creating New Container (Example) ---");
        
        try
        {
            // Get nodes to choose one for container creation
            var nodes = await _client.Nodes.GetNodesAsync();
            if (!nodes.Any())
            {
                _logger.LogWarning("No nodes available for container creation");
                return;
            }

            var targetNode = nodes.First();
            var containerId = 999; // Use a test ID
            
            var createParams = new ContainerCreateParameters
            {
                ContainerId = containerId,
                OsTemplate = "local:vztmpl/ubuntu-22.04-standard_22.04-1_amd64.tar.zst",
                Hostname = "test-container",
                Description = "Test container created by ProxmoxApi example",
                Memory = 1024, // 1GB RAM
                Cores = 2,
                RootFs = "local-lvm:8", // 8GB root filesystem
                Network = "name=eth0,bridge=vmbr0,ip=dhcp",
                Swap = 512,
                IsUnprivileged = true,
                StartAfterCreate = false,
                Tags = "test,example,api"
            };

            _logger.LogInformation($"Would create container {containerId} on node {targetNode.Node}");
            _logger.LogInformation($"Configuration: {createParams.Memory}MB RAM, {createParams.Cores} cores");
            _logger.LogInformation($"Template: {createParams.OsTemplate}");
            
            // Uncomment to actually create the container:
            /*
            var taskId = await _client.Containers.CreateContainerAsync(targetNode.Node, createParams);
            _logger.LogInformation($"Container creation task started: {taskId}");
            */
            
            _logger.LogInformation("Container creation example completed (not actually created)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create container");
        }
    }

    /// <summary>
    /// Manages container lifecycle operations
    /// </summary>
    private async Task ManageContainerLifecycle()
    {
        _logger.LogInformation("--- Container Lifecycle Management ---");
        
        try
        {
            var containers = await _client.Containers.GetAllContainersAsync();
            var runningContainer = containers.FirstOrDefault(c => c.Status.Equals("running", StringComparison.OrdinalIgnoreCase));
            
            if (runningContainer != null)
            {
                _logger.LogInformation($"Found running container: {runningContainer.ContainerId} ({runningContainer.Name})");
                
                // Get detailed status
                var status = await _client.Containers.GetContainerStatusAsync(runningContainer.Node, runningContainer.ContainerId);
                if (status != null)
                {
                    _logger.LogInformation($"Container Status Details:");
                    _logger.LogInformation($"  Status: {status.Status}");
                    _logger.LogInformation($"  PID: {status.ProcessId}");
                    
                    if (status.Uptime.HasValue)
                    {
                        var uptime = TimeSpan.FromSeconds(status.Uptime.Value);
                        _logger.LogInformation($"  Uptime: {uptime.Days}d {uptime.Hours}h {uptime.Minutes}m");
                    }
                    
                    if (status.CpuUsage.HasValue)
                        _logger.LogInformation($"  CPU Usage: {status.CpuUsage.Value:P2}");
                    
                    if (status.MemoryUsage.HasValue && status.MaxMemory.HasValue)
                        _logger.LogInformation($"  Memory: {status.MemoryUsage.Value / 1024 / 1024} MB / {status.MaxMemory.Value / 1024 / 1024} MB");
                }

                // Example lifecycle operations (commented to prevent actual execution)
                _logger.LogInformation("Lifecycle operations available:");
                _logger.LogInformation("  - Start: _client.Containers.StartContainerAsync()");
                _logger.LogInformation("  - Stop: _client.Containers.StopContainerAsync()");
                _logger.LogInformation("  - Restart: _client.Containers.RestartContainerAsync()");
                _logger.LogInformation("  - Delete: _client.Containers.DeleteContainerAsync()");
            }
            else
            {
                _logger.LogInformation("No running containers found for lifecycle management example");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to manage container lifecycle");
        }
    }

    /// <summary>
    /// Monitors container resource usage
    /// </summary>
    private async Task MonitorContainerResources()
    {
        _logger.LogInformation("--- Container Resource Monitoring ---");
        
        try
        {
            var containers = await _client.Containers.GetAllContainersAsync();
            var runningContainers = containers.Where(c => c.Status.Equals("running", StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (runningContainers.Any())
            {
                _logger.LogInformation($"Monitoring {runningContainers.Count} running containers:");
                
                foreach (var container in runningContainers.Take(3)) // Limit to first 3 for demo
                {
                    _logger.LogInformation($"\nContainer {container.ContainerId} ({container.Name}) on {container.Node}:");
                    
                    try
                    {
                        var statistics = await _client.Containers.GetContainerStatisticsAsync(container.Node, container.ContainerId);
                        if (statistics != null)
                        {
                            _logger.LogInformation($"  CPU Usage: {statistics.CpuUsage:P2}");
                            _logger.LogInformation($"  Memory: {statistics.MemoryUsage / 1024 / 1024:N0} MB / {statistics.MaxMemory / 1024 / 1024:N0} MB " +
                                                 $"({(double)statistics.MemoryUsage / statistics.MaxMemory:P1})");
                            _logger.LogInformation($"  Disk: {statistics.DiskUsage / 1024 / 1024:N0} MB / {statistics.MaxDisk / 1024 / 1024:N0} MB " +
                                                 $"({(double)statistics.DiskUsage / statistics.MaxDisk:P1})");
                            _logger.LogInformation($"  Network In: {statistics.NetworkIn / 1024 / 1024:N2} MB");
                            _logger.LogInformation($"  Network Out: {statistics.NetworkOut / 1024 / 1024:N2} MB");
                            _logger.LogInformation($"  Disk Read: {statistics.DiskRead / 1024 / 1024:N2} MB");
                            _logger.LogInformation($"  Disk Write: {statistics.DiskWrite / 1024 / 1024:N2} MB");
                        }
                        else
                        {
                            _logger.LogWarning($"  No statistics available for container {container.ContainerId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to get statistics for container {container.ContainerId}");
                    }
                }
            }
            else
            {
                _logger.LogInformation("No running containers found for resource monitoring");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to monitor container resources");
        }
    }

    /// <summary>
    /// Manages container snapshots
    /// </summary>
    private async Task ManageContainerSnapshots()
    {
        _logger.LogInformation("--- Container Snapshot Management ---");
        
        try
        {
            var containers = await _client.Containers.GetAllContainersAsync();
            var targetContainer = containers.FirstOrDefault();
            
            if (targetContainer != null)
            {
                _logger.LogInformation($"Managing snapshots for container {targetContainer.ContainerId} ({targetContainer.Name})");
                
                // List existing snapshots
                var snapshots = await _client.Containers.GetContainerSnapshotsAsync(targetContainer.Node, targetContainer.ContainerId);
                
                if (snapshots.Any())
                {
                    _logger.LogInformation($"Found {snapshots.Count} snapshots:");
                    foreach (var snapshot in snapshots)
                    {
                        _logger.LogInformation($"  {snapshot.Name}: {snapshot.Description ?? "No description"}");
                        if (snapshot.CreatedAt.HasValue)
                        {
                            _logger.LogInformation($"    Created: {snapshot.CreatedAt.Value:yyyy-MM-dd HH:mm:ss}");
                        }
                        if (!string.IsNullOrEmpty(snapshot.Parent))
                        {
                            _logger.LogInformation($"    Parent: {snapshot.Parent}");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("  No snapshots found");
                }

                // Example snapshot operations (commented to prevent actual execution)
                _logger.LogInformation("\nSnapshot operations available:");
                _logger.LogInformation("  - Create: _client.Containers.CreateContainerSnapshotAsync()");
                _logger.LogInformation("  - Delete: _client.Containers.DeleteContainerSnapshotAsync()");
                
                /*
                // Example: Create a snapshot
                var taskId = await _client.Containers.CreateContainerSnapshotAsync(
                    targetContainer.Node, 
                    targetContainer.ContainerId, 
                    "api-test-snapshot", 
                    "Snapshot created by ProxmoxApi example"
                );
                _logger.LogInformation($"Snapshot creation task: {taskId}");
                */
            }
            else
            {
                _logger.LogInformation("No containers found for snapshot management example");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to manage container snapshots");
        }
    }

    /// <summary>
    /// Manages container configuration
    /// </summary>
    private async Task ManageContainerConfiguration()
    {
        _logger.LogInformation("--- Container Configuration Management ---");
        
        try
        {
            var containers = await _client.Containers.GetAllContainersAsync();
            var targetContainer = containers.FirstOrDefault();
            
            if (targetContainer != null)
            {
                _logger.LogInformation($"Getting configuration for container {targetContainer.ContainerId} ({targetContainer.Name})");
                
                var config = await _client.Containers.GetContainerConfigAsync(targetContainer.Node, targetContainer.ContainerId);
                
                if (config != null)
                {
                    _logger.LogInformation("Container Configuration:");
                    _logger.LogInformation($"  Architecture: {config.Architecture}");
                    _logger.LogInformation($"  Hostname: {config.Hostname}");
                    _logger.LogInformation($"  OS Type: {config.OsType}");
                    _logger.LogInformation($"  Memory: {config.Memory} MB");
                    _logger.LogInformation($"  Cores: {config.Cores}");
                    _logger.LogInformation($"  Swap: {config.Swap} MB");
                    _logger.LogInformation($"  Root FS: {config.RootFs}");
                    _logger.LogInformation($"  Unprivileged: {config.IsUnprivileged}");
                    _logger.LogInformation($"  Template: {config.IsTemplate}");
                    
                    if (!string.IsNullOrEmpty(config.Description))
                        _logger.LogInformation($"  Description: {config.Description}");
                    
                    if (!string.IsNullOrEmpty(config.Tags))
                        _logger.LogInformation($"  Tags: {config.Tags}");
                    
                    if (!string.IsNullOrEmpty(config.Network0))
                        _logger.LogInformation($"  Network: {config.Network0}");

                    if (config.MountPoints.Any())
                    {
                        _logger.LogInformation("  Mount Points:");
                        foreach (var mp in config.MountPoints)
                        {
                            _logger.LogInformation($"    {mp.Key}: {mp.Value}");
                        }
                    }

                    if (config.NetworkInterfaces.Any())
                    {
                        _logger.LogInformation("  Network Interfaces:");
                        foreach (var net in config.NetworkInterfaces)
                        {
                            _logger.LogInformation($"    {net.Key}: {net.Value}");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning($"No configuration data available for container {targetContainer.ContainerId}");
                }
            }
            else
            {
                _logger.LogInformation("No containers found for configuration management example");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to manage container configuration");
        }
    }
}
