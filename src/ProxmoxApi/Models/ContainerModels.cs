using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProxmoxApi.Models;

/// <summary>
/// Represents a Proxmox LXC Container
/// </summary>
public class ProxmoxContainer
{
    /// <summary>
    /// Container ID (unique identifier)
    /// </summary>
    [JsonPropertyName("vmid")]
    public int ContainerId { get; set; }

    /// <summary>
    /// Container name/hostname
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current container status (running, stopped, paused)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Node where the container is located
    /// </summary>
    [JsonPropertyName("node")]
    public string Node { get; set; } = string.Empty;

    /// <summary>
    /// Allocated memory in MB
    /// </summary>
    [JsonPropertyName("maxmem")]
    public long Memory { get; set; }

    /// <summary>
    /// Number of CPU cores
    /// </summary>
    [JsonPropertyName("cpus")]
    public int Cores { get; set; }

    /// <summary>
    /// Container operating system template
    /// </summary>
    [JsonPropertyName("ostype")]
    public string OsType { get; set; } = string.Empty;

    /// <summary>
    /// Container uptime in seconds
    /// </summary>
    [JsonPropertyName("uptime")]
    public long? Uptime { get; set; }

    /// <summary>
    /// Container configuration dictionary
    /// </summary>
    public Dictionary<string, object> Config { get; set; } = new();

    /// <summary>
    /// Container description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Container template flag
    /// </summary>
    [JsonPropertyName("template")]
    public bool IsTemplate { get; set; }

    /// <summary>
    /// Container tags
    /// </summary>
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }

    /// <summary>
    /// Container privilege level
    /// </summary>
    [JsonPropertyName("unprivileged")]
    public bool IsUnprivileged { get; set; }

    /// <summary>
    /// Container architecture
    /// </summary>
    [JsonPropertyName("arch")]
    public string Architecture { get; set; } = "amd64";

    /// <summary>
    /// Container lock status
    /// </summary>
    [JsonPropertyName("lock")]
    public string? Lock { get; set; }

    /// <summary>
    /// Container root filesystem size in GB
    /// </summary>
    [JsonPropertyName("maxdisk")]
    public long? MaxDisk { get; set; }
}

/// <summary>
/// Container resource statistics
/// </summary>
public class ContainerStatistics
{
    /// <summary>
    /// CPU usage percentage (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("cpu")]
    public double CpuUsage { get; set; }

    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    [JsonPropertyName("mem")]
    public long MemoryUsage { get; set; }

    /// <summary>
    /// Maximum available memory in bytes
    /// </summary>
    [JsonPropertyName("maxmem")]
    public long MaxMemory { get; set; }

    /// <summary>
    /// Network input in bytes
    /// </summary>
    [JsonPropertyName("netin")]
    public long NetworkIn { get; set; }

    /// <summary>
    /// Network output in bytes
    /// </summary>
    [JsonPropertyName("netout")]
    public long NetworkOut { get; set; }

    /// <summary>
    /// Disk read bytes
    /// </summary>
    [JsonPropertyName("diskread")]
    public long DiskRead { get; set; }

    /// <summary>
    /// Disk write bytes
    /// </summary>
    [JsonPropertyName("diskwrite")]
    public long DiskWrite { get; set; }

    /// <summary>
    /// Container uptime in seconds
    /// </summary>
    [JsonPropertyName("uptime")]
    public long? Uptime { get; set; }

    /// <summary>
    /// Used disk space in bytes
    /// </summary>
    [JsonPropertyName("disk")]
    public long DiskUsage { get; set; }

    /// <summary>
    /// Maximum disk space in bytes
    /// </summary>
    [JsonPropertyName("maxdisk")]
    public long MaxDisk { get; set; }
}

/// <summary>
/// Container status information
/// </summary>
public class ContainerStatus
{
    /// <summary>
    /// Current status (running, stopped, etc.)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Container uptime in seconds
    /// </summary>
    [JsonPropertyName("uptime")]
    public long? Uptime { get; set; }

