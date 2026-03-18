using System.ComponentModel.DataAnnotations;

namespace GGHubBot.Models
{
    public class UserState
    {
        [Key]
        public long TelegramId { get; set; }

        public string Language { get; set; } = "ua";

        public BotState State { get; set; } = BotState.Start;

        public string? StateData { get; set; }

        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        public bool IsAuthenticated { get; set; }

        public Guid? UserId { get; set; }

        public string? SteamId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum BotState
    {
        Start,
        LanguageSelection,
        MainMenu,

        CreateDuel,
        CreateDuelFormat,
        CreateDuelEntryFee,
        CreateDuelRounds,
        CreateDuelPrime,
        CreateDuelWarmup,
        CreateDuelMaxRounds,
        CreateDuelMaps,
        CreateDuelConfirm,

        JoinDuel,
        MyDuels,

        CreateTournament,
        CreateTournamentStep1,
        CreateTournamentStep2,
        CreateTournamentStep3,
        CreateTournamentStep4,
        CreateTournamentStep5,
        CreateTournamentConfirm,

        JoinTournament,
        MyTournaments,

        Profile,
        Wallet,
        WalletDeposit,
        WalletWithdraw,

        Settings,
        Help,

        SteamAuth,
        SteamAuthCallback
    }

    public class CreateDuelState
    {
        public string? Format { get; set; }
        public decimal? EntryFee { get; set; }
        public List<string> Maps { get; set; } = new();
        public string? RoundFormat { get; set; }
        public bool PrimeOnly { get; set; }
        public int WarmupMinutes { get; set; } = 5;
        public int? MaxRounds { get; set; }
        public int CurrentStep { get; set; } = 1;
    }

    public class CreateTournamentState
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? PlayersPerTeam { get; set; }
        public int? MaxTeams { get; set; }
        public decimal? EntryFee { get; set; }
        public List<string> Maps { get; set; } = new();
        public DateTime? StartTime { get; set; }
        public string? Rules { get; set; }
        public int CurrentStep { get; set; } = 1;
    }

    public class WalletOperationState
    {
        public string Operation { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string? PaymentProvider { get; set; }
    }
}
