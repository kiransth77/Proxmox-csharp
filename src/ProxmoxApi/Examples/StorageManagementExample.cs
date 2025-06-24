using System;
using System.Threading.Tasks;
using ProxmoxApi;
using ProxmoxApi.Models;

namespace ProxmoxApi.Examples;

/// <summary>
/// Example demonstrating comprehensive storage management operations
/// </summary>
public static class StorageManagementExample
{
    /// <summary>
    /// Demonstrates various storage management operations
    /// </summary>
    /// <param name="client">Proxmox client</param>
    /// <param name="nodeName">Node name to use for operations</param>
    public static async Task RunStorageManagementExampleAsync(ProxmoxClient client, string nodeName = "pve")
    {
        Console.WriteLine("=== Proxmox Storage Management Example ===");
        Console.WriteLine();

        try
        {
            await DemonstrateStorageListingAsync(client, nodeName);
            await DemonstrateStorageStatusAsync(client, nodeName);
            await DemonstrateStorageContentAsync(client, nodeName);
            await DemonstrateBackupManagementAsync(client, nodeName);
            await DemonstrateIsoAndTemplateManagementAsync(client, nodeName);
            // Note: Storage creation/deletion examples commented out for safety
            // await DemonstrateStorageLifecycleAsync(client);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Storage management example failed: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("=== Storage Management Example Complete ===");
    }

    /// <summary>
    /// Demonstrates listing storage configurations
    /// </summary>
    private static async Task DemonstrateStorageListingAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("📦 Storage Configuration Listing");
        Console.WriteLine("================================");

        // Get all storage configurations
        var storages = await client.Storage.GetStoragesAsync();
        Console.WriteLine($"Found {storages.Count} storage configurations:");

        foreach (var storage in storages)
        {
            Console.WriteLine($"  📁 {storage.Storage} ({storage.Type})");
            Console.WriteLine($"     Path: {storage.Path}");
            Console.WriteLine($"     Content: {storage.Content}");
            Console.WriteLine($"     Enabled: {storage.Enabled}");
            Console.WriteLine($"     Shared: {storage.Shared}");
            
            if (!string.IsNullOrEmpty(storage.Nodes))
            {
                Console.WriteLine($"     Nodes: {storage.Nodes}");
            }
            
            Console.WriteLine();
        }

        // Get specific storage details
        if (storages.Count > 0)
        {
            var firstStorage = storages[0];
            Console.WriteLine($"📋 Detailed configuration for '{firstStorage.Storage}':");
            
            var storageDetails = await client.Storage.GetStorageAsync(firstStorage.Storage);
            if (storageDetails != null)
            {
                Console.WriteLine($"  Type: {storageDetails.Type}");
                Console.WriteLine($"  Path: {storageDetails.Path}");
                Console.WriteLine($"  Content Types: {storageDetails.Content}");
                Console.WriteLine($"  Max Files: {storageDetails.MaxFiles?.ToString() ?? "Unlimited"}");
                
                if (storageDetails.Options.Count > 0)
                {
                    Console.WriteLine("  Options:");
                    foreach (var option in storageDetails.Options)
                    {
                        Console.WriteLine($"    {option.Key}: {option.Value}");
                    }
                }
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates storage status monitoring
    /// </summary>
    private static async Task DemonstrateStorageStatusAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("📊 Storage Status Monitoring");
        Console.WriteLine("============================");

        // Get storage status for the node
        var storageStatuses = await client.Storage.GetStorageStatusAsync(nodeName);
        Console.WriteLine($"Storage status for node '{nodeName}':");

        foreach (var status in storageStatuses)
        {
            Console.WriteLine($"  💽 {status.Storage} ({status.Type})");
            Console.WriteLine($"     Status: {(status.Active ? "✅ Active" : "❌ Inactive")}");
            
            if (status.Total > 0)
            {
                var totalGB = Math.Round(status.Total / (1024.0 * 1024 * 1024), 2);
                var usedGB = Math.Round(status.Used / (1024.0 * 1024 * 1024), 2);
                var availGB = Math.Round(status.Available / (1024.0 * 1024 * 1024), 2);
                
                Console.WriteLine($"     Total: {totalGB:F2} GB");
                Console.WriteLine($"     Used: {usedGB:F2} GB ({status.UsagePercentage:F1}%)");
                Console.WriteLine($"     Available: {availGB:F2} GB");
                
                // Show usage bar
                var usageBar = CreateUsageBar(status.UsagePercentage);
                Console.WriteLine($"     Usage: {usageBar}");
            }
            
            Console.WriteLine($"     Content: {status.Content}");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Demonstrates storage content management
    /// </summary>
    private static async Task DemonstrateStorageContentAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("📁 Storage Content Management");
        Console.WriteLine("=============================");

        var storages = await client.Storage.GetStoragesAsync();
        var dataStorage = storages.Find(s => s.Content.Contains("images")) ?? storages.FirstOrDefault();
        
        if (dataStorage == null)
        {
            Console.WriteLine("No suitable storage found for content demonstration.");
            return;
        }

        Console.WriteLine($"Examining content in storage '{dataStorage.Storage}':");

        // Get all content
        var allContent = await client.Storage.GetStorageContentAsync(nodeName, dataStorage.Storage);
        Console.WriteLine($"  Total content items: {allContent.Count}");

        // Group by content type
        var contentByType = allContent.GroupBy(c => c.Content).ToDictionary(g => g.Key, g => g.ToList());
        
        foreach (var contentType in contentByType)
        {
            Console.WriteLine($"  📂 {contentType.Key}: {contentType.Value.Count} items");
            
            // Show first few items of each type
            var itemsToShow = contentType.Value.Take(3);
            foreach (var item in itemsToShow)
            {
                var sizeText = item.Size > 0 ? $" ({FormatBytes(item.Size)})" : "";
                var vmText = item.VmId.HasValue ? $" [VM {item.VmId}]" : "";
                Console.WriteLine($"    - {item.VolumeId.Split('/').LastOrDefault()}{sizeText}{vmText}");
            }
            
            if (contentType.Value.Count > 3)
            {
                Console.WriteLine($"    ... and {contentType.Value.Count - 3} more");
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates backup management
    /// </summary>
    private static async Task DemonstrateBackupManagementAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("💾 Backup Management");
        Console.WriteLine("===================");

        var storages = await client.Storage.GetStoragesAsync();
        var backupStorage = storages.Find(s => s.Content.Contains("backup")) ?? storages.FirstOrDefault();
        
        if (backupStorage == null)
        {
            Console.WriteLine("No backup storage found.");
            return;
        }

        Console.WriteLine($"Backup management for storage '{backupStorage.Storage}':");

        // Get backup files
        var backups = await client.Storage.GetBackupsAsync(nodeName, backupStorage.Storage);
        Console.WriteLine($"  Found {backups.Count} backup files:");

        foreach (var backup in backups.Take(5)) // Show first 5 backups
        {
            Console.WriteLine($"  📦 {backup.Filename}");
            Console.WriteLine($"     VM ID: {backup.VmId}");
            Console.WriteLine($"     Created: {backup.CreatedAt:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"     Size: {FormatBytes(backup.Size)}");
            Console.WriteLine($"     Format: {backup.Format}");
            Console.WriteLine($"     Protected: {(backup.Protected ? "Yes" : "No")}");
            
            if (!string.IsNullOrEmpty(backup.Notes))
            {
                Console.WriteLine($"     Notes: {backup.Notes}");
            }
            
            Console.WriteLine();
        }

        if (backups.Count > 5)
        {
            Console.WriteLine($"  ... and {backups.Count - 5} more backup files");
        }

        // Group backups by VM
        var backupsByVm = backups.GroupBy(b => b.VmId).ToDictionary(g => g.Key, g => g.ToList());
        Console.WriteLine($"  Backups by VM:");
        foreach (var vmBackups in backupsByVm.Take(3))
        {
            Console.WriteLine($"    VM {vmBackups.Key}: {vmBackups.Value.Count} backups");
            var totalSize = vmBackups.Value.Sum(b => b.Size);
            Console.WriteLine($"      Total size: {FormatBytes(totalSize)}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates ISO and template management
    /// </summary>
    private static async Task DemonstrateIsoAndTemplateManagementAsync(ProxmoxClient client, string nodeName)
    {
        Console.WriteLine("💿 ISO and Template Management");
        Console.WriteLine("==============================");

        var storages = await client.Storage.GetStoragesAsync();
        
        // Find ISO storage
        var isoStorage = storages.Find(s => s.Content.Contains("iso"));
        if (isoStorage != null)
        {
            Console.WriteLine($"ISO images in storage '{isoStorage.Storage}':");
            
            var isoImages = await client.Storage.GetIsoImagesAsync(nodeName, isoStorage.Storage);
            Console.WriteLine($"  Found {isoImages.Count} ISO images:");
            
            foreach (var iso in isoImages.Take(5))
            {
                var filename = iso.VolumeId.Split('/').LastOrDefault() ?? iso.VolumeId;
                Console.WriteLine($"  💿 {filename}");
                Console.WriteLine($"     Size: {FormatBytes(iso.Size)}");
                
                if (iso.CreatedAt.HasValue)
                {
                    Console.WriteLine($"     Created: {iso.CreatedAt:yyyy-MM-dd HH:mm}");
                }
                
                Console.WriteLine();
            }
            
            if (isoImages.Count > 5)
            {
                Console.WriteLine($"  ... and {isoImages.Count - 5} more ISO images");
            }
        }

        // Find template storage
        var templateStorage = storages.Find(s => s.Content.Contains("vztmpl"));
        if (templateStorage != null)
        {
            Console.WriteLine($"Container templates in storage '{templateStorage.Storage}':");
            
            var templates = await client.Storage.GetContainerTemplatesAsync(nodeName, templateStorage.Storage);
            Console.WriteLine($"  Found {templates.Count} container templates:");
            
            foreach (var template in templates.Take(5))
            {
                var filename = template.VolumeId.Split('/').LastOrDefault() ?? template.VolumeId;
                Console.WriteLine($"  📦 {filename}");
                Console.WriteLine($"     Size: {FormatBytes(template.Size)}");
                
                if (template.CreatedAt.HasValue)
                {
                    Console.WriteLine($"     Created: {template.CreatedAt:yyyy-MM-dd HH:mm}");
                }
                
                Console.WriteLine();
            }
            
            if (templates.Count > 5)
            {
                Console.WriteLine($"  ... and {templates.Count - 5} more templates");
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates storage lifecycle management (creation/deletion)
    /// WARNING: These operations modify storage configuration
    /// </summary>
    private static async Task DemonstrateStorageLifecycleAsync(ProxmoxClient client)
    {
        Console.WriteLine("⚠️  Storage Lifecycle Management (DANGEROUS)");
        Console.WriteLine("============================================");
        Console.WriteLine("NOTE: This example demonstrates storage creation/deletion.");
        Console.WriteLine("These operations are commented out for safety.");
        
        /*
        // Example: Create a directory storage
        var storageOptions = new StorageCreateOptions
        {
            Storage = "test-storage",
            Type = "dir",
            Path = "/mnt/test-storage",
            Content = "images,iso,backup",
            Shared = false,
            MaxFiles = 10
        };

        Console.WriteLine($"Creating storage '{storageOptions.Storage}'...");
        await client.Storage.CreateStorageAsync(storageOptions);
        Console.WriteLine("✅ Storage created successfully");

        // Update storage
        storageOptions.Content = "images,iso,backup,vztmpl";
        Console.WriteLine("Updating storage configuration...");
        await client.Storage.UpdateStorageAsync(storageOptions.Storage, storageOptions);
        Console.WriteLine("✅ Storage updated successfully");

        // Delete storage
        Console.WriteLine("Deleting test storage...");
        await client.Storage.DeleteStorageAsync(storageOptions.Storage);
        Console.WriteLine("✅ Storage deleted successfully");
        */

        Console.WriteLine("Storage lifecycle operations completed (simulated).");
        Console.WriteLine();
    }

    /// <summary>
    /// Creates a visual usage bar
    /// </summary>
    private static string CreateUsageBar(double percentage, int width = 20)
    {
        var filled = (int)Math.Round(percentage / 100.0 * width);
        var empty = width - filled;
        
        var bar = new string('█', filled) + new string('░', empty);
        return $"[{bar}] {percentage:F1}%";
    }

    /// <summary>
    /// Formats bytes into human-readable format
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:F2} {suffixes[counter]}";
    }
}
