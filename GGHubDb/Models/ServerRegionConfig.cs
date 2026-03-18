using GGHubShared.Models;
using System.ComponentModel.DataAnnotations;

namespace GGHubDb.Models
{
    public class ServerRegionConfig : BaseEntity
    {
        [MaxLength(50)]
        public string RegionCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PrimaryLocation { get; set; } = string.Empty;

        public string FallbackLocations { get; set; } = string.Empty;

        public string CountryCodes { get; set; } = string.Empty;

        public int Priority { get; set; } = 1;

        public bool IsEnabled { get; set; } = true;

        public int EstimatedLatency { get; set; } = 50;
    }
}
