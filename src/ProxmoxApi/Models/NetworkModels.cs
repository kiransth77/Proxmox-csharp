using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProxmoxApi.Models;

/// <summary>
/// Represents network interface information
/// </summary>
public class NetworkInterface
{
    [JsonPropertyName("iface")]
    public string InterfaceName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("netmask")]
    public string? Netmask { get; set; }

    [JsonPropertyName("gateway")]
    public string? Gateway { get; set; }

    [JsonPropertyName("cidr")]
    public string? Cidr { get; set; }

    [JsonPropertyName("bridge_ports")]
    public string? BridgePorts { get; set; }

    [JsonPropertyName("bridge_stp")]
    public string? BridgeStp { get; set; }

    [JsonPropertyName("bridge_fd")]
    public string? BridgeForwardDelay { get; set; }

    [JsonPropertyName("bridge_vlan_aware")]
    public int? BridgeVlanAware { get; set; }

    [JsonPropertyName("autostart")]
    public int? Autostart { get; set; }

    [JsonPropertyName("active")]
    public int? Active { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("vlan-id")]
    public int? VlanId { get; set; }

    [JsonPropertyName("vlan-raw-device")]
    public string? VlanRawDevice { get; set; }

    [JsonPropertyName("bond-slaves")]
    public string? BondSlaves { get; set; }

    [JsonPropertyName("bond_mode")]
    public string? BondMode { get; set; }

    [JsonPropertyName("bond_xmit_hash_policy")]
    public string? BondXmitHashPolicy { get; set; }

    [JsonPropertyName("mtu")]
    public int? Mtu { get; set; }

    [JsonPropertyName("comments")]
    public string? Comments { get; set; }

    [JsonPropertyName("families")]
    public List<string> Families { get; set; } = new();

    /// <summary>
    /// Additional options for the network interface
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> Options { get; set; } = new();

    /// <summary>
    /// Gets whether this interface is a bridge
    /// </summary>
    public bool IsBridge => Type.Equals("bridge", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether this interface is a VLAN
    /// </summary>
    public bool IsVlan => Type.Equals("vlan", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether this interface is a bond
    /// </summary>
    public bool IsBond => Type.Equals("bond", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets whether this interface is active
    /// </summary>
    public bool IsActive => Active == 1;

    /// <summary>
    /// Gets whether this interface has autostart enabled
    /// </summary>
    public bool IsAutostart => Autostart == 1;
}

/// <summary>
/// Options for creating a network interface
/// </summary>
public class NetworkInterfaceCreateOptions
{
    [JsonPropertyName("iface")]
    public string InterfaceName { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = "manual";

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("netmask")]
    public string? Netmask { get; set; }

    [JsonPropertyName("gateway")]
    public string? Gateway { get; set; }

    [JsonPropertyName("cidr")]
    public string? Cidr { get; set; }

    [JsonPropertyName("bridge_ports")]
    public string? BridgePorts { get; set; }

    [JsonPropertyName("bridge_stp")]
    public string? BridgeStp { get; set; }

    [JsonPropertyName("bridge_fd")]
    public string? BridgeForwardDelay { get; set; }

    [JsonPropertyName("bridge_vlan_aware")]
    public int? BridgeVlanAware { get; set; }

    [JsonPropertyName("autostart")]
    public int? Autostart { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("vlan-id")]
    public int? VlanId { get; set; }

    [JsonPropertyName("vlan-raw-device")]
    public string? VlanRawDevice { get; set; }

    [JsonPropertyName("bond-slaves")]
    public string? BondSlaves { get; set; }

    [JsonPropertyName("bond_mode")]
    public string? BondMode { get; set; }

    [JsonPropertyName("bond_xmit_hash_policy")]
    public string? BondXmitHashPolicy { get; set; }

    [JsonPropertyName("mtu")]
    public int? Mtu { get; set; }

    [JsonPropertyName("comments")]
    public string? Comments { get; set; }

    /// <summary>
    /// Additional options for the network interface
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> Options { get; set; } = new();
}

/// <summary>
/// Options for updating a network interface
/// </summary>
public class NetworkInterfaceUpdateOptions
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("netmask")]
    public string? Netmask { get; set; }

    [JsonPropertyName("gateway")]
    public string? Gateway { get; set; }

    [JsonPropertyName("cidr")]
    public string? Cidr { get; set; }

    [JsonPropertyName("bridge_ports")]
    public string? BridgePorts { get; set; }

    [JsonPropertyName("bridge_stp")]
    public string? BridgeStp { get; set; }

    [JsonPropertyName("bridge_fd")]
    public string? BridgeForwardDelay { get; set; }

    [JsonPropertyName("bridge_vlan_aware")]
    public int? BridgeVlanAware { get; set; }

    [JsonPropertyName("autostart")]
    public int? Autostart { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("vlan-id")]
    public int? VlanId { get; set; }

    [JsonPropertyName("vlan-raw-device")]
    public string? VlanRawDevice { get; set; }

    [JsonPropertyName("bond-slaves")]
    public string? BondSlaves { get; set; }

    [JsonPropertyName("bond_mode")]
    public string? BondMode { get; set; }

    [JsonPropertyName("bond_xmit_hash_policy")]
    public string? BondXmitHashPolicy { get; set; }

    [JsonPropertyName("mtu")]
    public int? Mtu { get; set; }

    [JsonPropertyName("comments")]
    public string? Comments { get; set; }

    [JsonPropertyName("delete")]
    public string? Delete { get; set; }

    /// <summary>
    /// Additional options for the network interface
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> Options { get; set; } = new();
}

