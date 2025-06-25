using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProxmoxApi.Models;

/// <summary>
/// Represents a Proxmox user account
/// </summary>
public class ProxmoxUser
{
    /// <summary>
    /// User ID (username@realm)
    /// </summary>
    [JsonPropertyName("userid")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User comment/description
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    [JsonPropertyName("firstname")]
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    [JsonPropertyName("lastname")]
    public string? LastName { get; set; }

    /// <summary>
    /// User groups (comma-separated)
    /// </summary>
    [JsonPropertyName("groups")]
    public string? Groups { get; set; }

    /// <summary>
    /// Whether the user is enabled
    /// </summary>
    [JsonPropertyName("enable")]
    public bool? Enabled { get; set; }

    /// <summary>
    /// Account expiration date
    /// </summary>
    [JsonPropertyName("expire")]
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Authentication realm
    /// </summary>
    public string Realm => UserId.Contains('@') ? UserId.Split('@')[1] : "pam";

    /// <summary>
    /// Username without realm
    /// </summary>
    public string Username => UserId.Contains('@') ? UserId.Split('@')[0] : UserId;
}

/// <summary>
/// Parameters for creating a new user
/// </summary>
public class UserCreateParameters
{
    /// <summary>
    /// User ID (username@realm)
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User password
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// User comment/description
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User groups (comma-separated)
    /// </summary>
    public string? Groups { get; set; }

    /// <summary>
    /// Whether the user is enabled
    /// </summary>
    public bool? Enabled { get; set; } = true;

    /// <summary>
    /// Account expiration date
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
}

/// <summary>
/// Parameters for updating a user
/// </summary>
public class UserUpdateParameters
{
    /// <summary>
    /// User password
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// User comment/description
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// User email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User's first name
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User groups (comma-separated)
    /// </summary>
    public string? Groups { get; set; }

    /// <summary>
    /// Whether the user is enabled
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    /// Account expiration date
    /// </summary>
    public DateTime? ExpirationDate { get; set; }
}

/// <summary>
/// Represents a Proxmox group
/// </summary>
public class ProxmoxGroup
{
    /// <summary>
    /// Group ID/name
    /// </summary>
    [JsonPropertyName("groupid")]
    public string GroupId { get; set; } = string.Empty;

    /// <summary>
    /// Group comment/description
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Group members (list of user IDs)
    /// </summary>
    public List<string> Members { get; set; } = new();
}

/// <summary>
/// Parameters for creating a new group
/// </summary>
public class GroupCreateParameters
{
    /// <summary>
    /// Group ID/name
    /// </summary>
    public string GroupId { get; set; } = string.Empty;

    /// <summary>
    /// Group comment/description
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Parameters for updating a group
/// </summary>
public class GroupUpdateParameters
{
    /// <summary>
    /// Group comment/description
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents a Proxmox role
/// </summary>
public class ProxmoxRole
{
    /// <summary>
    /// Role ID/name
    /// </summary>
    [JsonPropertyName("roleid")]
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// Role privileges (comma-separated)
    /// </summary>
    [JsonPropertyName("privs")]
    public string? Privileges { get; set; }

    /// <summary>
    /// Role comment/description
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Whether this is a special role
    /// </summary>
    [JsonPropertyName("special")]
    public bool? IsSpecial { get; set; }

    /// <summary>
    /// List of individual privileges
    /// </summary>
    public List<string> PrivilegeList => 
        string.IsNullOrEmpty(Privileges) ? new List<string>() : 
        new List<string>(Privileges.Split(',', StringSplitOptions.RemoveEmptyEntries));
}

/// <summary>
/// Parameters for creating a new role
/// </summary>
public class RoleCreateParameters
{
    /// <summary>
    /// Role ID/name
    /// </summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// Role privileges (comma-separated)
    /// </summary>
    public string? Privileges { get; set; }

