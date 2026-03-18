namespace GGHubShared.Enums
{
    public enum DuelFormat
    {
        OneVsOne = 1,
        TwoVsTwo = 2,
        FiveVsFive = 5
    }

    public enum DuelStatus
    {
        Created,
        WaitingForPlayers,
        PaymentPending,
        WaitingForLaunch,
        Starting,
        InProgress,
        Completed,
        Cancelled,
        Disputed
    }

    public enum RoundFormat
    {
        BestOfOne = 1,
        BestOfThree = 3,
        BestOfFive = 5
    }

    public enum UserRole
    {
        User,
        Moderator,
        Admin,
        Bot
    }

    public enum TransactionType
    {
        Deposit,
        EntryFee,
        Prize,
        Refund,
        Withdrawal
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled
    }

    public enum PaymentProvider
    {
        Cryptomus,
        Manual
    }

    public enum ComplaintStatus
    {
        Pending,
        UnderReview,
        Resolved,
        Rejected
    }

    public enum ServerProvider
    {
        Dathost
    }

    public enum ServerStatus
    {
        Creating,
        Starting,
        Running,
        Stopping,
        Stopped,
        Error
    }
    public enum TournamentStatus
    {
        Created,
        WaitingForTeams,
        WaitingForPayments,
        PaymentCompleted,
        InProgress,
        Completed,
        Cancelled
    }

    public enum TournamentMatchStatus
    {
        Waiting,
        ScheduledForCreation,
        CreatingServer,
        Ready,
        InProgress,
        Completed,
        Cancelled,
        TechnicalDefeat
    }

    public enum ForfeitReason
    {
        /// <summary>
        /// Гравець підтвердив вихід (технічна поразка)
        /// </summary>
        Confirmed = 0,

        /// <summary>
        /// Поганий пінг або лаги
        /// </summary>
        BadPing = 1,

        /// <summary>
        /// Інші технічні проблеми
        /// </summary>
        TechnicalIssues = 2
    }
}
