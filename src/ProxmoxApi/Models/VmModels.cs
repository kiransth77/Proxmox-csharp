using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProxmoxApi.Models;

/// <summary>
/// Represents a Proxmox Virtual Machine
/// </summary>
public class ProxmoxVm
{
    /// <summary>
    /// VM ID (unique identifier)
    /// </summary>
    [JsonPropertyName("vmid")]
    public int VmId { get; set; }

    /// <summary>
    /// VM name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current VM status (running, stopped, paused)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Node where the VM is located
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
    /// Operating system type
    /// </summary>
    [JsonPropertyName("ostype")]
    public string OsType { get; set; } = string.Empty;

    /// <summary>
    /// VM uptime in seconds
    /// </summary>
    [JsonPropertyName("uptime")]
    public long? Uptime { get; set; }

    /// <summary>
    /// VM configuration dictionary
    /// </summary>
    public Dictionary<string, object> Config { get; set; } = new();

    /// <summary>
    /// VM description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// VM template flag
    /// </summary>
    [JsonPropertyName("template")]
    public bool IsTemplate { get; set; }

    /// <summary>
    /// VM tags
    /// </summary>
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }
}

/// <summary>
/// VM resource statistics
/// </summary>
public class VmStatistics
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
    /// Maximum memory in bytes
    /// </summary>
    [JsonPropertyName("maxmem")]
    public long MaxMemory { get; set; }

    /// <summary>
    /// Disk read bytes per second
    /// </summary>
    [JsonPropertyName("diskread")]
    public long DiskRead { get; set; }

    /// <summary>
    /// Disk write bytes per second
    /// </summary>
    [JsonPropertyName("diskwrite")]
    public long DiskWrite { get; set; }

    /// <summary>
    /// Network input bytes per second
    /// </summary>
    [JsonPropertyName("netin")]
    public long NetworkIn { get; set; }

    /// <summary>
    /// Network output bytes per second
    /// </summary>
    [JsonPropertyName("netout")]
    public long NetworkOut { get; set; }

    /// <summary>
    /// VM uptime in seconds
    /// </summary>
    [JsonPropertyName("uptime")]
    public long? Uptime { get; set; }

    /// <summary>
    /// Memory usage percentage
    /// </summary>
    public double MemoryUsagePercentage => MaxMemory > 0 ? (double)MemoryUsage / MaxMemory : 0.0;
}

/// <summary>
/// VM snapshot information
/// </summary>
public class VmSnapshot
{
    /// <summary>
    /// Snapshot name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Snapshot creation time
    /// </summary>
    [JsonPropertyName("snaptime")]
    public long SnapTime { get; set; }

    /// <summary>
    /// Snapshot creation date
    /// </summary>
    public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(SnapTime).DateTime;

    /// <summary>
    /// Snapshot description
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this is the current snapshot
    /// </summary>
    [JsonPropertyName("current")]
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Parent snapshot name
    /// </summary>
    [JsonPropertyName("parent")]
    public string? Parent { get; set; }
}

/// <summary>
/// VM hook script configuration
/// </summary>
public class VmHookScript
{
    /// <summary>
    /// Hook type (pre-start, post-start, pre-stop, post-stop)
    /// </summary>
    public string HookType { get; set; } = string.Empty;

    /// <summary>
    /// Script path in Proxmox storage
    /// </summary>
    public string ScriptPath { get; set; } = string.Empty;

    /// <summary>
    /// Script content
    /// </summary>
    public string ScriptContent { get; set; } = string.Empty;

    /// <summary>
    /// Whether the hook script is enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Script creation date
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Last modification date
    /// </summary>
    public DateTime? LastModified { get; set; }
}

/// <summary>
/// VM hook script execution result
/// </summary>
public class VmHookExecution
{
    /// <summary>
    /// VM ID
    /// </summary>
    public string VmId { get; set; } = string.Empty;

    /// <summary>
    /// Hook type that was executed
    /// </summary>
    public string HookType { get; set; } = string.Empty;

    /// <summary>
    /// Execution start time
    /// </summary>
    public DateTime ExecutionTime { get; set; }

    /// <summary>
    /// Exit code
    /// </summary>
    public int ExitCode { get; set; }

    /// <summary>
    /// Standard output
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// Error output
    /// </summary>
    public string ErrorOutput { get; set; } = string.Empty;

    /// <summary>
    /// Execution duration
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Whether execution was successful
    /// </summary>
    public bool IsSuccess => ExitCode == 0;
}

/// <summary>
/// VM status information
/// </summary>
public class VmStatus
{
    /// <summary>
    /// VM ID
    /// </summary>
    [JsonPropertyName("vmid")]
    public int VmId { get; set; }

    /// <summary>
    /// Current status
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Whether VM is running
    /// </summary>
    public bool IsRunning => Status.Equals("running", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether VM is stopped
    /// </summary>
    public bool IsStopped => Status.Equals("stopped", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether VM is paused
    /// </summary>
    public bool IsPaused => Status.Equals("paused", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Process ID (when running)
    /// </summary>
    [JsonPropertyName("pid")]
    public int? ProcessId { get; set; }

    /// <summary>
    /// CPU usage percentage
    /// </summary>
    [JsonPropertyName("cpu")]
    public double? CpuUsage { get; set; }

    /// <summary>
    /// Memory usage in bytes
    /// </summary>
    [JsonPropertyName("mem")]
    public long? MemoryUsage { get; set; }

    /// <summary>
    /// Maximum memory in bytes
    /// </summary>
    [JsonPropertyName("maxmem")]
    public long? MaxMemory { get; set; }

    /// <summary>
    /// Uptime in seconds
    /// </summary>
    [JsonPropertyName("uptime")]
    public long? Uptime { get; set; }
}

/// <summary>
/// VM creation/configuration parameters
/// </summary>
public class VmCreateOptions
{
    /// <summary>
    /// VM ID (required)
    /// </summary>
    public int VmId { get; set; }

    /// <summary>
    /// VM name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Memory in MB
    /// </summary>
    public int? Memory { get; set; }

    /// <summary>
    /// Number of cores
    /// </summary>
    public int? Cores { get; set; }

    /// <summary>
    /// Number of sockets
    /// </summary>
    public int? Sockets { get; set; } = 1;

    /// <summary>
    /// Operating system type
    /// </summary>
    public string? OsType { get; set; }

    /// <summary>
    /// Boot order
    /// </summary>
    public string? Boot { get; set; }

    /// <summary>
    /// CDROM/ISO
    /// </summary>
    public string? CdRom { get; set; }

    /// <summary>
    /// Network configuration
    /// </summary>
    public string? Net0 { get; set; }

    /// <summary>
    /// Primary disk configuration
    /// </summary>
    public string? Scsi0 { get; set; }

    /// <summary>
    /// VM description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Start VM after creation
    /// </summary>
    public bool StartAfterCreate { get; set; }
}
