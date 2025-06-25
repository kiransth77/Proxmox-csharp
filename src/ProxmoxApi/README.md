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

### Iteration 5 - Storage Management (COMPLETED ✅)
- ✅ **Storage Configuration**: List, create, update, and delete storage configurations
- ✅ **Storage Status & Monitoring**: Monitor storage usage, capacity, and health across nodes
- ✅ **Storage Content Management**: Browse, upload, and manage files in storage
- ✅ **Volume Operations**: Create, delete, copy, and move volumes between storages
- ✅ **Backup Management**: List, download, and manage backup files
- ✅ **ISO & Template Management**: Manage ISO images and container templates
- ✅ **Multi-Storage Support**: Handle directory, LVM, NFS, Ceph, and other storage types
- ✅ **Content Filtering**: Filter storage content by type (images, backups, ISOs, templates)

### Iteration 6 - Network Management (COMPLETED ✅)
- ✅ **Network Interface Management**: List, create, update, and delete network interfaces
- ✅ **Bridge Configuration**: Create and manage bridge interfaces with VLAN awareness
- ✅ **VLAN Management**: Create and configure VLAN interfaces with proper tagging
- ✅ **Bond Configuration**: Set up and manage bonded network interfaces for redundancy
- ✅ **Network Status Monitoring**: Monitor interface status, configuration, and activity
- ✅ **Firewall Rules**: Manage node-level firewall rules and security policies
- ✅ **DNS Configuration**: Configure DNS settings and search domains
- ✅ **Hosts File Management**: Manage static host entries and IP-to-hostname mappings
- ✅ **Network Validation**: Validate network configurations before applying changes
- ✅ **Network Summary**: Get comprehensive network overview and statistics

### Iteration 7 - Backup and Restore Management (COMPLETED ✅)
- ✅ **Backup Job Management**: Create, update, delete, and list backup jobs with full configuration
- ✅ **Backup File Operations**: List, browse, and manage backup files across storage systems
- ✅ **VM/Container Backup**: Create on-demand backups for VMs and containers with customizable settings
- ✅ **Restore Operations**: Restore VMs and containers from backup files with flexible configuration
- ✅ **Backup Task Monitoring**: Monitor backup and restore task progress and status
- ✅ **Cross-Storage Support**: Handle backups across different storage types and locations
- ✅ **Backup Filtering**: Filter and search backup files by VM ID, date, and storage location
- ✅ **Task Management**: Track backup task execution, progress, and completion status

### Iteration 8 - User and Permission Management (COMPLETED ✅)
- ✅ **User Management**: Create, update, delete, and list users with comprehensive configuration
- ✅ **Group Management**: Manage user groups with member assignment and configuration
- ✅ **Role Management**: Create and manage custom roles with specific permissions
- ✅ **Access Control Lists (ACL)**: Configure fine-grained permissions for resources and paths
- ✅ **API Token Management**: Create, manage, and delete API tokens for service accounts
- ✅ **Password Management**: Set and update user passwords with security validation
- ✅ **Realm Integration**: Support for different authentication realms (PAM, LDAP, etc.)
- ✅ **Permission Inheritance**: Handle permission inheritance through groups and roles
- ✅ **Security Validation**: Validate user permissions and access rights
- ✅ **Audit Support**: Track user management operations and access patterns

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

- **Iteration 7**: Backup and restore operations
- **Iteration 8**: User and permission management  
- **Iteration 9**: Clustering support
- **Iteration 10**: Advanced monitoring and metrics

## Contributing

This is an open-source project. Contributions are welcome!

## License

MIT License

## Network Management

### Network Interface Management

```csharp
// Get all network interfaces on a node
var interfaces = await client.Network.GetNetworkInterfacesAsync("node1");

foreach (var iface in interfaces)
{
    Console.WriteLine($"Interface: {iface.InterfaceName} ({iface.Type})");
    Console.WriteLine($"  Address: {iface.Address ?? "None"}");
    Console.WriteLine($"  Active: {(iface.IsActive ? "Yes" : "No")}");
    Console.WriteLine($"  Autostart: {(iface.IsAutostart ? "Yes" : "No")}");
}

// Get specific interface details
var ethInterface = await client.Network.GetNetworkInterfaceAsync("node1", "eth0");
```