/// <summary>
/// Represents firewall rule information
/// </summary>
public class FirewallRule
{
    [JsonPropertyName("pos")]
    public int Position { get; set; }

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("enable")]
    public int? Enable { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("dest")]
    public string? Destination { get; set; }

    [JsonPropertyName("proto")]
    public string? Protocol { get; set; }

    [JsonPropertyName("dport")]
    public string? DestinationPort { get; set; }

    [JsonPropertyName("sport")]
    public string? SourcePort { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("log")]
    public string? Log { get; set; }

    [JsonPropertyName("macro")]
    public string? Macro { get; set; }

    [JsonPropertyName("iface")]
    public string? Interface { get; set; }

    /// <summary>
    /// Gets whether this rule is enabled
    /// </summary>
    public bool IsEnabled => Enable == 1;
}

/// <summary>
/// Options for creating a firewall rule
/// </summary>
public class FirewallRuleCreateOptions
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "ACCEPT";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "in";

    [JsonPropertyName("enable")]
    public int Enable { get; set; } = 1;

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("dest")]
    public string? Destination { get; set; }

    [JsonPropertyName("proto")]
    public string? Protocol { get; set; }

    [JsonPropertyName("dport")]
    public string? DestinationPort { get; set; }

    [JsonPropertyName("sport")]
    public string? SourcePort { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("log")]
    public string? Log { get; set; }

    [JsonPropertyName("macro")]
    public string? Macro { get; set; }

    [JsonPropertyName("iface")]
    public string? Interface { get; set; }

    [JsonPropertyName("pos")]
    public int? Position { get; set; }
}

/// <summary>
/// Represents network configuration status
/// </summary>
public class NetworkStatus
{
    [JsonPropertyName("node")]
    public string NodeName { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("uptime")]
    public long Uptime { get; set; }

    [JsonPropertyName("interfaces")]
    public List<NetworkInterfaceStatus> Interfaces { get; set; } = new();
}

/// <summary>
/// Represents the status of a network interface
/// </summary>
public class NetworkInterfaceStatus
{
    [JsonPropertyName("iface")]
    public string InterfaceName { get; set; } = string.Empty;

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("address6")]
    public string? Address6 { get; set; }

    [JsonPropertyName("netmask")]
    public string? Netmask { get; set; }

    [JsonPropertyName("gateway")]
    public string? Gateway { get; set; }

    [JsonPropertyName("gateway6")]
    public string? Gateway6 { get; set; }

    [JsonPropertyName("exists")]
    public bool Exists { get; set; }

    [JsonPropertyName("autostart")]
    public bool Autostart { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }
}

