using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Services;
using ProxmoxApi.Models;
using ProxmoxApi.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace ProxmoxApi.Tests.Services;

public class StorageServiceTests : IDisposable
{
    private readonly Mock<IProxmoxHttpClient> _mockHttpClient;
    private readonly Mock<ILogger<StorageService>> _mockLogger;
    private readonly StorageService _storageService;

    public StorageServiceTests()
    {
        _mockHttpClient = new Mock<IProxmoxHttpClient>();
        _mockLogger = new Mock<ILogger<StorageService>>();
        _storageService = new StorageService(_mockHttpClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetStoragesAsync_ReturnsStorageList_WhenSuccessful()
    {
        // Arrange
        var expectedStorages = new List<ProxmoxStorage>
        {
            new ProxmoxStorage
            {
                Storage = "local",
                Type = "dir",
                Path = "/var/lib/vz",
                Content = "images,iso,backup",
                Enabled = true,
                Shared = false
            },
            new ProxmoxStorage
            {
                Storage = "local-lvm",
                Type = "lvm",
                Content = "images",
                Enabled = true,
                Shared = false
            }
        };

        var apiResponse = new ProxmoxApiResponse<List<ProxmoxStorage>> { Data = expectedStorages };
        _mockHttpClient.Setup(x => x.GetAsync<List<ProxmoxStorage>>("storage", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedStorages);

        // Act
        var result = await _storageService.GetStoragesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("local", result[0].Storage);
        Assert.Equal("dir", result[0].Type);
        Assert.Equal("local-lvm", result[1].Storage);
        Assert.Equal("lvm", result[1].Type);

        _mockHttpClient.Verify(x => x.GetAsync<List<ProxmoxStorage>>("storage", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStorageAsync_ReturnsStorage_WhenSuccessful()
    {
        // Arrange
        var storageId = "local";
        var expectedStorage = new ProxmoxStorage
        {
            Storage = storageId,
            Type = "dir",
            Path = "/var/lib/vz",
            Content = "images,iso,backup",
            Enabled = true,
            Shared = false
        };

        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxStorage>($"storage/{storageId}", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedStorage);

        // Act
        var result = await _storageService.GetStorageAsync(storageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(storageId, result.Storage);
        Assert.Equal("dir", result.Type);
        Assert.Equal("/var/lib/vz", result.Path);

        _mockHttpClient.Verify(x => x.GetAsync<ProxmoxStorage>($"storage/{storageId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStorageAsync_ThrowsArgumentException_WhenStorageIdIsEmpty()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _storageService.GetStorageAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => _storageService.GetStorageAsync(null!));
    }

    [Fact]
    public async Task CreateStorageAsync_CallsHttpClient_WhenSuccessful()
    {
        // Arrange
        var options = new StorageCreateOptions
        {
            Storage = "test-storage",
            Type = "dir",
            Path = "/mnt/test",
            Content = "images,iso",
            Shared = true,
            MaxFiles = 10
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>("storage", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new object());

        // Act
        await _storageService.CreateStorageAsync(options);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>("storage",
            It.Is<Dictionary<string, object>>(d =>
                d["storage"].ToString() == "test-storage" &&
                d["type"].ToString() == "dir" &&
                d["path"].ToString() == "/mnt/test" &&
                d["content"].ToString() == "images,iso" &&
                d["shared"].Equals(1) &&
                d["maxfiles"].Equals(10)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateStorageAsync_ThrowsArgumentException_WhenOptionsAreInvalid()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _storageService.CreateStorageAsync(null!));

        await Assert.ThrowsAsync<ArgumentException>(() => _storageService.CreateStorageAsync(new StorageCreateOptions()));

        await Assert.ThrowsAsync<ArgumentException>(() => _storageService.CreateStorageAsync(new StorageCreateOptions { Storage = "test" }));
    }

    [Fact]
    public async Task UpdateStorageAsync_CallsHttpClient_WhenSuccessful()
    {
        // Arrange
        var storageId = "test-storage";
        var options = new StorageCreateOptions
        {
            Storage = storageId,
            Type = "dir",
            Path = "/mnt/test-updated",
            Content = "images,iso,backup",
            Shared = false
        };

        _mockHttpClient.Setup(x => x.PutAsync<object>($"storage/{storageId}", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new object());

        // Act
        await _storageService.UpdateStorageAsync(storageId, options);

        // Assert
        _mockHttpClient.Verify(x => x.PutAsync<object>($"storage/{storageId}",
            It.Is<Dictionary<string, object>>(d =>
                d["content"].ToString() == "images,iso,backup" &&
                d["path"].ToString() == "/mnt/test-updated"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStorageAsync_CallsHttpClient_WhenSuccessful()
    {
        // Arrange
        var storageId = "test-storage";

        _mockHttpClient.Setup(x => x.DeleteAsync<object>($"storage/{storageId}", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new object());

        // Act
        await _storageService.DeleteStorageAsync(storageId);

        // Assert
        _mockHttpClient.Verify(x => x.DeleteAsync<object>($"storage/{storageId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStorageStatusAsync_ReturnsStatusList_WhenSuccessful()
    {
        // Arrange
        var nodeName = "pve";
        var expectedStatuses = new List<StorageStatus>
        {
            new StorageStatus
            {
                Storage = "local",
                Type = "dir",
                Total = 100_000_000_000,
                Used = 50_000_000_000,
                Available = 50_000_000_000,
                Active = true,
                Content = "images,iso,backup"
            }
        };

        _mockHttpClient.Setup(x => x.GetAsync<List<StorageStatus>>($"nodes/{nodeName}/storage", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedStatuses);

        // Act
        var result = await _storageService.GetStorageStatusAsync(nodeName);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("local", result[0].Storage);
        Assert.Equal(50.0, result[0].UsagePercentage);
        Assert.True(result[0].Active);

        _mockHttpClient.Verify(x => x.GetAsync<List<StorageStatus>>($"nodes/{nodeName}/storage", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStorageContentAsync_ReturnsContentList_WhenSuccessful()
    {
        // Arrange
        var nodeName = "pve";
        var storageId = "local";
        var expectedContent = new List<StorageContent>
        {
            new StorageContent
            {
                VolumeId = "local:iso/ubuntu-20.04.iso",
                Content = "iso",
                Format = "iso",
                Size = 1_000_000_000,
                Used = 1_000_000_000
            },
            new StorageContent
            {
                VolumeId = "local:backup/vzdump-qemu-100.vma.zst",
                Content = "backup",
                Format = "vma.zst",
                Size = 5_000_000_000,
                Used = 5_000_000_000,
                VmId = 100
            }
        };

        _mockHttpClient.Setup(x => x.GetAsync<List<StorageContent>>($"nodes/{nodeName}/storage/{storageId}/content", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedContent);

        // Act
        var result = await _storageService.GetStorageContentAsync(nodeName, storageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("iso", result[0].Content);
        Assert.Equal("backup", result[1].Content);
        Assert.Equal(100, result[1].VmId);

        _mockHttpClient.Verify(x => x.GetAsync<List<StorageContent>>($"nodes/{nodeName}/storage/{storageId}/content", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStorageContentAsync_WithFilters_AppliesCorrectQueryParameters()
    {
        // Arrange
        var nodeName = "pve";
        var storageId = "local";
        var contentType = "backup";
        var vmId = 100;

        _mockHttpClient.Setup(x => x.GetAsync<List<StorageContent>>($"nodes/{nodeName}/storage/{storageId}/content?content={contentType}&vmid={vmId}", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<StorageContent>());

        // Act
        await _storageService.GetStorageContentAsync(nodeName, storageId, contentType, vmId);

        // Assert
        _mockHttpClient.Verify(x => x.GetAsync<List<StorageContent>>($"nodes/{nodeName}/storage/{storageId}/content?content={contentType}&vmid={vmId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVolumeAsync_CallsHttpClient_WhenSuccessful()
    {
        // Arrange
        var nodeName = "pve";
        var storageId = "local-lvm";
        var options = new VolumeCreateOptions
        {
            VolumeId = "vm-100-disk-0",
            Size = "32G",
            Format = "raw",
            VmId = 100,
            Notes = "Test disk"
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>($"nodes/{nodeName}/storage/{storageId}/content", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new object());

        // Act
        await _storageService.CreateVolumeAsync(nodeName, storageId, options);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>($"nodes/{nodeName}/storage/{storageId}/content",
            It.Is<Dictionary<string, object>>(d =>
                d["filename"].ToString() == "vm-100-disk-0" &&
                d["size"].ToString() == "32G" &&
                d["format"].ToString() == "raw" &&
                d["vmid"].Equals(100) &&
                d["notes"].ToString() == "Test disk"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteVolumeAsync_CallsHttpClient_WhenSuccessful()
    {
        // Arrange
        var nodeName = "pve";
        var storageId = "local-lvm";
        var volumeId = "vm-100-disk-0";

        _mockHttpClient.Setup(x => x.DeleteAsync<object>($"nodes/{nodeName}/storage/{storageId}/content/{Uri.EscapeDataString(volumeId)}", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new object());

        // Act
        await _storageService.DeleteVolumeAsync(nodeName, storageId, volumeId);

        // Assert
        _mockHttpClient.Verify(x => x.DeleteAsync<object>($"nodes/{nodeName}/storage/{storageId}/content/{Uri.EscapeDataString(volumeId)}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CopyVolumeAsync_ReturnsTaskId_WhenSuccessful()
    {
        // Arrange
        var nodeName = "pve";
        var storageId = "local-lvm";
        var volumeId = "vm-100-disk-0";
        var targetStorage = "backup-storage";
        var expectedTaskId = "UPID:pve:00001234:00000000:5F123456:vdisk:local-lvm:copy:root@pam:";

        _mockHttpClient.Setup(x => x.PostAsync<string>($"nodes/{nodeName}/storage/{storageId}/content/{Uri.EscapeDataString(volumeId)}/copy",
                It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedTaskId);

        // Act
        var result = await _storageService.CopyVolumeAsync(nodeName, storageId, volumeId, targetStorage);

        // Assert
        Assert.Equal(expectedTaskId, result);

        _mockHttpClient.Verify(x => x.PostAsync<string>($"nodes/{nodeName}/storage/{storageId}/content/{Uri.EscapeDataString(volumeId)}/copy",
            It.Is<Dictionary<string, object>>(d => d["target"].ToString() == targetStorage),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetIsoImagesAsync_ReturnsIsoContent_WhenSuccessful()
    {
        // Arrange
        var nodeName = "pve";
        var storageId = "local";
        var expectedContent = new List<StorageContent>
        {
            new StorageContent
            {
                VolumeId = "local:iso/ubuntu-20.04.iso",
                Content = "iso",
                Format = "iso",
                Size = 1_000_000_000
            }
        };

        _mockHttpClient.Setup(x => x.GetAsync<List<StorageContent>>($"nodes/{nodeName}/storage/{storageId}/content?content=iso", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedContent);

        // Act
        var result = await _storageService.GetIsoImagesAsync(nodeName, storageId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("iso", result[0].Content);
        Assert.Contains("ubuntu-20.04.iso", result[0].VolumeId);

        _mockHttpClient.Verify(x => x.GetAsync<List<StorageContent>>($"nodes/{nodeName}/storage/{storageId}/content?content=iso", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetContainerTemplatesAsync_ReturnsTemplateContent_WhenSuccessful()
    {
        // Arrange
        var nodeName = "pve";
        var storageId = "local";
        var expectedContent = new List<StorageContent>
        {
            new StorageContent
            {
                VolumeId = "local:vztmpl/ubuntu-20.04-standard_20.04-1_amd64.tar.gz",
                Content = "vztmpl",
                Format = "tgz",
                Size = 500_000_000
            }
        };

        _mockHttpClient.Setup(x => x.GetAsync<List<StorageContent>>($"nodes/{nodeName}/storage/{storageId}/content?content=vztmpl", It.IsAny<CancellationToken>()))
                      .ReturnsAsync(expectedContent);

        // Act
        var result = await _storageService.GetContainerTemplatesAsync(nodeName, storageId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("vztmpl", result[0].Content);
        Assert.Contains("ubuntu-20.04", result[0].VolumeId);

        _mockHttpClient.Verify(x => x.GetAsync<List<StorageContent>>($"nodes/{nodeName}/storage/{storageId}/content?content=vztmpl", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "local", "test.iso")]
    [InlineData("pve", "", "test.iso")]
    [InlineData("pve", "local", "")]
    public async Task Methods_WithInvalidParameters_ThrowArgumentException(string nodeName, string storageId, string filename)
    {
        // Arrange & Act & Assert
        if (string.IsNullOrEmpty(nodeName))
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _storageService.GetStorageStatusAsync(nodeName));
        }

        if (string.IsNullOrEmpty(storageId) && !string.IsNullOrEmpty(nodeName))
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _storageService.GetStorageContentAsync(nodeName, storageId));
        }

        if (string.IsNullOrEmpty(filename) && !string.IsNullOrEmpty(nodeName) && !string.IsNullOrEmpty(storageId))
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _storageService.UploadFileAsync(nodeName, storageId, filename));
        }
    }

    public void Dispose()
    {
        // Clean up any resources if needed
    }
}
