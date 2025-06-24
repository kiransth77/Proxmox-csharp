using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using ProxmoxApi.Core;
using ProxmoxApi.Models;
using ProxmoxApi.Services;
using Xunit;

namespace ProxmoxApi.Tests.Services;

public class NetworkServiceTests
{
    private readonly Mock<IProxmoxHttpClient> _mockHttpClient;
    private readonly Mock<ILogger<NetworkService>> _mockLogger;
    private readonly NetworkService _networkService;
    private const string TestNodeName = "test-node";

    public NetworkServiceTests()
    {
        _mockHttpClient = new Mock<IProxmoxHttpClient>();
        _mockLogger = new Mock<ILogger<NetworkService>>();
        _networkService = new NetworkService(_mockHttpClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetNetworkInterfacesAsync_ReturnsListOfInterfaces()
    {
        // Arrange
        var expectedInterfaces = new List<NetworkInterface>
        {
            new() { InterfaceName = "eth0", Type = "eth", Method = "static", Address = "192.168.1.10" },
            new() { InterfaceName = "vmbr0", Type = "bridge", Method = "static", Address = "192.168.1.1" }
        };

        _mockHttpClient.Setup(x => x.GetAsync<List<NetworkInterface>>(
                $"nodes/{TestNodeName}/network", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInterfaces);

        // Act
        var result = await _networkService.GetNetworkInterfacesAsync(TestNodeName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("eth0", result[0].InterfaceName);
        Assert.Equal("vmbr0", result[1].InterfaceName);
        _mockHttpClient.Verify(x => x.GetAsync<List<NetworkInterface>>(
            $"nodes/{TestNodeName}/network", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNetworkInterfaceAsync_ReturnsSpecificInterface()
    {
        // Arrange
        const string interfaceName = "eth0";
        var expectedInterface = new NetworkInterface 
        { 
            InterfaceName = interfaceName, 
            Type = "eth", 
            Method = "static", 
            Address = "192.168.1.10" 
        };

        _mockHttpClient.Setup(x => x.GetAsync<NetworkInterface>(
                $"nodes/{TestNodeName}/network/{interfaceName}", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedInterface);

        // Act
        var result = await _networkService.GetNetworkInterfaceAsync(TestNodeName, interfaceName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(interfaceName, result.InterfaceName);
        Assert.Equal("eth", result.Type);
        _mockHttpClient.Verify(x => x.GetAsync<NetworkInterface>(
            $"nodes/{TestNodeName}/network/{interfaceName}", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateNetworkInterfaceAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var options = new NetworkInterfaceCreateOptions
        {
            InterfaceName = "vmbr1",
            Type = "bridge",
            Method = "static",
            Address = "192.168.2.1"
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>(
                $"nodes/{TestNodeName}/network", 
                It.IsAny<NetworkInterfaceCreateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _networkService.CreateNetworkInterfaceAsync(TestNodeName, options);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>(
            $"nodes/{TestNodeName}/network", 
            It.Is<NetworkInterfaceCreateOptions>(o => o.InterfaceName == "vmbr1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateNetworkInterfaceAsync_CallsCorrectEndpoint()
    {
        // Arrange
        const string interfaceName = "vmbr1";
        var options = new NetworkInterfaceUpdateOptions
        {
            Method = "dhcp"
        };

        _mockHttpClient.Setup(x => x.PutAsync<object>(
                $"nodes/{TestNodeName}/network/{interfaceName}", 
                It.IsAny<NetworkInterfaceUpdateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _networkService.UpdateNetworkInterfaceAsync(TestNodeName, interfaceName, options);

        // Assert
        _mockHttpClient.Verify(x => x.PutAsync<object>(
            $"nodes/{TestNodeName}/network/{interfaceName}", 
            It.Is<NetworkInterfaceUpdateOptions>(o => o.Method == "dhcp"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteNetworkInterfaceAsync_CallsCorrectEndpoint()
    {
        // Arrange
        const string interfaceName = "vmbr1";

        _mockHttpClient.Setup(x => x.DeleteAsync<object>(
                $"nodes/{TestNodeName}/network/{interfaceName}", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _networkService.DeleteNetworkInterfaceAsync(TestNodeName, interfaceName);

        // Assert
        _mockHttpClient.Verify(x => x.DeleteAsync<object>(
            $"nodes/{TestNodeName}/network/{interfaceName}", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFirewallRulesAsync_ReturnsListOfRules()
    {
        // Arrange
        var expectedRules = new List<FirewallRule>
        {
            new() { Position = 0, Action = "ACCEPT", Type = "in", Source = "192.168.1.0/24" },
            new() { Position = 1, Action = "REJECT", Type = "out", Destination = "192.168.2.0/24" }
        };

        _mockHttpClient.Setup(x => x.GetAsync<List<FirewallRule>>(
                $"nodes/{TestNodeName}/firewall/rules", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRules);

        // Act
        var result = await _networkService.GetFirewallRulesAsync(TestNodeName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(0, result[0].Position);
        Assert.Equal("ACCEPT", result[0].Action);
        _mockHttpClient.Verify(x => x.GetAsync<List<FirewallRule>>(
            $"nodes/{TestNodeName}/firewall/rules", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFirewallRuleAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var options = new FirewallRuleCreateOptions
        {
            Action = "ACCEPT",
            Type = "in",
            Source = "192.168.1.0/24",
            DestinationPort = "22"
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>(
                $"nodes/{TestNodeName}/firewall/rules", 
                It.IsAny<FirewallRuleCreateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _networkService.CreateFirewallRuleAsync(TestNodeName, options);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>(
            $"nodes/{TestNodeName}/firewall/rules", 
            It.Is<FirewallRuleCreateOptions>(o => o.Action == "ACCEPT" && o.DestinationPort == "22"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDnsConfigAsync_ReturnsDnsConfiguration()
    {
        // Arrange
        var expectedDnsConfig = new DnsConfig
        {
            SearchDomain = "example.com",
            Dns1 = "8.8.8.8",
            Dns2 = "8.8.4.4"
        };

        _mockHttpClient.Setup(x => x.GetAsync<DnsConfig>(
                $"nodes/{TestNodeName}/dns", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDnsConfig);

        // Act
        var result = await _networkService.GetDnsConfigAsync(TestNodeName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("example.com", result.SearchDomain);
        Assert.Equal("8.8.8.8", result.Dns1);
        _mockHttpClient.Verify(x => x.GetAsync<DnsConfig>(
            $"nodes/{TestNodeName}/dns", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateDnsConfigAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var dnsConfig = new DnsConfig
        {
            SearchDomain = "example.com",
            Dns1 = "1.1.1.1",
            Dns2 = "1.0.0.1"
        };

        _mockHttpClient.Setup(x => x.PutAsync<object>(
                $"nodes/{TestNodeName}/dns", 
                It.IsAny<DnsConfig>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _networkService.UpdateDnsConfigAsync(TestNodeName, dnsConfig);

        // Assert
        _mockHttpClient.Verify(x => x.PutAsync<object>(
            $"nodes/{TestNodeName}/dns", 
            It.Is<DnsConfig>(d => d.Dns1 == "1.1.1.1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetHostEntriesAsync_ReturnsListOfHostEntries()
    {
        // Arrange
        var expectedHostEntries = new List<HostEntry>
        {
            new() { IpAddress = "127.0.0.1", Hostname = "localhost" },
            new() { IpAddress = "192.168.1.10", Hostname = "server.example.com" }
        };

        _mockHttpClient.Setup(x => x.GetAsync<List<HostEntry>>(
                $"nodes/{TestNodeName}/hosts", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHostEntries);

        // Act
        var result = await _networkService.GetHostEntriesAsync(TestNodeName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("localhost", result[0].Hostname);
        Assert.Equal("192.168.1.10", result[1].IpAddress);
        _mockHttpClient.Verify(x => x.GetAsync<List<HostEntry>>(
            $"nodes/{TestNodeName}/hosts", 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBridgeAsync_CallsCreateNetworkInterface()
    {
        // Arrange
        var bridgeConfig = new BridgeConfig
        {
            Name = "vmbr1",
            Ports = "eth0,eth1",
            SpanningTreeProtocol = true,
            VlanAware = true
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>(
                $"nodes/{TestNodeName}/network", 
                It.IsAny<NetworkInterfaceCreateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _networkService.CreateBridgeAsync(TestNodeName, bridgeConfig);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>(
            $"nodes/{TestNodeName}/network", 
            It.Is<NetworkInterfaceCreateOptions>(o => 
                o.InterfaceName == "vmbr1" && 
                o.Type == "bridge" && 
                o.BridgePorts == "eth0,eth1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVlanAsync_CallsCreateNetworkInterface()
    {
        // Arrange
        var vlanConfig = new VlanConfig
        {
            Name = "vlan100",
            VlanId = 100,
            RawDevice = "eth0",
            Method = "static",
            Address = "192.168.100.1"
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>(
                $"nodes/{TestNodeName}/network", 
                It.IsAny<NetworkInterfaceCreateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _networkService.CreateVlanAsync(TestNodeName, vlanConfig);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>(
            $"nodes/{TestNodeName}/network", 
            It.Is<NetworkInterfaceCreateOptions>(o => 
                o.InterfaceName == "vlan100" && 
                o.Type == "vlan" && 
                o.VlanId == 100 &&
                o.VlanRawDevice == "eth0"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBondAsync_CallsCreateNetworkInterface()
    {
        // Arrange
        var bondConfig = new BondConfig
        {
            Name = "bond0",
            Slaves = "eth0 eth1",
            Mode = "active-backup",
            Method = "static",
            Address = "192.168.1.10"
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>(
                $"nodes/{TestNodeName}/network", 
                It.IsAny<NetworkInterfaceCreateOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _networkService.CreateBondAsync(TestNodeName, bondConfig);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>(
            $"nodes/{TestNodeName}/network", 
            It.Is<NetworkInterfaceCreateOptions>(o => 
                o.InterfaceName == "bond0" && 
                o.Type == "bond" && 
                o.BondSlaves == "eth0 eth1" &&
                o.BondMode == "active-backup"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]  
    public async Task GetBridgesAsync_FiltersOnlyBridgeInterfaces()
    {
        // Arrange
        var allInterfaces = new List<NetworkInterface>
        {
            new() { InterfaceName = "eth0", Type = "eth" },
            new() { InterfaceName = "vmbr0", Type = "bridge" },
            new() { InterfaceName = "vlan100", Type = "vlan" },
            new() { InterfaceName = "vmbr1", Type = "bridge" }
        };

        _mockHttpClient.Setup(x => x.GetAsync<List<NetworkInterface>>(
                $"nodes/{TestNodeName}/network", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(allInterfaces);

        // Act
        var result = await _networkService.GetBridgesAsync(TestNodeName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, bridge => Assert.Equal("bridge", bridge.Type));
    }

    [Fact]
    public async Task GetNetworkSummaryAsync_ReturnsCorrectSummary()
    {
        // Arrange
        var interfaces = new List<NetworkInterface>
        {
            new() { InterfaceName = "eth0", Type = "eth", Active = 1, Autostart = 1 },
            new() { InterfaceName = "vmbr0", Type = "bridge", Active = 1, Autostart = 1 },
            new() { InterfaceName = "vlan100", Type = "vlan", Active = 0, Autostart = 1 },
            new() { InterfaceName = "bond0", Type = "bond", Active = 1, Autostart = 0 }
        };

        _mockHttpClient.Setup(x => x.GetAsync<List<NetworkInterface>>(
                $"nodes/{TestNodeName}/network", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(interfaces);

        // Act
        var result = await _networkService.GetNetworkSummaryAsync(TestNodeName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result["TotalInterfaces"]);
        Assert.Equal(1, result["BridgeCount"]);
        Assert.Equal(1, result["VlanCount"]);
        Assert.Equal(1, result["BondCount"]);
        Assert.Equal(3, result["ActiveInterfaces"]);
        Assert.Equal(3, result["AutostartInterfaces"]);
    }

    [Fact]
    public void ValidateNetworkInterfaceConfig_ValidBridge_ReturnsTrue()
    {
        // Arrange
        var options = new NetworkInterfaceCreateOptions
        {
            InterfaceName = "vmbr0",
            Type = "bridge",
            BridgePorts = "eth0,eth1"
        };

        // Act
        var result = NetworkService.ValidateNetworkInterfaceConfig(options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateNetworkInterfaceConfig_ValidVlan_ReturnsTrue()
    {
        // Arrange
        var options = new NetworkInterfaceCreateOptions
        {
            InterfaceName = "vlan100",
            Type = "vlan",
            VlanId = 100,
            VlanRawDevice = "eth0"
        };

        // Act
        var result = NetworkService.ValidateNetworkInterfaceConfig(options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateNetworkInterfaceConfig_ValidBond_ReturnsTrue()
    {
        // Arrange
        var options = new NetworkInterfaceCreateOptions
        {
            InterfaceName = "bond0",
            Type = "bond",
            BondSlaves = "eth0 eth1"
        };

        // Act
        var result = NetworkService.ValidateNetworkInterfaceConfig(options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateNetworkInterfaceConfig_InvalidBridge_ReturnsFalse()
    {
        // Arrange
        var options = new NetworkInterfaceCreateOptions
        {
            InterfaceName = "vmbr0",
            Type = "bridge"
            // Missing BridgePorts
        };

        // Act
        var result = NetworkService.ValidateNetworkInterfaceConfig(options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateNetworkInterfaceConfig_InvalidVlan_ReturnsFalse()
    {
        // Arrange
        var options = new NetworkInterfaceCreateOptions
        {
            InterfaceName = "vlan100",
            Type = "vlan"
            // Missing VlanId and VlanRawDevice
        };

        // Act
        var result = NetworkService.ValidateNetworkInterfaceConfig(options);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetNetworkStatusAsync_ReturnsNetworkStatus()
    {
        // Arrange
        var expectedStatus = new NetworkStatus
        {
            NodeName = TestNodeName,
            Version = 1,
            Uptime = 12345,
            Interfaces = new List<NetworkInterfaceStatus>
            {
                new() { InterfaceName = "eth0", Active = true, Type = "eth" }
            }
        };

        _mockHttpClient.Setup(x => x.GetAsync<NetworkStatus>(
                $"nodes/{TestNodeName}/status", 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _networkService.GetNetworkStatusAsync(TestNodeName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestNodeName, result.NodeName);
        Assert.Equal(1, result.Version);
        Assert.Single(result.Interfaces);
    }

    [Fact]
    public async Task ApplyNetworkConfigurationAsync_CallsCorrectEndpoint()
    {
        // Arrange
        _mockHttpClient.Setup(x => x.PutAsync<object>(
                $"nodes/{TestNodeName}/network", 
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _networkService.ApplyNetworkConfigurationAsync(TestNodeName);

        // Assert
        _mockHttpClient.Verify(x => x.PutAsync<object>(
            $"nodes/{TestNodeName}/network", 
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
