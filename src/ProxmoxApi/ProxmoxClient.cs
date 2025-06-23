using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Core;
using ProxmoxApi.Models;
using ProxmoxApi.Services;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ProxmoxApi;

/// <summary>
/// Main client for interacting with Proxmox VE API
/// </summary>
public class ProxmoxClient : IDisposable
{    private readonly ProxmoxHttpClient _httpClient;
    private readonly ILogger<ProxmoxClient> _logger;
    private readonly NodeService _nodeService;
    private readonly VmService _vmService;
    private readonly ContainerService _containerService;
    private bool _disposed;

    /// <summary>
    /// Node management service
    /// </summary>
    public NodeService Nodes => _nodeService;

    /// <summary>
    /// Virtual Machine management service
    /// </summary>
    public VmService Vms => _vmService;

    /// <summary>
    /// Container (LXC) management service
    /// </summary>
    public ContainerService Containers => _containerService;

    /// <summary>
    /// Initializes a new instance of ProxmoxClient
    /// </summary>
    /// <param name="connectionInfo">Connection information</param>
    /// <param name="logger">Logger instance</param>
    public ProxmoxClient(ProxmoxConnectionInfo connectionInfo, ILogger<ProxmoxClient>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(connectionInfo);

        _logger = logger ?? new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILogger<ProxmoxClient>>();        var httpClient = CreateHttpClient(connectionInfo);
        var httpLogger = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILogger<ProxmoxHttpClient>>();

        _httpClient = new ProxmoxHttpClient(httpClient, connectionInfo, httpLogger);
        
        // Initialize services with proper logger
        var nodeLogger = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILogger<NodeService>>();
        _nodeService = new NodeService(_httpClient, nodeLogger);
        
        var vmLogger = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILogger<VmService>>();
        _vmService = new VmService(_httpClient, vmLogger);
        
        var containerLogger = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILogger<ContainerService>>();
        _containerService = new ContainerService(_httpClient, containerLogger);
    }

    /// <summary>
    /// Initializes a new instance of ProxmoxClient with dependency injection
    /// </summary>
    /// <param name="httpClient">HTTP client instance</param>
    /// <param name="connectionInfo">Connection information</param>
    /// <param name="logger">Logger instance</param>
    public ProxmoxClient(HttpClient httpClient, ProxmoxConnectionInfo connectionInfo, ILogger<ProxmoxClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(connectionInfo);
        ArgumentNullException.ThrowIfNull(logger);        _logger = logger;
        var httpLogger = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILogger<ProxmoxHttpClient>>();

        _httpClient = new ProxmoxHttpClient(httpClient, connectionInfo, httpLogger);
        
        // Initialize services with proper logger
        var nodeLogger = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILogger<NodeService>>();
        _nodeService = new NodeService(_httpClient, nodeLogger);
        
        var vmLogger = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILogger<VmService>>();
        _vmService = new VmService(_httpClient, vmLogger);
        
        var containerLogger = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider()
            .GetRequiredService<ILogger<ContainerService>>();
        _containerService = new ContainerService(_httpClient, containerLogger);
    }

    /// <summary>
    /// Authenticates with the Proxmox server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if authentication succeeded</returns>
    public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing authentication with Proxmox server");
        return await _httpClient.AuthenticateAsync(cancellationToken);
    }

    /// <summary>
    /// Tests the connection to the Proxmox server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection is successful</returns>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing connection to Proxmox server");
            
            // Try to get version information as a connection test
            var version = await _httpClient.GetAsync<Dictionary<string, object>>("/version", cancellationToken);
            
            _logger.LogInformation("Connection test successful");
            return version != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed");
            return false;
        }
    }    /// <summary>
    /// Gets the version information from the Proxmox server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Version information</returns>
    public async Task<Dictionary<string, object>> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting Proxmox server version");
        return await _httpClient.GetAsync<Dictionary<string, object>>("/version", cancellationToken);
    }

    private static HttpClient CreateHttpClient(ProxmoxConnectionInfo connectionInfo)
    {
        var handler = new HttpClientHandler();
        
        if (connectionInfo.IgnoreSslErrors)
        {
            handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
        }

        return new HttpClient(handler);    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
