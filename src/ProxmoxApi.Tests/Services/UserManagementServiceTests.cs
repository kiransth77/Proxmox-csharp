using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using ProxmoxApi.Core;
using ProxmoxApi.Models;
using ProxmoxApi.Services;
using Xunit;

namespace ProxmoxApi.Tests.Services;

public class UserManagementServiceTests
{
    private readonly Mock<IProxmoxHttpClient> _mockHttpClient;
    private readonly Mock<ILogger<UserManagementService>> _mockLogger;
    private readonly UserManagementService _userManagementService;

    public UserManagementServiceTests()
    {
        _mockHttpClient = new Mock<IProxmoxHttpClient>();
        _mockLogger = new Mock<ILogger<UserManagementService>>();
        _userManagementService = new UserManagementService(_mockHttpClient.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidHttpClient_ShouldCreateInstance()
    {
        // Arrange & Act
        var service = new UserManagementService(_mockHttpClient.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UserManagementService(null!));
    }

    #endregion

    #region User Management Tests

    [Fact]
    public async Task GetUsersAsync_ShouldReturnListOfUsers()
    {
        // Arrange
        var expectedUsers = new List<ProxmoxUser>
        {
            new() { UserId = "user1@pam", Email = "user1@example.com" },
            new() { UserId = "user2@pam", Email = "user2@example.com" }
        };

        var response = new ProxmoxApiResponse<List<ProxmoxUser>> { Data = expectedUsers };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<List<ProxmoxUser>>>(
            "/access/users", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userManagementService.GetUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("user1@pam", result[0].UserId);
        Assert.Equal("user2@pam", result[1].UserId);
    }

    [Fact]
    public async Task GetUserAsync_WithValidUserId_ShouldReturnUser()
    {
        // Arrange
        const string userId = "testuser@pam";
        var expectedUser = new ProxmoxUser 
        { 
            UserId = userId, 
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var response = new ProxmoxApiResponse<ProxmoxUser> { Data = expectedUser };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<ProxmoxUser>>(
            $"/access/users/{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userManagementService.GetUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("test@example.com", result.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task GetUserAsync_WithInvalidUserId_ShouldThrowArgumentException(string userId)
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _userManagementService.GetUserAsync(userId));
    }

    [Fact]
    public async Task GetUserAsync_WithNullUserId_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userManagementService.GetUserAsync(null!));
    }

    [Fact]
    public async Task CreateUserAsync_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var parameters = new UserCreateParameters
        {
            UserId = "newuser@pam",
            Password = "password123",
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Enabled = true
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>("/access/users", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _userManagementService.CreateUserAsync(parameters);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>("/access/users", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithNullParameters_ShouldThrowArgumentNullException()
    {
        // Arrange, Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _userManagementService.CreateUserAsync(null!));
    }

    [Fact]
    public async Task CreateUserAsync_WithEmptyUserId_ShouldThrowArgumentException()
    {
        // Arrange
        var parameters = new UserCreateParameters { UserId = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _userManagementService.CreateUserAsync(parameters));
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidParameters_ShouldUpdateUser()
    {
        // Arrange
        const string userId = "testuser@pam";
        var parameters = new UserUpdateParameters
        {
            Email = "updated@example.com",
            FirstName = "Updated",
            Enabled = false
        };

        _mockHttpClient.Setup(x => x.PutAsync<object>($"/access/users/{userId}", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _userManagementService.UpdateUserAsync(userId, parameters);

        // Assert
        _mockHttpClient.Verify(x => x.PutAsync<object>($"/access/users/{userId}", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WithValidUserId_ShouldDeleteUser()
    {
        // Arrange
        const string userId = "testuser@pam";

        _mockHttpClient.Setup(x => x.DeleteAsync<object>($"/access/users/{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _userManagementService.DeleteUserAsync(userId);

        // Assert
        _mockHttpClient.Verify(x => x.DeleteAsync<object>($"/access/users/{userId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Group Management Tests

    [Fact]
    public async Task GetGroupsAsync_ShouldReturnListOfGroups()
    {
        // Arrange
        var expectedGroups = new List<ProxmoxGroup>
        {
            new() { GroupId = "group1", Comment = "First group" },
            new() { GroupId = "group2", Comment = "Second group" }
        };

        var response = new ProxmoxApiResponse<List<ProxmoxGroup>> { Data = expectedGroups };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<List<ProxmoxGroup>>>(
            "/access/groups", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userManagementService.GetGroupsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("group1", result[0].GroupId);
        Assert.Equal("group2", result[1].GroupId);
    }

    [Fact]
    public async Task CreateGroupAsync_WithValidParameters_ShouldCreateGroup()
    {
        // Arrange
        var parameters = new GroupCreateParameters
        {
            GroupId = "newgroup",
            Comment = "New test group"
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>("/access/groups", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _userManagementService.CreateGroupAsync(parameters);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>("/access/groups", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteGroupAsync_WithValidGroupId_ShouldDeleteGroup()
    {
        // Arrange
        const string groupId = "testgroup";

        _mockHttpClient.Setup(x => x.DeleteAsync<object>($"/access/groups/{groupId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _userManagementService.DeleteGroupAsync(groupId);

        // Assert
        _mockHttpClient.Verify(x => x.DeleteAsync<object>($"/access/groups/{groupId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Role Management Tests

    [Fact]
    public async Task GetRolesAsync_ShouldReturnListOfRoles()
    {
        // Arrange
        var expectedRoles = new List<ProxmoxRole>
        {
            new() { RoleId = "Administrator", Privileges = "Sys.Audit,Sys.Modify,VM.Allocate" },
            new() { RoleId = "PVEAdmin", Privileges = "Sys.PowerMgmt,Sys.Console" }
        };

        var response = new ProxmoxApiResponse<List<ProxmoxRole>> { Data = expectedRoles };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<List<ProxmoxRole>>>(
            "/access/roles", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userManagementService.GetRolesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Administrator", result[0].RoleId);
        Assert.Equal("PVEAdmin", result[1].RoleId);
    }

    [Fact]
    public async Task CreateRoleAsync_WithValidParameters_ShouldCreateRole()
    {
        // Arrange
        var parameters = new RoleCreateParameters
        {
            RoleId = "CustomRole",
            Privileges = "VM.Console,VM.PowerMgmt",
            Comment = "Custom role for testing"
        };

        _mockHttpClient.Setup(x => x.PostAsync<object>("/access/roles", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _userManagementService.CreateRoleAsync(parameters);

        // Assert
        _mockHttpClient.Verify(x => x.PostAsync<object>("/access/roles", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ACL Management Tests

    [Fact]
    public async Task GetAccessControlListAsync_ShouldReturnListOfAclEntries()
    {
        // Arrange
        var expectedEntries = new List<AccessControlEntry>
        {
            new() { Path = "/", UserId = "root@pam", RoleId = "Administrator", Type = "user" },
            new() { Path = "/vms", UserId = "group1", RoleId = "PVEVMAdmin", Type = "group" }
        };

        var response = new ProxmoxApiResponse<List<AccessControlEntry>> { Data = expectedEntries };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<List<AccessControlEntry>>>(
            "/access/acl", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userManagementService.GetAccessControlListAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("/", result[0].Path);
        Assert.Equal("root@pam", result[0].UserId);
    }

    [Fact]
    public async Task CreateAclEntryAsync_WithValidParameters_ShouldCreateAclEntry()
    {
        // Arrange
        var parameters = new AclCreateParameters
        {
            Path = "/vms/100",
            UserId = "user@pam",
            RoleId = "PVEVMUser",
            Propagate = true
        };

        _mockHttpClient.Setup(x => x.PutAsync<object>("/access/acl", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _userManagementService.CreateAclEntryAsync(parameters);

        // Assert
        _mockHttpClient.Verify(x => x.PutAsync<object>("/access/acl", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region API Token Management Tests

    [Fact]
    public async Task GetApiTokensAsync_WithValidUserId_ShouldReturnTokens()
    {
        // Arrange
        const string userId = "user@pam";
        var expectedTokens = new List<ApiToken>
        {
            new() { TokenId = "user@pam!token1", Comment = "First token" },
            new() { TokenId = "user@pam!token2", Comment = "Second token" }
        };

        var response = new ProxmoxApiResponse<List<ApiToken>> { Data = expectedTokens };
        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<List<ApiToken>>>(
            $"/access/users/{userId}/token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userManagementService.GetApiTokensAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("user@pam!token1", result[0].TokenId);
    }

    [Fact]
    public async Task CreateApiTokenAsync_WithValidParameters_ShouldCreateToken()
    {
        // Arrange
        const string userId = "user@pam";
        const string tokenName = "newtoken";
        var parameters = new TokenCreateParameters
        {
            Comment = "Test token",
            PrivilegeSeparated = true
        };

        var expectedResult = new TokenCreateResult
        {
            Value = "abc123-def456-ghi789",
            Info = new ApiToken { TokenId = $"{userId}!{tokenName}" }
        };

        var response = new ProxmoxApiResponse<TokenCreateResult> { Data = expectedResult };
        _mockHttpClient.Setup(x => x.PostAsync<ProxmoxApiResponse<TokenCreateResult>>(
            $"/access/users/{userId}/token/{tokenName}", It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userManagementService.CreateApiTokenAsync(userId, tokenName, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("abc123-def456-ghi789", result.Value);
        Assert.Equal($"{userId}!{tokenName}", result.Info?.TokenId);
    }

    [Fact]
    public async Task DeleteApiTokenAsync_WithValidParameters_ShouldDeleteToken()
    {
        // Arrange
        const string userId = "user@pam";
        const string tokenName = "testtoken";

        _mockHttpClient.Setup(x => x.DeleteAsync<object>($"/access/users/{userId}/token/{tokenName}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new object());

        // Act
        await _userManagementService.DeleteApiTokenAsync(userId, tokenName);

        // Assert
        _mockHttpClient.Verify(x => x.DeleteAsync<object>($"/access/users/{userId}/token/{tokenName}", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Method Tests

    [Fact]
    public async Task UserExistsAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        const string userId = "existinguser@pam";
        var user = new ProxmoxUser { UserId = userId };
        var response = new ProxmoxApiResponse<ProxmoxUser> { Data = user };

        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<ProxmoxUser>>(
            $"/access/users/{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _userManagementService.UserExistsAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserExistsAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        const string userId = "nonexistentuser@pam";

        _mockHttpClient.Setup(x => x.GetAsync<ProxmoxApiResponse<ProxmoxUser>>(
            $"/access/users/{userId}", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("User not found"));

        // Act
        var result = await _userManagementService.UserExistsAsync(userId);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region ProxmoxUser Model Tests

    [Fact]
    public void ProxmoxUser_Realm_ShouldExtractRealmFromUserId()
    {
        // Arrange
        var user = new ProxmoxUser { UserId = "testuser@ldap" };

        // Act
        var realm = user.Realm;

        // Assert
        Assert.Equal("ldap", realm);
    }

    [Fact]
    public void ProxmoxUser_Username_ShouldExtractUsernameFromUserId()
    {
        // Arrange
        var user = new ProxmoxUser { UserId = "testuser@pam" };

        // Act
        var username = user.Username;

        // Assert
        Assert.Equal("testuser", username);
    }

    [Fact]
    public void ProxmoxRole_PrivilegeList_ShouldSplitPrivileges()
    {
        // Arrange
        var role = new ProxmoxRole { Privileges = "VM.Console,VM.PowerMgmt,Sys.Audit" };

        // Act
        var privileges = role.PrivilegeList;

        // Assert
        Assert.Equal(3, privileges.Count);
        Assert.Contains("VM.Console", privileges);
        Assert.Contains("VM.PowerMgmt", privileges);
        Assert.Contains("Sys.Audit", privileges);
    }

    [Fact]
    public void AccessControlEntry_IsUser_ShouldReturnTrueForUserType()
    {
        // Arrange
        var entry = new AccessControlEntry { Type = "user" };

        // Act & Assert
        Assert.True(entry.IsUser);
        Assert.False(entry.IsGroup);
    }

    [Fact]
    public void AccessControlEntry_IsGroup_ShouldReturnTrueForGroupType()
    {
        // Arrange
        var entry = new AccessControlEntry { Type = "group" };

        // Act & Assert
        Assert.True(entry.IsGroup);
        Assert.False(entry.IsUser);
    }

    #endregion
}
