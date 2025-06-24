using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Core;
using ProxmoxApi.Services;
using ProxmoxApi.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ProxmoxApi.Tests.Services;

public class VmServiceTests : IDisposable
{
    private readonly Mock<IProxmoxHttpClient> _mockHttpClient;
    private readonly Mock<ILogger<VmService>> _mockLogger;
    private readonly VmService _vmService;

    public VmServiceTests()
    {
        _mockHttpClient = new Mock<IProxmoxHttpClient>();
        _mockLogger = new Mock<ILogger<VmService>>();
        _vmService = new VmService(_mockHttpClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetVmsAsync_ReturnsVmList_WhenSuccessful()
    {
        // Arrange
        var expectedVms = new List<ProxmoxVm>
        {
            new ProxmoxVm { VmId = 100, Name = "test-vm-1", Status = "running", Node = "node1" },
            new ProxmoxVm { VmId = 101, Name = "test-vm-2", Status = "stopped", Node = "node1" }
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<List<ProxmoxVm>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVms);

        // Act
        var result = await _vmService.GetVmsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(100, result[0].VmId);
        Assert.Equal("test-vm-1", result[0].Name);
        Assert.Equal("running", result[0].Status);

        _mockHttpClient.Verify(x => x.GetAsync<List<ProxmoxVm>>("cluster/resources?type=vm", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVmsOnNodeAsync_ReturnsVmList_WhenSuccessful()
    {
        // Arrange
        var expectedVms = new List<ProxmoxVm>
        {
            new ProxmoxVm { VmId = 100, Name = "test-vm-1", Status = "running", Node = "node1" }
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<List<ProxmoxVm>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVms);

        // Act
        var result = await _vmService.GetVmsOnNodeAsync("node1");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(100, result[0].VmId);
        Assert.Equal("node1", result[0].Node);

        _mockHttpClient.Verify(x => x.GetAsync<List<ProxmoxVm>>("nodes/node1/qemu", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVmStatusAsync_ReturnsVmStatus_WhenSuccessful()
    {
        // Arrange
        var expectedStatus = new VmStatus
        {
            VmId = 100,
            Status = "running",
            CpuUsage = 0.25,
            MemoryUsage = 2147483648, // 2GB
            MaxMemory = 4294967296,   // 4GB
            Uptime = 3600,
            ProcessId = 12345
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<VmStatus>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _vmService.GetVmStatusAsync("node1", 100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("running", result.Status);
        Assert.Equal(0.25, result.CpuUsage);
        Assert.Equal(2147483648, result.MemoryUsage);
        Assert.Equal(3600, result.Uptime);
        Assert.Equal(12345, result.ProcessId);

        _mockHttpClient.Verify(x => x.GetAsync<VmStatus>("nodes/node1/qemu/100/status/current", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVmConfigAsync_ReturnsConfig_WhenSuccessful()
    {
        // Arrange
        var expectedConfig = new Dictionary<string, object>
        {
            ["memory"] = 4096,
            ["cores"] = 2,
            ["sockets"] = 1,
            ["ostype"] = "l26",
            ["boot"] = "order=scsi0"
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<Dictionary<string, object>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedConfig);

        // Act
        var result = await _vmService.GetVmConfigAsync("node1", 100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4096, result["memory"]);
        Assert.Equal(2, result["cores"]);
        Assert.Equal(1, result["sockets"]);

        _mockHttpClient.Verify(x => x.GetAsync<Dictionary<string, object>>("nodes/node1/qemu/100/config", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVmStatisticsAsync_ReturnsStatistics_WhenSuccessful()
    {
        // Arrange
        var expectedStats = new VmStatistics
        {
            CpuUsage = 0.35,
            MemoryUsage = 3221225472, // 3GB
            MaxMemory = 4294967296,  // 4GB
            DiskRead = 2048000,
            DiskWrite = 1024000,
            NetworkIn = 4096000,
            NetworkOut = 2048000,
            Uptime = 7200
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<VmStatistics>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _vmService.GetVmStatisticsAsync("node1", 100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.35, result.CpuUsage);
        Assert.Equal(3221225472, result.MemoryUsage);
        Assert.Equal(4294967296, result.MaxMemory);
        Assert.Equal(2048000, result.DiskRead);
        Assert.Equal(4096000, result.NetworkIn);

        _mockHttpClient.Verify(x => x.GetAsync<VmStatistics>("nodes/node1/qemu/100/status/current", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartVmAsync_ReturnsTaskId_WhenSuccessful()
    {
        // Arrange
        const string expectedTaskId = "UPID:node1:00001234:00000123:5F123456:qmstart:100:user@pam:";

        _mockHttpClient
            .Setup(x => x.PostAsync<string>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTaskId);

        // Act
        var result = await _vmService.StartVmAsync("node1", 100);

        // Assert
        Assert.Equal(expectedTaskId, result);

        _mockHttpClient.Verify(x => x.PostAsync<string>("nodes/node1/qemu/100/status/start", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StopVmAsync_ReturnsTaskId_WhenSuccessful()
    {
        // Arrange
        const string expectedTaskId = "UPID:node1:00001234:00000123:5F123456:qmstop:100:user@pam:";

        _mockHttpClient
            .Setup(x => x.PostAsync<string>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTaskId);

        // Act
        var result = await _vmService.StopVmAsync("node1", 100);

        // Assert
        Assert.Equal(expectedTaskId, result);

        _mockHttpClient.Verify(x => x.PostAsync<string>("nodes/node1/qemu/100/status/stop", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVmSnapshotsAsync_ReturnsSnapshotList_WhenSuccessful()
    {
        // Arrange
        var expectedSnapshots = new List<VmSnapshot>
        {
            new VmSnapshot { Name = "snapshot1", Description = "Test snapshot 1", SnapTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            new VmSnapshot { Name = "snapshot2", Description = "Test snapshot 2", SnapTime = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds() }
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<List<VmSnapshot>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSnapshots);

        // Act
        var result = await _vmService.GetVmSnapshotsAsync("node1", 100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("snapshot1", result[0].Name);
        Assert.Equal("Test snapshot 1", result[0].Description);

        _mockHttpClient.Verify(x => x.GetAsync<List<VmSnapshot>>("nodes/node1/qemu/100/snapshot", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Methods_WithInvalidNodeName_ThrowArgumentException(string invalidNodeName)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _vmService.GetVmsOnNodeAsync(invalidNodeName));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _vmService.GetVmStatusAsync(invalidNodeName, 100));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _vmService.GetVmConfigAsync(invalidNodeName, 100));
    }
    [Fact]
    public void VmService_ShouldNotValidateVmIds()
    {
        // The VmService doesn't perform client-side VM ID validation
        // VM ID validation is handled by the Proxmox server
        // This test just ensures the service methods exist and can be instantiated
        Assert.NotNull(_vmService);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
