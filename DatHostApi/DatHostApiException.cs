using System.Net;

namespace DatHost.Api.Client;

/// <summary>
/// Exception thrown when DatHost API operations fail
/// </summary>
public class DatHostApiException : Exception
{
    /// <summary>
    /// HTTP status code returned by API
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// API error response content
    /// </summary>
    public string? ResponseContent { get; }

    /// <summary>
    /// Initialize exception with message and status code
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    public DatHostApiException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initialize exception with message, status code and response content
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="responseContent">API response content</param>
    public DatHostApiException(string message, HttpStatusCode statusCode, string? responseContent) : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
    }

    /// <summary>
    /// Initialize exception with message, status code and inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="innerException">Inner exception</param>
    public DatHostApiException(string message, HttpStatusCode statusCode, Exception innerException) : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}