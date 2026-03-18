using System.Text.Json.Serialization;

namespace DatHostApi.Models
{
    /// <summary>
    /// Custom domain information
    /// </summary>
    public class CustomDomain
    {
        /// <summary>
        /// Domain name
        /// </summary>
        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Domain status
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Domain type
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Associated game server ID
        /// </summary>
        [JsonPropertyName("game_server_id")]
        public string? GameServerId { get; set; }

        /// <summary>
        /// Domain creation date
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Domain last update date
        /// </summary>
        [JsonPropertyName("updated")]
        public DateTime Updated { get; set; }

        /// <summary>
        /// Domain verification status
        /// </summary>
        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        /// <summary>
        /// Domain configuration
        /// </summary>
        [JsonPropertyName("config")]
        public DomainConfig? Config { get; set; }
    }

    /// <summary>
    /// Domain configuration
    /// </summary>
    public class DomainConfig
    {
        /// <summary>
        /// SSL certificate enabled
        /// </summary>
        [JsonPropertyName("ssl_enabled")]
        public bool SslEnabled { get; set; }

        /// <summary>
        /// SSL certificate status
        /// </summary>
        [JsonPropertyName("ssl_status")]
        public string SslStatus { get; set; } = string.Empty;

        /// <summary>
        /// SSL certificate expiration date
        /// </summary>
        [JsonPropertyName("ssl_expires")]
        public DateTime? SslExpires { get; set; }

        /// <summary>
        /// Proxy enabled
        /// </summary>
        [JsonPropertyName("proxy_enabled")]
        public bool ProxyEnabled { get; set; }

        /// <summary>
        /// CDN enabled
        /// </summary>
        [JsonPropertyName("cdn_enabled")]
        public bool CdnEnabled { get; set; }
    }

    /// <summary>
    /// Request model for creating a custom domain
    /// </summary>
    public class CreateCustomDomainRequest
    {
        /// <summary>
        /// Domain name
        /// </summary>
        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Domain type
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Game server ID to associate with domain
        /// </summary>
        [JsonPropertyName("game_server_id")]
        public string? GameServerId { get; set; }

        /// <summary>
        /// Domain configuration
        /// </summary>
        [JsonPropertyName("config")]
        public DomainConfig? Config { get; set; }
    }

    /// <summary>
    /// Request model for updating a custom domain
    /// </summary>
    public class UpdateCustomDomainRequest
    {
        /// <summary>
        /// Game server ID to associate with domain
        /// </summary>
        [JsonPropertyName("game_server_id")]
        public string? GameServerId { get; set; }

        /// <summary>
        /// Domain configuration
        /// </summary>
        [JsonPropertyName("config")]
        public DomainConfig? Config { get; set; }
    }
}
