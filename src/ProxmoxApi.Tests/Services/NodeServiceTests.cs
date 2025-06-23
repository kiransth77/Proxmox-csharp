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

public class NodeServiceTests : IDisposable
{
    private readonly Mock<IProxmoxHttpClient> _mockHttpClient;
    private readonly Mock<ILogger<NodeService>> _mockLogger;
    private readonly NodeService _nodeService;

    public NodeServiceTests()
    {
        _mockHttpClient = new Mock<IProxmoxHttpClient>();
        _mockLogger = new Mock<ILogger<NodeService>>();
        _nodeService = new NodeService(_mockHttpClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetNodesAsync_ReturnsNodeList_WhenSuccessful()
    {
        // Arrange
        var expectedNodes = new List<ProxmoxNode>
        {
            new() { Node = "node1", Status = "online", Type = "node", Uptime = 86400 },
            new() { Node = "node2", Status = "online", Type = "node", Uptime = 43200 }
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<List<ProxmoxNode>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedNodes);

        // Act
        var result = await _nodeService.GetNodesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("node1", result[0].Node);
        Assert.Equal("online", result[0].Status);
        
        _mockHttpClient.Verify(x => x.GetAsync<List<ProxmoxNode>>("/nodes", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNodesAsync_ReturnsEmptyList_WhenExceptionThrown()
    {
        // Arrange
        _mockHttpClient
            .Setup(x => x.GetAsync<List<ProxmoxNode>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _nodeService.GetNodesAsync());
    }

    [Fact]
    public async Task GetNodeStatusAsync_ReturnsNodeStatus_WhenSuccessful()
    {
        // Arrange
        var expectedStatus = new NodeStatus
        {
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Uptime = 86400,
            LoadAverage = new double[] { 1.5, 1.2, 1.0 },
            CpuInfo = new CpuInfo
            {
                Cpus = 8,
                Model = "Intel Core i7",
                Mhz = 2400,
                User = 10.5,
                System = 5.2,
                Idle = 84.3
            },
            Memory = new MemoryInfo
            {
                Total = 17179869184, // 16GB
                Used = 8589934592,   // 8GB
                Free = 8589934592    // 8GB
            },
            Swap = new SwapInfo
            {
                Total = 2147483648, // 2GB
                Used = 0,
                Free = 2147483648
            },
            RootFs = new FilesystemInfo
            {
                Total = 107374182400, // 100GB
                Used = 53687091200,   // 50GB
                Available = 53687091200
            }
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<NodeStatus>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStatus);

        // Act
        var result = await _nodeService.GetNodeStatusAsync("node1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(86400, result.Uptime);
        Assert.NotNull(result.CpuInfo);
        Assert.Equal(8, result.CpuInfo.Cpus);
        Assert.NotNull(result.Memory);
        Assert.Equal(17179869184, result.Memory.Total);
        Assert.NotNull(result.LoadAverage);
        Assert.Equal(3, result.LoadAverage.Length);
        
        _mockHttpClient.Verify(x => x.GetAsync<NodeStatus>("/nodes/node1/status", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNodeVersionAsync_ReturnsVersionInfo_WhenSuccessful()
    {
        // Arrange
        var expectedVersion = new Dictionary<string, object>
        {
            ["version"] = "7.4-3",
            ["release"] = "bullseye",
            ["repoid"] = "12345"
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<Dictionary<string, object>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVersion);

        // Act
        var result = await _nodeService.GetNodeVersionAsync("node1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("7.4-3", result["version"]);
        Assert.Equal("bullseye", result["release"]);
        
        _mockHttpClient.Verify(x => x.GetAsync<Dictionary<string, object>>("/nodes/node1/version", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetNodeSubscriptionAsync_ReturnsSubscriptionInfo_WhenSuccessful()
    {
        // Arrange
        var expectedSubscription = new Dictionary<string, object>
        {
            ["status"] = "Active",
            ["level"] = "standard",
            ["key"] = "test-key-123"
        };

        _mockHttpClient
            .Setup(x => x.GetAsync<Dictionary<string, object>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSubscription);

        // Act
        var result = await _nodeService.GetNodeSubscriptionAsync("node1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Active", result["status"]);
        Assert.Equal("standard", result["level"]);
        
        _mockHttpClient.Verify(x => x.GetAsync<Dictionary<string, object>>("/nodes/node1/subscription", It.IsAny<CancellationToken>()), Times.Once);
    }    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Methods_WithInvalidNodeName_ThrowArgumentException(string invalidNodeName)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _nodeService.GetNodeStatusAsync(invalidNodeName));
            
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _nodeService.GetNodeVersionAsync(invalidNodeName));
            
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _nodeService.GetNodeSubscriptionAsync(invalidNodeName));
    }

    [Fact]
    public async Task Methods_WithNullNodeName_ThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _nodeService.GetNodeStatusAsync(null!));
            
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _nodeService.GetNodeVersionAsync(null!));
            
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _nodeService.GetNodeSubscriptionAsync(null!));
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
