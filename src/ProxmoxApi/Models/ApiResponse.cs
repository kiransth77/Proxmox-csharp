using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ProxmoxApi.Models;

/// <summary>
/// Base response from Proxmox API
/// </summary>
/// <typeparam name="T">Type of the data payload</typeparam>
public class ProxmoxApiResponse<T>
{
    /// <summary>
    /// Response data
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// Error information if request failed
    /// </summary>
    [JsonPropertyName("errors")]
    public Dictionary<string, string>? Errors { get; set; }

    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool IsSuccess => Errors == null || !Errors.Any();
}

/// <summary>
/// Authentication ticket response
/// </summary>
public class AuthTicket
{
    /// <summary>
    /// Authentication ticket
    /// </summary>
    [JsonPropertyName("ticket")]
    public string? Ticket { get; set; }

    /// <summary>
    /// CSRF prevention token
    /// </summary>
    [JsonPropertyName("CSRFPreventionToken")]
    public string? CsrfToken { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Capabilities
    /// </summary>
    [JsonPropertyName("cap")]
    public Dictionary<string, object>? Capabilities { get; set; }
}

/// <summary>
/// Generic error response
/// </summary>
public class ProxmoxErrorResponse
{
    /// <summary>
    /// Error message
    /// </summary>
    [JsonPropertyName("data")]
    public string? Message { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }
}
