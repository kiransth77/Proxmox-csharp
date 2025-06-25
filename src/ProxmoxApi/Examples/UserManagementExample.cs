using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProxmoxApi;
using ProxmoxApi.Models;

namespace ProxmoxApi.Examples;

/// <summary>
/// Examples demonstrating user and permission management operations using ProxmoxApi
/// </summary>
public static class UserManagementExample
{
    /// <summary>
    /// Runs comprehensive user management examples
    /// </summary>
    public static async Task RunAsync(ProxmoxClient client)
    {
        Console.WriteLine("=== User and Permission Management Example ===");

        await DemonstrateUserManagementAsync(client);
        await DemonstrateGroupManagementAsync(client);
        await DemonstrateRoleManagementAsync(client);
        await DemonstratePermissionManagementAsync(client);
        await DemonstrateApiTokenManagementAsync(client);
        await DemonstrateAuthRealmManagementAsync(client);

        Console.WriteLine("=== User Management Example Completed ===");
    }

    /// <summary>
    /// Demonstrates user account management
    /// </summary>
    private static async Task DemonstrateUserManagementAsync(ProxmoxClient client)
    {
        Console.WriteLine("\n👥 User Management");
        Console.WriteLine("==================");

        // Get all users
        var users = await client.Users.GetUsersAsync();
        Console.WriteLine($"Found {users.Count} users:");

        foreach (var user in users.Take(10)) // Show first 10
        {
            Console.WriteLine($"  👤 {user.UserId}");
            Console.WriteLine($"     Name: {user.FirstName} {user.LastName}");
            Console.WriteLine($"     Email: {user.Email ?? "N/A"}");
            Console.WriteLine($"     Realm: {user.Realm}");
            Console.WriteLine($"     Groups: {user.Groups ?? "None"}");
            Console.WriteLine($"     Enabled: {user.Enabled ?? true}");
            
            if (user.ExpirationDate.HasValue)
            {
                Console.WriteLine($"     Expires: {user.ExpirationDate:yyyy-MM-dd}");
            }
            Console.WriteLine();
        }

        // Example: Create a new user (commented out for safety)
        /*
        Console.WriteLine("📝 Creating a new user...");
        var newUserParams = new UserCreateParameters
        {
            UserId = "testuser@pam",
            Password = "SecurePassword123!",
            Email = "testuser@example.com",
            FirstName = "Test",
            LastName = "User",
            Comment = "Test user for API demonstration",
            Groups = "test-group",
            Enabled = true,
            ExpirationDate = DateTime.Now.AddMonths(6)
        };

        await client.Users.CreateUserAsync(newUserParams);
        Console.WriteLine("✅ User created successfully");

        // Update the user
        Console.WriteLine("📝 Updating user information...");
        var updateParams = new UserUpdateParameters
        {
            Email = "updated.testuser@example.com",
            Comment = "Updated test user",
            Enabled = false
        };

        await client.Users.UpdateUserAsync("testuser@pam", updateParams);
        Console.WriteLine("✅ User updated successfully");

        // Delete the user
        Console.WriteLine("🗑️ Deleting test user...");
        await client.Users.DeleteUserAsync("testuser@pam");
        Console.WriteLine("✅ User deleted successfully");
        */
    }

    /// <summary>
    /// Demonstrates group management
    /// </summary>
    private static async Task DemonstrateGroupManagementAsync(ProxmoxClient client)
    {
        Console.WriteLine("\n👥 Group Management");
        Console.WriteLine("===================");

        // Get all groups
        var groups = await client.Users.GetGroupsAsync();
        Console.WriteLine($"Found {groups.Count} groups:");

        foreach (var group in groups)
        {
            Console.WriteLine($"  🏷️  {group.GroupId}");
            Console.WriteLine($"     Comment: {group.Comment ?? "N/A"}");
            Console.WriteLine($"     Members: {group.Members.Count}");
            Console.WriteLine();
        }

        // Example: Create a new group (commented out for safety)
        /*
        Console.WriteLine("📝 Creating a new group...");
        var newGroupParams = new GroupCreateParameters
        {
            GroupId = "api-test-group",
            Comment = "Test group created via API"
        };

        await client.Users.CreateGroupAsync(newGroupParams);
        Console.WriteLine("✅ Group created successfully");

        // Update the group
        var updateGroupParams = new GroupUpdateParameters
        {
            Comment = "Updated test group comment"
        };

        await client.Users.UpdateGroupAsync("api-test-group", updateGroupParams);
        Console.WriteLine("✅ Group updated successfully");

        // Delete the group
        await client.Users.DeleteGroupAsync("api-test-group");
        Console.WriteLine("✅ Group deleted successfully");
        */
    }

