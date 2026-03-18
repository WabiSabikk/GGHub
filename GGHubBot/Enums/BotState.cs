namespace GGHubBot.Enums
{
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
}
