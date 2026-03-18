using System.ComponentModel.DataAnnotations;
using GGHubShared.Enums;
using GGHubShared.Models;

namespace GGHubDb.Models
{
    public class DuelFormatConfig : BaseEntity
    {
        public DuelFormat Format { get; set; }

        [Range(1, 60)]
        public int DefaultWarmupMinutes { get; set; } = 5;

        [Range(10, 180)]
        public int AutostopMinutes { get; set; } = 30;

        [Range(64, 128)]
        public int DefaultTickrate { get; set; } = 128;

        [Range(1, 100)]
        public int DefaultMaxRounds { get; set; } = 30;

        public bool DefaultOvertimeEnabled { get; set; } = true;

        [Range(1, 10)]
        public int OvertimeMaxRounds { get; set; } = 6;

        [Range(800, 16000)]
        public int OvertimeStartMoney { get; set; } = 10000;

        [Range(5, 60)]
        public int FreezeTime { get; set; } = 15;

        [Range(1, 30)]
        public int RoundRestartDelay { get; set; } = 7;

        [Range(10, 300)]
        public int TeamTimeoutTime { get; set; } = 30;

        public bool AllowCustomTickrate { get; set; } = false;
        public bool AllowCustomRounds { get; set; } = true;
        public bool AllowRegionSelection { get; set; } = true;

        [Range(16, 50)]
        public int MinRounds { get; set; } = 16;
        [Range(16, 50)]
        public int MaxRounds { get; set; } = 30;

        public string AllowedTickrates { get; set; } = "64,128";

        public string? CustomConfig { get; set; }

        public bool IsEnabled { get; set; } = true;

        public decimal CostMultiplier { get; set; } = 1.0m;
    }
}
