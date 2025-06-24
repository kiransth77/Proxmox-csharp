using System;
using System.Collections.Generic;

namespace ProxmoxApi.Exceptions;

/// <summary>
/// Base exception for Proxmox API operations
/// </summary>
public class ProxmoxApiException : Exception
{
    /// <summary>
    /// HTTP status code if available
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Error details from the API response
    /// </summary>
    public Dictionary<string, string>? ErrorDetails { get; }

    public ProxmoxApiException(string message) : base(message)
    {
    }

    public ProxmoxApiException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ProxmoxApiException(string message, int statusCode, Dictionary<string, string>? errorDetails = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorDetails = errorDetails;
    }
}

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class ProxmoxAuthenticationException : ProxmoxApiException
{
    public ProxmoxAuthenticationException(string message) : base(message)
    {
    }

    public ProxmoxAuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when the request is not authorized
/// </summary>
public class ProxmoxAuthorizationException : ProxmoxApiException
{
    public ProxmoxAuthorizationException(string message) : base(message)
    {
    }

    public ProxmoxAuthorizationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