    /// <summary>
    /// Process ID of the container
    /// </summary>
    [JsonPropertyName("pid")]
    public int? ProcessId { get; set; }

    /// <summary>
    /// CPU usage percentage
    /// </summary>
    [JsonPropertyName("cpu")]
    public double? CpuUsage { get; set; }

    /// <summary>
    /// Current memory usage in bytes
    /// </summary>
    [JsonPropertyName("mem")]
    public long? MemoryUsage { get; set; }

    /// <summary>
    /// Maximum memory in bytes
    /// </summary>
    [JsonPropertyName("maxmem")]
    public long? MaxMemory { get; set; }

    /// <summary>
    /// Current disk usage in bytes
    /// </summary>
    [JsonPropertyName("disk")]
    public long? DiskUsage { get; set; }

    /// <summary>
    /// Maximum disk space in bytes
    /// </summary>
    [JsonPropertyName("maxdisk")]
    public long? MaxDisk { get; set; }    /// <summary>
                                          /// Ha status information
                                          /// </summary>
    [JsonPropertyName("ha")]
    public Dictionary<string, object>? Ha { get; set; }

    /// <summary>
    /// Container lock information
    /// </summary>
    [JsonPropertyName("lock")]
    public string? Lock { get; set; }
}

/// <summary>
/// Container configuration details
/// </summary>
public class ContainerConfig
{
    /// <summary>
    /// Container architecture
    /// </summary>
    [JsonPropertyName("arch")]
    public string Architecture { get; set; } = "amd64";

    /// <summary>
    /// Container cores
    /// </summary>
    [JsonPropertyName("cores")]
    public int? Cores { get; set; }

    /// <summary>
    /// CPU limit
    /// </summary>
    [JsonPropertyName("cpulimit")]
    public double? CpuLimit { get; set; }

    /// <summary>
    /// CPU units (weight)
    /// </summary>
    [JsonPropertyName("cpuunits")]
    public int? CpuUnits { get; set; }

    /// <summary>
    /// Container description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Container hostname
    /// </summary>
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    /// <summary>
    /// Memory limit in MB
    /// </summary>
    [JsonPropertyName("memory")]
    public int? Memory { get; set; }

    /// <summary>
    /// Network interfaces
    /// </summary>
    [JsonPropertyName("net0")]
    public string? Network0 { get; set; }

    /// <summary>
    /// Operating system type
    /// </summary>
    [JsonPropertyName("ostype")]
    public string? OsType { get; set; }

    /// <summary>
    /// Root filesystem configuration
    /// </summary>
    [JsonPropertyName("rootfs")]
    public string? RootFs { get; set; }

    /// <summary>
    /// Swap size in MB
    /// </summary>
    [JsonPropertyName("swap")]
    public int? Swap { get; set; }

    /// <summary>
    /// Container tags
    /// </summary>
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }

    /// <summary>
    /// Template flag
    /// </summary>
    [JsonPropertyName("template")]
    public bool IsTemplate { get; set; }

    /// <summary>
    /// Unprivileged container flag
    /// </summary>
    [JsonPropertyName("unprivileged")]
    public bool IsUnprivileged { get; set; }

    /// <summary>
    /// Additional mount points
    /// </summary>
    public Dictionary<string, string> MountPoints { get; set; } = new();

    /// <summary>
    /// Environment variables
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new();

    /// <summary>
    /// Additional network interfaces
    /// </summary>
    public Dictionary<string, string> NetworkInterfaces { get; set; } = new();
}

/// <summary>
/// Container creation parameters
/// </summary>
public class ContainerCreateParameters
{
    /// <summary>
    /// Container ID (required)
    /// </summary>
    public int ContainerId { get; set; }

    /// <summary>
    /// OS template (required)
    /// </summary>
    public string OsTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Container hostname (optional)
    /// </summary>
    public string? Hostname { get; set; }

    /// <summary>
    /// Container description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Memory in MB (default: 512)
    /// </summary>
    public int Memory { get; set; } = 512;