    /// <summary>
    /// Role comment/description
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Parameters for updating a role
/// </summary>
public class RoleUpdateParameters
{
    /// <summary>
    /// Role privileges (comma-separated)
    /// </summary>
    public string? Privileges { get; set; }

    /// <summary>
    /// Role comment/description
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// Represents an Access Control List (ACL) entry
/// </summary>
public class AccessControlEntry
{
    /// <summary>
    /// Path for the ACL (e.g., "/", "/nodes/node1", "/vms/100")
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// User or group ID
    /// </summary>
    [JsonPropertyName("ugid")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Role ID
    /// </summary>
    [JsonPropertyName("roleid")]
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// Type of entry (user or group)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the ACL is propagated to child objects
    /// </summary>
    [JsonPropertyName("propagate")]
    public bool? Propagate { get; set; }

    /// <summary>
    /// Whether this is a user or group entry
    /// </summary>
    public bool IsUser => Type == "user";

    /// <summary>
    /// Whether this is a group entry
    /// </summary>
    public bool IsGroup => Type == "group";
}

/// <summary>
/// Parameters for creating an ACL entry
/// </summary>
public class AclCreateParameters
{
    /// <summary>
    /// Path for the ACL (e.g., "/", "/nodes/node1", "/vms/100")
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// User or group ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Role ID
    /// </summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the ACL is propagated to child objects
    /// </summary>
    public bool? Propagate { get; set; } = true;
}

/// <summary>
/// Represents an API token
/// </summary>
public class ApiToken
{
    /// <summary>
    /// Token ID (format: user@realm!tokenname)
    /// </summary>
    [JsonPropertyName("tokenid")]
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    /// Token comment/description
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Token expiration date
    /// </summary>
    [JsonPropertyName("expire")]
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Whether the token is privilege-separated
    /// </summary>
    [JsonPropertyName("privsep")]
    public bool? PrivilegeSeparated { get; set; }

    /// <summary>
    /// User ID that owns the token
    /// </summary>
    public string UserId => TokenId.Contains('!') ? TokenId.Split('!')[0] : string.Empty;

    /// <summary>
    /// Token name
    /// </summary>
    public string TokenName => TokenId.Contains('!') ? TokenId.Split('!')[1] : string.Empty;
}

/// <summary>
/// Parameters for creating an API token
/// </summary>
public class TokenCreateParameters
{
    /// <summary>
    /// Token ID (format: user@realm!tokenname)
    /// </summary>
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    /// Token comment/description
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Token expiration date
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Whether the token is privilege-separated
    /// </summary>
    public bool? PrivilegeSeparated { get; set; } = true;
}

/// <summary>
/// Parameters for updating an API token
/// </summary>
public class TokenUpdateParameters
{
    /// <summary>
    /// Token comment/description
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Token expiration date
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Whether the token is privilege-separated
    /// </summary>
    public bool? PrivilegeSeparated { get; set; }
}

/// <summary>
/// Result of creating an API token
/// </summary>
public class TokenCreateResult
{
    /// <summary>
    /// The generated token value
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Token information
    /// </summary>
    [JsonPropertyName("info")]
    public ApiToken? Info { get; set; }

    /// <summary>
    /// Full token string (user@realm!tokenname=value)
    /// </summary>
    public string FullToken => Info != null ? $"{Info.TokenId}={Value}" : string.Empty;
}

/// <summary>
/// Authentication realm information
/// </summary>
public class AuthRealm
{
    /// <summary>
    /// Realm name/ID
    /// </summary>
    [JsonPropertyName("realm")]
    public string RealmId { get; set; } = string.Empty;

    /// <summary>
    /// Realm type (pam, ad, ldap, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Realm comment/description
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    /// <summary>
    /// Whether the realm is enabled
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    /// <summary>
    /// Whether this is the default realm
    /// </summary>
    [JsonPropertyName("default")]
    public bool? IsDefault { get; set; }
}
