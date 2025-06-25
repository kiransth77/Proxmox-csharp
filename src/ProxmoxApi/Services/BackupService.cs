using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Core;
using ProxmoxApi.Models;

namespace ProxmoxApi.Services;

/// <summary>
/// Service for managing Proxmox VE backups and restore operations
/// </summary>
public class BackupService
{
    private readonly IProxmoxHttpClient _httpClient;
    private readonly ILogger<BackupService> _logger;

    public BackupService(IProxmoxHttpClient httpClient, ILogger<BackupService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Backup Jobs Management

    /// <summary>
    /// Get all backup jobs
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of backup jobs</returns>
    public async Task<List<BackupJob>> GetBackupJobsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting all backup jobs");

        try
        {
            var response = await _httpClient.GetAsync<ProxmoxApiResponse<List<BackupJob>>>(
                "/cluster/backup", cancellationToken);

            return response?.Data ?? new List<BackupJob>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get backup jobs");
            throw;
        }
    }

    /// <summary>
    /// Get a specific backup job by ID
    /// </summary>
    /// <param name="jobId">Backup job ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Backup job details</returns>
    public async Task<BackupJob?> GetBackupJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);
        
        _logger.LogInformation("Getting backup job {JobId}", jobId);

        try
        {
            var response = await _httpClient.GetAsync<ProxmoxApiResponse<BackupJob>>(
                $"/cluster/backup/{jobId}", cancellationToken);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get backup job {JobId}", jobId);
            throw;
        }
    }

    /// <summary>
    /// Create a new backup job
    /// </summary>
    /// <param name="parameters">Backup job parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task<string> CreateBackupJobAsync(BackupJobParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        
        _logger.LogInformation("Creating backup job");

        try
        {
            var formData = new Dictionary<string, string>();
            
            if (!string.IsNullOrEmpty(parameters.Schedule))
                formData["schedule"] = parameters.Schedule;
            if (!string.IsNullOrEmpty(parameters.Storage))
                formData["storage"] = parameters.Storage;
            if (!string.IsNullOrEmpty(parameters.Node))
                formData["node"] = parameters.Node;
            if (!string.IsNullOrEmpty(parameters.VmIds))
                formData["vmid"] = parameters.VmIds;
            if (parameters.Enabled.HasValue)
                formData["enabled"] = parameters.Enabled.Value ? "1" : "0";
            if (!string.IsNullOrEmpty(parameters.Compress))
                formData["compress"] = parameters.Compress;
            if (!string.IsNullOrEmpty(parameters.Mode))
                formData["mode"] = parameters.Mode;
            if (parameters.Remove.HasValue)
                formData["remove"] = parameters.Remove.Value.ToString();
            if (!string.IsNullOrEmpty(parameters.Comment))
                formData["comment"] = parameters.Comment;
            if (!string.IsNullOrEmpty(parameters.StartTime))
                formData["starttime"] = parameters.StartTime;
            if (parameters.MaxFiles.HasValue)
                formData["maxfiles"] = parameters.MaxFiles.Value.ToString();

            var response = await _httpClient.PostAsync<ProxmoxApiResponse<object>>(
                "/cluster/backup", formData, cancellationToken);

            _logger.LogInformation("Backup job created successfully");
            return "success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup job");
            throw;
        }
    }

    /// <summary>
    /// Update an existing backup job
    /// </summary>
    /// <param name="jobId">Backup job ID</param>
    /// <param name="parameters">Updated parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task UpdateBackupJobAsync(string jobId, BackupJobParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);
        ArgumentNullException.ThrowIfNull(parameters);
        
        _logger.LogInformation("Updating backup job {JobId}", jobId);

        try
        {
            var formData = new Dictionary<string, string>();
            
            if (!string.IsNullOrEmpty(parameters.Schedule))
                formData["schedule"] = parameters.Schedule;
            if (!string.IsNullOrEmpty(parameters.Storage))
                formData["storage"] = parameters.Storage;
            if (!string.IsNullOrEmpty(parameters.VmIds))
                formData["vmid"] = parameters.VmIds;
            if (parameters.Enabled.HasValue)
                formData["enabled"] = parameters.Enabled.Value ? "1" : "0";
            if (!string.IsNullOrEmpty(parameters.Comment))
                formData["comment"] = parameters.Comment;

            await _httpClient.PutAsync<ProxmoxApiResponse<object>>(
                $"/cluster/backup/{jobId}", formData, cancellationToken);

            _logger.LogInformation("Backup job {JobId} updated successfully", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update backup job {JobId}", jobId);
            throw;
        }
    }

    /// <summary>
    /// Delete a backup job
    /// </summary>
    /// <param name="jobId">Backup job ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task DeleteBackupJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);
        
        _logger.LogInformation("Deleting backup job {JobId}", jobId);

        try
        {
            await _httpClient.DeleteAsync<object>($"/cluster/backup/{jobId}", cancellationToken);
            
            _logger.LogInformation("Backup job {JobId} deleted successfully", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete backup job {JobId}", jobId);
            throw;
        }
    }

    #endregion

    #region Backup Files Management

    /// <summary>
    /// Get all backup files on a node
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="storage">Storage name (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of backup files</returns>
    public async Task<List<BackupFile>> GetBackupFilesAsync(string node, string? storage = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        
        _logger.LogInformation("Getting backup files on node {Node}", node);

        try
        {
            var url = $"/nodes/{node}/storage";
            if (!string.IsNullOrEmpty(storage))
                url += $"/{storage}/content?content=backup";
            else
                url += "?content=backup";

            var response = await _httpClient.GetAsync<ProxmoxApiResponse<List<BackupFile>>>(url, cancellationToken);

            return response?.Data ?? new List<BackupFile>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get backup files on node {Node}", node);
            throw;
        }
    }

    /// <summary>
    /// Get backup files for a specific VM/Container
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="vmId">VM/Container ID</param>
    /// <param name="storage">Storage name (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of backup files for the VM</returns>
    public async Task<List<BackupFile>> GetVmBackupFilesAsync(string node, int vmId, string? storage = null, CancellationToken cancellationToken = default)
    {
        var allBackups = await GetBackupFilesAsync(node, storage, cancellationToken);
        return allBackups.Where(b => b.VmId == vmId).ToList();
    }

    /// <summary>
    /// Delete a backup file
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="storage">Storage name</param>
    /// <param name="volumeId">Volume ID of the backup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task DeleteBackupFileAsync(string node, string storage, string volumeId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(storage);
        ArgumentException.ThrowIfNullOrWhiteSpace(volumeId);
        
        _logger.LogInformation("Deleting backup file {VolumeId} on node {Node} storage {Storage}", volumeId, node, storage);

        try
        {
            await _httpClient.DeleteAsync<object>($"/nodes/{node}/storage/{storage}/content/{volumeId}", cancellationToken);
            
            _logger.LogInformation("Backup file {VolumeId} deleted successfully", volumeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete backup file {VolumeId}", volumeId);
            throw;
        }
    }

    #endregion

    #region Backup Operations

    /// <summary>
    /// Create a backup of a VM
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="storage">Storage name</param>
    /// <param name="mode">Backup mode (stop, suspend, snapshot)</param>
    /// <param name="compress">Compression type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID</returns>
    public async Task<string> CreateVmBackupAsync(string node, int vmId, string storage, 
        string mode = "snapshot", string compress = "zstd", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(storage);
        
        _logger.LogInformation("Creating backup for VM {VmId} on node {Node}", vmId, node);

        try
        {
            var formData = new Dictionary<string, string>
            {
                ["storage"] = storage,
                ["mode"] = mode,
                ["compress"] = compress
            };

            var response = await _httpClient.PostAsync<ProxmoxApiResponse<string>>(
                $"/nodes/{node}/qemu/{vmId}/backup", formData, cancellationToken);

            var taskId = response?.Data ?? string.Empty;
            _logger.LogInformation("VM backup task {TaskId} started for VM {VmId}", taskId, vmId);
            
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup for VM {VmId}", vmId);
            throw;
        }
    }

    /// <summary>
    /// Create a backup of a Container
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="storage">Storage name</param>
    /// <param name="mode">Backup mode (stop, suspend, snapshot)</param>
    /// <param name="compress">Compression type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID</returns>
    public async Task<string> CreateContainerBackupAsync(string node, int containerId, string storage,
        string mode = "snapshot", string compress = "zstd", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(storage);
        
        _logger.LogInformation("Creating backup for Container {ContainerId} on node {Node}", containerId, node);

        try
        {
            var formData = new Dictionary<string, string>
            {
                ["storage"] = storage,
                ["mode"] = mode,
                ["compress"] = compress
            };

            var response = await _httpClient.PostAsync<ProxmoxApiResponse<string>>(
                $"/nodes/{node}/lxc/{containerId}/backup", formData, cancellationToken);

            var taskId = response?.Data ?? string.Empty;
            _logger.LogInformation("Container backup task {TaskId} started for Container {ContainerId}", taskId, containerId);
            
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup for Container {ContainerId}", containerId);
            throw;
        }
    }

    #endregion

    #region Restore Operations

    /// <summary>
    /// Restore a VM from backup
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="vmId">New VM ID</param>
    /// <param name="archive">Backup archive path</param>
    /// <param name="parameters">Restore parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID</returns>
    public async Task<string> RestoreVmAsync(string node, int vmId, string archive, 
        RestoreParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(archive);
        
        _logger.LogInformation("Restoring VM {VmId} from backup {Archive} on node {Node}", vmId, archive, node);

        try
        {
            var formData = new Dictionary<string, string>
            {
                ["vmid"] = vmId.ToString(),
                ["archive"] = archive
            };

            if (parameters != null)
            {
                if (!string.IsNullOrEmpty(parameters.Storage))
                    formData["storage"] = parameters.Storage;
                if (parameters.Force.HasValue)
                    formData["force"] = parameters.Force.Value ? "1" : "0";
                if (parameters.Start.HasValue)
                    formData["start"] = parameters.Start.Value ? "1" : "0";
            }

            var response = await _httpClient.PostAsync<ProxmoxApiResponse<string>>(
                $"/nodes/{node}/qemu", formData, cancellationToken);

            var taskId = response?.Data ?? string.Empty;
            _logger.LogInformation("VM restore task {TaskId} started for VM {VmId}", taskId, vmId);
            
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore VM {VmId} from backup {Archive}", vmId, archive);
            throw;
        }
    }

    /// <summary>
    /// Restore a Container from backup
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="containerId">New Container ID</param>
    /// <param name="archive">Backup archive path</param>
    /// <param name="parameters">Restore parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID</returns>
    public async Task<string> RestoreContainerAsync(string node, int containerId, string archive,
        RestoreParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(archive);
        
        _logger.LogInformation("Restoring Container {ContainerId} from backup {Archive} on node {Node}", containerId, archive, node);

        try
        {
            var formData = new Dictionary<string, string>
            {
                ["vmid"] = containerId.ToString(),
                ["archive"] = archive
            };

            if (parameters != null)
            {
                if (!string.IsNullOrEmpty(parameters.Storage))
                    formData["storage"] = parameters.Storage;
                if (parameters.Force.HasValue)
                    formData["force"] = parameters.Force.Value ? "1" : "0";
                if (parameters.Start.HasValue)
                    formData["start"] = parameters.Start.Value ? "1" : "0";
            }

            var response = await _httpClient.PostAsync<ProxmoxApiResponse<string>>(
                $"/nodes/{node}/lxc", formData, cancellationToken);

            var taskId = response?.Data ?? string.Empty;
            _logger.LogInformation("Container restore task {TaskId} started for Container {ContainerId}", taskId, containerId);
            
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore Container {ContainerId} from backup {Archive}", containerId, archive);
            throw;
        }
    }

    #endregion

    #region Task Management

    /// <summary>
    /// Get backup task status
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="taskId">Task ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task status</returns>
    public async Task<BackupTask?> GetTaskStatusAsync(string node, string taskId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);
        
        _logger.LogInformation("Getting task status {TaskId} on node {Node}", taskId, node);

        try
        {
            var response = await _httpClient.GetAsync<ProxmoxApiResponse<BackupTask>>(
                $"/nodes/{node}/tasks/{taskId}/status", cancellationToken);

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get task status {TaskId}", taskId);
            throw;
        }
    }

    /// <summary>
    /// Get all backup-related tasks on a node
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of backup tasks</returns>
    public async Task<List<BackupTask>> GetBackupTasksAsync(string node, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        
        _logger.LogInformation("Getting backup tasks on node {Node}", node);

        try
        {
            var response = await _httpClient.GetAsync<ProxmoxApiResponse<List<BackupTask>>>(
                $"/nodes/{node}/tasks?typefilter=backup", cancellationToken);

            return response?.Data ?? new List<BackupTask>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get backup tasks on node {Node}", node);
            throw;
        }
    }

    /// <summary>
    /// Wait for a backup task to complete
    /// </summary>
    /// <param name="node">Node name</param>
    /// <param name="taskId">Task ID</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Final task status</returns>
    public async Task<BackupTask?> WaitForTaskCompletionAsync(string node, string taskId, 
        int timeout = 3600, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(taskId);
        
        _logger.LogInformation("Waiting for task {TaskId} to complete on node {Node}", taskId, node);

        var startTime = DateTime.UtcNow;
        var timeoutSpan = TimeSpan.FromSeconds(timeout);

        while (DateTime.UtcNow - startTime < timeoutSpan)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var task = await GetTaskStatusAsync(node, taskId, cancellationToken);
            
            if (task != null && !task.IsRunning)
            {
                _logger.LogInformation("Task {TaskId} completed with status {Status}", taskId, task.Status);
                return task;
            }

            await Task.Delay(5000, cancellationToken); // Check every 5 seconds
        }

        _logger.LogWarning("Task {TaskId} did not complete within timeout period", taskId);
        throw new TimeoutException($"Task {taskId} did not complete within {timeout} seconds");
    }

    #endregion
}
