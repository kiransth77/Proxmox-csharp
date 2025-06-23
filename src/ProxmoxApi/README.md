# ProxmoxApi Library

A C# client library for interacting with Proxmox VE REST API.

## Features

### Iteration 1 - Foundation (COMPLETED ✅)
- ✅ Basic project structure with .NET 8.0
- ✅ Connection configuration and management
- ✅ Authentication support (username/password and API tokens)
- ✅ HTTP client wrapper with proper error handling
- ✅ JSON serialization/deserialization
- ✅ Comprehensive logging support
- ✅ Custom exception types
- ✅ SSL certificate validation options
- ✅ Connection testing
- ✅ Version information retrieval

### Iteration 2 - Node Management (COMPLETED ✅)
- ✅ **Node Discovery**: List all nodes in the cluster
- ✅ **Node Status**: Get detailed status information for specific nodes
- ✅ **Node Statistics**: Retrieve CPU, memory, disk, and network statistics
- ✅ **Node Information**: Get version and subscription details
- ✅ **Node Operations**: Execute commands (reboot, shutdown)
- ✅ **Real-time Monitoring**: Check node online status and health
- ✅ **Cluster Summary**: Get overview of all nodes with their status

### Iteration 3 - Virtual Machine Management (COMPLETED ✅)
- ✅ **VM Discovery**: List all VMs across nodes or on specific nodes
- ✅ **VM Lifecycle**: Start, stop, restart, shutdown, suspend, and resume VMs
- ✅ **VM Status & Monitoring**: Get detailed status and real-time statistics
- ✅ **VM Configuration**: Retrieve and update VM configuration settings
- ✅ **VM Snapshots**: Create, list, and delete VM snapshots
- ✅ **VM Cloning**: Clone VMs with configurable options
- ✅ **VM Deletion**: Safely delete VMs with confirmation
- ✅ **Resource Monitoring**: Track CPU, memory, disk, and network usage

### Iteration 4 - Container (LXC) Management (COMPLETED ✅)
- ✅ **Container Discovery**: List all LXC containers on nodes
- ✅ **Container Lifecycle**: Start, stop, restart, shutdown, suspend, and resume containers
- ✅ **Container Status & Monitoring**: Get detailed status and real-time statistics  
- ✅ **Container Configuration**: Retrieve and update container configuration
- ✅ **Container Snapshots**: Create, list, and delete container snapshots
- ✅ **Container Cloning**: Clone containers with customizable parameters
- ✅ **Container Deletion**: Safely delete containers with optional snapshot cleanup
- ✅ **Resource Monitoring**: Track CPU, memory, disk, and network usage for containers

## Installation

Add the package reference to your project:

```xml
<PackageReference Include="ProxmoxApi" Version="1.0.0" />
```

## Quick Start

### Basic Usage with Username/Password

```csharp
using ProxmoxApi;
using ProxmoxApi.Models;

var connectionInfo = new ProxmoxConnectionInfo
{
    Host = "your-proxmox-server.local",
    Username = "root",
    Password = "your-password",
    IgnoreSslErrors = true // Only for development
};

using var client = new ProxmoxClient(connectionInfo);

// Test connection
var isConnected = await client.TestConnectionAsync();
if (!isConnected)
{
    Console.WriteLine("Failed to connect");
    return;
}

// Authenticate
var isAuthenticated = await client.AuthenticateAsync();
if (!isAuthenticated)
{
    Console.WriteLine("Authentication failed");
    return;
}

// Get version information
var version = await client.GetVersionAsync();
Console.WriteLine($"Proxmox Version: {version}");

// Access node management features
var nodes = await client.Nodes.GetNodesAsync();
foreach (var node in nodes)
{
    Console.WriteLine($"Node: {node.Node}, Status: {node.Status}");
    
    if (node.IsOnline)
    {
        var nodeStatus = await client.Nodes.GetNodeStatusAsync(node.Node);
        Console.WriteLine($"  Uptime: {nodeStatus.UptimeSpan}");
        Console.WriteLine($"  Memory: {nodeStatus.Memory?.UsagePercentage * 100:F1}%");
    }
}
```

