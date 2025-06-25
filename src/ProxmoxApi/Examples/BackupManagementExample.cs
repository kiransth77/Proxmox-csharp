using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProxmoxApi;
using ProxmoxApi.Models;

namespace ProxmoxApi.Examples;

/// <summary>
/// Examples demonstrating backup and restore operations using ProxmoxApi
/// </summary>
public static class BackupManagementExample
{
    /// <summary>
    /// Demonstrates comprehensive backup and restore management
    /// </summary>
    public static async Task RunExamplesAsync()
    {
        var connectionInfo = new ProxmoxConnectionInfo
        {
            Host = "your-proxmox-server.local",
            Username = "root",
            Password = "your-password",
            IgnoreSslErrors = true // Only for development
        };

        using var client = new ProxmoxClient(connectionInfo);

        // Test connection
        if (!await client.TestConnectionAsync())
        {
            Console.WriteLine("❌ Failed to connect to Proxmox server");
            return;
        }

        Console.WriteLine("✅ Connected to Proxmox server");

        try
        {
            // Backup Jobs Management Examples
            await BackupJobManagementExamples(client);
            
            // Backup File Management Examples
            await BackupFileManagementExamples(client);
            
            // Backup Operations Examples
            await BackupOperationsExamples(client);
            
            // Restore Operations Examples
            await RestoreOperationsExamples(client);
            
            // Task Management Examples
            await TaskManagementExamples(client);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates backup job management operations
    /// </summary>
    private static async Task BackupJobManagementExamples(ProxmoxClient client)
    {
        Console.WriteLine("\n🔄 === Backup Job Management Examples ===");

        try
        {
            // Get all backup jobs
            Console.WriteLine("\n📋 Getting all backup jobs...");
            var backupJobs = await client.Backup.GetBackupJobsAsync();
            
            Console.WriteLine($"Found {backupJobs.Count} backup jobs:");
            foreach (var job in backupJobs)
            {
                var status = job.Enabled ? "✅ Enabled" : "❌ Disabled";
                Console.WriteLine($"  📦 Job {job.Id}: {job.Schedule} -> {job.Storage} ({status})");
                Console.WriteLine($"     VMs: {job.VmIds}");
                Console.WriteLine($"     Comment: {job.Comment}");
            }

            // Get specific backup job
            if (backupJobs.Any())
            {
                Console.WriteLine($"\n📄 Getting details for job {backupJobs.First().Id}...");
                var jobDetails = await client.Backup.GetBackupJobAsync(backupJobs.First().Id);
                
                if (jobDetails != null)
                {
                    Console.WriteLine($"  📦 Job ID: {jobDetails.Id}");
                    Console.WriteLine($"  ⏰ Schedule: {jobDetails.Schedule}");
                    Console.WriteLine($"  💾 Storage: {jobDetails.Storage}");
                    Console.WriteLine($"  🖥️ Node: {jobDetails.Node}");
                    Console.WriteLine($"  🔧 Mode: {jobDetails.Mode}");
                    Console.WriteLine($"  📋 Compression: {jobDetails.Compress}");
                    Console.WriteLine($"  📨 Mail to: {jobDetails.MailTo}");
                }
            }

            // Create a new backup job
            Console.WriteLine("\n➕ Creating a new backup job...");
            var newJobParameters = new BackupJobParameters
            {
                Schedule = "0 2 * * 0", // Weekly backup at 2 AM on Sundays
                Storage = "local",
                Node = "pve",
                VmIds = "100,101",
                Enabled = true,
                Mode = "snapshot",
                Compress = "zstd",
                Comment = "Weekly backup created by API example",
                MailTo = "admin@example.com",
                MailNotification = "failure",
                Remove = 3 // Keep 3 backups
            };

            var createResult = await client.Backup.CreateBackupJobAsync(newJobParameters);
            Console.WriteLine($"✅ Backup job creation result: {createResult}");

            // Update backup job (example - you would use an actual job ID)
            /*
            Console.WriteLine("\n✏️ Updating backup job...");
            var updateParameters = new BackupJobParameters
            {
                Enabled = false,
                Comment = "Temporarily disabled for maintenance"
            };
            
            await client.Backup.UpdateBackupJobAsync("backup-job-id", updateParameters);
            Console.WriteLine("✅ Backup job updated successfully");
            */

            // Delete backup job (example - be careful with this!)
            /*
            Console.WriteLine("\n🗑️ Deleting backup job...");
            await client.Backup.DeleteBackupJobAsync("backup-job-id");
            Console.WriteLine("✅ Backup job deleted successfully");
            */
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Backup job management error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates backup file management operations
    /// </summary>
    private static async Task BackupFileManagementExamples(ProxmoxClient client)
    {
        Console.WriteLine("\n💾 === Backup File Management Examples ===");

        try
        {
            const string node = "pve"; // Replace with your node name

            // Get all backup files
            Console.WriteLine($"\n📂 Getting all backup files on node {node}...");
            var backupFiles = await client.Backup.GetBackupFilesAsync(node);
            
            Console.WriteLine($"Found {backupFiles.Count} backup files:");
            foreach (var file in backupFiles.Take(5)) // Show first 5
            {
                Console.WriteLine($"  📦 {file.VolumeId}");
                Console.WriteLine($"     VM/CT: {file.VmId}");
                Console.WriteLine($"     Size: {file.FormattedSize}");
                Console.WriteLine($"     Created: {file.CreationDateTime:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"     Format: {file.Format}");
                Console.WriteLine($"     Protected: {(file.Protected ? "✅" : "❌")}");
                Console.WriteLine($"     Encrypted: {(file.Encrypted ? "🔒" : "🔓")}");
                Console.WriteLine();
            }

            // Get backup files for specific VM
            if (backupFiles.Any())
            {
                var firstVmId = backupFiles.First().VmId;
                Console.WriteLine($"\n🖥️ Getting backup files for VM {firstVmId}...");
                var vmBackups = await client.Backup.GetVmBackupFilesAsync(node, firstVmId);
                
                Console.WriteLine($"Found {vmBackups.Count} backup files for VM {firstVmId}:");
                foreach (var backup in vmBackups)
                {
                    Console.WriteLine($"  📦 {Path.GetFileName(backup.VolumeId)}");
                    Console.WriteLine($"     Created: {backup.CreationDateTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"     Size: {backup.FormattedSize}");
                }
            }

            // Delete a backup file (example - be very careful with this!)
            /*
            Console.WriteLine("\n🗑️ Deleting a backup file...");
            if (backupFiles.Any())
            {
                var backupToDelete = backupFiles.First();
                await client.Backup.DeleteBackupFileAsync(node, backupToDelete.Storage, backupToDelete.VolumeId);
                Console.WriteLine($"✅ Backup file {backupToDelete.VolumeId} deleted successfully");
            }
            */
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Backup file management error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates backup creation operations
    /// </summary>
    private static async Task BackupOperationsExamples(ProxmoxClient client)
    {
        Console.WriteLine("\n💾 === Backup Operations Examples ===");

        try
        {
            const string node = "pve"; // Replace with your node name
            const string storage = "local"; // Replace with your storage name

            // Create VM backup
            Console.WriteLine("\n🖥️ Creating VM backup...");
            const int vmId = 100; // Replace with actual VM ID
            
            var vmBackupTaskId = await client.Backup.CreateVmBackupAsync(
                node: node,
                vmId: vmId,
                storage: storage,
                mode: "snapshot", // snapshot, suspend, stop
                compress: "zstd" // zstd, gzip, lzo
            );
            
            Console.WriteLine($"✅ VM backup task started: {vmBackupTaskId}");

            // Monitor the backup task
            Console.WriteLine("⏳ Monitoring backup progress...");
            var vmBackupTask = await MonitorTask(client, node, vmBackupTaskId, "VM backup");

            // Create Container backup
            Console.WriteLine("\n📦 Creating Container backup...");
            const int containerId = 200; // Replace with actual Container ID
            
            var containerBackupTaskId = await client.Backup.CreateContainerBackupAsync(
                node: node,
                containerId: containerId,
                storage: storage,
                mode: "snapshot",
                compress: "zstd"
            );
            
            Console.WriteLine($"✅ Container backup task started: {containerBackupTaskId}");

            // Monitor the container backup task
            Console.WriteLine("⏳ Monitoring container backup progress...");
            var containerBackupTask = await MonitorTask(client, node, containerBackupTaskId, "Container backup");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Backup operations error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates restore operations
    /// </summary>
    private static async Task RestoreOperationsExamples(ProxmoxClient client)
    {
        Console.WriteLine("\n🔄 === Restore Operations Examples ===");

        try
        {
            const string node = "pve"; // Replace with your node name

            // Get available backup files for restore
            var backupFiles = await client.Backup.GetBackupFilesAsync(node);
            
            if (!backupFiles.Any())
            {
                Console.WriteLine("ℹ️ No backup files available for restore examples");
                return;
            }

            var latestVmBackup = backupFiles
                .Where(b => b.VolumeId.Contains("qemu"))
                .OrderByDescending(b => b.CreationTime)
                .FirstOrDefault();

            var latestContainerBackup = backupFiles
                .Where(b => b.VolumeId.Contains("lxc"))
                .OrderByDescending(b => b.CreationTime)
                .FirstOrDefault();

            // Restore VM from backup
            if (latestVmBackup != null)
            {
                Console.WriteLine($"\n🖥️ Restoring VM from backup: {Path.GetFileName(latestVmBackup.VolumeId)}");
                
                const int newVmId = 999; // New VM ID for restore
                var restoreParameters = new RestoreParameters
                {
                    Storage = "local",
                    Start = false, // Don't auto-start after restore
                    Force = false  // Don't overwrite existing VM
                };

                var vmRestoreTaskId = await client.Backup.RestoreVmAsync(
                    node: node,
                    vmId: newVmId,
                    archive: latestVmBackup.VolumeId,
                    parameters: restoreParameters
                );
                
                Console.WriteLine($"✅ VM restore task started: {vmRestoreTaskId}");
                
                // Monitor restore progress
                var vmRestoreTask = await MonitorTask(client, node, vmRestoreTaskId, "VM restore");
            }

            // Restore Container from backup
            if (latestContainerBackup != null)
            {
                Console.WriteLine($"\n📦 Restoring Container from backup: {Path.GetFileName(latestContainerBackup.VolumeId)}");
                
                const int newContainerId = 998; // New Container ID for restore
                var restoreParameters = new RestoreParameters
                {
                    Storage = "local",
                    Start = false,
                    Force = false
                };

                var containerRestoreTaskId = await client.Backup.RestoreContainerAsync(
                    node: node,
                    containerId: newContainerId,
                    archive: latestContainerBackup.VolumeId,
                    parameters: restoreParameters
                );
                
                Console.WriteLine($"✅ Container restore task started: {containerRestoreTaskId}");
                
                // Monitor restore progress
                var containerRestoreTask = await MonitorTask(client, node, containerRestoreTaskId, "Container restore");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Restore operations error: {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates task management operations
    /// </summary>
    private static async Task TaskManagementExamples(ProxmoxClient client)
    {
        Console.WriteLine("\n📋 === Task Management Examples ===");

        try
        {
            const string node = "pve"; // Replace with your node name

            // Get all backup tasks
            Console.WriteLine($"\n📂 Getting backup tasks on node {node}...");
            var backupTasks = await client.Backup.GetBackupTasksAsync(node);
            
            Console.WriteLine($"Found {backupTasks.Count} backup tasks:");
            foreach (var task in backupTasks.Take(5)) // Show first 5
            {
                var statusIcon = task.IsSuccessful ? "✅" : task.IsRunning ? "⏳" : "❌";
                var duration = task.Duration?.ToString(@"hh\:mm\:ss") ?? "N/A";
                
                Console.WriteLine($"  {statusIcon} {task.TaskId}");
                Console.WriteLine($"     Type: {task.Type}");
                Console.WriteLine($"     Status: {task.Status}");
                Console.WriteLine($"     Started: {task.StartDateTime:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"     Duration: {duration}");
                Console.WriteLine($"     User: {task.User}");
                Console.WriteLine();
            }

            // Get status of a specific task
            if (backupTasks.Any())
            {
                var task = backupTasks.First();
                Console.WriteLine($"\n📄 Getting detailed status for task {task.TaskId}...");
                
                var taskStatus = await client.Backup.GetTaskStatusAsync(node, task.TaskId);
                if (taskStatus != null)
                {
                    Console.WriteLine($"  📋 Task ID: {taskStatus.TaskId}");
                    Console.WriteLine($"  🖥️ Node: {taskStatus.Node}");
                    Console.WriteLine($"  👤 User: {taskStatus.User}");
                    Console.WriteLine($"  📊 Status: {taskStatus.Status}");
                    Console.WriteLine($"  ⏰ Started: {taskStatus.StartDateTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"  ⏰ Ended: {taskStatus.EndDateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
                    Console.WriteLine($"  ⏱️ Duration: {taskStatus.Duration?.ToString(@"hh\:mm\:ss") ?? "N/A"}");
                    Console.WriteLine($"  ✅ Running: {taskStatus.IsRunning}");
                    Console.WriteLine($"  🎯 Successful: {taskStatus.IsSuccessful}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Task management error: {ex.Message}");
        }
    }

    /// <summary>
    /// Monitor a task until completion
    /// </summary>
    private static async Task<BackupTask?> MonitorTask(ProxmoxClient client, string node, string taskId, string taskType)
    {
        try
        {
            Console.WriteLine($"⏳ Monitoring {taskType} task {taskId}...");
            
            var completedTask = await client.Backup.WaitForTaskCompletionAsync(
                node: node,
                taskId: taskId,
                timeout: 300 // 5 minutes timeout
            );

            if (completedTask != null)
            {
                var statusIcon = completedTask.IsSuccessful ? "✅" : "❌";
                var duration = completedTask.Duration?.ToString(@"hh\:mm\:ss") ?? "N/A";
                
                Console.WriteLine($"{statusIcon} {taskType} task completed in {duration}");
                Console.WriteLine($"   Status: {completedTask.Status}");
                Console.WriteLine($"   Exit Status: {completedTask.ExitStatus}");
                
                return completedTask;
            }
        }
        catch (TimeoutException)
        {
            Console.WriteLine($"⏰ {taskType} task monitoring timed out");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error monitoring {taskType} task: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Demonstrates backup scheduling and automation
    /// </summary>
    public static async Task BackupSchedulingExampleAsync()
    {
        Console.WriteLine("\n📅 === Backup Scheduling Example ===");

        var connectionInfo = new ProxmoxConnectionInfo
        {
            Host = "your-proxmox-server.local",
            Username = "root",
            Password = "your-password",
            IgnoreSslErrors = true
        };

        using var client = new ProxmoxClient(connectionInfo);

        try
        {
            // Create comprehensive backup strategy
            Console.WriteLine("🏗️ Creating comprehensive backup strategy...");

            // Daily backup for critical VMs
            var dailyBackupJob = new BackupJobParameters
            {
                Schedule = "0 1 * * *", // Daily at 1 AM
                Storage = "backup-storage",
                Node = "pve",
                VmIds = "100,101,102", // Critical VMs
                Enabled = true,
                Mode = "snapshot",
                Compress = "zstd",
                Comment = "Daily backup for critical VMs",
                MailTo = "admin@example.com",
                MailNotification = "failure",
                Remove = 7, // Keep 7 daily backups
                MaxFiles = 1,
                BandwidthLimit = 100000, // 100 MB/s limit
                StartTime = "01:00"
            };

            var dailyJobResult = await client.Backup.CreateBackupJobAsync(dailyBackupJob);
            Console.WriteLine($"✅ Daily backup job created: {dailyJobResult}");

            // Weekly backup for all VMs
            var weeklyBackupJob = new BackupJobParameters
            {
                Schedule = "0 2 * * 0", // Weekly on Sunday at 2 AM
                Storage = "backup-storage",
                Node = "pve",
                VmIds = "all", // All VMs
                Enabled = true,
                Mode = "snapshot",
                Compress = "zstd",
                Comment = "Weekly backup for all VMs",
                MailTo = "admin@example.com",
                MailNotification = "always",
                Remove = 4, // Keep 4 weekly backups
                MaxFiles = 1,
                StartTime = "02:00"
            };

            var weeklyJobResult = await client.Backup.CreateBackupJobAsync(weeklyBackupJob);
            Console.WriteLine($"✅ Weekly backup job created: {weeklyJobResult}");

            Console.WriteLine("🎯 Backup strategy implemented successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Backup scheduling error: {ex.Message}");
        }
    }
}
