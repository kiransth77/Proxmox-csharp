using System;
using System.Text.Json.Serialization;

namespace ProxmoxApi.Models;

/// <summary>
/// Represents a backup job configuration
/// </summary>
public class BackupJob
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("schedule")]
    public string Schedule { get; set; } = string.Empty;

    [JsonPropertyName("storage")]
    public string Storage { get; set; } = string.Empty;

    [JsonPropertyName("node")]
    public string Node { get; set; } = string.Empty;

    [JsonPropertyName("vmid")]
    public string VmIds { get; set; } = string.Empty;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("compress")]
    public string Compress { get; set; } = "zstd";

    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "snapshot";

    [JsonPropertyName("remove")]
    public int Remove { get; set; } = 1;

    [JsonPropertyName("exclude")]
    public string Exclude { get; set; } = string.Empty;

    [JsonPropertyName("exclude-path")]
    public string ExcludePath { get; set; } = string.Empty;

    [JsonPropertyName("mailto")]
    public string MailTo { get; set; } = string.Empty;

    [JsonPropertyName("mailnotification")]
    public string MailNotification { get; set; } = "always";

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("dow")]
    public string DaysOfWeek { get; set; } = string.Empty;

    [JsonPropertyName("starttime")]
    public string StartTime { get; set; } = "02:00";

    [JsonPropertyName("maxfiles")]
    public int MaxFiles { get; set; } = 1;

    [JsonPropertyName("bwlimit")]
    public int BandwidthLimit { get; set; } = 0;

    [JsonPropertyName("pigz")]
    public int Pigz { get; set; } = 0;

    [JsonPropertyName("lockwait")]
    public int LockWait { get; set; } = 180;

    [JsonPropertyName("stopwait")]
    public int StopWait { get; set; } = 10;

    [JsonPropertyName("tmpdir")]
    public string TempDir { get; set; } = string.Empty;

    [JsonPropertyName("dumpdir")]
    public string DumpDir { get; set; } = string.Empty;

    [JsonPropertyName("script")]
    public string Script { get; set; } = string.Empty;

    [JsonPropertyName("protected")]
    public bool Protected { get; set; } = false;

    [JsonPropertyName("prune-backups")]
    public string PruneBackups { get; set; } = string.Empty;
}

/// <summary>
/// Represents a backup file/archive
/// </summary>
public class BackupFile
{
    [JsonPropertyName("volid")]
    public string VolumeId { get; set; } = string.Empty;

    [JsonPropertyName("node")]
    public string Node { get; set; } = string.Empty;

    [JsonPropertyName("storage")]
    public string Storage { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("ctime")]
    public long CreationTime { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("used")]
    public long Used { get; set; }

    [JsonPropertyName("vmid")]
    public int VmId { get; set; }

    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("protected")]
    public bool Protected { get; set; } = false;

    [JsonPropertyName("verification")]
    public string Verification { get; set; } = string.Empty;

    [JsonPropertyName("encrypted")]
    public bool Encrypted { get; set; } = false;

    [JsonPropertyName("parent")]
    public string Parent { get; set; } = string.Empty;

    /// <summary>
    /// Formatted creation time
    /// </summary>
    public DateTime CreationDateTime => DateTimeOffset.FromUnixTimeSeconds(CreationTime).DateTime;

    /// <summary>
    /// Formatted file size
    /// </summary>
    public string FormattedSize
    {
        get
        {
            if (Size == 0) return "0 B";
            
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            int unit = 0;
            double size = Size;
            
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }
            
            return $"{size:F1} {units[unit]}";
        }
    }
}

/// <summary>
/// Represents a backup task/job execution
/// </summary>
public class BackupTask
{
    [JsonPropertyName("upid")]
    public string TaskId { get; set; } = string.Empty;

    [JsonPropertyName("node")]
    public string Node { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public string User { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("starttime")]
    public long StartTime { get; set; }

    [JsonPropertyName("endtime")]
    public long EndTime { get; set; }

    [JsonPropertyName("pid")]
    public int ProcessId { get; set; }

    [JsonPropertyName("exitstatus")]
    public string ExitStatus { get; set; } = string.Empty;

    [JsonPropertyName("worker")]
    public string Worker { get; set; } = string.Empty;

    [JsonPropertyName("pstart")]
    public long PStart { get; set; }

    /// <summary>
    /// Formatted start time
    /// </summary>
    public DateTime StartDateTime => DateTimeOffset.FromUnixTimeSeconds(StartTime).DateTime;

    /// <summary>
    /// Formatted end time
    /// </summary>
    public DateTime? EndDateTime => EndTime > 0 ? DateTimeOffset.FromUnixTimeSeconds(EndTime).DateTime : null;

    /// <summary>
    /// Task duration
    /// </summary>
    public TimeSpan? Duration => EndDateTime.HasValue ? EndDateTime.Value - StartDateTime : null;

    /// <summary>
    /// Check if task is running
    /// </summary>
    public bool IsRunning => Status == "running";

    /// <summary>
    /// Check if task completed successfully
    /// </summary>
    public bool IsSuccessful => Status == "stopped" && (ExitStatus == "OK" || string.IsNullOrEmpty(ExitStatus));
}

/// <summary>
/// Represents backup job creation/update parameters
/// </summary>
public class BackupJobParameters
{
    public string? Schedule { get; set; }
    public string? Storage { get; set; }
    public string? Node { get; set; }
    public string? VmIds { get; set; }
    public bool? Enabled { get; set; }
    public string? Compress { get; set; }
    public string? Mode { get; set; }
    public int? Remove { get; set; }
    public string? Exclude { get; set; }
    public string? ExcludePath { get; set; }
    public string? MailTo { get; set; }
    public string? MailNotification { get; set; }
    public string? Comment { get; set; }
    public string? DaysOfWeek { get; set; }
    public string? StartTime { get; set; }
    public int? MaxFiles { get; set; }
    public int? BandwidthLimit { get; set; }
    public int? Pigz { get; set; }
    public int? LockWait { get; set; }
    public int? StopWait { get; set; }
    public string? TempDir { get; set; }
    public string? DumpDir { get; set; }
    public string? Script { get; set; }
    public bool? Protected { get; set; }
    public string? PruneBackups { get; set; }
}

/// <summary>
/// Represents backup restore parameters
/// </summary>
public class RestoreParameters
{
    public string? Archive { get; set; }
    public string? Storage { get; set; }
    public int? UniqueId { get; set; }
    public bool? Force { get; set; }
    public string? Pool { get; set; }
    public bool? Start { get; set; }
    public string? Bandwidth { get; set; }
    public bool? LiveRestore { get; set; }
    public string? Privileged { get; set; }
    public string? Cores { get; set; }
    public string? Memory { get; set; }
    public string? Net { get; set; }
    public string? Virtio { get; set; }
    public string? Ide { get; set; }
    public string? Sata { get; set; }
    public string? Scsi { get; set; }
}