### Using API Token Authentication

```csharp
var connectionInfo = new ProxmoxConnectionInfo
{
    Host = "your-proxmox-server.local",
    Username = "root",
    ApiToken = "root@pam!mytoken=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
};

using var client = new ProxmoxClient(connectionInfo);
// Authentication is automatic with API tokens
```

## Configuration Options

### ProxmoxConnectionInfo Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Host | string | required | Proxmox server hostname or IP |
| Port | int | 8006 | Proxmox web interface port |
| Username | string | required | Username for authentication |
| Password | string? | null | Password (alternative to API token) |
| ApiToken | string? | null | API token (alternative to password) |
| Realm | string | "pam" | Authentication realm |
| UseHttps | bool | true | Use HTTPS protocol |
| IgnoreSslErrors | bool | false | Ignore SSL certificate errors |
| TimeoutSeconds | int | 30 | Request timeout in seconds |

## Node Management API

### Basic Node Operations

```csharp
// Get all nodes in the cluster
var nodes = await client.Nodes.GetNodesAsync();

// Get detailed status for a specific node
var nodeStatus = await client.Nodes.GetNodeStatusAsync("node1");

// Check if a node is online
var isOnline = await client.Nodes.IsNodeOnlineAsync("node1");

// Get node version information
var version = await client.Nodes.GetNodeVersionAsync("node1");

// Get subscription information
var subscription = await client.Nodes.GetNodeSubscriptionAsync("node1");
```

### Node Statistics and Monitoring

```csharp
// Get nodes summary
var summary = await client.Nodes.GetNodesSummaryAsync();

// Get historical statistics
var stats = await client.Nodes.GetNodeStatisticsAsync("node1", "hour");

// Monitor node resources
foreach (var node in nodes.Where(n => n.IsOnline))
{
    Console.WriteLine($"Node: {node.Node}");
    Console.WriteLine($"  CPU Usage: {node.CpuUsage * 100:F1}%");
    Console.WriteLine($"  Memory Usage: {node.MemoryUsagePercentage * 100:F1}%");
    Console.WriteLine($"  Disk Usage: {node.DiskUsagePercentage * 100:F1}%");
    Console.WriteLine($"  Uptime: {node.UptimeSpan?.Days}d {node.UptimeSpan?.Hours}h");
}
```

### Node Operations (Use with Caution)

```csharp
// Reboot a node
await client.Nodes.RebootNodeAsync("node1");

// Shutdown a node
await client.Nodes.ShutdownNodeAsync("node1");

// Execute custom command
await client.Nodes.ExecuteNodeCommandAsync("node1", "reboot");
```

## Node Models

### ProxmoxNode
Main node object with properties like:
- `Node`: Node name/identifier
- `Status`: Current status (online/offline)
- `CpuUsage`, `MemoryUsed`, `DiskUsed`: Resource usage
- `IsOnline`: Boolean indicating if node is accessible
- `UptimeSpan`: Node uptime as TimeSpan

### NodeStatus
Detailed status information including:
- `CurrentTime`: Current time on the node
- `UptimeSpan`: Uptime as TimeSpan
- `CpuInfo`: Detailed CPU information
- `Memory`: Memory statistics
- `RootFs`: Root filesystem usage

## Container (LXC) Management API

### Basic Container Operations

```csharp
// Get all containers on a node
var containers = await client.Containers.GetContainersAsync("node1");

// Get detailed container status
var containerStatus = await client.Containers.GetContainerStatusAsync("node1", 100);

// Get container configuration
var config = await client.Containers.GetContainerConfigAsync("node1", 100);

// Get real-time container statistics
var stats = await client.Containers.GetContainerStatisticsAsync("node1", 100);
```

### Container Lifecycle Management

