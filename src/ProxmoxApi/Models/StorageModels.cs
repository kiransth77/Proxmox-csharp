using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProxmoxApi.Models;

/// <summary>
/// Represents a Proxmox storage configuration
/// </summary>
public class ProxmoxStorage
{
    /// <summary>
    /// Storage identifier
    /// </summary>
    [JsonPropertyName("storage")]
    public string Storage { get; set; } = string.Empty;

    /// <summary>
    /// Storage type (dir, lvm, nfs, ceph, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Storage path or target
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Content types supported (images, iso, vztmpl, backup, etc.)
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether storage is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether storage is shared across cluster
    /// </summary>
    [JsonPropertyName("shared")]
    public bool Shared { get; set; }

    /// <summary>
    /// List of nodes where storage is available
    /// </summary>
    [JsonPropertyName("nodes")]
    public string Nodes { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of backups to keep
    /// </summary>
    [JsonPropertyName("maxfiles")]
    public int? MaxFiles { get; set; }

    /// <summary>
    /// Storage options (varies by type)
    /// </summary>
    [JsonPropertyName("options")]
    public Dictionary<string, object> Options { get; set; } = new();
}

/// <summary>
/// Storage status and usage information
/// </summary>
public class StorageStatus
{
    /// <summary>
    /// Storage identifier
    /// </summary>
    [JsonPropertyName("storage")]
    public string Storage { get; set; } = string.Empty;

    /// <summary>
    /// Storage type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Total storage space in bytes
    /// </summary>
    [JsonPropertyName("total")]
    public long Total { get; set; }

    /// <summary>
    /// Used storage space in bytes
    /// </summary>
    [JsonPropertyName("used")]
    public long Used { get; set; }

    /// <summary>
    /// Available storage space in bytes
    /// </summary>
    [JsonPropertyName("avail")]
    public long Available { get; set; }

    /// <summary>
    /// Storage usage percentage (0-100)
    /// </summary>
    public double UsagePercentage => Total > 0 ? Math.Round((double)Used / Total * 100, 2) : 0;

    /// <summary>
    /// Whether storage is active
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    /// <summary>
    /// Content types available on this storage
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Storage-specific status information
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Represents content stored in Proxmox storage
/// </summary>
public class StorageContent
{
    /// <summary>
    /// Volume identifier
    /// </summary>
    [JsonPropertyName("volid")]
    public string VolumeId { get; set; } = string.Empty;

    /// <summary>
    /// Content type (images, iso, vztmpl, backup, etc.)
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Content format (raw, qcow2, vmdk, etc.)
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Size in bytes
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// Used space in bytes
    /// </summary>
    [JsonPropertyName("used")]
    public long Used { get; set; }

    /// <summary>
    /// Associated VM/Container ID
    /// </summary>
    [JsonPropertyName("vmid")]
    public int? VmId { get; set; }

    /// <summary>
    /// Creation time
    /// </summary>
    [JsonPropertyName("ctime")]
    public long? CreationTime { get; set; }

    /// <summary>
    /// Creation time as DateTime
    /// </summary>
    public DateTime? CreatedAt => CreationTime.HasValue
        ? DateTimeOffset.FromUnixTimeSeconds(CreationTime.Value).DateTime
        : null;

    /// <summary>
    /// Parent volume (for snapshots)
    /// </summary>
    [JsonPropertyName("parent")]
    public string Parent { get; set; } = string.Empty;

    /// <summary>
    /// Notes or description
    /// </summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Protection status (prevents deletion)
    /// </summary>
    [JsonPropertyName("protected")]
    public bool Protected { get; set; }
}

/// <summary>
/// Storage backup information
/// </summary>
public class StorageBackup
{
    /// <summary>
    /// Backup file name
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Backup type (vzdump, lxc, qemu)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// VM/Container ID that was backed up
    /// </summary>
    [JsonPropertyName("vmid")]
    public int VmId { get; set; }

    /// <summary>
    /// Backup creation time
    /// </summary>
    [JsonPropertyName("ctime")]
    public long CreationTime { get; set; }

    /// <summary>
    /// Creation time as DateTime
    /// </summary>
    public DateTime CreatedAt => DateTimeOffset.FromUnixTimeSeconds(CreationTime).DateTime;

    /// <summary>
    /// Backup file size in bytes
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>
    /// Backup format (tar, vma, etc.)
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Whether backup is encrypted
    /// </summary>
    [JsonPropertyName("encrypted")]
    public bool Encrypted { get; set; }

    /// <summary>
    /// Backup notes
    /// </summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Protection status
    /// </summary>
    [JsonPropertyName("protected")]
    public bool Protected { get; set; }

    /// <summary>
    /// Verification status
    /// </summary>
    [JsonPropertyName("verification")]
    public string Verification { get; set; } = string.Empty;
}

/// <summary>
/// Storage creation/configuration options
/// </summary>
public class StorageCreateOptions
{
    /// <summary>
    /// Storage identifier
    /// </summary>
    public string Storage { get; set; } = string.Empty;

    /// <summary>
    /// Storage type (dir, lvm, nfs, ceph, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Storage path or target
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Content types to store (comma-separated)
    /// </summary>
    public string Content { get; set; } = "images,iso,vztmpl,backup";

    /// <summary>
    /// Whether storage is shared
    /// </summary>
    public bool Shared { get; set; }

    /// <summary>
    /// Nodes where storage is available
    /// </summary>
    public string Nodes { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of backups
    /// </summary>
    public int? MaxFiles { get; set; }

    /// <summary>
    /// Storage-specific options
    /// </summary>
    public Dictionary<string, object> Options { get; set; } = new();
}

/// <summary>
/// Volume allocation options
/// </summary>
public class VolumeCreateOptions
{
    /// <summary>
    /// Volume identifier
    /// </summary>
    public string VolumeId { get; set; } = string.Empty;

    /// <summary>
    /// Volume size (e.g., "32G", "1024M")
    /// </summary>
    public string Size { get; set; } = string.Empty;

    /// <summary>
    /// Volume format (raw, qcow2, vmdk)
    /// </summary>
    public string Format { get; set; } = "raw";

    /// <summary>
    /// VM/Container ID
    /// </summary>
    public int? VmId { get; set; }

    /// <summary>
    /// Volume notes
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Backup job configuration
/// </summary>
public class BackupJob
{
    /// <summary>
    /// Job identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Job type (vzdump)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Target VMs/Containers (comma-separated IDs or 'all')
    /// </summary>
    [JsonPropertyName("vmid")]
    public string VmId { get; set; } = string.Empty;

    /// <summary>
    /// Target storage for backups
    /// </summary>
    [JsonPropertyName("storage")]
    public string Storage { get; set; } = string.Empty;

    /// <summary>
    /// Backup schedule (cron format)
    /// </summary>
    [JsonPropertyName("schedule")]
    public string Schedule { get; set; } = string.Empty;

    /// <summary>
    /// Backup mode (snapshot, suspend, stop)
    /// </summary>
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "snapshot";

    /// <summary>
    /// Compression method (0=none, 1=lzo, gzip, zstd)
    /// </summary>
    [JsonPropertyName("compress")]
    public string Compress { get; set; } = "zstd";

    /// <summary>
    /// Maximum number of backups to keep
    /// </summary>
    [JsonPropertyName("maxfiles")]
    public int MaxFiles { get; set; } = 3;

    /// <summary>
    /// Whether job is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Email notification settings
    /// </summary>
    [JsonPropertyName("mailto")]
    public string MailTo { get; set; } = string.Empty;

    /// <summary>
    /// Job comment/description
    /// </summary>
    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;
}