### Bridge Management

```csharp
// Create a bridge interface
var bridgeConfig = new BridgeConfig
{
    Name = "vmbr1",
    Ports = "eth1,eth2",
    SpanningTreeProtocol = false,
    VlanAware = true,
    Comments = "VM bridge"
};

await client.Network.CreateBridgeAsync("node1", bridgeConfig);

// Get all bridges
var bridges = await client.Network.GetBridgesAsync("node1");
```

### VLAN Configuration

```csharp
// Create a VLAN interface
var vlanConfig = new VlanConfig
{
    Name = "vlan100",
    VlanId = 100,
    RawDevice = "eth0",
    Method = "static",
    Address = "192.168.100.1",
    Netmask = "255.255.255.0"
};

await client.Network.CreateVlanAsync("node1", vlanConfig);

// Get all VLANs
var vlans = await client.Network.GetVlansAsync("node1");
```

### Bond Configuration

```csharp
// Create a bond interface for redundancy
var bondConfig = new BondConfig
{
    Name = "bond0",
    Slaves = "eth0 eth1",
    Mode = "active-backup",
    Method = "static",
    Address = "192.168.1.10",
    Netmask = "255.255.255.0",
    Gateway = "192.168.1.1"
};

await client.Network.CreateBondAsync("node1", bondConfig);

// Get all bonds
var bonds = await client.Network.GetBondsAsync("node1");
```

### Firewall Management

```csharp
// Get firewall rules for a node
var firewallRules = await client.Network.GetFirewallRulesAsync("node1");

// Create a new firewall rule
var newRule = new FirewallRuleCreateOptions
{
    Action = "ACCEPT",
    Type = "in",
    Source = "192.168.1.0/24",
    DestinationPort = "22",
    Protocol = "tcp",
    Comment = "SSH access from LAN"
};

await client.Network.CreateFirewallRuleAsync("node1", newRule);

// Update an existing rule
await client.Network.UpdateFirewallRuleAsync("node1", 0, updatedRule);

// Delete a firewall rule
await client.Network.DeleteFirewallRuleAsync("node1", 0);
```

### DNS and Hosts Management

```csharp
// Get DNS configuration
var dnsConfig = await client.Network.GetDnsConfigAsync("node1");

// Update DNS settings
var newDnsConfig = new DnsConfig
{
    SearchDomain = "example.com",
    Dns1 = "8.8.8.8",
    Dns2 = "8.8.4.4"
};

await client.Network.UpdateDnsConfigAsync("node1", newDnsConfig);

// Get host entries
var hostEntries = await client.Network.GetHostEntriesAsync("node1");

// Update host entries
var newHostEntries = new List<HostEntry>
{
    new() { IpAddress = "192.168.1.10", Hostname = "server1.example.com" },
    new() { IpAddress = "192.168.1.20", Hostname = "server2.example.com" }
};

await client.Network.UpdateHostEntriesAsync("node1", newHostEntries);
```

### Network Status and Monitoring

```csharp
// Get network status
var networkStatus = await client.Network.GetNetworkStatusAsync("node1");

// Get network summary
var summary = await client.Network.GetNetworkSummaryAsync("node1");
Console.WriteLine($"Total Interfaces: {summary["TotalInterfaces"]}");
Console.WriteLine($"Active Interfaces: {summary["ActiveInterfaces"]}");
Console.WriteLine($"Bridges: {summary["BridgeCount"]}");
Console.WriteLine($"VLANs: {summary["VlanCount"]}");
Console.WriteLine($"Bonds: {summary["BondCount"]}");

// Validate network configuration
var interfaceOptions = new NetworkInterfaceCreateOptions
{
    InterfaceName = "vmbr0",
    Type = "bridge",
    BridgePorts = "eth0"
};

var isValid = NetworkService.ValidateNetworkInterfaceConfig(interfaceOptions);
```

### Network Configuration Changes