```csharp
// Start a container
var startTask = await client.Containers.StartContainerAsync("node1", 100);

// Stop a container (force stop)
var stopTask = await client.Containers.StopContainerAsync("node1", 100);

// Gracefully shutdown a container (with 30 second timeout)
var shutdownTask = await client.Containers.ShutdownContainerAsync("node1", 100, 30);

// Restart a container
var restartTask = await client.Containers.RestartContainerAsync("node1", 100, 30);

// Suspend a container
var suspendTask = await client.Containers.SuspendContainerAsync("node1", 100);

// Resume a suspended container
var resumeTask = await client.Containers.ResumeContainerAsync("node1", 100);

// All lifecycle operations return task IDs for monitoring
Console.WriteLine($"Container operation started: {startTask}");
```

### Container Snapshots

```csharp
// List all snapshots for a container
var snapshots = await client.Containers.GetContainerSnapshotsAsync("node1", 100);

// Create a new snapshot
var snapshotTask = await client.Containers.CreateContainerSnapshotAsync(
    "node1", 100, "backup-before-update", "Snapshot before system update");

// Delete a snapshot
var deleteTask = await client.Containers.DeleteContainerSnapshotAsync("node1", 100, "backup-before-update");
```

### Advanced Container Operations

```csharp
// Clone a container
var cloneTask = await client.Containers.CloneContainerAsync(
    "node1", 100, 200, "cloned-container", "Cloned from container 100");

// Update container configuration
var configUpdates = new Dictionary<string, object>
{
    {"memory", 2048},        // Set memory to 2GB
    {"cores", 4},            // Set CPU cores to 4
    {"description", "Updated container configuration"}
};
var updateTask = await client.Containers.UpdateContainerConfigAsync("node1", 100, configUpdates);

// Delete a container (with optional snapshot cleanup)
var deleteTask = await client.Containers.DeleteContainerAsync("node1", 100, purgeSnapshots: true);
```

### Container Monitoring Example

```csharp
// Monitor containers across all nodes
var allNodes = await client.Nodes.GetNodesAsync();

foreach (var node in allNodes.Where(n => n.IsOnline))
{
    var containers = await client.Containers.GetContainersAsync(node.Node);
    
    Console.WriteLine($"Node: {node.Node} - {containers.Count} containers");
    
    foreach (var container in containers)
    {
        Console.WriteLine($"  Container {container.ContainerId}: {container.Name}");
        Console.WriteLine($"    Status: {container.Status}");
        Console.WriteLine($"    Memory: {container.Memory / 1024 / 1024}MB");
        Console.WriteLine($"    Cores: {container.Cores}");
        
        // Get real-time statistics for running containers
        if (container.Status == "running")
        {
            var stats = await client.Containers.GetContainerStatisticsAsync(node.Node, container.ContainerId);
            Console.WriteLine($"    CPU Usage: {stats.CpuUsage * 100:F1}%");
            Console.WriteLine($"    Memory Usage: {stats.MemoryUsage / 1024 / 1024:F0}MB of {stats.MaxMemory / 1024 / 1024:F0}MB");
            Console.WriteLine($"    Network In/Out: {stats.NetworkIn / 1024:F0}KB / {stats.NetworkOut / 1024:F0}KB");
        }
    }
}
```

## Container Models

### ProxmoxContainer
Main container object with properties:
- `ContainerId`: Unique container ID (integer)
- `Name`: Container hostname/name
- `Status`: Current status (running, stopped, etc.)
- `Node`: Node where container is located
- `Memory`: Allocated memory in bytes
- `Cores`: Number of CPU cores
- `Template`: OS template used

### ContainerStatus
Real-time container status:
- `Status`: Current status string
- `Uptime`: Container uptime in seconds
- `ProcessId`: Container process ID
- `CpuUsage`: Current CPU usage percentage
- `MemoryUsage`: Current memory usage in bytes

