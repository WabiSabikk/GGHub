using System.Text.Json.Serialization;

namespace GGHubShared.Models
{
    public class DathostMatchWebhook
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("game_server_id")]
        public string GameServerId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("finished")]
        public bool Finished { get; set; }

        [JsonPropertyName("cancel_reason")]
        public string? CancelReason { get; set; }

        [JsonPropertyName("rounds_played")]
        public int RoundsPlayed { get; set; }

        [JsonPropertyName("team1")]
        public DathostTeamWebhook? Team1 { get; set; }

        [JsonPropertyName("team2")]
        public DathostTeamWebhook? Team2 { get; set; }

        [JsonPropertyName("players")]
        public List<DathostPlayerWebhook> Players { get; set; } = new();

        [JsonPropertyName("settings")]
        public DathostMatchSettings? Settings { get; set; }

        [JsonPropertyName("webhooks")]
        public DathostWebhookSettings? Webhooks { get; set; }

        [JsonPropertyName("events")]
        public List<DathostEventRecord> Events { get; set; } = new();
    }

    public class DathostRoundWebhook
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("game_server_id")]
        public string GameServerId { get; set; } = string.Empty;

        [JsonPropertyName("team1_score")]
        public int Team1Score { get; set; }

        [JsonPropertyName("team2_score")]
        public int Team2Score { get; set; }

        [JsonPropertyName("rounds_played")]
        public int RoundsPlayed { get; set; }

        [JsonPropertyName("team1")]
        public DathostTeamWebhook? Team1 { get; set; }

        [JsonPropertyName("team2")]
        public DathostTeamWebhook? Team2 { get; set; }

        [JsonPropertyName("players")]
        public List<DathostPlayerWebhook> Players { get; set; } = new();
    }

    public class DathostEventWebhook
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("payload")]
        public DathostEventPayload? Payload { get; set; }
    }

    public class DathostVotekickWebhook
    {
        [JsonPropertyName("match_id")]
        public string MatchId { get; set; } = string.Empty;

        [JsonPropertyName("steam_id_64")]
        public string SteamId64 { get; set; } = string.Empty;

        [JsonPropertyName("team")]
        public string Team { get; set; } = string.Empty;

        [JsonPropertyName("nickname_override")]
        public string? NicknameOverride { get; set; }

        [JsonPropertyName("connected")]
        public bool Connected { get; set; }

        [JsonPropertyName("kicked")]
        public bool Kicked { get; set; }

        [JsonPropertyName("stats")]
        public DathostPlayerStats? Stats { get; set; }
    }

    public class DathostTeamWebhook
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("flag")]
        public string? Flag { get; set; }

        [JsonPropertyName("stats")]
        public DathostTeamStats? Stats { get; set; }
    }

    public class DathostTeamStats
    {
        [JsonPropertyName("score")]
        public int Score { get; set; }
    }

    public class DathostPlayerWebhook
    {
        [JsonPropertyName("match_id")]
        public string MatchId { get; set; } = string.Empty;

        [JsonPropertyName("steam_id_64")]
        public string SteamId64 { get; set; } = string.Empty;

        [JsonPropertyName("team")]
        public string Team { get; set; } = string.Empty;

        [JsonPropertyName("nickname_override")]
        public string? NicknameOverride { get; set; }

        [JsonPropertyName("connected")]
        public bool Connected { get; set; }

        [JsonPropertyName("kicked")]
        public bool Kicked { get; set; }

        [JsonPropertyName("disconnected_at")]
        public DateTime? DisconnectedAt { get; set; }

        [JsonPropertyName("disconnected_reason")]
        public string? DisconnectedReason { get; set; }

        [JsonPropertyName("stats")]
        public DathostPlayerStats? Stats { get; set; }
    }

    public class DathostPlayerStats
    {
        [JsonPropertyName("kills")]
        public int Kills { get; set; }

        [JsonPropertyName("assists")]
        public int Assists { get; set; }

        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }

        [JsonPropertyName("mvps")]
        public int Mvps { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("2ks")]
        public int TwoKs { get; set; }

        [JsonPropertyName("3ks")]
        public int ThreeKs { get; set; }

        [JsonPropertyName("4ks")]
        public int FourKs { get; set; }

        [JsonPropertyName("5ks")]
        public int FiveKs { get; set; }

        [JsonPropertyName("kills_with_headshot")]
        public int KillsWithHeadshot { get; set; }

        [JsonPropertyName("kills_with_pistol")]
        public int KillsWithPistol { get; set; }

        [JsonPropertyName("kills_with_sniper")]
        public int KillsWithSniper { get; set; }

        [JsonPropertyName("damage_dealt")]
        public int DamageDealt { get; set; }

        [JsonPropertyName("entry_attempts")]
        public int EntryAttempts { get; set; }

        [JsonPropertyName("entry_successes")]
        public int EntrySuccesses { get; set; }

        [JsonPropertyName("flashes_thrown")]
        public int FlashesThrown { get; set; }

        [JsonPropertyName("flashes_successful")]
        public int FlashesSuccessful { get; set; }

        [JsonPropertyName("flashes_enemies_blinded")]
        public int FlashesEnemiesBlinded { get; set; }

        [JsonPropertyName("utility_thrown")]
        public int UtilityThrown { get; set; }

        [JsonPropertyName("utility_damage")]
        public int UtilityDamage { get; set; }

        [JsonPropertyName("1vX_attempts")]
        public int OneVsXAttempts { get; set; }

        [JsonPropertyName("1vX_wins")]
        public int OneVsXWins { get; set; }
    }

    public class DathostMatchSettings
    {
        [JsonPropertyName("map")]
        public string? Map { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("connect_time")]
        public int ConnectTime { get; set; }

        [JsonPropertyName("match_begin_countdown")]
        public int MatchBeginCountdown { get; set; }

        [JsonPropertyName("team_size")]
        public int? TeamSize { get; set; }

        [JsonPropertyName("wait_for_gotv")]
        public bool WaitForGotv { get; set; }

        [JsonPropertyName("enable_plugin")]
        public bool EnablePlugin { get; set; }

        [JsonPropertyName("enable_tech_pause")]
        public bool EnableTechPause { get; set; }
    }

    public class DathostWebhookSettings
    {
        [JsonPropertyName("match_end_url")]
        public string? MatchEndUrl { get; set; }

        [JsonPropertyName("round_end_url")]
        public string? RoundEndUrl { get; set; }

        [JsonPropertyName("player_votekick_success_url")]
        public string? PlayerVotekickSuccessUrl { get; set; }

        [JsonPropertyName("event_url")]
        public string? EventUrl { get; set; }

        [JsonPropertyName("enabled_events")]
        public List<string>? EnabledEvents { get; set; }

        [JsonPropertyName("authorization_header")]
        public string? AuthorizationHeader { get; set; }
    }

    public class DathostEventRecord
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("payload")]
        public DathostEventPayload? Payload { get; set; }
    }

    public class DathostEventPayload
    {
        [JsonPropertyName("steam_id_64")]
        public string? SteamId64 { get; set; }

        [JsonPropertyName("team1_score")]
        public int? Team1Score { get; set; }

        [JsonPropertyName("team2_score")]
        public int? Team2Score { get; set; }
    }

    public enum MatchType
    {
        Unknown,
        Duel,
        Tournament
    }
}