```csharp
// Apply network configuration changes
await client.Network.ApplyNetworkConfigurationAsync("node1");

// Revert network configuration changes
await client.Network.RevertNetworkConfigurationAsync("node1");
```

## Backup and Restore Management API

### Backup Job Management

```csharp
// Get all backup jobs
var backupJobs = await client.Backup.GetBackupJobsAsync();

foreach (var job in backupJobs)
{
    Console.WriteLine($"Job: {job.Id}, Schedule: {job.Schedule}");
    Console.WriteLine($"  Storage: {job.Storage}, VMs: {job.VmIds}");
    Console.WriteLine($"  Enabled: {job.Enabled}, Mode: {job.Mode}");
}

// Get specific backup job
var job = await client.Backup.GetBackupJobAsync("backup-job-1");

// Create new backup job
var newJob = new BackupJobParameters
{
    Schedule = "0 2 * * *", // Daily at 2 AM
    Storage = "backup-storage",
    VmIds = "100,101,102", // Specific VMs or "all"
    Mode = "snapshot",
    Compress = "zstd",
    Remove = 7, // Keep 7 backups
    MailTo = "admin@example.com",
    Comment = "Daily VM backup"
};

await client.Backup.CreateBackupJobAsync(newJob);

// Update existing backup job
var updatedParams = new BackupJobParameters
{
    Schedule = "0 3 * * 0", // Weekly on Sunday at 3 AM
    Enabled = true
};

await client.Backup.UpdateBackupJobAsync("backup-job-1", updatedParams);

// Delete backup job
await client.Backup.DeleteBackupJobAsync("backup-job-1");
```

### Backup File Management

```csharp
// Get all backup files
var allBackups = await client.Backup.GetBackupFilesAsync("node1");

foreach (var backup in allBackups)
{
    Console.WriteLine($"Backup: {backup.VolumeId}");
    Console.WriteLine($"  VM ID: {backup.VmId}, Size: {backup.Size} bytes");
    Console.WriteLine($"  Created: {backup.CreationTime}, Format: {backup.Format}");
}

// Get backup files for specific VM
var vmBackups = await client.Backup.GetVmBackupFilesAsync("node1", 100);

// Get backup files from specific storage
var storageBackups = await client.Backup.GetBackupFilesAsync("node1", "backup-storage");

// Delete backup file
await client.Backup.DeleteBackupFileAsync("node1", "backup-storage", "vzdump-qemu-100-2024_06_24-02_00_05.vma.zst");
```

### VM and Container Backup Operations

```csharp
// Create VM backup
var vmBackupTask = await client.Backup.CreateVmBackupAsync(
    node: "node1",
    vmId: 100,
    storage: "backup-storage",
    mode: "snapshot",
    compress: "zstd"
);

Console.WriteLine($"Backup task started: {vmBackupTask}");

// Create container backup
var containerBackupTask = await client.Backup.CreateContainerBackupAsync(
    node: "node1", 
    containerId: 200,
    storage: "backup-storage",
    mode: "snapshot",
    compress: "zstd"
);

// Backup with custom options
var backupTask = await client.Backup.CreateVmBackupAsync(
    node: "node1",
    vmId: 100, 
    storage: "backup-storage",
    mode: "stop", // stop, suspend, or snapshot
    compress: "gzip" // none, lzo, gzip, or zstd
);
```

### Restore Operations

```csharp
// Restore VM from backup
var restoreParams = new RestoreParameters
{
    VmId = 150, // New VM ID
    Storage = "local-lvm", // Target storage
    Unique = true, // Generate new MAC addresses
    Force = false // Don't overwrite existing VM
};

var restoreTask = await client.Backup.RestoreVmBackupAsync(
    node: "node1",
    backupFile: "vzdump-qemu-100-2024_06_24-02_00_05.vma.zst",
    restoreParams
);

// Restore container from backup  
var containerRestoreParams = new RestoreParameters
{
    VmId = 250,
    Storage = "local",
    Unprivileged = true
};

var containerRestoreTask = await client.Backup.RestoreContainerBackupAsync(
    node: "node1", 
    backupFile: "vzdump-lxc-200-2024_06_24-02_00_05.tar.zst",
    containerRestoreParams
);

Console.WriteLine($"Restore task: {restoreTask}");
```