    /// <summary>
    /// CPU cores (default: 1)
    /// </summary>
    public int Cores { get; set; } = 1;

    /// <summary>
    /// Root filesystem storage and size (default: "local-lvm:8")
    /// </summary>
    public string RootFs { get; set; } = "local-lvm:8";

    /// <summary>
    /// Network configuration (default: "name=eth0,bridge=vmbr0,dhcp=1")
    /// </summary>
    public string Network { get; set; } = "name=eth0,bridge=vmbr0,dhcp=1";

    /// <summary>
    /// Swap size in MB (default: 512)
    /// </summary>
    public int Swap { get; set; } = 512;

    /// <summary>
    /// Create unprivileged container (default: true)
    /// </summary>
    public bool IsUnprivileged { get; set; } = true;

    /// <summary>
    /// Container architecture (default: amd64)
    /// </summary>
    public string Architecture { get; set; } = "amd64";

    /// <summary>
    /// Start container after creation
    /// </summary>
    public bool StartAfterCreate { get; set; } = false;

    /// <summary>
    /// Container tags
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Password for root user
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// SSH public key for root user
    /// </summary>
    public string? SshPublicKeys { get; set; }

    /// <summary>
    /// Additional mount points
    /// </summary>
    public Dictionary<string, string> MountPoints { get; set; } = new();

    /// <summary>
    /// Additional network interfaces
    /// </summary>
    public Dictionary<string, string> AdditionalNetworks { get; set; } = new();
}

/// <summary>
/// Container snapshot information
/// </summary>
public class ContainerSnapshot
{
    /// <summary>
    /// Snapshot name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Snapshot description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Snapshot creation time
    /// </summary>
    [JsonPropertyName("snaptime")]
    public long? SnapTime { get; set; }

    /// <summary>
    /// Whether the snapshot includes VM state
    /// </summary>
    [JsonPropertyName("vmstate")]
    public bool IncludesVmState { get; set; }

    /// <summary>
    /// Snapshot creation time as DateTime
    /// </summary>
    public DateTime? CreatedAt => SnapTime.HasValue ? DateTimeOffset.FromUnixTimeSeconds(SnapTime.Value).DateTime : null;

    /// <summary>
    /// Parent snapshot name
    /// </summary>
    [JsonPropertyName("parent")]
    public string? Parent { get; set; }
}

/// <summary>
/// Container clone parameters
/// </summary>
public class ContainerCloneParameters
{
    /// <summary>
    /// New container ID (required)
    /// </summary>
    public int NewContainerId { get; set; }

    /// <summary>
    /// New container hostname (optional)
    /// </summary>
    public string? Hostname { get; set; }

    /// <summary>
    /// Description for the new container
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Target node for the clone (optional, defaults to source node)
    /// </summary>
    public string? TargetNode { get; set; }

    /// <summary>
    /// Create a full clone (default: false for linked clone)
    /// </summary>
    public bool FullClone { get; set; } = false;

    /// <summary>
    /// Target storage for full clone
    /// </summary>
    public string? TargetStorage { get; set; }

    /// <summary>
    /// Snapshot to clone from (optional)
    /// </summary>
    public string? SnapName { get; set; }
}

/// <summary>
/// Container migration parameters
/// </summary>
public class ContainerMigrationParameters
{
    /// <summary>
    /// Target node for migration (required)
    /// </summary>
    public string TargetNode { get; set; } = string.Empty;

    /// <summary>
    /// Perform online migration (default: false)
    /// </summary>
    public bool Online { get; set; } = false;

    /// <summary>
    /// Restart container after migration
    /// </summary>
    public bool Restart { get; set; } = false;

    /// <summary>
    /// Timeout for migration in seconds
    /// </summary>
    public int? Timeout { get; set; }

    /// <summary>
    /// Target storage for migration
    /// </summary>
    public string? TargetStorage { get; set; }

    /// <summary>
    /// Use bandwidth limit for migration (in KB/s)
    /// </summary>
    public int? BandwidthLimit { get; set; }
}
