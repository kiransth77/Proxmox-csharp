using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxmoxApi;
using ProxmoxApi.Models;
using ProxmoxApi.Services;

namespace ProxmoxApi.Examples;

/// <summary>
/// Example demonstrating comprehensive network management operations
/// </summary>
public static class NetworkManagementExample
{
    /// <summary>
    /// Demonstrates various network management operations
    /// </summary>
    /// <param name="client">Proxmox client</param>
    /// <param name="nodeName">Node name to use for operations</param>
    public static async Task RunNetworkManagementExampleAsync(ProxmoxClient client, string nodeName = "pve")
    {
        Console.WriteLine("=== Proxmox Network Management Example ===");
        Console.WriteLine();

        try
        {
            await DemonstrateNetworkInterfaceListingAsync(client, nodeName);
            await DemonstrateNetworkStatusAsync(client, nodeName);
            await DemonstrateBridgeManagementAsync(client, nodeName);
            await DemonstrateVlanManagementAsync(client, nodeName);
            await DemonstrateBondManagementAsync(client, nodeName);
            await DemonstrateFirewallManagementAsync(client, nodeName);
            await DemonstrateDnsManagementAsync(client, nodeName);
            await DemonstrateHostsManagementAsync(client, nodeName);
            // Note: Network creation/deletion examples commented out for safety
            // await DemonstrateNetworkLifecycleAsync(client, nodeName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Network management example failed: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("=== Network Management Example Complete ===");
    }

    /// <summary>
    /// Demonstrates listing network interfaces
    /// </summary>
    private static async Task DemonstrateNetworkInterfaceListingAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("🌐 Network Interface Listing");
        Console.WriteLine("===========================");

        // Get all network interfaces
        var interfaces = await client.Network.GetNetworkInterfacesAsync(nodeName);
        Console.WriteLine($"Found {interfaces.Count} network interfaces:");

        foreach (var networkInterface in interfaces)
        {
            Console.WriteLine($"  🔌 {networkInterface.InterfaceName} ({networkInterface.Type})");
            Console.WriteLine($"     Method: {networkInterface.Method}");
            Console.WriteLine($"     Address: {networkInterface.Address ?? "None"}");
            Console.WriteLine($"     Active: {(networkInterface.IsActive ? "✅" : "❌")}");
            Console.WriteLine($"     Autostart: {(networkInterface.IsAutostart ? "✅" : "❌")}");

            if (!string.IsNullOrEmpty(networkInterface.BridgePorts))
            {
                Console.WriteLine($"     Bridge Ports: {networkInterface.BridgePorts}");
            }

            if (networkInterface.VlanId.HasValue)
            {
                Console.WriteLine($"     VLAN ID: {networkInterface.VlanId}");
                Console.WriteLine($"     VLAN Device: {networkInterface.VlanRawDevice}");
            }

            if (!string.IsNullOrEmpty(networkInterface.BondSlaves))
            {
                Console.WriteLine($"     Bond Slaves: {networkInterface.BondSlaves}");
                Console.WriteLine($"     Bond Mode: {networkInterface.BondMode}");
            }

            if (networkInterface.Mtu.HasValue)
            {
                Console.WriteLine($"     MTU: {networkInterface.Mtu}");
            }

            Console.WriteLine();
        }

        // Get detailed info for first interface
        if (interfaces.Count > 0)
        {
            var firstInterface = interfaces[0];
            Console.WriteLine($"📋 Detailed configuration for '{firstInterface.InterfaceName}':");

            var interfaceDetails = await client.Network.GetNetworkInterfaceAsync(nodeName, firstInterface.InterfaceName);
            if (interfaceDetails != null)
            {
                Console.WriteLine($"  Type: {interfaceDetails.Type}");
                Console.WriteLine($"  Method: {interfaceDetails.Method}");
                Console.WriteLine($"  Address: {interfaceDetails.Address ?? "None"}");
                Console.WriteLine($"  Netmask: {interfaceDetails.Netmask ?? "None"}");
                Console.WriteLine($"  Gateway: {interfaceDetails.Gateway ?? "None"}");
                Console.WriteLine($"  Priority: {interfaceDetails.Priority?.ToString() ?? "Default"}");

                if (interfaceDetails.Options.Count > 0)
                {
                    Console.WriteLine("  Advanced Options:");
                    foreach (var option in interfaceDetails.Options)
                    {
                        Console.WriteLine($"    {option.Key}: {option.Value}");
                    }
                }
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates network status monitoring
    /// </summary>
    private static async Task DemonstrateNetworkStatusAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("📊 Network Status Monitoring");
        Console.WriteLine("============================");

        // Get network summary
        var summary = await client.Network.GetNetworkSummaryAsync(nodeName);
        Console.WriteLine($"Network Summary for node '{nodeName}':");

        foreach (var item in summary)
        {
            Console.WriteLine($"  {item.Key}: {item.Value}");
        }

        Console.WriteLine();

        // Get network status
        var status = await client.Network.GetNetworkStatusAsync(nodeName);
        if (status != null)
        {
            Console.WriteLine($"Network Status:");
            Console.WriteLine($"  Node: {status.NodeName}");
            Console.WriteLine($"  Version: {status.Version}");
            Console.WriteLine($"  Uptime: {TimeSpan.FromSeconds(status.Uptime):dd\\:hh\\:mm\\:ss}");
            Console.WriteLine($"  Interfaces: {status.Interfaces.Count}");

            foreach (var interfaceStatus in status.Interfaces.Take(5))
            {
                Console.WriteLine($"    🔌 {interfaceStatus.InterfaceName}:");
                Console.WriteLine($"       Status: {(interfaceStatus.Active ? "✅ Active" : "❌ Inactive")}");
                Console.WriteLine($"       Type: {interfaceStatus.Type}");
                Console.WriteLine($"       Method: {interfaceStatus.Method}");
                Console.WriteLine($"       Address: {interfaceStatus.Address ?? "None"}");
                Console.WriteLine($"       Exists: {(interfaceStatus.Exists ? "Yes" : "No")}");
                Console.WriteLine($"       Autostart: {(interfaceStatus.Autostart ? "Yes" : "No")}");
            }

            if (status.Interfaces.Count > 5)
            {
                Console.WriteLine($"    ... and {status.Interfaces.Count - 5} more interfaces");
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates bridge management
    /// </summary>
    private static async Task DemonstrateBridgeManagementAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("🌉 Bridge Management");
        Console.WriteLine("===================");

        // Get all bridges
        var bridges = await client.Network.GetBridgesAsync(nodeName);
        Console.WriteLine($"Found {bridges.Count} bridge interfaces:");

        foreach (var bridge in bridges)
        {
            Console.WriteLine($"  🌉 {bridge.InterfaceName}");
            Console.WriteLine($"     Address: {bridge.Address ?? "None"}");
            Console.WriteLine($"     Ports: {bridge.BridgePorts ?? "None"}");
            Console.WriteLine($"     STP: {(bridge.BridgeStp == "on" ? "Enabled" : "Disabled")}");
            Console.WriteLine($"     VLAN Aware: {(bridge.BridgeVlanAware == 1 ? "Yes" : "No")}");
            Console.WriteLine($"     Forward Delay: {bridge.BridgeForwardDelay ?? "Default"}");
            Console.WriteLine();
        }

        // Demonstrate bridge validation
        var bridgeConfig = new BridgeConfig
        {
            Name = "vmbr99",
            Ports = "eth1,eth2",
            SpanningTreeProtocol = false,
            VlanAware = true,
            ForwardDelay = 15,
            Comments = "Test bridge for example"
        };

        var bridgeOptions = new NetworkInterfaceCreateOptions
        {
            InterfaceName = bridgeConfig.Name,
            Type = "bridge",
            BridgePorts = bridgeConfig.Ports,
            BridgeVlanAware = bridgeConfig.VlanAware ? 1 : 0,
            Comments = bridgeConfig.Comments
        };

        var isValid = NetworkService.ValidateNetworkInterfaceConfig(bridgeOptions);
        Console.WriteLine($"Bridge configuration validation: {(isValid ? "✅ Valid" : "❌ Invalid")}");

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates VLAN management
    /// </summary>
    private static async Task DemonstrateVlanManagementAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("🏷️  VLAN Management");
        Console.WriteLine("==================");

        // Get all VLANs
        var vlans = await client.Network.GetVlansAsync(nodeName);
        Console.WriteLine($"Found {vlans.Count} VLAN interfaces:");

        foreach (var vlan in vlans)
        {
            Console.WriteLine($"  🏷️  {vlan.InterfaceName}");
            Console.WriteLine($"     VLAN ID: {vlan.VlanId}");
            Console.WriteLine($"     Raw Device: {vlan.VlanRawDevice}");
            Console.WriteLine($"     Address: {vlan.Address ?? "None"}");
            Console.WriteLine($"     Method: {vlan.Method}");
            Console.WriteLine();
        }

        // Demonstrate VLAN configuration examples
        var vlanConfigs = new[]
        {
            new VlanConfig
            {
                Name = "vlan100",
                VlanId = 100,
                RawDevice = "eth0",
                Method = "static",
                Address = "192.168.100.1",
                Netmask = "255.255.255.0",
                Comments = "Management VLAN"
            },
            new VlanConfig
            {
                Name = "vlan200",
                VlanId = 200,
                RawDevice = "eth1",
                Method = "dhcp",
                Comments = "Guest VLAN"
            }
        };

        Console.WriteLine("VLAN Configuration Examples:");
        foreach (var vlanConfig in vlanConfigs)
        {
            Console.WriteLine($"  📋 {vlanConfig.Name}:");
            Console.WriteLine($"     VLAN ID: {vlanConfig.VlanId}");
            Console.WriteLine($"     Raw Device: {vlanConfig.RawDevice}");
            Console.WriteLine($"     Method: {vlanConfig.Method}");
            Console.WriteLine($"     Address: {vlanConfig.Address ?? "DHCP"}");
            Console.WriteLine($"     Comments: {vlanConfig.Comments}");

            var vlanOptions = new NetworkInterfaceCreateOptions
            {
                InterfaceName = vlanConfig.Name,
                Type = "vlan",
                VlanId = vlanConfig.VlanId,
                VlanRawDevice = vlanConfig.RawDevice,
                Method = vlanConfig.Method,
                Address = vlanConfig.Address,
                Netmask = vlanConfig.Netmask,
                Comments = vlanConfig.Comments
            };

            var isValid = NetworkService.ValidateNetworkInterfaceConfig(vlanOptions);
            Console.WriteLine($"     Validation: {(isValid ? "✅ Valid" : "❌ Invalid")}");
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates bond management
    /// </summary>
    private static async Task DemonstrateBondManagementAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("🔗 Bond Management");
        Console.WriteLine("=================");

        // Get all bonds
        var bonds = await client.Network.GetBondsAsync(nodeName);
        Console.WriteLine($"Found {bonds.Count} bond interfaces:");

        foreach (var bond in bonds)
        {
            Console.WriteLine($"  🔗 {bond.InterfaceName}");
            Console.WriteLine($"     Slaves: {bond.BondSlaves ?? "None"}");
            Console.WriteLine($"     Mode: {bond.BondMode ?? "Default"}");
            Console.WriteLine($"     Hash Policy: {bond.BondXmitHashPolicy ?? "Default"}");
            Console.WriteLine($"     Address: {bond.Address ?? "None"}");
            Console.WriteLine();
        }

        // Demonstrate bond configuration examples
        var bondConfigs = new[]
        {
            new BondConfig
            {
                Name = "bond0",
                Slaves = "eth0 eth1",
                Mode = "active-backup",
                Method = "static",
                Address = "192.168.1.10",
                Netmask = "255.255.255.0",
                Gateway = "192.168.1.1",
                Comments = "Primary bond interface"
            },
            new BondConfig
            {
                Name = "bond1",
                Slaves = "eth2 eth3",
                Mode = "balance-rr",
                XmitHashPolicy = "layer2",
                Method = "dhcp",
                Comments = "Load balancing bond"
            }
        };

        Console.WriteLine("Bond Configuration Examples:");
        foreach (var bondConfig in bondConfigs)
        {
            Console.WriteLine($"  📋 {bondConfig.Name}:");
            Console.WriteLine($"     Slaves: {bondConfig.Slaves}");
            Console.WriteLine($"     Mode: {bondConfig.Mode}");
            Console.WriteLine($"     Hash Policy: {bondConfig.XmitHashPolicy ?? "Default"}");
            Console.WriteLine($"     Method: {bondConfig.Method}");
            Console.WriteLine($"     Address: {bondConfig.Address ?? "DHCP"}");
            Console.WriteLine($"     Comments: {bondConfig.Comments}");

            var bondOptions = new NetworkInterfaceCreateOptions
            {
                InterfaceName = bondConfig.Name,
                Type = "bond",
                BondSlaves = bondConfig.Slaves,
                BondMode = bondConfig.Mode,
                BondXmitHashPolicy = bondConfig.XmitHashPolicy,
                Method = bondConfig.Method,
                Address = bondConfig.Address,
                Netmask = bondConfig.Netmask,
                Gateway = bondConfig.Gateway,
                Comments = bondConfig.Comments
            };

            var isValid = NetworkService.ValidateNetworkInterfaceConfig(bondOptions);
            Console.WriteLine($"     Validation: {(isValid ? "✅ Valid" : "❌ Invalid")}");
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates firewall management
    /// </summary>
    private static async Task DemonstrateFirewallManagementAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("🛡️  Firewall Management");
        Console.WriteLine("======================");

        // Get firewall rules
        var firewallRules = await client.Network.GetFirewallRulesAsync(nodeName);
        Console.WriteLine($"Found {firewallRules.Count} firewall rules:");

        foreach (var rule in firewallRules.Take(10)) // Show first 10 rules
        {
            Console.WriteLine($"  🛡️  Rule {rule.Position}:");
            Console.WriteLine($"     Action: {rule.Action}");
            Console.WriteLine($"     Type: {rule.Type}");
            Console.WriteLine($"     Enabled: {(rule.IsEnabled ? "✅" : "❌")}");
            Console.WriteLine($"     Source: {rule.Source ?? "Any"}");
            Console.WriteLine($"     Destination: {rule.Destination ?? "Any"}");
            Console.WriteLine($"     Protocol: {rule.Protocol ?? "Any"}");
            Console.WriteLine($"     Dest Port: {rule.DestinationPort ?? "Any"}");
            Console.WriteLine($"     Comment: {rule.Comment ?? "None"}");
            Console.WriteLine();
        }

        if (firewallRules.Count > 10)
        {
            Console.WriteLine($"... and {firewallRules.Count - 10} more firewall rules");
        }

        // Demonstrate firewall rule examples
        var ruleExamples = new[]
        {
            new FirewallRuleCreateOptions
            {
                Action = "ACCEPT",
                Type = "in",
                Source = "192.168.1.0/24",
                DestinationPort = "22",
                Protocol = "tcp",
                Comment = "SSH access from LAN"
            },
            new FirewallRuleCreateOptions
            {
                Action = "ACCEPT",
                Type = "in",
                Source = "10.0.0.0/8",
                DestinationPort = "80,443",
                Protocol = "tcp",
                Comment = "HTTP/HTTPS from internal networks"
            },
            new FirewallRuleCreateOptions
            {
                Action = "REJECT",
                Type = "out",
                Destination = "192.168.100.0/24",
                Comment = "Block access to guest network"
            }
        };

        Console.WriteLine("Firewall Rule Examples:");
        foreach (var ruleExample in ruleExamples)
        {
            Console.WriteLine($"  📋 {ruleExample.Comment}:");
            Console.WriteLine($"     Action: {ruleExample.Action}");
            Console.WriteLine($"     Type: {ruleExample.Type}");
            Console.WriteLine($"     Source: {ruleExample.Source ?? "Any"}");
            Console.WriteLine($"     Destination: {ruleExample.Destination ?? "Any"}");
            Console.WriteLine($"     Protocol: {ruleExample.Protocol ?? "Any"}");
            Console.WriteLine($"     Dest Port: {ruleExample.DestinationPort ?? "Any"}");
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates DNS management
    /// </summary>
    private static async Task DemonstrateDnsManagementAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("🌐 DNS Management");
        Console.WriteLine("================");

        // Get DNS configuration
        var dnsConfig = await client.Network.GetDnsConfigAsync(nodeName);
        if (dnsConfig != null)
        {
            Console.WriteLine($"Current DNS configuration for node '{nodeName}':");
            Console.WriteLine($"  Search Domain: {dnsConfig.SearchDomain ?? "None"}");
            Console.WriteLine($"  Primary DNS: {dnsConfig.Dns1 ?? "None"}");
            Console.WriteLine($"  Secondary DNS: {dnsConfig.Dns2 ?? "None"}");
            Console.WriteLine($"  Tertiary DNS: {dnsConfig.Dns3 ?? "None"}");
        }
        else
        {
            Console.WriteLine("No DNS configuration found.");
        }

        // Demonstrate DNS configuration examples
        var dnsExamples = new[]
        {
            new DnsConfig
            {
                SearchDomain = "example.com",
                Dns1 = "8.8.8.8",
                Dns2 = "8.8.4.4",
                Dns3 = "1.1.1.1"
            },
            new DnsConfig
            {
                SearchDomain = "internal.local",
                Dns1 = "192.168.1.1",
                Dns2 = "192.168.1.2",
                Dns3 = "8.8.8.8"
            }
        };

        Console.WriteLine();
        Console.WriteLine("DNS Configuration Examples:");
        foreach (var dnsExample in dnsExamples)
        {
            Console.WriteLine($"  📋 {dnsExample.SearchDomain} domain:");
            Console.WriteLine($"     Search Domain: {dnsExample.SearchDomain}");
            Console.WriteLine($"     Primary DNS: {dnsExample.Dns1}");
            Console.WriteLine($"     Secondary DNS: {dnsExample.Dns2}");
            Console.WriteLine($"     Tertiary DNS: {dnsExample.Dns3}");
            Console.WriteLine();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates hosts file management
    /// </summary>
    private static async Task DemonstrateHostsManagementAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("📝 Hosts File Management");
        Console.WriteLine("=======================");

        // Get host entries
        var hostEntries = await client.Network.GetHostEntriesAsync(nodeName);
        Console.WriteLine($"Found {hostEntries.Count} host entries:");

        foreach (var hostEntry in hostEntries)
        {
            Console.WriteLine($"  📝 {hostEntry.IpAddress} -> {hostEntry.Hostname}");
        }

        // Demonstrate host entry examples
        var hostExamples = new[]
        {
            new HostEntry { IpAddress = "127.0.0.1", Hostname = "localhost" },
            new HostEntry { IpAddress = "192.168.1.10", Hostname = "server1.example.com server1" },
            new HostEntry { IpAddress = "192.168.1.20", Hostname = "server2.example.com server2" },
            new HostEntry { IpAddress = "10.0.0.1", Hostname = "gateway.internal.local gateway" }
        };

        Console.WriteLine();
        Console.WriteLine("Host Entry Examples:");
        foreach (var hostExample in hostExamples)
        {
            Console.WriteLine($"  📋 {hostExample.IpAddress} -> {hostExample.Hostname}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates network lifecycle management (creation/deletion)
    /// WARNING: These operations modify network configuration
    /// </summary>
    private static async Task DemonstrateNetworkLifecycleAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("⚠️  Network Lifecycle Management (DANGEROUS)");
        Console.WriteLine("============================================");
        Console.WriteLine("NOTE: This example demonstrates network interface creation/deletion.");
        Console.WriteLine("These operations are commented out for safety.");

        /*
        // Example: Create a test bridge
        var bridgeConfig = new BridgeConfig
        {
            Name = "vmbr99",
            Ports = "eth1",
            SpanningTreeProtocol = false,
            VlanAware = true,
            Comments = "Test bridge"
        };

        Console.WriteLine($"Creating bridge '{bridgeConfig.Name}'...");
        await client.Network.CreateBridgeAsync(nodeName, bridgeConfig);
        Console.WriteLine("✅ Bridge created successfully");

        // Example: Create a test VLAN
        var vlanConfig = new VlanConfig
        {
            Name = "vlan999",
            VlanId = 999,
            RawDevice = "vmbr99",
            Method = "manual",
            Comments = "Test VLAN"
        };

        Console.WriteLine($"Creating VLAN '{vlanConfig.Name}'...");
        await client.Network.CreateVlanAsync(nodeName, vlanConfig);
        Console.WriteLine("✅ VLAN created successfully");

        // Example: Create a firewall rule
        var firewallRule = new FirewallRuleCreateOptions
        {
            Action = "ACCEPT",
            Type = "in",
            Source = "192.168.1.0/24",
            DestinationPort = "8080",
            Protocol = "tcp",
            Comment = "Test rule"
        };

        Console.WriteLine("Creating firewall rule...");
        await client.Network.CreateFirewallRuleAsync(nodeName, firewallRule);
        Console.WriteLine("✅ Firewall rule created successfully");

        // Wait a moment
        await Task.Delay(2000);

        // Clean up: Delete the test interfaces
        Console.WriteLine("Cleaning up test interfaces...");
        await client.Network.DeleteNetworkInterfaceAsync(nodeName, "vlan999");
        await client.Network.DeleteNetworkInterfaceAsync(nodeName, "vmbr99");
        Console.WriteLine("✅ Test interfaces deleted successfully");

        // Apply network configuration
        Console.WriteLine("Applying network configuration...");
        await client.Network.ApplyNetworkConfigurationAsync(nodeName);
        Console.WriteLine("✅ Network configuration applied successfully");
        */

        Console.WriteLine("Network lifecycle operations completed (simulated).");
        Console.WriteLine();
        await Task.CompletedTask;
    }
}
