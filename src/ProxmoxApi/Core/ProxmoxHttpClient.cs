using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxmoxApi.Exceptions;
using ProxmoxApi.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProxmoxApi.Core;

/// <summary>
/// HTTP client wrapper for Proxmox API communication
/// </summary>
public class ProxmoxHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProxmoxHttpClient> _logger;
    private readonly ProxmoxConnectionInfo _connectionInfo;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _authTicket;
    private string? _csrfToken;
    private bool _disposed;

    public ProxmoxHttpClient(HttpClient httpClient, ProxmoxConnectionInfo connectionInfo, ILogger<ProxmoxHttpClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _connectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        ConfigureHttpClient();
    }

    /// <summary>
    /// Authenticates with the Proxmox server
    /// </summary>
    public async Task<bool> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Authenticating with Proxmox server at {Host}", _connectionInfo.Host);

        try
        {
            var authData = new Dictionary<string, string>
            {
                ["username"] = $"{_connectionInfo.Username}@{_connectionInfo.Realm}"
            };

            if (!string.IsNullOrEmpty(_connectionInfo.ApiToken))
            {
                // API token authentication
                var tokenParts = _connectionInfo.ApiToken.Split('=');
                if (tokenParts.Length == 2)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("PVEAPIToken", _connectionInfo.ApiToken);
                    _logger.LogInformation("Using API token authentication");
                    return true;
                }
                else
                {
                    throw new ProxmoxAuthenticationException("Invalid API token format. Expected format: 'user@realm!tokenid=uuid'");
                }
            }
            else if (!string.IsNullOrEmpty(_connectionInfo.Password))
            {
                // Username/password authentication
                authData["password"] = _connectionInfo.Password;
                
                var authContent = new FormUrlEncodedContent(authData);
                var response = await _httpClient.PostAsync("/access/ticket", authContent, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var authResponse = JsonSerializer.Deserialize<ProxmoxApiResponse<AuthTicket>>(responseContent, _jsonOptions);
                    
                    if (authResponse?.IsSuccess == true && authResponse.Data != null)
                    {
                        _authTicket = authResponse.Data.Ticket;
                        _csrfToken = authResponse.Data.CsrfToken;
                        
                        // Set authentication headers
                        _httpClient.DefaultRequestHeaders.Remove("Cookie");
                        _httpClient.DefaultRequestHeaders.Add("Cookie", $"PVEAuthCookie={_authTicket}");
                        
                        _logger.LogInformation("Successfully authenticated with username/password");
                        return true;
                    }
                }
                
                throw new ProxmoxAuthenticationException("Authentication failed: Invalid credentials");
            }
            else
            {
                throw new ProxmoxAuthenticationException("No authentication method provided. Please specify either Password or ApiToken");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during authentication");
            throw new ProxmoxAuthenticationException("Network error during authentication", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Authentication request timed out");
            throw new ProxmoxAuthenticationException("Authentication request timed out", ex);
        }
    }    /// <summary>
    /// Makes a GET request to the Proxmox API
    /// </summary>
    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Making GET request to {Endpoint}", endpoint);
        
        var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        return await ProcessResponseAsync<T>(response, cancellationToken);
    }    /// <summary>
    /// Makes a POST request to the Proxmox API
    /// </summary>
    public async Task<T> PostAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Making POST request to {Endpoint}", endpoint);
        
        HttpContent? content = null;
        if (data != null)
        {
            if (data is FormUrlEncodedContent formContent)
            {
                content = formContent;
            }
            else
            {
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }
        }

        // Add CSRF token for POST requests when using ticket authentication
        if (!string.IsNullOrEmpty(_csrfToken))
        {
            _httpClient.DefaultRequestHeaders.Remove("CSRFPreventionToken");
            _httpClient.DefaultRequestHeaders.Add("CSRFPreventionToken", _csrfToken);
        }

        var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        return await ProcessResponseAsync<T>(response, cancellationToken);
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_connectionInfo.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_connectionInfo.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ProxmoxApi-Client/1.0");
    }

    private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API request failed with status {StatusCode}: {Content}", response.StatusCode, content);
            
            var statusCode = (int)response.StatusCode;
            var message = $"API request failed with status {statusCode}";
            
            if (statusCode == 401)
            {
                throw new ProxmoxAuthenticationException(message);
            }
            else if (statusCode == 403)
            {
                throw new ProxmoxAuthorizationException(message);
            }
            else
            {
                throw new ProxmoxApiException(message, statusCode);
            }
        }        if (string.IsNullOrEmpty(content))
        {
            return default(T);
        }

        try
        {
            var apiResponse = JsonSerializer.Deserialize<ProxmoxApiResponse<T>>(content, _jsonOptions);            if (apiResponse?.IsSuccess == false)
            {
                var errorValues = apiResponse.Errors?.Values;
                var errorMessage = errorValues != null ? string.Join("; ", errorValues) : "Unknown error";
                throw new ProxmoxApiException($"API returned errors: {errorMessage}");
            }
            
            return apiResponse != null ? apiResponse.Data : default(T);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize API response: {Content}", content);
            throw new ProxmoxApiException("Failed to deserialize API response", ex);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
