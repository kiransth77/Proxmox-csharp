using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ProxmoxApi;
using ProxmoxApi.Models;

namespace ProxmoxApi.Tests.Integration;

/// <summary>
/// Integration tests for ProxmoxApi. These tests require a real Proxmox VE instance.
/// Set the PROXMOX_TEST_HOST, PROXMOX_TEST_USER, and PROXMOX_TEST_PASS environment variables to run these tests.
/// </summary>
public class ProxmoxApiIntegrationTests : IDisposable
{
    private readonly ProxmoxClient? _proxmoxClient;
    private readonly bool _canRunIntegrationTests;

    public ProxmoxApiIntegrationTests()
    {
        var host = Environment.GetEnvironmentVariable("PROXMOX_TEST_HOST");
        var username = Environment.GetEnvironmentVariable("PROXMOX_TEST_USER");
        var password = Environment.GetEnvironmentVariable("PROXMOX_TEST_PASS");

        _canRunIntegrationTests = !string.IsNullOrEmpty(host) && 
                                 !string.IsNullOrEmpty(username) && 
                                 !string.IsNullOrEmpty(password);

        if (_canRunIntegrationTests)
        {
            var connectionInfo = new ProxmoxConnectionInfo
            {
                Host = host!,
                Port = 8006,
                Username = username!,
                Password = password!,
                Realm = "pve"
            };

            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<ProxmoxClient>>();

            _proxmoxClient = new ProxmoxClient(connectionInfo, logger);
        }
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldSucceed_WithValidCredentials()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Act & Assert - Should not throw exception
        await _proxmoxClient.AuthenticateAsync();
    }

    [Fact]
    public async Task GetNodesAsync_ShouldReturnNodes_WhenAuthenticated()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Arrange
        await _proxmoxClient.AuthenticateAsync();

        // Act
        var nodes = await _proxmoxClient.Nodes.GetNodesAsync();

