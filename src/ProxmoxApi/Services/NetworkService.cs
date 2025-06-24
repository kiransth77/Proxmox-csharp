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
/// Service for managing Proxmox network configurations
/// </summary>
public class NetworkService
{
    private readonly IProxmoxHttpClient _httpClient;
    private readonly ILogger<NetworkService> _logger;

    /// <summary>
    /// Initializes a new instance of the NetworkService
    /// </summary>
    /// <param name="httpClient">HTTP client for API communication</param>
    /// <param name="logger">Logger instance</param>
    public NetworkService(IProxmoxHttpClient httpClient, ILogger<NetworkService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Network Interface Management

    /// <summary>
    /// Gets all network interfaces for a node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of network interfaces</returns>
    public async Task<List<NetworkInterface>> GetNetworkInterfacesAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        _logger.LogDebug("Getting network interfaces for node {NodeName}", nodeName);

        var response = await _httpClient.GetAsync<List<NetworkInterface>>(
            $"nodes/{nodeName}/network", 
            cancellationToken);

        return response ?? new List<NetworkInterface>();
    }

    /// <summary>
    /// Gets a specific network interface by name
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="interfaceName">Name of the interface</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Network interface details</returns>
    public async Task<NetworkInterface?> GetNetworkInterfaceAsync(string nodeName, string interfaceName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(interfaceName);

        _logger.LogDebug("Getting network interface {InterfaceName} for node {NodeName}", interfaceName, nodeName);

        var response = await _httpClient.GetAsync<NetworkInterface>(
            $"nodes/{nodeName}/network/{interfaceName}", 
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Creates a new network interface
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="options">Interface creation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task CreateNetworkInterfaceAsync(string nodeName, NetworkInterfaceCreateOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.InterfaceName);

        _logger.LogDebug("Creating network interface {InterfaceName} on node {NodeName}", options.InterfaceName, nodeName);

        await _httpClient.PostAsync<object>(
            $"nodes/{nodeName}/network", 
            options, 
            cancellationToken);

        _logger.LogInformation("Network interface {InterfaceName} created successfully on node {NodeName}", options.InterfaceName, nodeName);
    }

    /// <summary>
    /// Updates a network interface
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="interfaceName">Name of the interface</param>
    /// <param name="options">Interface update options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task UpdateNetworkInterfaceAsync(string nodeName, string interfaceName, NetworkInterfaceUpdateOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(interfaceName);
        ArgumentNullException.ThrowIfNull(options);

        _logger.LogDebug("Updating network interface {InterfaceName} on node {NodeName}", interfaceName, nodeName);

        await _httpClient.PutAsync<object>(
            $"nodes/{nodeName}/network/{interfaceName}", 
            options, 
            cancellationToken);

        _logger.LogInformation("Network interface {InterfaceName} updated successfully on node {NodeName}", interfaceName, nodeName);
    }

    /// <summary>
    /// Deletes a network interface
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="interfaceName">Name of the interface</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task DeleteNetworkInterfaceAsync(string nodeName, string interfaceName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(interfaceName);

        _logger.LogDebug("Deleting network interface {InterfaceName} from node {NodeName}", interfaceName, nodeName);

        await _httpClient.DeleteAsync<object>(
            $"nodes/{nodeName}/network/{interfaceName}", 
            cancellationToken);

        _logger.LogInformation("Network interface {InterfaceName} deleted successfully from node {NodeName}", interfaceName, nodeName);
    }

    #endregion

    #region Network Status

    /// <summary>
    /// Gets the network status for a node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Network status information</returns>
    public async Task<NetworkStatus?> GetNetworkStatusAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        _logger.LogDebug("Getting network status for node {NodeName}", nodeName);

