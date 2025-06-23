using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProxmoxApi.Core;

/// <summary>
/// Interface for Proxmox HTTP client operations
/// </summary>
public interface IProxmoxHttpClient : IDisposable
{
    /// <summary>
    /// Authenticates with the Proxmox server
    /// </summary>
    Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a GET request to the specified endpoint
    /// </summary>
    Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a POST request to the specified endpoint with data
    /// </summary>
    Task<T> PostAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request to the specified endpoint with data
    /// </summary>
    Task<T> PutAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DELETE request to the specified endpoint
    /// </summary>
    Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default);
}