    /// <summary>
    /// Demonstrates role management
    /// </summary>
    private static async Task DemonstrateRoleManagementAsync(ProxmoxClient client)
    {
        Console.WriteLine("\n🔐 Role Management");
        Console.WriteLine("==================");

        // Get all roles
        var roles = await client.Users.GetRolesAsync();
        Console.WriteLine($"Found {roles.Count} roles:");

        foreach (var role in roles)
        {
            Console.WriteLine($"  🎭 {role.RoleId}");
            Console.WriteLine($"     Comment: {role.Comment ?? "N/A"}");
            Console.WriteLine($"     Special: {role.IsSpecial ?? false}");
            
            if (!string.IsNullOrEmpty(role.Privileges))
            {
                Console.WriteLine($"     Privileges: {role.Privileges}");
                Console.WriteLine($"     Privilege Count: {role.PrivilegeList.Count}");
            }
            Console.WriteLine();
        }

        // Example: Create a custom role (commented out for safety)
        /*
        Console.WriteLine("📝 Creating a custom role...");
        var newRoleParams = new RoleCreateParameters
        {
            RoleId = "CustomVMManager",
            Privileges = "VM.Console,VM.PowerMgmt,VM.Monitor",
            Comment = "Custom role for VM management"
        };

        await client.Users.CreateRoleAsync(newRoleParams);
        Console.WriteLine("✅ Role created successfully");

        // Update the role
        var updateRoleParams = new RoleUpdateParameters
        {
            Privileges = "VM.Console,VM.PowerMgmt,VM.Monitor,VM.Config.Network",
            Comment = "Updated custom VM manager role"
        };

        await client.Users.UpdateRoleAsync("CustomVMManager", updateRoleParams);
        Console.WriteLine("✅ Role updated successfully");

        // Delete the role
        await client.Users.DeleteRoleAsync("CustomVMManager");
        Console.WriteLine("✅ Role deleted successfully");
        */
    }

    /// <summary>
    /// Demonstrates permission and ACL management
    /// </summary>
    private static async Task DemonstratePermissionManagementAsync(ProxmoxClient client)
    {
        Console.WriteLine("\n🔒 Permission Management (ACL)");
        Console.WriteLine("==============================");

        // Get all ACL entries
        var aclEntries = await client.Users.GetAccessControlListAsync();
        Console.WriteLine($"Found {aclEntries.Count} ACL entries:");

        // Group ACL entries by path for better readability
        var groupedEntries = aclEntries.GroupBy(e => e.Path).OrderBy(g => g.Key);

        foreach (var pathGroup in groupedEntries.Take(10)) // Show first 10 paths
        {
            Console.WriteLine($"  📁 Path: {pathGroup.Key}");
            
            foreach (var entry in pathGroup)
            {
                var userType = entry.IsUser ? "👤 User" : "👥 Group";
                var propagate = entry.Propagate == true ? " (propagated)" : "";
                
                Console.WriteLine($"     {userType}: {entry.UserId} → Role: {entry.RoleId}{propagate}");
            }
            Console.WriteLine();
        }

        // Example: Grant permissions to a user (commented out for safety)
        /*
        Console.WriteLine("📝 Granting permissions to a user...");
        var aclParams = new AclCreateParameters
        {
            Path = "/vms/100",
            UserId = "testuser@pam",
            RoleId = "PVEVMUser",
            Propagate = false
        };

        await client.Users.CreateAclEntryAsync(aclParams);
        Console.WriteLine("✅ ACL entry created successfully");

        // Remove the permission
        await client.Users.DeleteAclEntryAsync("/vms/100", "testuser@pam", "PVEVMUser");
        Console.WriteLine("✅ ACL entry deleted successfully");
        */

        // Demonstrate permission checking
        if (aclEntries.Any())
        {
            var firstUserEntry = aclEntries.FirstOrDefault(e => e.IsUser);
            if (firstUserEntry != null)
            {
                Console.WriteLine($"📋 Checking permissions for user: {firstUserEntry.UserId}");
                
                var permissions = await client.Users.GetUserPermissionsAsync(firstUserEntry.UserId, "/");
                Console.WriteLine($"Root permissions: {string.Join(", ", permissions)}");
            }
        }
    }

    /// <summary>
    /// Demonstrates API token management
    /// </summary>
    private static async Task DemonstrateApiTokenManagementAsync(ProxmoxClient client)
    {
        Console.WriteLine("\n🔑 API Token Management");
        Console.WriteLine("=======================");

        // Get all users to demonstrate token management
        var users = await client.Users.GetUsersAsync();
        var firstUser = users.FirstOrDefault();

        if (firstUser != null)
        {
            Console.WriteLine($"📋 Getting API tokens for user: {firstUser.UserId}");
            
            var tokens = await client.Users.GetApiTokensAsync(firstUser.UserId);
            Console.WriteLine($"Found {tokens.Count} API tokens:");

            foreach (var token in tokens)
            {
                Console.WriteLine($"  🔑 {token.TokenId}");
                Console.WriteLine($"     Comment: {token.Comment ?? "N/A"}");
                Console.WriteLine($"     Privilege Separated: {token.PrivilegeSeparated ?? true}");
                
                if (token.ExpirationDate.HasValue)
                {
                    Console.WriteLine($"     Expires: {token.ExpirationDate:yyyy-MM-dd}");
                }
                Console.WriteLine();
            }

            // Example: Create an API token (commented out for safety)
            /*
            Console.WriteLine("📝 Creating a new API token...");
            var tokenParams = new TokenCreateParameters
            {
                Comment = "API token for automation",
                PrivilegeSeparated = true,
                ExpirationDate = DateTime.Now.AddYears(1)
            };

            var tokenResult = await client.Users.CreateApiTokenAsync(firstUser.UserId, "automation-token", tokenParams);
            Console.WriteLine($"✅ API token created: {tokenResult.FullToken}");
            Console.WriteLine("⚠️  Save this token value - it cannot be retrieved again!");

            // Update the token
            var updateTokenParams = new TokenUpdateParameters
            {
                Comment = "Updated automation token",
                ExpirationDate = DateTime.Now.AddMonths(6)
            };

            await client.Users.UpdateApiTokenAsync(firstUser.UserId, "automation-token", updateTokenParams);
            Console.WriteLine("✅ API token updated successfully");

            // Delete the token
            await client.Users.DeleteApiTokenAsync(firstUser.UserId, "automation-token");
            Console.WriteLine("✅ API token deleted successfully");
            */
        }
        else
        {
            Console.WriteLine("No users found to demonstrate token management");
        }
    }