/// <summary>
/// Represents DNS configuration
/// </summary>
public class DnsConfig
{
    [JsonPropertyName("search")]
    public string? SearchDomain { get; set; }

    [JsonPropertyName("dns1")]
    public string? Dns1 { get; set; }

    [JsonPropertyName("dns2")]
    public string? Dns2 { get; set; }

    [JsonPropertyName("dns3")]
    public string? Dns3 { get; set; }
}

/// <summary>
/// Represents host entry information
/// </summary>
public class HostEntry
{
    [JsonPropertyName("ip")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Hostname { get; set; } = string.Empty;

    [JsonPropertyName("digest")]
    public string? Digest { get; set; }
}

/// <summary>
/// Network bridge configuration
/// </summary>
public class BridgeConfig
{
    /// <summary>
    /// Bridge name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Bridge ports (comma-separated list of interfaces)
    /// </summary>
    public string Ports { get; set; } = string.Empty;

    /// <summary>
    /// Enable/disable Spanning Tree Protocol
    /// </summary>
    public bool SpanningTreeProtocol { get; set; } = false;

    /// <summary>
    /// Forward delay in seconds
    /// </summary>
    public int ForwardDelay { get; set; } = 15;

    /// <summary>
    /// Enable VLAN aware bridge
    /// </summary>
    public bool VlanAware { get; set; } = false;

    /// <summary>
    /// Maximum Transmission Unit
    /// </summary>
    public int? Mtu { get; set; }

    /// <summary>
    /// Comments for the bridge
    /// </summary>
    public string? Comments { get; set; }
}

/// <summary>
/// VLAN configuration
/// </summary>
public class VlanConfig
{
    /// <summary>
    /// VLAN interface name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// VLAN ID
    /// </summary>
    public int VlanId { get; set; }

    /// <summary>
    /// Raw device (parent interface)
    /// </summary>
    public string RawDevice { get; set; } = string.Empty;

    /// <summary>
    /// IP address configuration method
    /// </summary>
    public string Method { get; set; } = "manual";

    /// <summary>
    /// IP address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Network mask
    /// </summary>
    public string? Netmask { get; set; }

    /// <summary>
    /// Gateway
    /// </summary>
    public string? Gateway { get; set; }

    /// <summary>
    /// Maximum Transmission Unit
    /// </summary>
    public int? Mtu { get; set; }

    /// <summary>
    /// Comments for the VLAN
    /// </summary>
    public string? Comments { get; set; }
}

/// <summary>
/// Bond configuration
/// </summary>
public class BondConfig
{
    /// <summary>
    /// Bond interface name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Bond slaves (space-separated list of interfaces)
    /// </summary>
    public string Slaves { get; set; } = string.Empty;

    /// <summary>
    /// Bond mode (balance-rr, active-backup, balance-xor, broadcast, 802.3ad, balance-tlb, balance-alb)
    /// </summary>
    public string Mode { get; set; } = "balance-rr";

    /// <summary>
    /// Hash policy for balance-xor and 802.3ad modes
    /// </summary>
    public string? XmitHashPolicy { get; set; }

    /// <summary>
    /// IP address configuration method
    /// </summary>
    public string Method { get; set; } = "manual";

    /// <summary>
    /// IP address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Network mask
    /// </summary>
    public string? Netmask { get; set; }

    /// <summary>
    /// Gateway
    /// </summary>
    public string? Gateway { get; set; }

    /// <summary>
    /// Maximum Transmission Unit
    /// </summary>
    public int? Mtu { get; set; }

    /// <summary>
    /// Comments for the bond
    /// </summary>
    public string? Comments { get; set; }
}
