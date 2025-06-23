using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProxmoxApi.Models;

/// <summary>
/// Represents a Proxmox node in the cluster
/// </summary>
public class ProxmoxNode
{
    /// <summary>
    /// Node name/identifier
    /// </summary>
    [JsonPropertyName("node")]
    public string Node { get; set; } = string.Empty;

    /// <summary>
    /// Node status (online, offline, unknown)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// CPU usage percentage (0-1)
    /// </summary>
    [JsonPropertyName("cpu")]
    public double? CpuUsage { get; set; }

    /// <summary>
    /// Maximum CPU count
    /// </summary>
    [JsonPropertyName("maxcpu")]
    public int? MaxCpu { get; set; }

    /// <summary>
    /// Used memory in bytes
    /// </summary>
    [JsonPropertyName("mem")]
    public long? MemoryUsed { get; set; }

    /// <summary>
    /// Maximum memory in bytes
    /// </summary>
    [JsonPropertyName("maxmem")]
    public long? MemoryTotal { get; set; }

    /// <summary>
    /// Used disk space in bytes
    /// </summary>
    [JsonPropertyName("disk")]
    public long? DiskUsed { get; set; }

    /// <summary>
    /// Maximum disk space in bytes
    /// </summary>
    [JsonPropertyName("maxdisk")]
    public long? DiskTotal { get; set; }

    /// <summary>
    /// Node uptime in seconds
    /// </summary>
    [JsonPropertyName("uptime")]
    public long? Uptime { get; set; }

    /// <summary>
    /// Node type (node, qemu, lxc, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Node level in hierarchy
    /// </summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// PVE version running on this node
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// SSL fingerprint
    /// </summary>
    [JsonPropertyName("ssl_fingerprint")]
    public string? SslFingerprint { get; set; }

    /// <summary>
    /// Gets memory usage percentage (0-1)
    /// </summary>
    public double? MemoryUsagePercentage =>
        MemoryUsed.HasValue && MemoryTotal.HasValue && MemoryTotal > 0
            ? (double)MemoryUsed.Value / MemoryTotal.Value
            : null;

    /// <summary>
    /// Gets disk usage percentage (0-1)
    /// </summary>
    public double? DiskUsagePercentage =>
        DiskUsed.HasValue && DiskTotal.HasValue && DiskTotal > 0
            ? (double)DiskUsed.Value / DiskTotal.Value
            : null;

    /// <summary>
    /// Gets uptime as TimeSpan
    /// </summary>
    public TimeSpan? UptimeSpan =>
        Uptime.HasValue ? TimeSpan.FromSeconds(Uptime.Value) : null;

    /// <summary>
    /// Indicates if the node is online
    /// </summary>
    public bool IsOnline => Status.Equals("online", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Detailed node status information
/// </summary>
public class NodeStatus
{
    /// <summary>
    /// Current time on the node
    /// </summary>
    [JsonPropertyName("time")]
    public long Time { get; set; }

    /// <summary>
    /// Node uptime in seconds
    /// </summary>
    [JsonPropertyName("uptime")]
    public long Uptime { get; set; }

    /// <summary>
    /// Load average values
    /// </summary>
    [JsonPropertyName("loadavg")]
    public double[]? LoadAverage { get; set; }

    /// <summary>
    /// CPU information
    /// </summary>
    [JsonPropertyName("cpuinfo")]
    public CpuInfo? CpuInfo { get; set; }

    /// <summary>
    /// Memory information
    /// </summary>
    [JsonPropertyName("memory")]
    public MemoryInfo? Memory { get; set; }

    /// <summary>
    /// Swap information
    /// </summary>
    [JsonPropertyName("swap")]
    public SwapInfo? Swap { get; set; }

    /// <summary>
    /// Root filesystem information
    /// </summary>
    [JsonPropertyName("rootfs")]
    public FilesystemInfo? RootFs { get; set; }

    /// <summary>
    /// PVE version
    /// </summary>
    [JsonPropertyName("pveversion")]
    public string? PveVersion { get; set; }

    /// <summary>
    /// Gets current time as DateTime
    /// </summary>
    public DateTime CurrentTime => DateTimeOffset.FromUnixTimeSeconds(Time).DateTime;

    /// <summary>
    /// Gets uptime as TimeSpan
    /// </summary>
    public TimeSpan UptimeSpan => TimeSpan.FromSeconds(Uptime);
}

/// <summary>
/// CPU information
/// </summary>
public class CpuInfo
{
    /// <summary>
    /// Number of CPU cores
    /// </summary>
    [JsonPropertyName("cpus")]
    public int Cpus { get; set; }

    /// <summary>
    /// CPU model name
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// CPU frequency in MHz
    /// </summary>
    [JsonPropertyName("mhz")]
    public double? Mhz { get; set; }

    /// <summary>
    /// CPU flags/features
    /// </summary>
    [JsonPropertyName("flags")]
    public string? Flags { get; set; }

    /// <summary>
    /// User CPU time
    /// </summary>
    [JsonPropertyName("user")]
    public double? User { get; set; }

    /// <summary>
    /// Nice CPU time
    /// </summary>
    [JsonPropertyName("nice")]
    public double? Nice { get; set; }

    /// <summary>
    /// System CPU time
    /// </summary>
    [JsonPropertyName("sys")]
    public double? System { get; set; }

    /// <summary>
    /// Idle CPU time
    /// </summary>
    [JsonPropertyName("idle")]
    public double? Idle { get; set; }

    /// <summary>
    /// IO wait time
    /// </summary>
    [JsonPropertyName("iowait")]
    public double? IoWait { get; set; }
}

/// <summary>
/// Memory information
/// </summary>
public class MemoryInfo
{
    /// <summary>
    /// Total memory in bytes
    /// </summary>
    [JsonPropertyName("total")]
    public long Total { get; set; }

    /// <summary>
    /// Used memory in bytes
    /// </summary>
    [JsonPropertyName("used")]
    public long Used { get; set; }

    /// <summary>
    /// Free memory in bytes
    /// </summary>
    [JsonPropertyName("free")]
    public long Free { get; set; }

    /// <summary>
    /// Gets memory usage percentage (0-1)
    /// </summary>
    public double UsagePercentage => Total > 0 ? (double)Used / Total : 0;
}

/// <summary>
/// Swap information
/// </summary>
public class SwapInfo
{
    /// <summary>
    /// Total swap in bytes
    /// </summary>
    [JsonPropertyName("total")]
    public long Total { get; set; }

    /// <summary>
    /// Used swap in bytes
    /// </summary>
    [JsonPropertyName("used")]
    public long Used { get; set; }

    /// <summary>
    /// Free swap in bytes
    /// </summary>
    [JsonPropertyName("free")]
    public long Free { get; set; }

    /// <summary>
    /// Gets swap usage percentage (0-1)
    /// </summary>
    public double UsagePercentage => Total > 0 ? (double)Used / Total : 0;
}

/// <summary>
/// Filesystem information
/// </summary>
public class FilesystemInfo
{
    /// <summary>
    /// Total space in bytes
    /// </summary>
    [JsonPropertyName("total")]
    public long Total { get; set; }

    /// <summary>
    /// Used space in bytes
    /// </summary>
    [JsonPropertyName("used")]
    public long Used { get; set; }

    /// <summary>
    /// Available space in bytes
    /// </summary>
    [JsonPropertyName("avail")]
    public long Available { get; set; }

    /// <summary>
    /// Gets usage percentage (0-1)
    /// </summary>
    public double UsagePercentage => Total > 0 ? (double)Used / Total : 0;
}
