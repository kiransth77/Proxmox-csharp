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
