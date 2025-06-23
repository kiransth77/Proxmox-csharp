using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Core;
using ProxmoxApi.Models;

namespace ProxmoxApi.Services;

/// <summary>
/// Service for managing Proxmox nodes
/// </summary>
public class NodeService
{    private readonly IProxmoxHttpClient _httpClient;
    private readonly ILogger<NodeService> _logger;

    public NodeService(IProxmoxHttpClient httpClient, ILogger<NodeService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a list of all nodes in the cluster
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of nodes</returns>
    public async Task<List<ProxmoxNode>> GetNodesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting list of cluster nodes");
        
        var nodes = await _httpClient.GetAsync<List<ProxmoxNode>>("/nodes", cancellationToken);
        
        _logger.LogInformation("Retrieved {NodeCount} nodes", nodes?.Count ?? 0);
        return nodes ?? new List<ProxmoxNode>();
    }

    /// <summary>
    /// Gets detailed status information for a specific node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Node status information</returns>
    public async Task<NodeStatus> GetNodeStatusAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        
        _logger.LogDebug("Getting status for node {NodeName}", nodeName);
        
        var status = await _httpClient.GetAsync<NodeStatus>($"/nodes/{nodeName}/status", cancellationToken);
        
        _logger.LogInformation("Retrieved status for node {NodeName}", nodeName);
        return status ?? new NodeStatus();
    }

    /// <summary>
    /// Gets version information for a specific node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Version information</returns>
    public async Task<Dictionary<string, object>> GetNodeVersionAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        
        _logger.LogDebug("Getting version information for node {NodeName}", nodeName);
        
        var version = await _httpClient.GetAsync<Dictionary<string, object>>($"/nodes/{nodeName}/version", cancellationToken);
        
        _logger.LogInformation("Retrieved version information for node {NodeName}", nodeName);
        return version ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets subscription information for a specific node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Subscription information</returns>
    public async Task<Dictionary<string, object>> GetNodeSubscriptionAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        
        _logger.LogDebug("Getting subscription information for node {NodeName}", nodeName);
        
        var subscription = await _httpClient.GetAsync<Dictionary<string, object>>($"/nodes/{nodeName}/subscription", cancellationToken);
        
        _logger.LogInformation("Retrieved subscription information for node {NodeName}", nodeName);
        return subscription ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Executes a command on a node (reboot, shutdown, etc.)
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="command">Command to execute (reboot, shutdown)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Command execution result</returns>
    public async Task<string> ExecuteNodeCommandAsync(string nodeName, string command, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        
        _logger.LogWarning("Executing command {Command} on node {NodeName}", command, nodeName);
        
        var data = new Dictionary<string, string>
        {
            ["command"] = command
        };
        
        var result = await _httpClient.PostAsync<string>($"/nodes/{nodeName}/status", data, cancellationToken);
        
        _logger.LogInformation("Command {Command} executed on node {NodeName}", command, nodeName);
        return result ?? string.Empty;
    }

    /// <summary>
    /// Reboots a specific node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Command execution result</returns>
    public async Task<string> RebootNodeAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Rebooting node {NodeName}", nodeName);
        return await ExecuteNodeCommandAsync(nodeName, "reboot", cancellationToken);
    }

    /// <summary>
    /// Shuts down a specific node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Command execution result</returns>
    public async Task<string> ShutdownNodeAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Shutting down node {NodeName}", nodeName);
        return await ExecuteNodeCommandAsync(nodeName, "shutdown", cancellationToken);
    }

    /// <summary>
    /// Gets RRD statistics for a node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="timeframe">Timeframe for statistics (hour, day, week, month, year)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>RRD statistics data</returns>
    public async Task<List<Dictionary<string, object>>> GetNodeStatisticsAsync(
        string nodeName, 
        string timeframe = "hour", 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        
        _logger.LogDebug("Getting {Timeframe} statistics for node {NodeName}", timeframe, nodeName);
        
        var stats = await _httpClient.GetAsync<List<Dictionary<string, object>>>(
            $"/nodes/{nodeName}/rrddata?timeframe={timeframe}", 
            cancellationToken);
        
        _logger.LogInformation("Retrieved {StatCount} statistics entries for node {NodeName}", 
            stats?.Count ?? 0, nodeName);
        
        return stats ?? new List<Dictionary<string, object>>();
    }

    /// <summary>
    /// Gets a summary of all nodes with their current status
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of node names to their status</returns>
    public async Task<Dictionary<string, ProxmoxNode>> GetNodesSummaryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting nodes summary");
        
        var nodes = await GetNodesAsync(cancellationToken);
        var summary = new Dictionary<string, ProxmoxNode>();
        
        foreach (var node in nodes)
        {
            if (!string.IsNullOrEmpty(node.Node))
            {
                summary[node.Node] = node;
            }
        }
        
        _logger.LogInformation("Created summary for {NodeCount} nodes", summary.Count);
        return summary;
    }

    /// <summary>
    /// Checks if a node is online and responsive
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if node is online</returns>
    public async Task<bool> IsNodeOnlineAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        
        try
        {
            var status = await GetNodeStatusAsync(nodeName, cancellationToken);
            var isOnline = status.Time > 0; // If we get a valid time, node is responsive
            
            _logger.LogDebug("Node {NodeName} online status: {IsOnline}", nodeName, isOnline);
            return isOnline;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check online status for node {NodeName}", nodeName);
            return false;
        }
    }
}
