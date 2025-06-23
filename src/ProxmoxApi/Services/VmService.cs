using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Core;
using ProxmoxApi.Models;

namespace ProxmoxApi.Services;

/// <summary>
/// Service for managing Proxmox Virtual Machines
/// </summary>
public class VmService
{    private readonly IProxmoxHttpClient _httpClient;
    private readonly ILogger<VmService> _logger;

    /// <summary>
    /// Initializes a new instance of the VmService
    /// </summary>
    public VmService(IProxmoxHttpClient httpClient, ILogger<VmService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }    /// <summary>
    /// Gets all VMs in the cluster
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of VMs across all nodes</returns>
    public async Task<List<ProxmoxVm>> GetVmsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching all VMs from cluster");

        try
        {
            var response = await _httpClient.GetAsync<List<ProxmoxVm>>(
                "cluster/resources?type=vm", 
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Successfully retrieved {Count} VMs", response.Count);
                return response;
            }

            _logger.LogWarning("No VM data returned from cluster resources");
            return new List<ProxmoxVm>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve VMs from cluster");
            throw;
        }
    }

    /// <summary>
    /// Gets VMs on a specific node
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of VMs on the specified node</returns>
    public async Task<List<ProxmoxVm>> GetVmsOnNodeAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogDebug("Fetching VMs from node {NodeName}", nodeName);        try
        {
            var response = await _httpClient.GetAsync<List<ProxmoxVm>>(
                $"nodes/{nodeName}/qemu",
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Successfully retrieved {Count} VMs from node {NodeName}", 
                    response.Count, nodeName);
                return response;
            }

            _logger.LogWarning("No VM data returned from node {NodeName}", nodeName);
            return new List<ProxmoxVm>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve VMs from node {NodeName}", nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets detailed status of a specific VM
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>VM status information</returns>
    public async Task<VmStatus?> GetVmStatusAsync(string nodeName, int vmId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogDebug("Fetching status for VM {VmId} on node {NodeName}", vmId, nodeName);

        try
        {            var response = await _httpClient.GetAsync<VmStatus>(
                $"nodes/{nodeName}/qemu/{vmId}/status/current",
                cancellationToken);

            if (response != null)
            {
                _logger.LogDebug("Successfully retrieved status for VM {VmId}: {Status}", 
                    vmId, response.Status);
                return response;
            }

            _logger.LogWarning("No status data returned for VM {VmId} on node {NodeName}", vmId, nodeName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve status for VM {VmId} on node {NodeName}", vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets VM configuration
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>VM configuration dictionary</returns>
    public async Task<Dictionary<string, object>?> GetVmConfigAsync(string nodeName, int vmId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogDebug("Fetching configuration for VM {VmId} on node {NodeName}", vmId, nodeName);

        try
        {            var response = await _httpClient.GetAsync<Dictionary<string, object>>(
                $"nodes/{nodeName}/qemu/{vmId}/config",
                cancellationToken);

            if (response != null)
            {
                _logger.LogDebug("Successfully retrieved configuration for VM {VmId}", vmId);
                return response;
            }

            _logger.LogWarning("No configuration data returned for VM {VmId} on node {NodeName}", vmId, nodeName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve configuration for VM {VmId} on node {NodeName}", vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets VM statistics
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>VM statistics</returns>
    public async Task<VmStatistics?> GetVmStatisticsAsync(string nodeName, int vmId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogDebug("Fetching statistics for VM {VmId} on node {NodeName}", vmId, nodeName);

        try
        {            var response = await _httpClient.GetAsync<VmStatistics>(
                $"nodes/{nodeName}/qemu/{vmId}/status/current",
                cancellationToken);

            if (response != null)
            {
                _logger.LogDebug("Successfully retrieved statistics for VM {VmId}", vmId);
                return response;
            }

            _logger.LogWarning("No statistics data returned for VM {VmId} on node {NodeName}", vmId, nodeName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve statistics for VM {VmId} on node {NodeName}", vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Starts a VM
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task result</returns>
    public async Task<string> StartVmAsync(string nodeName, int vmId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogInformation("Starting VM {VmId} on node {NodeName}", vmId, nodeName);

        try
        {            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/qemu/{vmId}/status/start",
                new Dictionary<string, object>(),
                cancellationToken);

            var taskId = response ?? "unknown";
            _logger.LogInformation("Started VM {VmId} on node {NodeName}, task ID: {TaskId}", vmId, nodeName, taskId);
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start VM {VmId} on node {NodeName}", vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Stops a VM
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="force">Force stop (equivalent to power off)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task result</returns>
    public async Task<string> StopVmAsync(string nodeName, int vmId, bool force = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogInformation("Stopping VM {VmId} on node {NodeName} (force: {Force})", vmId, nodeName, force);

        try
        {
            var parameters = new Dictionary<string, object>();
            if (force)
                parameters["forceStop"] = "1";            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/qemu/{vmId}/status/stop",
                parameters,
                cancellationToken);

            var taskId = response ?? "unknown";
            _logger.LogInformation("Stopped VM {VmId} on node {NodeName}, task ID: {TaskId}", vmId, nodeName, taskId);
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop VM {VmId} on node {NodeName}", vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Restarts a VM
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="force">Force restart</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task result</returns>
    public async Task<string> RestartVmAsync(string nodeName, int vmId, bool force = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogInformation("Restarting VM {VmId} on node {NodeName} (force: {Force})", vmId, nodeName, force);

        try
        {
            var parameters = new Dictionary<string, object>();
            if (force)
                parameters["forceStop"] = "1";            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/qemu/{vmId}/status/reboot",
                parameters,
                cancellationToken);

            var taskId = response ?? "unknown";
            _logger.LogInformation("Restarted VM {VmId} on node {NodeName}, task ID: {TaskId}", vmId, nodeName, taskId);
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart VM {VmId} on node {NodeName}", vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Pauses a VM
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task result</returns>
    public async Task<string> PauseVmAsync(string nodeName, int vmId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogInformation("Pausing VM {VmId} on node {NodeName}", vmId, nodeName);

        try
        {            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/qemu/{vmId}/status/suspend",
                new Dictionary<string, object>(),
                cancellationToken);

            var taskId = response ?? "unknown";
            _logger.LogInformation("Paused VM {VmId} on node {NodeName}, task ID: {TaskId}", vmId, nodeName, taskId);
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pause VM {VmId} on node {NodeName}", vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Resumes a paused VM
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task result</returns>
    public async Task<string> ResumeVmAsync(string nodeName, int vmId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogInformation("Resuming VM {VmId} on node {NodeName}", vmId, nodeName);

        try
        {            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/qemu/{vmId}/status/resume",
                new Dictionary<string, object>(),
                cancellationToken);

            var taskId = response ?? "unknown";
            _logger.LogInformation("Resumed VM {VmId} on node {NodeName}, task ID: {TaskId}", vmId, nodeName, taskId);
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resume VM {VmId} on node {NodeName}", vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets VM snapshots
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of VM snapshots</returns>
    public async Task<List<VmSnapshot>> GetVmSnapshotsAsync(string nodeName, int vmId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogDebug("Fetching snapshots for VM {VmId} on node {NodeName}", vmId, nodeName);

        try
        {            var response = await _httpClient.GetAsync<List<VmSnapshot>>(
                $"nodes/{nodeName}/qemu/{vmId}/snapshot",
                cancellationToken);

            if (response != null)
            {
                _logger.LogDebug("Successfully retrieved {Count} snapshots for VM {VmId}", 
                    response.Count, vmId);
                return response;
            }

            _logger.LogWarning("No snapshot data returned for VM {VmId} on node {NodeName}", vmId, nodeName);
            return new List<VmSnapshot>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve snapshots for VM {VmId} on node {NodeName}", vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Creates a VM snapshot
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="snapshotName">Snapshot name</param>
    /// <param name="description">Snapshot description</param>
    /// <param name="includeMemory">Include memory in snapshot</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task result</returns>
    public async Task<string> CreateVmSnapshotAsync(string nodeName, int vmId, string snapshotName, string? description = null, bool includeMemory = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));
        if (string.IsNullOrWhiteSpace(snapshotName))
            throw new ArgumentException("Snapshot name is required", nameof(snapshotName));

        _logger.LogInformation("Creating snapshot '{SnapshotName}' for VM {VmId} on node {NodeName}", 
            snapshotName, vmId, nodeName);

        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["snapname"] = snapshotName
            };

            if (!string.IsNullOrEmpty(description))
                parameters["description"] = description;

            if (includeMemory)
                parameters["vmstate"] = "1";            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/qemu/{vmId}/snapshot",
                parameters,
                cancellationToken);

            var taskId = response ?? "unknown";
            _logger.LogInformation("Created snapshot '{SnapshotName}' for VM {VmId}, task ID: {TaskId}", 
                snapshotName, vmId, taskId);
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create snapshot '{SnapshotName}' for VM {VmId} on node {NodeName}", 
                snapshotName, vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a VM snapshot
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">VM ID</param>
    /// <param name="snapshotName">Snapshot name</param>
    /// <param name="force">Force deletion</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task result</returns>
    public async Task<string> DeleteVmSnapshotAsync(string nodeName, int vmId, string snapshotName, bool force = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));
        if (string.IsNullOrWhiteSpace(snapshotName))
            throw new ArgumentException("Snapshot name is required", nameof(snapshotName));

        _logger.LogInformation("Deleting snapshot '{SnapshotName}' for VM {VmId} on node {NodeName}", 
            snapshotName, vmId, nodeName);

        try
        {
            var parameters = new Dictionary<string, object>();
            if (force)
                parameters["force"] = "1";            var response = await _httpClient.DeleteAsync<string>(
                $"nodes/{nodeName}/qemu/{vmId}/snapshot/{snapshotName}",
                cancellationToken);

            var taskId = response ?? "unknown";
            _logger.LogInformation("Deleted snapshot '{SnapshotName}' for VM {VmId}, task ID: {TaskId}", 
                snapshotName, vmId, taskId);
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete snapshot '{SnapshotName}' for VM {VmId} on node {NodeName}", 
                snapshotName, vmId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Clones a VM
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="vmId">Source VM ID</param>
    /// <param name="newVmId">New VM ID</param>
    /// <param name="name">New VM name</param>
    /// <param name="description">New VM description</param>
    /// <param name="fullClone">Create full clone (not linked)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task result</returns>
    public async Task<string> CloneVmAsync(string nodeName, int vmId, int newVmId, string? name = null, string? description = null, bool fullClone = false, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name is required", nameof(nodeName));

        _logger.LogInformation("Cloning VM {VmId} to {NewVmId} on node {NodeName}", vmId, newVmId, nodeName);

        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["newid"] = newVmId
            };

            if (!string.IsNullOrEmpty(name))
                parameters["name"] = name;

            if (!string.IsNullOrEmpty(description))
                parameters["description"] = description;

            if (fullClone)
                parameters["full"] = "1";            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/qemu/{vmId}/clone",
                parameters,
                cancellationToken);

            var taskId = response ?? "unknown";
            _logger.LogInformation("Cloned VM {VmId} to {NewVmId}, task ID: {TaskId}", vmId, newVmId, taskId);
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clone VM {VmId} to {NewVmId} on node {NodeName}", vmId, newVmId, nodeName);
            throw;
        }
    }
}