        // Assert
        Assert.NotNull(nodes);
        Assert.NotEmpty(nodes);
        Assert.All(nodes, node => 
        {
            Assert.NotNull(node.Node);
            Assert.NotEmpty(node.Node);
        });
    }

    [Fact]
    public async Task GetVirtualMachinesAsync_ShouldReturnVms_WhenNodeExists()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Arrange
        await _proxmoxClient.AuthenticateAsync();
        var nodes = await _proxmoxClient.Nodes.GetNodesAsync();
        
        if (!nodes.Any())
        {
            return; // Skip if no nodes available
        }

        var firstNode = nodes.First().Node;        // Act
        var vms = await _proxmoxClient.Vms.GetVmsOnNodeAsync(firstNode);

        // Assert
        Assert.NotNull(vms);
        // Note: VMs list can be empty, so we just check it's not null
    }

    [Fact]
    public async Task GetNodeStatusAsync_ShouldReturnStatus_WhenNodeExists()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Arrange
        await _proxmoxClient.AuthenticateAsync();
        var nodes = await _proxmoxClient.Nodes.GetNodesAsync();
        
        if (!nodes.Any())
        {
            return; // Skip if no nodes available
        }

        var firstNode = nodes.First().Node;

        // Act
        var status = await _proxmoxClient.Nodes.GetNodeStatusAsync(firstNode);        // Assert
        Assert.NotNull(status);
        Assert.True(status.Time > 0);
        Assert.True(status.Uptime >= 0);
    }    [Fact]
    public async Task VmLifecycle_ShouldWork_WithExistingRunningVm()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Arrange
        await _proxmoxClient.AuthenticateAsync();
        var nodes = await _proxmoxClient.Nodes.GetNodesAsync();
        
        if (!nodes.Any())
        {
            return; // Skip if no nodes available
        }

        var firstNode = nodes.First().Node;
        var vms = await _proxmoxClient.Vms.GetVmsOnNodeAsync(firstNode);
        var runningVm = vms.FirstOrDefault(vm => vm.Status == "running");

        if (runningVm == null)
        {
            return; // Skip if no running VM available for testing
        }

        // Act - Get VM status
        var vmStatus = await _proxmoxClient.Vms.GetVmStatusAsync(firstNode, runningVm.VmId);

        // Assert
        Assert.NotNull(vmStatus);
        Assert.Equal(runningVm.VmId, vmStatus.VmId);
        Assert.Equal("running", vmStatus.Status);
    }

    [Fact]
    public async Task GetContainersAsync_ShouldReturnContainers_WhenNodeExists()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Arrange
        await _proxmoxClient.AuthenticateAsync();
        var nodes = await _proxmoxClient.Nodes.GetNodesAsync();
        
        if (!nodes.Any())
        {
            return; // Skip if no nodes available
        }

        var firstNode = nodes.First().Node;

        // Act
        var containers = await _proxmoxClient.Containers.GetContainersAsync(firstNode);

        // Assert
        Assert.NotNull(containers);
        // Note: Containers list can be empty, so we just check it's not null
    }

    [Fact]
    public async Task GetContainerStatusAsync_ShouldReturnStatus_WhenContainerExists()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Arrange
        await _proxmoxClient.AuthenticateAsync();
        var nodes = await _proxmoxClient.Nodes.GetNodesAsync();
        
        if (!nodes.Any())
        {
            return; // Skip if no nodes available
        }

        var firstNode = nodes.First().Node;
        var containers = await _proxmoxClient.Containers.GetContainersAsync(firstNode);
        var firstContainer = containers.FirstOrDefault();

        if (firstContainer == null)
        {
            return; // Skip if no containers available for testing
        }

        // Act
        var containerStatus = await _proxmoxClient.Containers.GetContainerStatusAsync(firstNode, firstContainer.ContainerId);

        // Assert
        Assert.NotNull(containerStatus);
        Assert.NotNull(containerStatus.Status);
        Assert.NotEmpty(containerStatus.Status);
    }

    [Fact]
    public async Task GetContainerConfigAsync_ShouldReturnConfig_WhenContainerExists()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Arrange
        await _proxmoxClient.AuthenticateAsync();
        var nodes = await _proxmoxClient.Nodes.GetNodesAsync();
        
        if (!nodes.Any())
        {
            return; // Skip if no nodes available
        }

        var firstNode = nodes.First().Node;
        var containers = await _proxmoxClient.Containers.GetContainersAsync(firstNode);
        var firstContainer = containers.FirstOrDefault();

        if (firstContainer == null)
        {
            return; // Skip if no containers available for testing
        }

        // Act
        var containerConfig = await _proxmoxClient.Containers.GetContainerConfigAsync(firstNode, firstContainer.ContainerId);        // Assert
        Assert.NotNull(containerConfig);
        Assert.NotNull(containerConfig.Architecture);
        Assert.NotEmpty(containerConfig.Architecture);
    }

    [Fact]
    public async Task GetContainerStatisticsAsync_ShouldReturnStatistics_WhenRunningContainerExists()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Arrange
        await _proxmoxClient.AuthenticateAsync();
        var nodes = await _proxmoxClient.Nodes.GetNodesAsync();
        
        if (!nodes.Any())
        {
            return; // Skip if no nodes available
        }

        var firstNode = nodes.First().Node;
        var containers = await _proxmoxClient.Containers.GetContainersAsync(firstNode);
        var runningContainer = containers.FirstOrDefault(c => c.Status == "running");

        if (runningContainer == null)
        {
            return; // Skip if no running container available for testing
        }

        // Act
        var containerStats = await _proxmoxClient.Containers.GetContainerStatisticsAsync(firstNode, runningContainer.ContainerId);

        // Assert
        Assert.NotNull(containerStats);
        Assert.True(containerStats.CpuUsage >= 0);
        Assert.True(containerStats.MemoryUsage >= 0);
        Assert.True(containerStats.MaxMemory > 0);
    }

    [Fact]
    public async Task GetContainerSnapshotsAsync_ShouldReturnSnapshots_WhenContainerExists()
    {
        // Skip if integration test environment is not set up
        if (!_canRunIntegrationTests || _proxmoxClient == null)
        {
            return; // Skip test
        }

        // Arrange
        await _proxmoxClient.AuthenticateAsync();
        var nodes = await _proxmoxClient.Nodes.GetNodesAsync();
        
        if (!nodes.Any())
        {
            return; // Skip if no nodes available
        }

        var firstNode = nodes.First().Node;
        var containers = await _proxmoxClient.Containers.GetContainersAsync(firstNode);
        var firstContainer = containers.FirstOrDefault();

        if (firstContainer == null)
        {
            return; // Skip if no containers available for testing
        }

        // Act
        var containerSnapshots = await _proxmoxClient.Containers.GetContainerSnapshotsAsync(firstNode, firstContainer.ContainerId);

        // Assert
        Assert.NotNull(containerSnapshots);
        // Note: Snapshots list can be empty, so we just check it's not null
    }

    public void Dispose()
    {
        _proxmoxClient?.Dispose();
    }
}
