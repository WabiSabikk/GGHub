using System.ComponentModel.DataAnnotations;

namespace DatHost.Api.Client;

/// <summary>
/// Configuration options for DatHost API client
/// </summary>
public class DatHostApiOptions
{
    /// <summary>
    /// Base URL for DatHost API
    /// </summary>
    [Required]
    public string BaseUrl { get; set; } = "https://dathost.net/api/0.1/";

    /// <summary>
    /// Email for authentication
    /// </summary>
    [Required]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional account email for multi-account operations
    /// </summary>
    public string? AccountEmail { get; set; }

    /// <summary>
    /// HTTP timeout in seconds (default: 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of retry attempts (default: 3)
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Retry delay in milliseconds (default: 1000)
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;
}