### Backup Task Monitoring

```csharp
// Get backup task status
var taskStatus = await client.Backup.GetBackupTaskStatusAsync("node1", "UPID:node1:12345678");

Console.WriteLine($"Task Status: {taskStatus.Status}");
Console.WriteLine($"Progress: {taskStatus.Progress}%");
Console.WriteLine($"Started: {taskStatus.StartTime}");

if (taskStatus.IsCompleted)
{
    Console.WriteLine($"Completed: {taskStatus.EndTime}");
    Console.WriteLine($"Duration: {taskStatus.Duration}");
}

// Get all backup tasks
var allTasks = await client.Backup.GetBackupTasksAsync("node1");

foreach (var task in allTasks.Take(10)) // Show last 10
{
    Console.WriteLine($"Task: {task.Upid}");
    Console.WriteLine($"  Type: {task.Type}, Status: {task.Status}");
    Console.WriteLine($"  Started: {task.StartTime}, User: {task.User}");
}

// Monitor backup task progress
var taskId = await client.Backup.CreateVmBackupAsync("node1", 100, "backup-storage");

while (true)
{
    var status = await client.Backup.GetBackupTaskStatusAsync("node1", taskId);
    
    Console.WriteLine($"Progress: {status.Progress}% - {status.Status}");
    
    if (status.IsCompleted)
    {
        Console.WriteLine(status.ExitStatus == "OK" ? "Backup completed successfully!" : "Backup failed!");
        break;
    }
    
    await Task.Delay(5000); // Wait 5 seconds before checking again
}
```

## User and Permission Management API

### User Management

```csharp
// Get all users
var users = await client.UserManagement.GetUsersAsync();

// Get specific user details
var user = await client.UserManagement.GetUserAsync("testuser@pam");

// Create a new user
var newUser = new CreateUserParameters
{
    UserId = "newuser@pam",
    Password = "securepassword123",
    Email = "newuser@example.com",
    FirstName = "New",
    LastName = "User",
    Groups = new[] { "administrators" },
    Enabled = true,
    Expire = DateTime.Now.AddYears(1)
};

await client.UserManagement.CreateUserAsync(newUser);

// Update user information
var updateParams = new UpdateUserParameters
{
    Email = "updated@example.com",
    FirstName = "Updated",
    Groups = new[] { "administrators", "operators" }
};

await client.UserManagement.UpdateUserAsync("newuser@pam", updateParams);

// Set user password
await client.UserManagement.SetUserPasswordAsync("newuser@pam", "newpassword123");

// Delete user
await client.UserManagement.DeleteUserAsync("newuser@pam");
```

### Group Management

```csharp
// Get all groups
var groups = await client.UserManagement.GetGroupsAsync();

// Get specific group details
var group = await client.UserManagement.GetGroupAsync("administrators");

// Create a new group
var newGroup = new CreateGroupParameters
{
    GroupId = "developers",
    Comment = "Development team members"
};

await client.UserManagement.CreateGroupAsync(newGroup);

// Update group
var updateGroupParams = new UpdateGroupParameters
{
    Comment = "Updated development team"
};

await client.UserManagement.UpdateGroupAsync("developers", updateGroupParams);

// Delete group
await client.UserManagement.DeleteGroupAsync("developers");
```

### Role Management

```csharp
// Get all roles
var roles = await client.UserManagement.GetRolesAsync();

// Get specific role details
var role = await client.UserManagement.GetRoleAsync("Administrator");

// Create a custom role
var newRole = new CreateRoleParameters
{
    RoleId = "VMManager",
    Privileges = new[] { "VM.Allocate", "VM.Config.Disk", "VM.Config.Memory", "VM.PowerMgmt" }
};

await client.UserManagement.CreateRoleAsync(newRole);

// Update role privileges
var updateRoleParams = new UpdateRoleParameters
{
    Privileges = new[] { "VM.Allocate", "VM.Config.Disk", "VM.Config.Memory", "VM.PowerMgmt", "VM.Monitor" }
};

await client.UserManagement.UpdateRoleAsync("VMManager", updateRoleParams);

// Delete role
await client.UserManagement.DeleteRoleAsync("VMManager");
```

