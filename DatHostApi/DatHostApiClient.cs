using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DatHost.Api.Client;

/// <summary>
/// Interface for DatHost API client operations
/// </summary>
public interface IDatHostApiClient : IDisposable
{
    /// <summary>
    /// Send GET request to API endpoint
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response</returns>
    Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send POST request to API endpoint with response
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response</returns>
    Task<T> PostAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send POST request to API endpoint without response
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PostAsync(string endpoint, object? data = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Main client for DatHost API operations with high-performance optimizations
    /// </summary>
    /// 


    Task<T> PutAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default);
    Task PutAsync(string endpoint, object? data = null, CancellationToken cancellationToken = default);

    public class DatHostApiClient : IDatHostApiClient
{
    private readonly HttpClient _httpClient;
    private readonly DatHostApiOptions _options;

    // JSON serializer options for optimal performance
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public DatHostApiClient(
        HttpClient httpClient,
        IOptions<DatHostApiOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        ConfigureHttpClient();
    }

    /// <summary>
    /// Configure HTTP client with authentication and base settings
    /// </summary>
    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);

        // Setup Basic Authentication
        var authValue = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.Email}:{_options.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", authValue);

        // Set default headers for optimal performance
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        // Add account email header if specified
        if (!string.IsNullOrEmpty(_options.AccountEmail))
        {
            _httpClient.DefaultRequestHeaders.Add("Account-Email", _options.AccountEmail);
        }
    }

    /// <summary>
    /// Send GET request to API endpoint
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response</returns>
    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            await EnsureSuccessStatusCodeAsync(response);

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(json, JsonOptions)!;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Send POST request to API endpoint
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deserialized response</returns>
    public async Task<T> PostAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpContent? content = null;
            if (data != null)
            {
                if (data is MultipartFormDataContent formData)
                {
                    content = formData;
                }
                else
                {
                    var json = JsonSerializer.Serialize(data, JsonOptions);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            await EnsureSuccessStatusCodeAsync(response);

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(responseJson, JsonOptions)!;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    /// <summary>
    /// Send POST request without expecting response data
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task PostAsync(string endpoint, object? data = null, CancellationToken cancellationToken = default)
    {
        try
        {

            HttpContent? content = null;
            if (data != null)
            {
                if (data is MultipartFormDataContent formData)
                {
                    content = formData;
                }
                else
                {
                    var json = JsonSerializer.Serialize(data, JsonOptions);
                    content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            await EnsureSuccessStatusCodeAsync(response);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

        /// <summary>
        /// Send PUT request to API endpoint with response
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deserialized response</returns>
        public async Task<T> PutAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default)
        {
            try
            {
                HttpContent? content = null;
                if (data != null)
                {
                    if (data is MultipartFormDataContent formData)
                    {
                        content = formData;
                    }
                    else
                    {
                        var json = JsonSerializer.Serialize(data, JsonOptions);
                        content = new StringContent(json, Encoding.UTF8, "application/json");
                    }
                }

                var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
                await EnsureSuccessStatusCodeAsync(response);

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<T>(responseJson, JsonOptions)!;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Send PUT request without expecting response data
        /// </summary>
        /// <param name="endpoint">API endpoint</param>
        /// <param name="data">Request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task PutAsync(string endpoint, object? data = null, CancellationToken cancellationToken = default)
        {
            try
            {
                HttpContent? content = null;
                if (data != null)
                {
                    if (data is MultipartFormDataContent formData)
                    {
                        content = formData;
                    }
                    else
                    {
                        var json = JsonSerializer.Serialize(data, JsonOptions);
                        content = new StringContent(json, Encoding.UTF8, "application/json");
                    }
                }

                var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
                await EnsureSuccessStatusCodeAsync(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        /// <summary>
        /// Check response status and throw appropriate exception if failed
        /// </summary>
        /// <param name="response">HTTP response</param>
        private static async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new DatHostApiException(
                $"API request failed with status {response.StatusCode}: {content}",
                response.StatusCode);
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
    }
}