        var response = await _httpClient.GetAsync<NetworkStatus>(
            $"nodes/{nodeName}/status", 
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Applies network configuration changes
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task ApplyNetworkConfigurationAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        _logger.LogDebug("Applying network configuration for node {NodeName}", nodeName);

        await _httpClient.PutAsync<object>(
            $"nodes/{nodeName}/network", 
            new { }, 
            cancellationToken);

        _logger.LogInformation("Network configuration applied successfully for node {NodeName}", nodeName);
    }

    /// <summary>
    /// Reverts network configuration changes
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task RevertNetworkConfigurationAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        _logger.LogDebug("Reverting network configuration for node {NodeName}", nodeName);

        await _httpClient.DeleteAsync<object>(
            $"nodes/{nodeName}/network", 
            cancellationToken);

        _logger.LogInformation("Network configuration reverted successfully for node {NodeName}", nodeName);
    }

    #endregion

    #region Firewall Management

    /// <summary>
    /// Gets firewall rules for a node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of firewall rules</returns>
    public async Task<List<FirewallRule>> GetFirewallRulesAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        _logger.LogDebug("Getting firewall rules for node {NodeName}", nodeName);

        var response = await _httpClient.GetAsync<List<FirewallRule>>(
            $"nodes/{nodeName}/firewall/rules", 
            cancellationToken);

        return response ?? new List<FirewallRule>();
    }

    /// <summary>
    /// Gets a specific firewall rule by position
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="position">Rule position</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Firewall rule details</returns>
    public async Task<FirewallRule?> GetFirewallRuleAsync(string nodeName, int position, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        _logger.LogDebug("Getting firewall rule at position {Position} for node {NodeName}", position, nodeName);

        var response = await _httpClient.GetAsync<FirewallRule>(
            $"nodes/{nodeName}/firewall/rules/{position}", 
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Creates a new firewall rule
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="options">Rule creation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task CreateFirewallRuleAsync(string nodeName, FirewallRuleCreateOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentNullException.ThrowIfNull(options);

        _logger.LogDebug("Creating firewall rule on node {NodeName}", nodeName);

        await _httpClient.PostAsync<object>(
            $"nodes/{nodeName}/firewall/rules", 
            options, 
            cancellationToken);

        _logger.LogInformation("Firewall rule created successfully on node {NodeName}", nodeName);
    }

    /// <summary>
    /// Updates a firewall rule
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="position">Rule position</param>
    /// <param name="options">Rule update options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task UpdateFirewallRuleAsync(string nodeName, int position, FirewallRuleCreateOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentNullException.ThrowIfNull(options);

        _logger.LogDebug("Updating firewall rule at position {Position} on node {NodeName}", position, nodeName);

        await _httpClient.PutAsync<object>(
            $"nodes/{nodeName}/firewall/rules/{position}", 
            options, 
            cancellationToken);

        _logger.LogInformation("Firewall rule at position {Position} updated successfully on node {NodeName}", position, nodeName);
    }

    /// <summary>
    /// Deletes a firewall rule
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="position">Rule position</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task DeleteFirewallRuleAsync(string nodeName, int position, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        _logger.LogDebug("Deleting firewall rule at position {Position} from node {NodeName}", position, nodeName);

        await _httpClient.DeleteAsync<object>(
            $"nodes/{nodeName}/firewall/rules/{position}", 
            cancellationToken);

        _logger.LogInformation("Firewall rule at position {Position} deleted successfully from node {NodeName}", position, nodeName);
    }

    #endregion

    #region DNS Management

    /// <summary>
    /// Gets DNS configuration for a node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>DNS configuration</returns>
    public async Task<DnsConfig?> GetDnsConfigAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        _logger.LogDebug("Getting DNS configuration for node {NodeName}", nodeName);

        var response = await _httpClient.GetAsync<DnsConfig>(
            $"nodes/{nodeName}/dns", 
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Updates DNS configuration for a node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="dnsConfig">DNS configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task UpdateDnsConfigAsync(string nodeName, DnsConfig dnsConfig, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentNullException.ThrowIfNull(dnsConfig);

        _logger.LogDebug("Updating DNS configuration for node {NodeName}", nodeName);

        await _httpClient.PutAsync<object>(
            $"nodes/{nodeName}/dns", 
            dnsConfig, 
            cancellationToken);

        _logger.LogInformation("DNS configuration updated successfully for node {NodeName}", nodeName);
    }

    #endregion

    #region Hosts Management

    /// <summary>
    /// Gets host entries for a node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of host entries</returns>
    public async Task<List<HostEntry>> GetHostEntriesAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);

        _logger.LogDebug("Getting host entries for node {NodeName}", nodeName);

        var response = await _httpClient.GetAsync<List<HostEntry>>(
            $"nodes/{nodeName}/hosts", 
            cancellationToken);

        return response ?? new List<HostEntry>();
    }

    /// <summary>
    /// Updates host entries for a node
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="hostEntries">List of host entries</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task UpdateHostEntriesAsync(string nodeName, List<HostEntry> hostEntries, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentNullException.ThrowIfNull(hostEntries);

        _logger.LogDebug("Updating host entries for node {NodeName}", nodeName);

        await _httpClient.PostAsync<object>(
            $"nodes/{nodeName}/hosts", 
            hostEntries, 
            cancellationToken);

        _logger.LogInformation("Host entries updated successfully for node {NodeName}", nodeName);
    }

    #endregion

    #region Bridge Management

    /// <summary>
    /// Creates a bridge interface
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="bridgeConfig">Bridge configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task CreateBridgeAsync(string nodeName, BridgeConfig bridgeConfig, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentNullException.ThrowIfNull(bridgeConfig);
        ArgumentException.ThrowIfNullOrWhiteSpace(bridgeConfig.Name);

        _logger.LogDebug("Creating bridge {BridgeName} on node {NodeName}", bridgeConfig.Name, nodeName);

        var options = new NetworkInterfaceCreateOptions
        {
            InterfaceName = bridgeConfig.Name,
            Type = "bridge",
            BridgePorts = bridgeConfig.Ports,
            BridgeStp = bridgeConfig.SpanningTreeProtocol ? "on" : "off",
            BridgeForwardDelay = bridgeConfig.ForwardDelay.ToString(),
            BridgeVlanAware = bridgeConfig.VlanAware ? 1 : 0,
            Mtu = bridgeConfig.Mtu,
            Comments = bridgeConfig.Comments
        };

        await CreateNetworkInterfaceAsync(nodeName, options, cancellationToken);

        _logger.LogInformation("Bridge {BridgeName} created successfully on node {NodeName}", bridgeConfig.Name, nodeName);
    }

    /// <summary>
    /// Gets all bridge interfaces
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of bridge interfaces</returns>
    public async Task<List<NetworkInterface>> GetBridgesAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        var interfaces = await GetNetworkInterfacesAsync(nodeName, cancellationToken);
        return interfaces.Where(i => i.IsBridge).ToList();
    }

    #endregion

    #region VLAN Management

    /// <summary>
    /// Creates a VLAN interface
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="vlanConfig">VLAN configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task CreateVlanAsync(string nodeName, VlanConfig vlanConfig, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentNullException.ThrowIfNull(vlanConfig);
        ArgumentException.ThrowIfNullOrWhiteSpace(vlanConfig.Name);

        _logger.LogDebug("Creating VLAN {VlanName} on node {NodeName}", vlanConfig.Name, nodeName);

        var options = new NetworkInterfaceCreateOptions
        {
            InterfaceName = vlanConfig.Name,
            Type = "vlan",
            VlanId = vlanConfig.VlanId,
            VlanRawDevice = vlanConfig.RawDevice,
            Method = vlanConfig.Method,
            Address = vlanConfig.Address,
            Netmask = vlanConfig.Netmask,
            Mtu = vlanConfig.Mtu,
            Comments = vlanConfig.Comments
        };

        await CreateNetworkInterfaceAsync(nodeName, options, cancellationToken);

        _logger.LogInformation("VLAN {VlanName} created successfully on node {NodeName}", vlanConfig.Name, nodeName);
    }

    /// <summary>
    /// Gets all VLAN interfaces
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of VLAN interfaces</returns>
    public async Task<List<NetworkInterface>> GetVlansAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        var interfaces = await GetNetworkInterfacesAsync(nodeName, cancellationToken);
        return interfaces.Where(i => i.IsVlan).ToList();
    }

    #endregion

    #region Bond Management

    /// <summary>
    /// Creates a bond interface
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="bondConfig">Bond configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task CreateBondAsync(string nodeName, BondConfig bondConfig, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeName);
        ArgumentNullException.ThrowIfNull(bondConfig);
        ArgumentException.ThrowIfNullOrWhiteSpace(bondConfig.Name);

        _logger.LogDebug("Creating bond {BondName} on node {NodeName}", bondConfig.Name, nodeName);

        var options = new NetworkInterfaceCreateOptions
        {
            InterfaceName = bondConfig.Name,
            Type = "bond",
            BondSlaves = bondConfig.Slaves,
            BondMode = bondConfig.Mode,
            BondXmitHashPolicy = bondConfig.XmitHashPolicy,
            Method = bondConfig.Method,
            Address = bondConfig.Address,
            Netmask = bondConfig.Netmask,
            Mtu = bondConfig.Mtu,
            Comments = bondConfig.Comments
        };

        await CreateNetworkInterfaceAsync(nodeName, options, cancellationToken);

        _logger.LogInformation("Bond {BondName} created successfully on node {NodeName}", bondConfig.Name, nodeName);
    }

    /// <summary>
    /// Gets all bond interfaces
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of bond interfaces</returns>
    public async Task<List<NetworkInterface>> GetBondsAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        var interfaces = await GetNetworkInterfacesAsync(nodeName, cancellationToken);
        return interfaces.Where(i => i.IsBond).ToList();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Validates network interface configuration
    /// </summary>
    /// <param name="options">Interface configuration options</param>
    /// <returns>Validation result</returns>
    public static bool ValidateNetworkInterfaceConfig(NetworkInterfaceCreateOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.InterfaceName))
            return false;

        if (string.IsNullOrWhiteSpace(options.Type))
            return false;

        // Validate type-specific requirements
        switch (options.Type.ToLowerInvariant())
        {
            case "bridge":
                // Bridge should have ports defined
                return !string.IsNullOrWhiteSpace(options.BridgePorts);
            
            case "vlan":
                // VLAN should have VLAN ID and raw device
                return options.VlanId.HasValue && options.VlanId > 0 && 
                       !string.IsNullOrWhiteSpace(options.VlanRawDevice);
            
            case "bond":
                // Bond should have slaves defined
                return !string.IsNullOrWhiteSpace(options.BondSlaves);
            
            default:
                return true;
        }
    }

    /// <summary>
    /// Gets network interface summary information
    /// </summary>
    /// <param name="nodeName">Name of the node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Network interface summary</returns>
    public async Task<Dictionary<string, object>> GetNetworkSummaryAsync(string nodeName, CancellationToken cancellationToken = default)
    {
        var interfaces = await GetNetworkInterfacesAsync(nodeName, cancellationToken);
        
        var summary = new Dictionary<string, object>
        {
            ["TotalInterfaces"] = interfaces.Count,
            ["BridgeCount"] = interfaces.Count(i => i.IsBridge),
            ["VlanCount"] = interfaces.Count(i => i.IsVlan),
            ["BondCount"] = interfaces.Count(i => i.IsBond),
            ["ActiveInterfaces"] = interfaces.Count(i => i.IsActive),
            ["AutostartInterfaces"] = interfaces.Count(i => i.IsAutostart),
            ["InterfaceTypes"] = interfaces.GroupBy(i => i.Type).ToDictionary(g => g.Key, g => g.Count())
        };

        return summary;
    }

    #endregion
}
