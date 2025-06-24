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
/// Service for managing Proxmox LXC containers
/// </summary>
public class ContainerService
{
    private readonly IProxmoxHttpClient _httpClient;
    private readonly ILogger<ContainerService> _logger;

    /// <summary>
    /// Initializes a new instance of ContainerService
    /// </summary>
    /// <param name="httpClient">HTTP client for API communication</param>
    /// <param name="logger">Logger instance</param>
    public ContainerService(IProxmoxHttpClient httpClient, ILogger<ContainerService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all containers across all nodes
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all containers</returns>
    public async Task<List<ProxmoxContainer>> GetAllContainersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all containers");

        try
        {
            var response = await _httpClient.GetAsync<List<ProxmoxContainer>>(
                "cluster/resources?type=lxc",
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved {Count} containers", response.Count);
                return response;
            }

            _logger.LogWarning("No container data received from API");
            return new List<ProxmoxContainer>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all containers");
            throw;
        }
    }

    /// <summary>
    /// Gets containers on a specific node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of containers on the specified node</returns>
    public async Task<List<ProxmoxContainer>> GetContainersAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Retrieving containers for node: {NodeName}", nodeName);

        try
        {
            var response = await _httpClient.GetAsync<List<ProxmoxContainer>>(
                $"nodes/{nodeName}/lxc",
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved {Count} containers for node {NodeName}", response.Count, nodeName);
                return response;
            }

            _logger.LogWarning("No container data received for node {NodeName}", nodeName);
            return new List<ProxmoxContainer>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve containers for node {NodeName}", nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets a specific container by ID
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Container information</returns>
    public async Task<ProxmoxContainer?> GetContainerAsync(string nodeName, int containerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Retrieving container {ContainerId} from node {NodeName}", containerId, nodeName);

        try
        {
            var containers = await GetContainersAsync(nodeName, cancellationToken);
            var container = containers.FirstOrDefault(c => c.ContainerId == containerId);

            if (container != null)
            {
                _logger.LogInformation("Found container {ContainerId} on node {NodeName}", containerId, nodeName);
            }
            else
            {
                _logger.LogWarning("Container {ContainerId} not found on node {NodeName}", containerId, nodeName);
            }

            return container;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve container {ContainerId} from node {NodeName}", containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets container status
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Container status</returns>
    public async Task<ContainerStatus?> GetContainerStatusAsync(string nodeName, int containerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Retrieving status for container {ContainerId} on node {NodeName}", containerId, nodeName);

        try
        {
            var response = await _httpClient.GetAsync<ContainerStatus>(
                $"nodes/{nodeName}/lxc/{containerId}/status/current",
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved status for container {ContainerId}: {Status}", containerId, response.Status);
                return response;
            }

            _logger.LogWarning("No status data received for container {ContainerId}", containerId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve status for container {ContainerId} on node {NodeName}", containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets container configuration
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Container configuration</returns>
    public async Task<ContainerConfig?> GetContainerConfigAsync(string nodeName, int containerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Retrieving configuration for container {ContainerId} on node {NodeName}", containerId, nodeName);

        try
        {
            var response = await _httpClient.GetAsync<ContainerConfig>(
                $"nodes/{nodeName}/lxc/{containerId}/config",
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved configuration for container {ContainerId}", containerId);
                return response;
            }

            _logger.LogWarning("No configuration data received for container {ContainerId}", containerId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve configuration for container {ContainerId} on node {NodeName}", containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets container statistics
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Container statistics</returns>
    public async Task<ContainerStatistics?> GetContainerStatisticsAsync(string nodeName, int containerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Retrieving statistics for container {ContainerId} on node {NodeName}", containerId, nodeName);

        try
        {
            var response = await _httpClient.GetAsync<ContainerStatistics>(
                $"nodes/{nodeName}/lxc/{containerId}/status/current",
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved statistics for container {ContainerId}", containerId);
                return response;
            }

            _logger.LogWarning("No statistics data received for container {ContainerId}", containerId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve statistics for container {ContainerId} on node {NodeName}", containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Starts a container
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for the operation</returns>
    public async Task<string> StartContainerAsync(string nodeName, int containerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Starting container {ContainerId} on node {NodeName}", containerId, nodeName);

        try
        {
            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/lxc/{containerId}/status/start",
                null,
                cancellationToken);

            if (!string.IsNullOrEmpty(response))
            {
                _logger.LogInformation("Container {ContainerId} start initiated with task ID: {TaskId}", containerId, response);
                return response;
            }

            throw new InvalidOperationException("Failed to start container - no task ID received");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start container {ContainerId} on node {NodeName}", containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Stops a container
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="force">Force stop the container</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for the operation</returns>
    public async Task<string> StopContainerAsync(string nodeName, int containerId, bool force = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Stopping container {ContainerId} on node {NodeName} (force: {Force})", containerId, nodeName, force);

        try
        {
            var data = new Dictionary<string, object>();
            if (force)
                data["forceStop"] = 1;

            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/lxc/{containerId}/status/stop",
                data,
                cancellationToken);

            if (!string.IsNullOrEmpty(response))
            {
                _logger.LogInformation("Container {ContainerId} stop initiated with task ID: {TaskId}", containerId, response);
                return response;
            }

            throw new InvalidOperationException("Failed to stop container - no task ID received");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop container {ContainerId} on node {NodeName}", containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Restarts a container
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for the operation</returns>
    public async Task<string> RestartContainerAsync(string nodeName, int containerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Restarting container {ContainerId} on node {NodeName}", containerId, nodeName);

        try
        {
            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/lxc/{containerId}/status/restart",
                null,
                cancellationToken);

            if (!string.IsNullOrEmpty(response))
            {
                _logger.LogInformation("Container {ContainerId} restart initiated with task ID: {TaskId}", containerId, response);
                return response;
            }

            throw new InvalidOperationException("Failed to restart container - no task ID received");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart container {ContainerId} on node {NodeName}", containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a container
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="purge">Purge container from replication jobs</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for the operation</returns>
    public async Task<string> DeleteContainerAsync(string nodeName, int containerId, bool purge = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Deleting container {ContainerId} on node {NodeName} (purge: {Purge})", containerId, nodeName, purge);

        try
        {
            var endpoint = $"nodes/{nodeName}/lxc/{containerId}";
            if (purge)
                endpoint += "?purge=1";

            var response = await _httpClient.DeleteAsync<string>(endpoint, cancellationToken);

            if (!string.IsNullOrEmpty(response))
            {
                _logger.LogInformation("Container {ContainerId} deletion initiated with task ID: {TaskId}", containerId, response);
                return response;
            }

            throw new InvalidOperationException("Failed to delete container - no task ID received");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete container {ContainerId} on node {NodeName}", containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets container snapshots
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of container snapshots</returns>
    public async Task<List<ContainerSnapshot>> GetContainerSnapshotsAsync(string nodeName, int containerId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        _logger.LogInformation("Retrieving snapshots for container {ContainerId} on node {NodeName}", containerId, nodeName);

        try
        {
            var response = await _httpClient.GetAsync<List<ContainerSnapshot>>(
                $"nodes/{nodeName}/lxc/{containerId}/snapshot",
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved {Count} snapshots for container {ContainerId}", response.Count, containerId);
                return response;
            }

            _logger.LogWarning("No snapshot data received for container {ContainerId}", containerId);
            return new List<ContainerSnapshot>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve snapshots for container {ContainerId} on node {NodeName}", containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Creates a snapshot of a container
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="snapName">Snapshot name</param>
    /// <param name="description">Snapshot description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for the operation</returns>
    public async Task<string> CreateContainerSnapshotAsync(string nodeName, int containerId, string snapName, string? description = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapName);
        _logger.LogInformation("Creating snapshot '{SnapName}' for container {ContainerId} on node {NodeName}", snapName, containerId, nodeName);

        try
        {
            var data = new Dictionary<string, object>
            {
                ["snapname"] = snapName
            };

            if (!string.IsNullOrWhiteSpace(description))
                data["description"] = description;

            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/lxc/{containerId}/snapshot",
                data,
                cancellationToken);

            if (!string.IsNullOrEmpty(response))
            {
                _logger.LogInformation("Snapshot '{SnapName}' creation initiated for container {ContainerId} with task ID: {TaskId}", snapName, containerId, response);
                return response;
            }

            throw new InvalidOperationException("Failed to create snapshot - no task ID received");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create snapshot '{SnapName}' for container {ContainerId} on node {NodeName}", snapName, containerId, nodeName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a container snapshot
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="containerId">Container ID</param>
    /// <param name="snapName">Snapshot name</param>
    /// <param name="force">Force deletion</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for the operation</returns>
    public async Task<string> DeleteContainerSnapshotAsync(string nodeName, int containerId, string snapName, bool force = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapName);
        _logger.LogInformation("Deleting snapshot '{SnapName}' for container {ContainerId} on node {NodeName}", snapName, containerId, nodeName);

        try
        {
            var endpoint = $"nodes/{nodeName}/lxc/{containerId}/snapshot/{snapName}";
            if (force)
                endpoint += "?force=1";

            var response = await _httpClient.DeleteAsync<string>(endpoint, cancellationToken);

            if (!string.IsNullOrEmpty(response))
            {
                _logger.LogInformation("Snapshot '{SnapName}' deletion initiated for container {ContainerId} with task ID: {TaskId}", snapName, containerId, response);
                return response;
            }

            throw new InvalidOperationException("Failed to delete snapshot - no task ID received");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete snapshot '{SnapName}' for container {ContainerId} on node {NodeName}", snapName, containerId, nodeName);
            throw;
        }
    }
}
