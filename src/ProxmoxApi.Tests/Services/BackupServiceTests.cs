using Microsoft.Extensions.Logging;
using Moq;
using ProxmoxApi.Core;
using ProxmoxApi.Models;
using ProxmoxApi.Services;
using Xunit;

namespace ProxmoxApi.Tests.Services;

public class BackupServiceTests
{
    private readonly Mock<IProxmoxHttpClient> _mockHttpClient;
    private readonly Mock<ILogger<BackupService>> _mockLogger;
    private readonly BackupService _backupService;

    public BackupServiceTests()
    {
        _mockHttpClient = new Mock<IProxmoxHttpClient>();
        _mockLogger = new Mock<ILogger<BackupService>>();
        _backupService = new BackupService(_mockHttpClient.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeService()
    {
        // Arrange & Act
        var service = new BackupService(_mockHttpClient.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BackupService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BackupService(_mockHttpClient.Object, null!));
    }

    #endregion

    #region Backup Jobs Tests

    [Fact]
    public async Task GetBackupJobsAsync_ShouldReturnBackupJobs()
    {
        // Arrange
        var expectedJobs = new List<BackupJob>
        {
            new BackupJob
            {
                Id = "job1",
                Schedule = "0 2 * * *",
                Storage = "local",
                Node = "node1",
                VmIds = "100,101",
                Enabled = true
            },
            new BackupJob
            {
                Id = "job2",
                Schedule = "0 3 * * 0",
                Storage = "backup-storage",
                Node = "node2",
                VmIds = "200",
                Enabled = false
            }
        };

        var response = new ProxmoxApiResponse<List<BackupJob>> { Data = expectedJobs };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<List<BackupJob>>>(
            "/cluster/backup", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.GetBackupJobsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("job1", result[0].Id);
        Assert.Equal("job2", result[1].Id);
        Assert.True(result[0].Enabled);
        Assert.False(result[1].Enabled);
    }

    [Fact]
    public async Task GetBackupJobAsync_WithValidJobId_ShouldReturnBackupJob()
    {
        // Arrange
        const string jobId = "job1";
        var expectedJob = new BackupJob
        {
            Id = jobId,
            Schedule = "0 2 * * *",
            Storage = "local",
            Node = "node1",
            VmIds = "100,101",
            Enabled = true,
            Comment = "Daily backup"
        };

        var response = new ProxmoxApiResponse<BackupJob> { Data = expectedJob };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<BackupJob>>(
            $"/cluster/backup/{jobId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.GetBackupJobAsync(jobId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jobId, result.Id);
        Assert.Equal("Daily backup", result.Comment);
        Assert.True(result.Enabled);
    }

    [Fact]
    public async Task GetBackupJobAsync_WithNullJobId_ShouldThrowArgumentException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _backupService.GetBackupJobAsync(null!));
    }

    [Fact]
    public async Task CreateBackupJobAsync_WithValidParameters_ShouldCreateJob()
    {
        // Arrange
        var parameters = new BackupJobParameters
        {
            Schedule = "0 2 * * *",
            Storage = "local",
            Node = "node1",
            VmIds = "100,101",
            Enabled = true,
            Comment = "Daily backup"
        };

        var response = new ProxmoxApiResponse<object> { Data = new object() };
        _mockHttpClient.Setup(x => x.PostAsync<ProxmoxApiResponse<object>>(
            "/cluster/backup", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.CreateBackupJobAsync(parameters);

        // Assert
        Assert.Equal("success", result);
        _mockHttpClient.Verify(x => x.PostAsync<ProxmoxApiResponse<object>>(
            "/cluster/backup", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBackupJobAsync_WithNullParameters_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _backupService.CreateBackupJobAsync(null!));
    }

    [Fact]
    public async Task UpdateBackupJobAsync_WithValidParameters_ShouldUpdateJob()
    {
        // Arrange
        const string jobId = "job1";
        var parameters = new BackupJobParameters
        {
            Schedule = "0 3 * * *",
            Enabled = false,
            Comment = "Updated backup"
        };

        var response = new ProxmoxApiResponse<object> { Data = new object() };
        _mockHttpClient.Setup(x => x.PutAsync<ProxmoxApiResponse<object>>(
            $"/cluster/backup/{jobId}", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _backupService.UpdateBackupJobAsync(jobId, parameters);

        // Assert
        _mockHttpClient.Verify(x => x.PutAsync<ProxmoxApiResponse<object>>(
            $"/cluster/backup/{jobId}", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBackupJobAsync_WithValidJobId_ShouldDeleteJob()
    {
        // Arrange
        const string jobId = "job1";        _mockHttpClient.Setup(x => x.DeleteAsync<object>($"/cluster/backup/{jobId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _backupService.DeleteBackupJobAsync(jobId);

        // Assert
        _mockHttpClient.Verify(x => x.DeleteAsync<object>($"/cluster/backup/{jobId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Backup Files Tests

    [Fact]
    public async Task GetBackupFilesAsync_WithNode_ShouldReturnBackupFiles()
    {
        // Arrange
        const string node = "node1";
        var expectedFiles = new List<BackupFile>
        {
            new BackupFile
            {
                VolumeId = "local:backup/vzdump-qemu-100-2023_01_15-02_00_01.vma.zst",
                Node = node,
                Storage = "local",
                VmId = 100,
                Size = 1073741824,
                CreationTime = 1673769601,
                Format = "vma.zst"
            },
            new BackupFile
            {
                VolumeId = "local:backup/vzdump-lxc-200-2023_01_15-02_30_01.tar.zst",
                Node = node,
                Storage = "local",
                VmId = 200,
                Size = 536870912,
                CreationTime = 1673771401,
                Format = "tar.zst"
            }
        };

        var response = new ProxmoxApiResponse<List<BackupFile>> { Data = expectedFiles };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<List<BackupFile>>>(
            $"/nodes/{node}/storage?content=backup", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.GetBackupFilesAsync(node);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(100, result[0].VmId);
        Assert.Equal(200, result[1].VmId);
        Assert.Equal("1.0 GB", result[0].FormattedSize);
        Assert.Equal("512.0 MB", result[1].FormattedSize);
    }

    [Fact]
    public async Task GetVmBackupFilesAsync_WithVmId_ShouldReturnVmBackups()
    {
        // Arrange
        const string node = "node1";
        const int vmId = 100;
        var allBackups = new List<BackupFile>
        {
            new BackupFile { VmId = 100, VolumeId = "backup1" },
            new BackupFile { VmId = 200, VolumeId = "backup2" },
            new BackupFile { VmId = 100, VolumeId = "backup3" }
        };

        var response = new ProxmoxApiResponse<List<BackupFile>> { Data = allBackups };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<List<BackupFile>>>(
            $"/nodes/{node}/storage?content=backup", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.GetVmBackupFilesAsync(node, vmId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, backup => Assert.Equal(vmId, backup.VmId));
    }

    #endregion

    #region Backup Operations Tests

    [Fact]
    public async Task CreateVmBackupAsync_WithValidParameters_ShouldReturnTaskId()
    {
        // Arrange
        const string node = "node1";
        const int vmId = 100;
        const string storage = "local";
        const string expectedTaskId = "UPID:node1:12345678:backup";

        var response = new ProxmoxApiResponse<string> { Data = expectedTaskId };
        _mockHttpClient.Setup(x => x.PostAsync<ProxmoxApiResponse<string>>(
            $"/nodes/{node}/qemu/{vmId}/backup", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.CreateVmBackupAsync(node, vmId, storage);

        // Assert
        Assert.Equal(expectedTaskId, result);
        _mockHttpClient.Verify(x => x.PostAsync<ProxmoxApiResponse<string>>(
            $"/nodes/{node}/qemu/{vmId}/backup", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateContainerBackupAsync_WithValidParameters_ShouldReturnTaskId()
    {
        // Arrange
        const string node = "node1";
        const int containerId = 200;
        const string storage = "local";
        const string expectedTaskId = "UPID:node1:87654321:backup";

        var response = new ProxmoxApiResponse<string> { Data = expectedTaskId };
        _mockHttpClient.Setup(x => x.PostAsync<ProxmoxApiResponse<string>>(
            $"/nodes/{node}/lxc/{containerId}/backup", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.CreateContainerBackupAsync(node, containerId, storage);

        // Assert
        Assert.Equal(expectedTaskId, result);
        _mockHttpClient.Verify(x => x.PostAsync<ProxmoxApiResponse<string>>(
            $"/nodes/{node}/lxc/{containerId}/backup", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Restore Operations Tests

    [Fact]
    public async Task RestoreVmAsync_WithValidParameters_ShouldReturnTaskId()
    {
        // Arrange
        const string node = "node1";
        const int vmId = 100;
        const string archive = "local:backup/vzdump-qemu-100-2023_01_15-02_00_01.vma.zst";
        const string expectedTaskId = "UPID:node1:11111111:restore";

        var response = new ProxmoxApiResponse<string> { Data = expectedTaskId };
        _mockHttpClient.Setup(x => x.PostAsync<ProxmoxApiResponse<string>>(
            $"/nodes/{node}/qemu", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.RestoreVmAsync(node, vmId, archive);

        // Assert
        Assert.Equal(expectedTaskId, result);
        _mockHttpClient.Verify(x => x.PostAsync<ProxmoxApiResponse<string>>(
            $"/nodes/{node}/qemu", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreContainerAsync_WithValidParameters_ShouldReturnTaskId()
    {
        // Arrange
        const string node = "node1";
        const int containerId = 200;
        const string archive = "local:backup/vzdump-lxc-200-2023_01_15-02_30_01.tar.zst";
        const string expectedTaskId = "UPID:node1:22222222:restore";

        var response = new ProxmoxApiResponse<string> { Data = expectedTaskId };
        _mockHttpClient.Setup(x => x.PostAsync<ProxmoxApiResponse<string>>(
            $"/nodes/{node}/lxc", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.RestoreContainerAsync(node, containerId, archive);

        // Assert
        Assert.Equal(expectedTaskId, result);
        _mockHttpClient.Verify(x => x.PostAsync<ProxmoxApiResponse<string>>(
            $"/nodes/{node}/lxc", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Task Management Tests

    [Fact]
    public async Task GetTaskStatusAsync_WithValidTaskId_ShouldReturnTask()
    {
        // Arrange
        const string node = "node1";
        const string taskId = "UPID:node1:12345678:backup";
        var expectedTask = new BackupTask
        {
            TaskId = taskId,
            Node = node,
            Status = "running",
            StartTime = 1673769601,
            Type = "backup"
        };

        var response = new ProxmoxApiResponse<BackupTask> { Data = expectedTask };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<BackupTask>>(
            $"/nodes/{node}/tasks/{taskId}/status", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.GetTaskStatusAsync(node, taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.TaskId);
        Assert.Equal("running", result.Status);
        Assert.True(result.IsRunning);
        Assert.False(result.IsSuccessful);
    }

    [Fact]
    public async Task GetBackupTasksAsync_WithNode_ShouldReturnBackupTasks()
    {
        // Arrange
        const string node = "node1";
        var expectedTasks = new List<BackupTask>
        {
            new BackupTask
            {
                TaskId = "UPID:node1:12345678:backup",
                Node = node,
                Status = "stopped",
                ExitStatus = "OK",
                Type = "backup"
            },
            new BackupTask
            {
                TaskId = "UPID:node1:87654321:backup",
                Node = node,
                Status = "running",
                Type = "backup"
            }
        };

        var response = new ProxmoxApiResponse<List<BackupTask>> { Data = expectedTasks };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<List<BackupTask>>>(
            $"/nodes/{node}/tasks?typefilter=backup", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _backupService.GetBackupTasksAsync(node);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsSuccessful);
        Assert.True(result[1].IsRunning);
    }

    #endregion

    #region Parameter Validation Tests    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetBackupFilesAsync_WithInvalidNode_ShouldThrowArgumentException(string node)
    {
        // Arrange, Act & Assert  
        await Assert.ThrowsAsync<ArgumentException>(() => _backupService.GetBackupFilesAsync(node));
    }
    
    [Fact]
    public async Task GetBackupFilesAsync_WithNullNode_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _backupService.GetBackupFilesAsync(null!));
    }    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateVmBackupAsync_WithInvalidNode_ShouldThrowArgumentException(string node)
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _backupService.CreateVmBackupAsync(node, 100, "local"));
    }
    
    [Fact]
    public async Task CreateVmBackupAsync_WithNullNode_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _backupService.CreateVmBackupAsync(null!, 100, "local"));
    }    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateVmBackupAsync_WithInvalidStorage_ShouldThrowArgumentException(string storage)
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _backupService.CreateVmBackupAsync("node1", 100, storage));
    }
    
    [Fact]
    public async Task CreateVmBackupAsync_WithNullStorage_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _backupService.CreateVmBackupAsync("node1", 100, null!));
    }

    #endregion
}
