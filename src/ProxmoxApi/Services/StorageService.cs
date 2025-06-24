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
/// Service for managing Proxmox storage systems
/// </summary>
public class StorageService
{
    private readonly IProxmoxHttpClient _httpClient;
    private readonly ILogger<StorageService> _logger;

    /// <summary>
    /// Initializes a new instance of StorageService
    /// </summary>
    /// <param name="httpClient">HTTP client for API communication</param>
    /// <param name="logger">Logger instance</param>
    public StorageService(IProxmoxHttpClient httpClient, ILogger<StorageService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Storage Configuration Management

    /// <summary>
    /// Gets all configured storage systems
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of storage configurations</returns>
    public async Task<List<ProxmoxStorage>> GetStoragesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all storage configurations");

        try
        {
            var response = await _httpClient.GetAsync<List<ProxmoxStorage>>(
                "storage",
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved {Count} storage configurations", response.Count);
                return response;
            }

            _logger.LogWarning("No storage configurations found");
            return new List<ProxmoxStorage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve storage configurations");
            throw;
        }
    }

    /// <summary>
    /// Gets specific storage configuration
    /// </summary>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Storage configuration or null if not found</returns>
    public async Task<ProxmoxStorage?> GetStorageAsync(string storageId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageId))
            throw new ArgumentException("Storage ID cannot be null or empty", nameof(storageId));

        _logger.LogInformation("Retrieving storage configuration for {StorageId}", storageId);

        try
        {
            var response = await _httpClient.GetAsync<ProxmoxStorage>(
                $"storage/{storageId}",
                cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved storage configuration for {StorageId}", storageId);
            }
            else
            {
                _logger.LogWarning("Storage configuration not found for {StorageId}", storageId);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve storage configuration for {StorageId}", storageId);
            throw;
        }
    }

    /// <summary>
    /// Creates a new storage configuration
    /// </summary>
    /// <param name="options">Storage creation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task CreateStorageAsync(StorageCreateOptions options, CancellationToken cancellationToken = default)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(options.Storage))
            throw new ArgumentException("Storage ID is required", nameof(options));

        if (string.IsNullOrWhiteSpace(options.Type))
            throw new ArgumentException("Storage type is required", nameof(options));

        _logger.LogInformation("Creating storage {StorageId} of type {Type}", options.Storage, options.Type);

        try
        {
            var data = new Dictionary<string, object>
            {
                ["storage"] = options.Storage,
                ["type"] = options.Type,
                ["content"] = options.Content
            };

            if (!string.IsNullOrWhiteSpace(options.Path))
                data["path"] = options.Path;

            if (options.Shared)
                data["shared"] = 1;

            if (!string.IsNullOrWhiteSpace(options.Nodes))
                data["nodes"] = options.Nodes;

            if (options.MaxFiles.HasValue)
                data["maxfiles"] = options.MaxFiles.Value;

            // Add storage-specific options
            foreach (var option in options.Options)
            {
                data[option.Key] = option.Value;
            }

            await _httpClient.PostAsync<object>("storage", data, cancellationToken);

            _logger.LogInformation("Successfully created storage {StorageId}", options.Storage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create storage {StorageId}", options.Storage);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing storage configuration
    /// </summary>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="options">Updated storage options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task UpdateStorageAsync(string storageId, StorageCreateOptions options, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageId))
            throw new ArgumentException("Storage ID cannot be null or empty", nameof(storageId));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        _logger.LogInformation("Updating storage configuration for {StorageId}", storageId);

        try
        {
            var data = new Dictionary<string, object>
            {
                ["content"] = options.Content
            };

            if (!string.IsNullOrWhiteSpace(options.Path))
                data["path"] = options.Path;

            if (options.Shared)
                data["shared"] = 1;

            if (!string.IsNullOrWhiteSpace(options.Nodes))
                data["nodes"] = options.Nodes;

            if (options.MaxFiles.HasValue)
                data["maxfiles"] = options.MaxFiles.Value;

            // Add storage-specific options
            foreach (var option in options.Options)
            {
                data[option.Key] = option.Value;
            }

            await _httpClient.PutAsync<object>($"storage/{storageId}", data, cancellationToken);

            _logger.LogInformation("Successfully updated storage {StorageId}", storageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update storage {StorageId}", storageId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a storage configuration
    /// </summary>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task DeleteStorageAsync(string storageId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storageId))
            throw new ArgumentException("Storage ID cannot be null or empty", nameof(storageId));

        _logger.LogInformation("Deleting storage configuration for {StorageId}", storageId);

        try
        {
            await _httpClient.DeleteAsync<object>($"storage/{storageId}", cancellationToken);

            _logger.LogInformation("Successfully deleted storage {StorageId}", storageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete storage {StorageId}", storageId);
            throw;
        }
    }

    #endregion

    #region Storage Status and Monitoring

    /// <summary>
    /// Gets storage status for a specific node
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Optional storage ID to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of storage status information</returns>
    public async Task<List<StorageStatus>> GetStorageStatusAsync(string nodeName, string? storageId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name cannot be null or empty", nameof(nodeName));

        _logger.LogInformation("Retrieving storage status for node {NodeName}", nodeName);

        try
        {
            var endpoint = $"nodes/{nodeName}/storage";
            if (!string.IsNullOrWhiteSpace(storageId))
            {
                endpoint += $"/{storageId}/status";
            }

            var response = await _httpClient.GetAsync<List<StorageStatus>>(endpoint, cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved storage status for {Count} storages on node {NodeName}",
                    response.Count, nodeName);
                return response;
            }

            _logger.LogWarning("No storage status found for node {NodeName}", nodeName);
            return new List<StorageStatus>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve storage status for node {NodeName}", nodeName);
            throw;
        }
    }

    /// <summary>
    /// Gets content of a specific storage
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="contentType">Content type filter (images, iso, vztmpl, backup, etc.)</param>
    /// <param name="vmId">VM ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of storage content</returns>
    public async Task<List<StorageContent>> GetStorageContentAsync(
        string nodeName,
        string storageId,
        string? contentType = null,
        int? vmId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name cannot be null or empty", nameof(nodeName));

        if (string.IsNullOrWhiteSpace(storageId))
            throw new ArgumentException("Storage ID cannot be null or empty", nameof(storageId));

        _logger.LogInformation("Retrieving content for storage {StorageId} on node {NodeName}", storageId, nodeName);

        try
        {
            var endpoint = $"nodes/{nodeName}/storage/{storageId}/content";
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(contentType))
                queryParams.Add($"content={contentType}");

            if (vmId.HasValue)
                queryParams.Add($"vmid={vmId.Value}");

            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync<List<StorageContent>>(endpoint, cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Retrieved {Count} content items from storage {StorageId}",
                    response.Count, storageId);
                return response;
            }

            _logger.LogWarning("No content found in storage {StorageId}", storageId);
            return new List<StorageContent>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve content from storage {StorageId}", storageId);
            throw;
        }
    }

    #endregion

    #region Volume Management

    /// <summary>
    /// Creates a new volume
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="options">Volume creation options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task CreateVolumeAsync(
        string nodeName,
        string storageId,
        VolumeCreateOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name cannot be null or empty", nameof(nodeName));

        if (string.IsNullOrWhiteSpace(storageId))
            throw new ArgumentException("Storage ID cannot be null or empty", nameof(storageId));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(options.VolumeId))
            throw new ArgumentException("Volume ID is required", nameof(options));

        if (string.IsNullOrWhiteSpace(options.Size))
            throw new ArgumentException("Volume size is required", nameof(options));

        _logger.LogInformation("Creating volume {VolumeId} on storage {StorageId}", options.VolumeId, storageId);

        try
        {
            var data = new Dictionary<string, object>
            {
                ["filename"] = options.VolumeId,
                ["size"] = options.Size,
                ["format"] = options.Format
            };

            if (options.VmId.HasValue)
                data["vmid"] = options.VmId.Value;

            if (!string.IsNullOrWhiteSpace(options.Notes))
                data["notes"] = options.Notes;

            await _httpClient.PostAsync<object>(
                $"nodes/{nodeName}/storage/{storageId}/content",
                data,
                cancellationToken);

            _logger.LogInformation("Successfully created volume {VolumeId}", options.VolumeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create volume {VolumeId}", options.VolumeId);
            throw;
        }
    }

    /// <summary>
    /// Deletes a volume
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="volumeId">Volume identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task DeleteVolumeAsync(
        string nodeName,
        string storageId,
        string volumeId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name cannot be null or empty", nameof(nodeName));

        if (string.IsNullOrWhiteSpace(storageId))
            throw new ArgumentException("Storage ID cannot be null or empty", nameof(storageId));

        if (string.IsNullOrWhiteSpace(volumeId))
            throw new ArgumentException("Volume ID cannot be null or empty", nameof(volumeId));

        _logger.LogInformation("Deleting volume {VolumeId} from storage {StorageId}", volumeId, storageId);

        try
        {
            await _httpClient.DeleteAsync<object>(
                $"nodes/{nodeName}/storage/{storageId}/content/{Uri.EscapeDataString(volumeId)}",
                cancellationToken);

            _logger.LogInformation("Successfully deleted volume {VolumeId}", volumeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete volume {VolumeId}", volumeId);
            throw;
        }
    }

    /// <summary>
    /// Copies a volume to another storage
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Source storage identifier</param>
    /// <param name="volumeId">Volume identifier</param>
    /// <param name="targetStorage">Target storage identifier</param>
    /// <param name="targetVolumeId">Target volume identifier (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for tracking the operation</returns>
    public async Task<string> CopyVolumeAsync(
        string nodeName,
        string storageId,
        string volumeId,
        string targetStorage,
        string? targetVolumeId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name cannot be null or empty", nameof(nodeName));

        if (string.IsNullOrWhiteSpace(storageId))
            throw new ArgumentException("Storage ID cannot be null or empty", nameof(storageId));

        if (string.IsNullOrWhiteSpace(volumeId))
            throw new ArgumentException("Volume ID cannot be null or empty", nameof(volumeId));

        if (string.IsNullOrWhiteSpace(targetStorage))
            throw new ArgumentException("Target storage cannot be null or empty", nameof(targetStorage));

        _logger.LogInformation("Copying volume {VolumeId} to storage {TargetStorage}", volumeId, targetStorage);

        try
        {
            var data = new Dictionary<string, object>
            {
                ["target"] = targetStorage
            };

            if (!string.IsNullOrWhiteSpace(targetVolumeId))
                data["target_filename"] = targetVolumeId;

            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/storage/{storageId}/content/{Uri.EscapeDataString(volumeId)}/copy",
                data,
                cancellationToken);

            _logger.LogInformation("Started volume copy operation with task ID {TaskId}", response);
            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to copy volume {VolumeId}", volumeId);
            throw;
        }
    }

    #endregion

    #region Backup Management

    /// <summary>
    /// Gets backup files for a specific storage
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="vmId">VM ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of backup files</returns>
    public async Task<List<StorageBackup>> GetBackupsAsync(
        string nodeName,
        string storageId,
        int? vmId = null,
        CancellationToken cancellationToken = default)
    {
        var content = await GetStorageContentAsync(nodeName, storageId, "backup", vmId, cancellationToken);

        return content.Select(c => new StorageBackup
        {
            Filename = c.VolumeId.Split('/').LastOrDefault() ?? c.VolumeId,
            Type = c.Content,
            VmId = c.VmId ?? 0,
            CreationTime = c.CreationTime ?? 0,
            Size = c.Size,
            Format = c.Format,
            Notes = c.Notes,
            Protected = c.Protected
        }).ToList();
    }

    /// <summary>
    /// Downloads a backup file
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="backupFile">Backup filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Download URL or content</returns>
    public async Task<string> DownloadBackupAsync(
        string nodeName,
        string storageId,
        string backupFile,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name cannot be null or empty", nameof(nodeName));

        if (string.IsNullOrWhiteSpace(storageId))
            throw new ArgumentException("Storage ID cannot be null or empty", nameof(storageId));

        if (string.IsNullOrWhiteSpace(backupFile))
            throw new ArgumentException("Backup file cannot be null or empty", nameof(backupFile));

        _logger.LogInformation("Downloading backup {BackupFile} from storage {StorageId}", backupFile, storageId);

        try
        {
            var response = await _httpClient.GetAsync<string>(
                $"nodes/{nodeName}/storage/{storageId}/download/{Uri.EscapeDataString(backupFile)}",
                cancellationToken);

            _logger.LogInformation("Successfully retrieved download info for backup {BackupFile}", backupFile);
            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download backup {BackupFile}", backupFile);
            throw;
        }
    }

    /// <summary>
    /// Deletes a backup file
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="backupFile">Backup filename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    public async Task DeleteBackupAsync(
        string nodeName,
        string storageId,
        string backupFile,
        CancellationToken cancellationToken = default)
    {
        var volumeId = $"{storageId}:backup/{backupFile}";
        await DeleteVolumeAsync(nodeName, storageId, volumeId, cancellationToken);
    }

    #endregion

    #region ISO and Template Management

    /// <summary>
    /// Gets ISO images in storage
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of ISO images</returns>
    public async Task<List<StorageContent>> GetIsoImagesAsync(
        string nodeName,
        string storageId,
        CancellationToken cancellationToken = default)
    {
        return await GetStorageContentAsync(nodeName, storageId, "iso", null, cancellationToken);
    }

    /// <summary>
    /// Gets container templates in storage
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of container templates</returns>
    public async Task<List<StorageContent>> GetContainerTemplatesAsync(
        string nodeName,
        string storageId,
        CancellationToken cancellationToken = default)
    {
        return await GetStorageContentAsync(nodeName, storageId, "vztmpl", null, cancellationToken);
    }

    /// <summary>
    /// Uploads an ISO or template file to storage
    /// </summary>
    /// <param name="nodeName">Node name</param>
    /// <param name="storageId">Storage identifier</param>
    /// <param name="filename">Local filename</param>
    /// <param name="contentType">Content type (iso or vztmpl)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task ID for tracking the upload</returns>
    public async Task<string> UploadFileAsync(
        string nodeName,
        string storageId,
        string filename,
        string contentType = "iso",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentException("Node name cannot be null or empty", nameof(nodeName));

        if (string.IsNullOrWhiteSpace(storageId))
            throw new ArgumentException("Storage ID cannot be null or empty", nameof(storageId));

        if (string.IsNullOrWhiteSpace(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        _logger.LogInformation("Uploading file {Filename} to storage {StorageId}", filename, storageId);

        try
        {
            var data = new Dictionary<string, object>
            {
                ["filename"] = filename,
                ["content"] = contentType
            };

            var response = await _httpClient.PostAsync<string>(
                $"nodes/{nodeName}/storage/{storageId}/upload",
                data,
                cancellationToken);

            _logger.LogInformation("Started file upload with task ID {TaskId}", response);
            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {Filename}", filename);
            throw;
        }
    }

    #endregion
}