    /// <summary>
    /// Demonstrates authentication realm management
    /// </summary>
    private static async Task DemonstrateAuthRealmManagementAsync(ProxmoxClient client)
    {
        Console.WriteLine("\n🌐 Authentication Realm Management");
        Console.WriteLine("==================================");

        // Get all authentication realms
        var realms = await client.Users.GetAuthRealmsAsync();
        Console.WriteLine($"Found {realms.Count} authentication realms:");

        foreach (var realm in realms)
        {
            Console.WriteLine($"  🌐 {realm.RealmId}");
            Console.WriteLine($"     Type: {realm.Type}");
            Console.WriteLine($"     Comment: {realm.Comment ?? "N/A"}");
            Console.WriteLine($"     Enabled: {realm.Enabled ?? true}");
            Console.WriteLine($"     Default: {realm.IsDefault ?? false}");
            Console.WriteLine();
        }

        // Show realm usage statistics
        var users = await client.Users.GetUsersAsync();
        var realmUsage = users.GroupBy(u => u.Realm)
            .Select(g => new { Realm = g.Key, UserCount = g.Count() })
            .OrderByDescending(r => r.UserCount);

        Console.WriteLine("📊 Realm Usage Statistics:");
        foreach (var usage in realmUsage)
        {
            Console.WriteLine($"  {usage.Realm}: {usage.UserCount} users");
        }
    }

    /// <summary>
    /// Demonstrates user existence checking and validation
    /// </summary>
    private static async Task DemonstrateUserValidationAsync(ProxmoxClient client)
    {
        Console.WriteLine("\n✅ User Validation and Checking");
        Console.WriteLine("===============================");

        // Check if specific users exist
        string[] testUsers = { "root@pam", "nonexistent@pam", "admin@pve" };

        foreach (var userId in testUsers)
        {
            var exists = await client.Users.UserExistsAsync(userId);
            var status = exists ? "✅ EXISTS" : "❌ NOT FOUND";
            Console.WriteLine($"  {status}: {userId}");
        }

        // Check specific groups and roles
        var groupExists = await client.Users.GroupExistsAsync("administrators");
        var roleExists = await client.Users.RoleExistsAsync("Administrator");

        Console.WriteLine($"\nGroup 'administrators' exists: {(groupExists ? "✅ YES" : "❌ NO")}");
        Console.WriteLine($"Role 'Administrator' exists: {(roleExists ? "✅ YES" : "❌ NO")}");
    }

    /// <summary>
    /// Demonstrates comprehensive permission analysis
    /// </summary>
    private static async Task DemonstratePermissionAnalysisAsync(ProxmoxClient client)
    {
        Console.WriteLine("\n🔍 Permission Analysis");
        Console.WriteLine("======================");

        var users = await client.Users.GetUsersAsync();
        var firstUser = users.FirstOrDefault();

        if (firstUser != null)
        {
            Console.WriteLine($"📋 Analyzing permissions for: {firstUser.UserId}");

            // Check permissions on different paths
            string[] paths = { "/", "/nodes", "/vms", "/storage" };

            foreach (var path in paths)
            {
                var permissions = await client.Users.GetUserPermissionsAsync(firstUser.UserId, path);
                Console.WriteLine($"  📁 {path}: {permissions.Count} permissions");
                
                if (permissions.Any())
                {
                    Console.WriteLine($"     {string.Join(", ", permissions.Take(5))}");
                    if (permissions.Count > 5)
                    {
                        Console.WriteLine($"     ... and {permissions.Count - 5} more");
                    }
                }
            }

            // Show ACL entries for this user
            var userAcls = await client.Users.GetAclEntriesForUserAsync(firstUser.UserId);
            Console.WriteLine($"\n🔐 Direct ACL entries for {firstUser.UserId}: {userAcls.Count}");
            
            foreach (var acl in userAcls.Take(5))
            {
                Console.WriteLine($"  📁 {acl.Path} → {acl.RoleId}");
            }
        }
    }
}
