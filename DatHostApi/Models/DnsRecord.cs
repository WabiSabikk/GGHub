using System.Text.Json.Serialization;

namespace DatHostApi.Models
{
    /// <summary>
    /// DNS record information
    /// </summary>
    public class DnsRecord
    {
        /// <summary>
        /// DNS record type
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// DNS record name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// DNS record value
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// DNS record TTL
        /// </summary>
        [JsonPropertyName("ttl")]
        public int Ttl { get; set; }

        /// <summary>
        /// DNS record priority
        /// </summary>
        [JsonPropertyName("priority")]
        public int? Priority { get; set; }
    }
}
