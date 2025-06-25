using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Core;
using ProxmoxApi.Models;

namespace ProxmoxApi.Services;

/// <summary>
/// Service for managing Proxmox VE users, groups, roles, and permissions
/// </summary>
public class UserManagementService
{
    private readonly IProxmoxHttpClient _httpClient;
    private readonly ILogger<UserManagementService>? _logger;

    public UserManagementService(IProxmoxHttpClient httpClient, ILogger<UserManagementService>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger;
    }

    #region User Management

    /// <summary>
    /// Get all users
    /// </summary>
    public async Task<List<ProxmoxUser>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Getting all users");
        
        var response = await _httpClient.GetAsync<ProxmoxApiResponse<List<ProxmoxUser>>>(
            "/access/users", cancellationToken);

        return response.Data ?? new List<ProxmoxUser>();
    }

    /// <summary>
    /// Get specific user details
    /// </summary>
    public async Task<ProxmoxUser?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        
        _logger?.LogDebug("Getting user: {UserId}", userId);

        var response = await _httpClient.GetAsync<ProxmoxApiResponse<ProxmoxUser>>(
            $"/access/users/{userId}", cancellationToken);

        return response.Data;
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    public async Task CreateUserAsync(UserCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameters.UserId);

        _logger?.LogDebug("Creating user: {UserId}", parameters.UserId);

        var requestData = new Dictionary<string, object>
        {
            ["userid"] = parameters.UserId
        };

        if (!string.IsNullOrEmpty(parameters.Password))
            requestData["password"] = parameters.Password;

        if (!string.IsNullOrEmpty(parameters.Comment))
            requestData["comment"] = parameters.Comment;

        if (!string.IsNullOrEmpty(parameters.Email))
            requestData["email"] = parameters.Email;

        if (!string.IsNullOrEmpty(parameters.FirstName))
            requestData["firstname"] = parameters.FirstName;

        if (!string.IsNullOrEmpty(parameters.LastName))
            requestData["lastname"] = parameters.LastName;

        if (!string.IsNullOrEmpty(parameters.Groups))
            requestData["groups"] = parameters.Groups;

        if (parameters.Enabled.HasValue)
            requestData["enable"] = parameters.Enabled.Value ? 1 : 0;

        if (parameters.ExpirationDate.HasValue)
            requestData["expire"] = ((DateTimeOffset)parameters.ExpirationDate.Value).ToUnixTimeSeconds();

        await _httpClient.PostAsync<object>("/access/users", requestData, cancellationToken);
        
        _logger?.LogInformation("User created successfully: {UserId}", parameters.UserId);
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    public async Task UpdateUserAsync(string userId, UserUpdateParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(parameters);

        _logger?.LogDebug("Updating user: {UserId}", userId);

        var requestData = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(parameters.Password))
            requestData["password"] = parameters.Password;

        if (!string.IsNullOrEmpty(parameters.Comment))
            requestData["comment"] = parameters.Comment;

        if (!string.IsNullOrEmpty(parameters.Email))
            requestData["email"] = parameters.Email;

        if (!string.IsNullOrEmpty(parameters.FirstName))
            requestData["firstname"] = parameters.FirstName;

        if (!string.IsNullOrEmpty(parameters.LastName))
            requestData["lastname"] = parameters.LastName;

        if (!string.IsNullOrEmpty(parameters.Groups))
            requestData["groups"] = parameters.Groups;

        if (parameters.Enabled.HasValue)
            requestData["enable"] = parameters.Enabled.Value ? 1 : 0;

        if (parameters.ExpirationDate.HasValue)
            requestData["expire"] = ((DateTimeOffset)parameters.ExpirationDate.Value).ToUnixTimeSeconds();

        if (requestData.Any())
        {
            await _httpClient.PutAsync<object>($"/access/users/{userId}", requestData, cancellationToken);
            _logger?.LogInformation("User updated successfully: {UserId}", userId);
        }
        else
        {
            _logger?.LogWarning("No changes to update for user: {UserId}", userId);
        }
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        _logger?.LogDebug("Deleting user: {UserId}", userId);

        await _httpClient.DeleteAsync<object>($"/access/users/{userId}", cancellationToken);
        
        _logger?.LogInformation("User deleted successfully: {UserId}", userId);
    }

    #endregion

    #region Group Management

    /// <summary>
    /// Get all groups
    /// </summary>
    public async Task<List<ProxmoxGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Getting all groups");
        
        var response = await _httpClient.GetAsync<ProxmoxApiResponse<List<ProxmoxGroup>>>(
            "/access/groups", cancellationToken);

        return response.Data ?? new List<ProxmoxGroup>();
    }

    /// <summary>
    /// Get specific group details
    /// </summary>
    public async Task<ProxmoxGroup?> GetGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        
        _logger?.LogDebug("Getting group: {GroupId}", groupId);

        var response = await _httpClient.GetAsync<ProxmoxApiResponse<ProxmoxGroup>>(
            $"/access/groups/{groupId}", cancellationToken);

        return response.Data;
    }

    /// <summary>
    /// Create a new group
    /// </summary>
    public async Task CreateGroupAsync(GroupCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameters.GroupId);

        _logger?.LogDebug("Creating group: {GroupId}", parameters.GroupId);

        var requestData = new Dictionary<string, object>
        {
            ["groupid"] = parameters.GroupId
        };

        if (!string.IsNullOrEmpty(parameters.Comment))
            requestData["comment"] = parameters.Comment;

        await _httpClient.PostAsync<object>("/access/groups", requestData, cancellationToken);
        
        _logger?.LogInformation("Group created successfully: {GroupId}", parameters.GroupId);
    }

    /// <summary>
    /// Update an existing group
    /// </summary>
    public async Task UpdateGroupAsync(string groupId, GroupUpdateParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        ArgumentNullException.ThrowIfNull(parameters);

        _logger?.LogDebug("Updating group: {GroupId}", groupId);

        var requestData = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(parameters.Comment))
            requestData["comment"] = parameters.Comment;

        if (requestData.Any())
        {
            await _httpClient.PutAsync<object>($"/access/groups/{groupId}", requestData, cancellationToken);
            _logger?.LogInformation("Group updated successfully: {GroupId}", groupId);
        }
        else
        {
            _logger?.LogWarning("No changes to update for group: {GroupId}", groupId);
        }
    }

    /// <summary>
    /// Delete a group
    /// </summary>
    public async Task DeleteGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        _logger?.LogDebug("Deleting group: {GroupId}", groupId);

        await _httpClient.DeleteAsync<object>($"/access/groups/{groupId}", cancellationToken);
        
        _logger?.LogInformation("Group deleted successfully: {GroupId}", groupId);
    }

    #endregion

    #region Role Management

    /// <summary>
    /// Get all roles
    /// </summary>
    public async Task<List<ProxmoxRole>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Getting all roles");
        
        var response = await _httpClient.GetAsync<ProxmoxApiResponse<List<ProxmoxRole>>>(
            "/access/roles", cancellationToken);

        return response.Data ?? new List<ProxmoxRole>();
    }

    /// <summary>
    /// Get specific role details
    /// </summary>
    public async Task<ProxmoxRole?> GetRoleAsync(string roleId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        
        _logger?.LogDebug("Getting role: {RoleId}", roleId);

        var response = await _httpClient.GetAsync<ProxmoxApiResponse<ProxmoxRole>>(
            $"/access/roles/{roleId}", cancellationToken);

        return response.Data;
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    public async Task CreateRoleAsync(RoleCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameters.RoleId);

        _logger?.LogDebug("Creating role: {RoleId}", parameters.RoleId);

        var requestData = new Dictionary<string, object>
        {
            ["roleid"] = parameters.RoleId
        };

        if (!string.IsNullOrEmpty(parameters.Privileges))
            requestData["privs"] = parameters.Privileges;

        if (!string.IsNullOrEmpty(parameters.Comment))
            requestData["comment"] = parameters.Comment;

        await _httpClient.PostAsync<object>("/access/roles", requestData, cancellationToken);
        
        _logger?.LogInformation("Role created successfully: {RoleId}", parameters.RoleId);
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    public async Task UpdateRoleAsync(string roleId, RoleUpdateParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);
        ArgumentNullException.ThrowIfNull(parameters);

        _logger?.LogDebug("Updating role: {RoleId}", roleId);

        var requestData = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(parameters.Privileges))
            requestData["privs"] = parameters.Privileges;

        if (!string.IsNullOrEmpty(parameters.Comment))
            requestData["comment"] = parameters.Comment;

        if (requestData.Any())
        {
            await _httpClient.PutAsync<object>($"/access/roles/{roleId}", requestData, cancellationToken);
            _logger?.LogInformation("Role updated successfully: {RoleId}", roleId);
        }
        else
        {
            _logger?.LogWarning("No changes to update for role: {RoleId}", roleId);
        }
    }

    /// <summary>
    /// Delete a role
    /// </summary>
    public async Task DeleteRoleAsync(string roleId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        _logger?.LogDebug("Deleting role: {RoleId}", roleId);

        await _httpClient.DeleteAsync<object>($"/access/roles/{roleId}", cancellationToken);
        
        _logger?.LogInformation("Role deleted successfully: {RoleId}", roleId);
    }

    #endregion

    #region Access Control Lists (ACL)

    /// <summary>
    /// Get all ACL entries
    /// </summary>
    public async Task<List<AccessControlEntry>> GetAccessControlListAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Getting access control list");
        
        var response = await _httpClient.GetAsync<ProxmoxApiResponse<List<AccessControlEntry>>>(
            "/access/acl", cancellationToken);

        return response.Data ?? new List<AccessControlEntry>();
    }

    /// <summary>
    /// Create an ACL entry
    /// </summary>
    public async Task CreateAclEntryAsync(AclCreateParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameters.Path);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameters.UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameters.RoleId);

        _logger?.LogDebug("Creating ACL entry for path: {Path}, user: {UserId}, role: {RoleId}", 
            parameters.Path, parameters.UserId, parameters.RoleId);

        var requestData = new Dictionary<string, object>
        {
            ["path"] = parameters.Path,
            ["users"] = parameters.UserId,
            ["roles"] = parameters.RoleId
        };

        if (parameters.Propagate.HasValue)
            requestData["propagate"] = parameters.Propagate.Value ? 1 : 0;

        await _httpClient.PutAsync<object>("/access/acl", requestData, cancellationToken);
        
        _logger?.LogInformation("ACL entry created successfully for path: {Path}, user: {UserId}, role: {RoleId}", 
            parameters.Path, parameters.UserId, parameters.RoleId);
    }

    /// <summary>
    /// Delete an ACL entry
    /// </summary>
    public async Task DeleteAclEntryAsync(string path, string userId, string roleId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleId);

        _logger?.LogDebug("Deleting ACL entry for path: {Path}, user: {UserId}, role: {RoleId}", 
            path, userId, roleId);

        var requestData = new Dictionary<string, object>
        {
            ["path"] = path,
            ["users"] = userId,
            ["roles"] = roleId,
            ["delete"] = 1
        };

        await _httpClient.PutAsync<object>("/access/acl", requestData, cancellationToken);
        
        _logger?.LogInformation("ACL entry deleted successfully for path: {Path}, user: {UserId}, role: {RoleId}", 
            path, userId, roleId);
    }

    /// <summary>
    /// Get ACL entries for a specific path
    /// </summary>
    public async Task<List<AccessControlEntry>> GetAclEntriesForPathAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var allEntries = await GetAccessControlListAsync(cancellationToken);
        return allEntries.Where(e => e.Path == path).ToList();
    }

    /// <summary>
    /// Get ACL entries for a specific user
    /// </summary>
    public async Task<List<AccessControlEntry>> GetAclEntriesForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var allEntries = await GetAccessControlListAsync(cancellationToken);
        return allEntries.Where(e => e.UserId == userId && e.IsUser).ToList();
    }

    #endregion

    #region API Token Management

    /// <summary>
    /// Get all API tokens for a user
    /// </summary>
    public async Task<List<ApiToken>> GetApiTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        
        _logger?.LogDebug("Getting API tokens for user: {UserId}", userId);
        
        var response = await _httpClient.GetAsync<ProxmoxApiResponse<List<ApiToken>>>(
            $"/access/users/{userId}/token", cancellationToken);

        return response.Data ?? new List<ApiToken>();
    }

    /// <summary>
    /// Get specific API token details
    /// </summary>
    public async Task<ApiToken?> GetApiTokenAsync(string userId, string tokenName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);
        
        _logger?.LogDebug("Getting API token: {UserId}!{TokenName}", userId, tokenName);

        var response = await _httpClient.GetAsync<ProxmoxApiResponse<ApiToken>>(
            $"/access/users/{userId}/token/{tokenName}", cancellationToken);

        return response.Data;
    }

    /// <summary>
    /// Create a new API token
    /// </summary>
    public async Task<TokenCreateResult> CreateApiTokenAsync(string userId, string tokenName, TokenCreateParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);

        _logger?.LogDebug("Creating API token: {UserId}!{TokenName}", userId, tokenName);

        var requestData = new Dictionary<string, object>();

        if (parameters != null)
        {
            if (!string.IsNullOrEmpty(parameters.Comment))
                requestData["comment"] = parameters.Comment;

            if (parameters.ExpirationDate.HasValue)
                requestData["expire"] = ((DateTimeOffset)parameters.ExpirationDate.Value).ToUnixTimeSeconds();

            if (parameters.PrivilegeSeparated.HasValue)
                requestData["privsep"] = parameters.PrivilegeSeparated.Value ? 1 : 0;
        }

        var response = await _httpClient.PostAsync<ProxmoxApiResponse<TokenCreateResult>>(
            $"/access/users/{userId}/token/{tokenName}", requestData, cancellationToken);

        _logger?.LogInformation("API token created successfully: {UserId}!{TokenName}", userId, tokenName);
        
        return response.Data ?? new TokenCreateResult();
    }

    /// <summary>
    /// Update an existing API token
    /// </summary>
    public async Task UpdateApiTokenAsync(string userId, string tokenName, TokenUpdateParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);
        ArgumentNullException.ThrowIfNull(parameters);

        _logger?.LogDebug("Updating API token: {UserId}!{TokenName}", userId, tokenName);

        var requestData = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(parameters.Comment))
            requestData["comment"] = parameters.Comment;

        if (parameters.ExpirationDate.HasValue)
            requestData["expire"] = ((DateTimeOffset)parameters.ExpirationDate.Value).ToUnixTimeSeconds();

        if (parameters.PrivilegeSeparated.HasValue)
            requestData["privsep"] = parameters.PrivilegeSeparated.Value ? 1 : 0;

        if (requestData.Any())
        {
            await _httpClient.PutAsync<object>($"/access/users/{userId}/token/{tokenName}", requestData, cancellationToken);
            _logger?.LogInformation("API token updated successfully: {UserId}!{TokenName}", userId, tokenName);
        }
        else
        {
            _logger?.LogWarning("No changes to update for API token: {UserId}!{TokenName}", userId, tokenName);
        }
    }

    /// <summary>
    /// Delete an API token
    /// </summary>
    public async Task DeleteApiTokenAsync(string userId, string tokenName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);

        _logger?.LogDebug("Deleting API token: {UserId}!{TokenName}", userId, tokenName);

        await _httpClient.DeleteAsync<object>($"/access/users/{userId}/token/{tokenName}", cancellationToken);
        
        _logger?.LogInformation("API token deleted successfully: {UserId}!{TokenName}", userId, tokenName);
    }

    #endregion

    #region Authentication Realms

    /// <summary>
    /// Get all authentication realms
    /// </summary>
    public async Task<List<AuthRealm>> GetAuthRealmsAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("Getting authentication realms");
        
        var response = await _httpClient.GetAsync<ProxmoxApiResponse<List<AuthRealm>>>(
            "/access/domains", cancellationToken);

        return response.Data ?? new List<AuthRealm>();
    }

    /// <summary>
    /// Get specific authentication realm details
    /// </summary>
    public async Task<AuthRealm?> GetAuthRealmAsync(string realmId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(realmId);
        
        _logger?.LogDebug("Getting authentication realm: {RealmId}", realmId);

        var response = await _httpClient.GetAsync<ProxmoxApiResponse<AuthRealm>>(
            $"/access/domains/{realmId}", cancellationToken);

        return response.Data;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Check if a user exists
    /// </summary>
    public async Task<bool> UserExistsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetUserAsync(userId, cancellationToken);
            return user != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if a group exists
    /// </summary>
    public async Task<bool> GroupExistsAsync(string groupId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await GetGroupAsync(groupId, cancellationToken);
            return group != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if a role exists
    /// </summary>
    public async Task<bool> RoleExistsAsync(string roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await GetRoleAsync(roleId, cancellationToken);
            return role != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get effective permissions for a user on a specific path
    /// </summary>
    public async Task<List<string>> GetUserPermissionsAsync(string userId, string path = "/", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var permissions = new HashSet<string>();
        var aclEntries = await GetAccessControlListAsync(cancellationToken);
        
        // Get direct user permissions
        var userEntries = aclEntries.Where(e => e.UserId == userId && e.IsUser && (e.Path == path || (e.Propagate == true && path.StartsWith(e.Path))));
        
        foreach (var entry in userEntries)
        {
            var role = await GetRoleAsync(entry.RoleId, cancellationToken);
            if (role != null && !string.IsNullOrEmpty(role.Privileges))
            {
                foreach (var priv in role.PrivilegeList)
                {
                    permissions.Add(priv);
                }
            }
        }

        // Get group permissions
        var user = await GetUserAsync(userId, cancellationToken);
        if (user != null && !string.IsNullOrEmpty(user.Groups))
        {
            var userGroups = user.Groups.Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var groupId in userGroups)
            {
                var groupEntries = aclEntries.Where(e => e.UserId == groupId.Trim() && e.IsGroup && (e.Path == path || (e.Propagate == true && path.StartsWith(e.Path))));
                
                foreach (var entry in groupEntries)
                {
                    var role = await GetRoleAsync(entry.RoleId, cancellationToken);
                    if (role != null && !string.IsNullOrEmpty(role.Privileges))
                    {
                        foreach (var priv in role.PrivilegeList)
                        {
                            permissions.Add(priv);
                        }
                    }
                }
            }
        }

        return permissions.ToList();
    }

    #endregion
}
