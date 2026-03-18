using System.Text.Json.Serialization;

namespace GGHubShared.Models
{
    #region User Management Requests
    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
    }

    public class UpdateBalanceRequest
    {
        public decimal Amount { get; set; }
    }

    public class UpdateStatsRequest
    {
        public bool IsWin { get; set; }
    }

    public class LinkSteamRequest
    {
        public string SteamId { get; set; } = string.Empty;
    }

    public class LinkTelegramRequest
    {
        public string TelegramUsername { get; set; } = string.Empty;
        public long TelegramChatId { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
    #endregion

    #region Cryptomus Payment Models
    public class CreateCryptomusPaymentRequest
    {
        public decimal Amount { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }

    public class CryptomusPaymentResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? PaymentId { get; set; }
        public string? PaymentUrl { get; set; }
        public string? Status { get; set; }
        public long? ExpiredAt { get; set; }

        public static CryptomusPaymentResult FromApiResponse(CryptomusPaymentApiResponse response)
        {
            return new CryptomusPaymentResult
            {
                Success = response.State == 0,
                Message = response.Message,
                PaymentId = response.Result?.Uuid,
                PaymentUrl = response.Result?.Url,
                Status = response.Result?.PaymentStatus ?? response.Result?.Status,
                ExpiredAt = response.Result?.ExpiredAt
            };
        }
    }

    public class CryptomusPaymentApiResponse
    {
        [JsonPropertyName("state")]
        public int State { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("result")]
        public CryptomusPaymentData? Result { get; set; }
    }

    public class CryptomusPaymentData
    {
        [JsonPropertyName("uuid")]
        public string? Uuid { get; set; }

        [JsonPropertyName("order_id")]
        public string? OrderId { get; set; }

        [JsonPropertyName("amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("payment_status")]
        public string? PaymentStatus { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("expired_at")]
        public long? ExpiredAt { get; set; }

        [JsonPropertyName("is_final")]
        public bool? IsFinal { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }
    }

    public class CryptomusBalanceResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public decimal? Balance { get; set; }

        public static CryptomusBalanceResult FromApiResponse(CryptomusApiResponse response)
        {
            return new CryptomusBalanceResult
            {
                Success = response.State == 0,
                Message = response.Message,
                Balance = response.Result?.Balance
            };
        }
    }

    public class CryptomusApiResponse
    {
        public int State { get; set; }
        public string? Message { get; set; }
        public CryptomusBalanceData? Result { get; set; }
    }

    public class CryptomusBalanceData
    {
        public decimal Balance { get; set; }
    }
    #endregion

    #region Dathost Server & Match Models
    public class DathostServerResult
    {
        public bool Success { get; set; }
        public string? ServerId { get; set; }
        public string? MatchId { get; set; }
        public string? ServerIp { get; set; }
        public int? ServerPort { get; set; }
        public string? Password { get; set; }
        public string? Rcon { get; set; }
        public string? Location { get; set; }
        public int? Slots { get; set; }
        public int? Tickrate { get; set; }
        public string? RawIp { get; set; }
        public bool Autostop { get; set; }
        public int AutostopMinutes { get; set; }
        public bool On { get; set; }
        public bool Booting { get; set; }
        public int PlayersOnline { get; set; }
        public string? ServerError { get; set; }
        public bool Confirmed { get; set; }
        public decimal CostPerHour { get; set; }
        public decimal MaxCostPerHour { get; set; }
        public DateTime? MonthResetAt { get; set; }
        public decimal MaxCostPerMonth { get; set; }
        public long ManualSortOrder { get; set; }
        public long DiskUsageBytes { get; set; }
        public bool DeletionProtection { get; set; }
        public bool OngoingMaintenance { get; set; }
        public string? ConnectString { get; set; }
        public string? SteamConnectUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class DathostMatchResult
    {
        public bool Success { get; set; }
        public string? MatchId { get; set; }
        public string? Status { get; set; }
        public string? WinnerTeam { get; set; }
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
    #endregion

    #region Steam oauth
    public class SteamUserResponse
    {
        [JsonPropertyName("response")]
        public SteamResponse Response { get; set; }
    }


    public class SteamResponse
    {
        [JsonPropertyName("players")]
        public List<SteamPlayer> Players { get; set; } = new List<SteamPlayer>();
    }

    public class SteamPlayer
    {
        // Steam ID of the player
        [JsonPropertyName("steamid")]
        public string SteamId { get; set; }

        // Community visibility state (1 = private, 3 = public)
        [JsonPropertyName("communityvisibilitystate")]
        public int CommunityVisibilityState { get; set; }

        // Profile state (1 = configured)
        [JsonPropertyName("profilestate")]
        public int ProfileState { get; set; }

        // Display name of the player
        [JsonPropertyName("personaname")]
        public string PersonaName { get; set; }

        // URL to Steam profile
        [JsonPropertyName("profileurl")]
        public string ProfileUrl { get; set; }

        // Small avatar image URL (32x32)
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        // Medium avatar image URL (64x64)
        [JsonPropertyName("avatarmedium")]
        public string AvatarMedium { get; set; }

        // Full avatar image URL (184x184)
        [JsonPropertyName("avatarfull")]
        public string AvatarFull { get; set; }

        // Avatar hash for generating custom URLs
        [JsonPropertyName("avatarhash")]
        public string AvatarHash { get; set; }

        // Unix timestamp of last logout
        [JsonPropertyName("lastlogoff")]
        public long LastLogoff { get; set; }

        // Current persona state (0 = offline, 1 = online, etc.)
        [JsonPropertyName("personastate")]
        public int PersonaState { get; set; }

        // Primary Steam group ID
        [JsonPropertyName("primaryclanid")]
        public string PrimaryClanId { get; set; }

        // Unix timestamp when account was created
        [JsonPropertyName("timecreated")]
        public long TimeCreated { get; set; }

        // Persona state flags
        [JsonPropertyName("personastateflags")]
        public int PersonaStateFlags { get; set; }

        // ISO country code
        [JsonPropertyName("loccountrycode")]
        public string LocationCountryCode { get; set; }

        // Helper properties for easier datetime handling
        public DateTime LastLogoffDateTime => DateTimeOffset.FromUnixTimeSeconds(LastLogoff).DateTime;
        public DateTime TimeCreatedDateTime => DateTimeOffset.FromUnixTimeSeconds(TimeCreated).DateTime;

        // Helper property for persona state description
        public string PersonaStateDescription => PersonaState switch
        {
            0 => "Offline",
            1 => "Online",
            2 => "Busy",
            3 => "Away",
            4 => "Snooze",
            5 => "Looking to trade",
            6 => "Looking to play",
            _ => "Unknown"
        };
    }

    public class LinkSteamAccountRequest
    {
        public string SteamId { get; set; } = string.Empty;
    }

    public class SteamUserInfo
    {
        public string SteamId { get; set; } = string.Empty;
        public string PersonaName { get; set; } = string.Empty;
        public string ProfileUrl { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string AvatarMedium { get; set; } = string.Empty;
        public string AvatarFull { get; set; } = string.Empty;
        public int PersonaState { get; set; }
        public int CommunityVisibilityState { get; set; }
        public bool IsOnline { get; set; }
        public bool IsPublicProfile { get; set; }
    }
    #endregion
}