### ContainerStatistics
Detailed performance statistics:
- `CpuUsage`: CPU usage (0.0 to 1.0)
- `MemoryUsage`, `MaxMemory`: Memory statistics
- `NetworkIn`, `NetworkOut`: Network traffic counters
- `DiskRead`, `DiskWrite`: Disk I/O counters

### ContainerConfig
Container configuration settings:
- `Architecture`: Container architecture (amd64, etc.)
- `Cores`, `CpuLimit`: CPU configuration
- `Memory`, `Swap`: Memory configuration
- `Hostname`: Container hostname
- `Description`: Container description
- `OsType`: Operating system type
- `NetworkInterfaces`: Network configuration

## Error Handling

The library provides specific exception types:

- **ProxmoxApiException**: Base exception for API operations
- **ProxmoxAuthenticationException**: Authentication failures
- **ProxmoxAuthorizationException**: Authorization failures

```csharp
try
{
    var nodes = await client.Nodes.GetNodesAsync();
}
catch (ProxmoxAuthenticationException ex)
{
    Console.WriteLine($"Auth failed: {ex.Message}");
}
catch (ProxmoxApiException ex)
{
    Console.WriteLine($"API error: {ex.Message}, Status: {ex.StatusCode}");
}
```

## Logging

The library uses Microsoft.Extensions.Logging for comprehensive logging:

```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var logger = loggerFactory.CreateLogger<ProxmoxClient>();
var client = new ProxmoxClient(connectionInfo, logger);
```

## Platform Compatibility

This library is built on .NET 8.0 and is compatible with:

- ✅ **ASP.NET Core** applications (Web APIs, MVC, Blazor)
- ✅ **.NET MAUI** applications (Cross-platform mobile and desktop)
- ✅ **WPF** applications (Windows desktop)
- ✅ **WinUI 3** applications (Modern Windows apps)
- ✅ **Console** applications (.NET Core/8+)
- ✅ **Xamarin.Forms** applications (with .NET Standard 2.0 compatibility)
- ✅ **Unity** applications (with appropriate .NET compatibility)

### MAUI Integration Example

The library works seamlessly with .NET MAUI for building cross-platform Proxmox management applications:

```csharp
// In your MAUI project
public partial class MainPage : ContentPage
{
    private readonly ProxmoxClient _client;

    public MainPage()
    {
        InitializeComponent();
        
        var connectionInfo = new ProxmoxConnectionInfo
        {
            Host = "your-proxmox-server.local",
            Username = "admin",
            Password = "password",
            IgnoreSslErrors = true
        };
        
        _client = new ProxmoxClient(connectionInfo);
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        try
        {
            if (await _client.TestConnectionAsync() && await _client.AuthenticateAsync())
            {
                // Get node information for mobile display
                var nodes = await _client.Nodes.GetNodesAsync();
                // Update your MAUI UI with node data
                DisplayNodes(nodes);
            }
        }
        catch (Exception ex)
        {
            // Handle connection errors in mobile UI
            await DisplayAlert("Connection Error", ex.Message, "OK");
        }
    }
}
```

### Cross-Platform Considerations

- **Network Connectivity**: Handle mobile network scenarios (WiFi/cellular switching)
- **SSL Certificates**: Consider certificate pinning for production mobile apps
- **Battery Optimization**: Implement efficient polling for live data updates
- **Offline Capability**: Cache critical data for offline viewing
- **Platform-Specific Features**: Leverage native notifications and background tasks

## Next Iterations

### Planned Features

- **Iteration 3**: Virtual machine operations (list, create, start, stop, delete)
- **Iteration 4**: Container (LXC) operations
- **Iteration 5**: Storage management
- **Iteration 6**: Network configuration
- **Iteration 7**: Backup and restore operations
- **Iteration 8**: User and permission management
- **Iteration 9**: Clustering support
- **Iteration 10**: Advanced monitoring and metrics

## Contributing

This is an open-source project. Contributions are welcome!

## License

MIT License
