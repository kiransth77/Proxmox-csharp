using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Models;

namespace ProxmoxApi.Examples;

/// <summary>
/// Example demonstrating VM management operations
/// </summary>
public static class VmManagementExample
{
    /// <summary>
    /// Demonstrates comprehensive VM management operations
    /// </summary>
    /// <param name="client">Proxmox client instance</param>
    /// <param name="logger">Logger instance</param>
    public static async Task RunExampleAsync(ProxmoxClient client, ILogger logger)
    {
        logger.LogInformation("=== Proxmox VM Management Example ===");

        try
        {
            // Get all VMs in the cluster
            logger.LogInformation("\n1. Listing all VMs in cluster:");
            var allVms = await client.Vms.GetVmsAsync();
            foreach (var vm in allVms)
            {
                logger.LogInformation("  VM {VmId}: {Name} on {Node} - Status: {Status}",
                    vm.VmId, vm.Name, vm.Node, vm.Status);
            }

            if (allVms.Count == 0)
            {
                logger.LogWarning("No VMs found in cluster. Create a VM first to test VM operations.");
                return;
            }

            // Get VMs on first available node
            var firstVm = allVms[0];
            logger.LogInformation("\n2. Getting VMs on node '{Node}':", firstVm.Node);
            var nodeVms = await client.Vms.GetVmsOnNodeAsync(firstVm.Node);
            foreach (var vm in nodeVms)
            {
                logger.LogInformation("  VM {VmId}: {Name} - Status: {Status}",
                    vm.VmId, vm.Name, vm.Status);
            }

            // Demonstrate detailed VM operations with the first VM
            await DemonstrateVmOperationsAsync(client, firstVm, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during VM management example");
        }
    }

    private static async Task DemonstrateVmOperationsAsync(ProxmoxClient client, ProxmoxVm vm, ILogger logger)
    {
        try
        {
            logger.LogInformation("\n3. Demonstrating operations with VM {VmId} ({Name}):", vm.VmId, vm.Name);

            // Get VM status
            logger.LogInformation("Getting VM status...");
            var vmStatus = await client.Vms.GetVmStatusAsync(vm.Node, vm.VmId); if (vmStatus != null)
            {
                logger.LogInformation("  Status: {Status}", vmStatus.Status);
                logger.LogInformation("  CPU Usage: {CpuUsage:P2}", vmStatus.CpuUsage ?? 0);
                logger.LogInformation("  Memory: {MemoryUsage}/{MaxMemory} MB",
                    vmStatus.MemoryUsage / (1024 * 1024), vmStatus.MaxMemory / (1024 * 1024));
                logger.LogInformation("  Uptime: {Uptime}s", vmStatus.Uptime);
            }

            // Get VM configuration
            logger.LogInformation("Getting VM configuration...");
            var vmConfig = await client.Vms.GetVmConfigAsync(vm.Node, vm.VmId);
            if (vmConfig != null)
            {
                logger.LogInformation("  Configuration keys: {Keys}", string.Join(", ", vmConfig.Keys));

                // Show some common configuration items
                if (vmConfig.TryGetValue("memory", out var memory))
                    logger.LogInformation("  Configured Memory: {Memory}", memory);
                if (vmConfig.TryGetValue("cores", out var cores))
                    logger.LogInformation("  Configured Cores: {Cores}", cores);
                if (vmConfig.TryGetValue("ostype", out var ostype))
                    logger.LogInformation("  OS Type: {OsType}", ostype);
            }

            // Get VM statistics
            logger.LogInformation("Getting VM statistics...");
            var vmStats = await client.Vms.GetVmStatisticsAsync(vm.Node, vm.VmId);
            if (vmStats != null)
            {
                logger.LogInformation("  CPU Usage: {CpuUsage:P2}", vmStats.CpuUsage);
                logger.LogInformation("  Memory Usage: {MemoryUsage} MB", vmStats.MemoryUsage);
                logger.LogInformation("  Disk Read: {DiskRead} bytes", vmStats.DiskRead);
                logger.LogInformation("  Disk Write: {DiskWrite} bytes", vmStats.DiskWrite);
                logger.LogInformation("  Network In: {NetworkIn} bytes", vmStats.NetworkIn);
                logger.LogInformation("  Network Out: {NetworkOut} bytes", vmStats.NetworkOut);
            }

            // Demonstrate VM lifecycle operations (only if stopped)
            if (vmStatus?.Status == "stopped")
            {
                await DemonstrateVmLifecycleAsync(client, vm, logger);
            }
            else
            {
                logger.LogInformation("VM is running. Skipping lifecycle operations for safety.");
                logger.LogInformation("To test lifecycle operations, ensure you have a stopped VM.");
            }

            // Demonstrate snapshot operations
            await DemonstrateSnapshotOperationsAsync(client, vm, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during VM operations demonstration");
        }
    }

    private static async Task DemonstrateVmLifecycleAsync(ProxmoxClient client, ProxmoxVm vm, ILogger logger)
    {
        try
        {
            logger.LogInformation("\n4. Demonstrating VM lifecycle operations:");
            logger.LogWarning("⚠️  This will start the VM. Make sure this is safe in your environment!");

            // Start VM
            logger.LogInformation("Starting VM {VmId}...", vm.VmId);
            var startTaskId = await client.Vms.StartVmAsync(vm.Node, vm.VmId);
            logger.LogInformation("  Start task initiated: {TaskId}", startTaskId);

            // Wait a moment for the operation to process
            await Task.Delay(2000);

            // Check status after start
            var statusAfterStart = await client.Vms.GetVmStatusAsync(vm.Node, vm.VmId);
            logger.LogInformation("  Status after start: {Status}", statusAfterStart?.Status);

            // Demonstrate other operations (commented out for safety)
            logger.LogInformation("Other available operations:");
            logger.LogInformation("  - Stop VM: client.Vms.StopVmAsync(node, vmId)");
            logger.LogInformation("  - Restart VM: client.Vms.RestartVmAsync(node, vmId)");
            logger.LogInformation("  - Pause VM: client.Vms.PauseVmAsync(node, vmId)");
            logger.LogInformation("  - Resume VM: client.Vms.ResumeVmAsync(node, vmId)");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during VM lifecycle demonstration");
        }
    }

    private static async Task DemonstrateSnapshotOperationsAsync(ProxmoxClient client, ProxmoxVm vm, ILogger logger)
    {
        try
        {
            logger.LogInformation("\n5. Demonstrating snapshot operations:");

            // List existing snapshots
            logger.LogInformation("Getting existing snapshots...");
            var snapshots = await client.Vms.GetVmSnapshotsAsync(vm.Node, vm.VmId);
            logger.LogInformation("  Found {Count} snapshots", snapshots.Count);

            foreach (var snapshot in snapshots)
            {
                logger.LogInformation("  Snapshot: {Name} - {Description} (Created: {Created})",
                    snapshot.Name, snapshot.Description, snapshot.Created);
            }

            // Create a test snapshot
            var snapshotName = $"api-test-{DateTime.Now:yyyyMMdd-HHmmss}";
            logger.LogInformation("Creating test snapshot '{SnapshotName}'...", snapshotName);

            var createTaskId = await client.Vms.CreateVmSnapshotAsync(
                vm.Node,
                vm.VmId,
                snapshotName,
                "Test snapshot created via ProxmoxApi library",
                includeMemory: false);

            logger.LogInformation("  Snapshot creation task initiated: {TaskId}", createTaskId);

            logger.LogInformation("Snapshot operations demonstrated. You can:");
            logger.LogInformation("  - Delete snapshot: client.Vms.DeleteVmSnapshotAsync(node, vmId, snapshotName)");
            logger.LogInformation("  - Rollback to snapshot: client.Vms.RollbackVmSnapshotAsync(node, vmId, snapshotName)");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during snapshot operations demonstration");
        }
    }

    /// <summary>
    /// Demonstrates VM cloning operations
    /// </summary>
    /// <param name="client">Proxmox client instance</param>
    /// <param name="sourceVm">Source VM to clone</param>
    /// <param name="newVmId">New VM ID for the clone</param>
    /// <param name="logger">Logger instance</param>
    public static async Task DemonstrateVmCloningAsync(ProxmoxClient client, ProxmoxVm sourceVm, int newVmId, ILogger logger)
    {
        try
        {
            logger.LogInformation("\n6. Demonstrating VM cloning:");
            logger.LogInformation("Cloning VM {SourceVmId} to new VM {NewVmId}...", sourceVm.VmId, newVmId);

            var cloneTaskId = await client.Vms.CloneVmAsync(
                sourceVm.Node,
                sourceVm.VmId,
                newVmId,
                $"{sourceVm.Name}-clone",
                "Cloned via ProxmoxApi library",
                fullClone: false);

            logger.LogInformation("  Clone task initiated: {TaskId}", cloneTaskId);
            logger.LogInformation("  Monitor task progress in Proxmox web interface");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during VM cloning demonstration");
        }
    }
}
