using DatHost.Api.Client;
using DatHostApi.Models;

namespace DatHostApi
{
    /// <summary>
    /// Interface for custom domain management operations
    /// </summary>
    public interface ICustomDomainService
    {
        /// <summary>
        /// Get all available custom domains
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of custom domains</returns>
        Task<List<CustomDomain>> GetCustomDomainsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get specific custom domain by name
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Custom domain information</returns>
        Task<CustomDomain> GetCustomDomainAsync(string domainName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new custom domain
        /// </summary>
        /// <param name="request">Create domain request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created custom domain</returns>
        Task<CustomDomain> CreateCustomDomainAsync(CreateCustomDomainRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="request">Update domain request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated custom domain</returns>
        Task<CustomDomain> UpdateCustomDomainAsync(string domainName, UpdateCustomDomainRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteCustomDomainAsync(string domainName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verify a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Domain verification result</returns>
        Task<CustomDomain> VerifyCustomDomainAsync(string domainName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Enable SSL for a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated custom domain</returns>
        Task<CustomDomain> EnableSslAsync(string domainName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disable SSL for a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated custom domain</returns>
        Task<CustomDomain> DisableSslAsync(string domainName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renew SSL certificate for a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated custom domain</returns>
        Task<CustomDomain> RenewSslCertificateAsync(string domainName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get DNS records for a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>DNS records information</returns>
        Task<List<DnsRecord>> GetDnsRecordsAsync(string domainName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check domain availability
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Domain availability status</returns>
        Task<bool> CheckDomainAvailabilityAsync(string domainName, CancellationToken cancellationToken = default);
    }
    /// <summary>
    /// Service for managing custom domains through DatHost API
    /// </summary>
    public class CustomDomainService : ICustomDomainService
    {
        private readonly IDatHostApiClient _apiClient;

        public CustomDomainService(IDatHostApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        /// <summary>
        /// Get all available custom domains
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of custom domains</returns>
        public async Task<List<CustomDomain>> GetCustomDomainsAsync(CancellationToken cancellationToken = default)
        {
            return await _apiClient.GetAsync<List<CustomDomain>>("custom-domains", cancellationToken);
        }

        /// <summary>
        /// Get specific custom domain by name
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Custom domain information</returns>
        public async Task<CustomDomain> GetCustomDomainAsync(string domainName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));

            return await _apiClient.GetAsync<CustomDomain>($"custom-domains/{Uri.EscapeDataString(domainName)}", cancellationToken);
        }

        /// <summary>
        /// Create a new custom domain
        /// </summary>
        /// <param name="request">Create domain request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created custom domain</returns>
        public async Task<CustomDomain> CreateCustomDomainAsync(CreateCustomDomainRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await _apiClient.PostAsync<CustomDomain>("custom-domains", request, cancellationToken);
        }

        /// <summary>
        /// Update an existing custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="request">Update domain request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated custom domain</returns>
        public async Task<CustomDomain> UpdateCustomDomainAsync(string domainName, UpdateCustomDomainRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await _apiClient.PostAsync<CustomDomain>($"custom-domains/{Uri.EscapeDataString(domainName)}", request, cancellationToken);
        }

        /// <summary>
        /// Delete a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task DeleteCustomDomainAsync(string domainName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));

            await _apiClient.PostAsync($"custom-domains/{Uri.EscapeDataString(domainName)}/delete", null, cancellationToken);
        }

        /// <summary>
        /// Verify a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Domain verification result</returns>
        public async Task<CustomDomain> VerifyCustomDomainAsync(string domainName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));

            return await _apiClient.PostAsync<CustomDomain>($"custom-domains/{Uri.EscapeDataString(domainName)}/verify", null, cancellationToken);
        }

        /// <summary>
        /// Enable SSL for a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated custom domain</returns>
        public async Task<CustomDomain> EnableSslAsync(string domainName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));

            return await _apiClient.PostAsync<CustomDomain>($"custom-domains/{Uri.EscapeDataString(domainName)}/ssl/enable", null, cancellationToken);
        }

        /// <summary>
        /// Disable SSL for a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated custom domain</returns>
        public async Task<CustomDomain> DisableSslAsync(string domainName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));

            return await _apiClient.PostAsync<CustomDomain>($"custom-domains/{Uri.EscapeDataString(domainName)}/ssl/disable", null, cancellationToken);
        }

        /// <summary>
        /// Renew SSL certificate for a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated custom domain</returns>
        public async Task<CustomDomain> RenewSslCertificateAsync(string domainName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));

            return await _apiClient.PostAsync<CustomDomain>($"custom-domains/{Uri.EscapeDataString(domainName)}/ssl/renew", null, cancellationToken);
        }

        /// <summary>
        /// Get DNS records for a custom domain
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>DNS records information</returns>
        public async Task<List<DnsRecord>> GetDnsRecordsAsync(string domainName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));

            return await _apiClient.GetAsync<List<DnsRecord>>($"custom-domains/{Uri.EscapeDataString(domainName)}/dns", cancellationToken);
        }

        /// <summary>
        /// Check domain availability
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Domain availability status</returns>
        public async Task<bool> CheckDomainAvailabilityAsync(string domainName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(domainName))
                throw new ArgumentException("Domain name cannot be null or empty", nameof(domainName));


            try
            {
                await _apiClient.GetAsync<CustomDomain>($"custom-domains/{Uri.EscapeDataString(domainName)}/availability", cancellationToken);
                return true;
            }
            catch (DatHostApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }

   
}