### Access Control Lists (ACL)

```csharp
// Get all ACL entries
var aclEntries = await client.UserManagement.GetAclAsync();

// Create ACL entry for user
var userAcl = new CreateAclParameters
{
    Path = "/vms/100",
    Users = new[] { "vmuser@pam" },
    RoleId = "VMManager",
    Propagate = true
};

await client.UserManagement.CreateAclAsync(userAcl);

// Create ACL entry for group
var groupAcl = new CreateAclParameters
{
    Path = "/storage/local",
    Groups = new[] { "storage-admins" },
    RoleId = "Administrator",
    Propagate = false
};

await client.UserManagement.CreateAclAsync(groupAcl);

// Delete ACL entry
await client.UserManagement.DeleteAclAsync("/vms/100", "vmuser@pam", "VMManager");
```

### API Token Management

```csharp
// Get all API tokens for a user
var tokens = await client.UserManagement.GetApiTokensAsync("admin@pam");

// Create a new API token
var tokenParams = new CreateApiTokenParameters
{
    TokenId = "automation-token",
    Comment = "Token for automation scripts",
    Expire = DateTime.Now.AddMonths(6),
    Privileges = new[] { "VM.Allocate", "VM.PowerMgmt" }
};

var tokenInfo = await client.UserManagement.CreateApiTokenAsync("admin@pam", tokenParams);
Console.WriteLine($"Token Value: {tokenInfo.Value}"); // Store this securely!

// Update API token
var updateTokenParams = new UpdateApiTokenParameters
{
    Comment = "Updated automation token",
    Expire = DateTime.Now.AddMonths(12)
};

await client.UserManagement.UpdateApiTokenAsync("admin@pam", "automation-token", updateTokenParams);

// Delete API token
await client.UserManagement.DeleteApiTokenAsync("admin@pam", "automation-token");
```

### Permission Checking and Validation

```csharp
// Check if user has specific permission
var hasPermission = await client.UserManagement.CheckUserPermissionAsync(
    "vmuser@pam", "/vms/100", "VM.PowerMgmt");

if (hasPermission)
{
    Console.WriteLine("User can manage VM power state");
}

// Get effective permissions for user on path
var permissions = await client.UserManagement.GetUserPermissionsAsync("vmuser@pam", "/vms");

foreach (var permission in permissions)
{
    Console.WriteLine($"Permission: {permission} on path: /vms");
}

// Check user group membership
var userGroups = await client.UserManagement.GetUserGroupsAsync("vmuser@pam");

foreach (var group in userGroups)
{
    Console.WriteLine($"User is member of group: {group}");
}
```

### Security Best Practices

```csharp
// Example: Secure user creation with validation
public async Task<bool> CreateSecureUserAsync(string userId, string password, string[] groups)
{
    try
    {
        // Validate password strength
        if (password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters");
        }

        // Check if user already exists
        var existingUser = await client.UserManagement.GetUserAsync(userId);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User {userId} already exists");
        }

        // Validate groups exist
        var allGroups = await client.UserManagement.GetGroupsAsync();
        var validGroups = groups.Where(g => allGroups.Any(existing => existing.GroupId == g)).ToArray();

        if (validGroups.Length != groups.Length)
        {
            var invalidGroups = groups.Except(validGroups);
            throw new ArgumentException($"Invalid groups: {string.Join(", ", invalidGroups)}");
        }

        // Create user with validated parameters
        var userParams = new CreateUserParameters
        {
            UserId = userId,
            Password = password,
            Groups = validGroups,
            Enabled = true,
            Expire = DateTime.Now.AddYears(1) // Set reasonable expiration
        };

        await client.UserManagement.CreateUserAsync(userParams);
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to create user: {ex.Message}");
        return false;
    }
}
```

## Planned Features

- **Iteration 7**: Backup and restore operations
- **Iteration 8**: User and permission management  
- **Iteration 9**: Clustering support
- **Iteration 10**: Advanced monitoring and metrics
