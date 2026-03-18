using System.Text.Json.Serialization;

namespace DatHostApi.Models
{
    /// <summary>
    /// CS2 match information
    /// </summary>
    public class Cs2Match
    {
        /// <summary>
        /// Unique match identifier
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Game server ID where match is running
        /// </summary>
        [JsonPropertyName("game_server_id")]
        public string GameServerId { get; set; } = string.Empty;

        /// <summary>
        /// Match status
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Match creation time
        /// </summary>
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        /// <summary>
        /// Match start time
        /// </summary>
        [JsonPropertyName("started")]
        public DateTime? Started { get; set; }

        /// <summary>
        /// Match end time
        /// </summary>
        [JsonPropertyName("ended")]
        public DateTime? Ended { get; set; }

        /// <summary>
        /// Match cancelled status
        /// </summary>
        [JsonPropertyName("cancelled")]
        public bool Cancelled { get; set; }

        /// <summary>
        /// Match playback information
        /// </summary>
        [JsonPropertyName("playback")]
        public MatchPlayback? Playback { get; set; }

        /// <summary>
        /// Match connect information
        /// </summary>
        [JsonPropertyName("connect")]
        public MatchConnect? Connect { get; set; }

        /// <summary>
        /// Team 1 information (згідно з документацією DatHost)
        /// </summary>
        [JsonPropertyName("team1")]
        public TeamInfo? Team1 { get; set; }

        /// <summary>
        /// Team 2 information (згідно з документацією DatHost)
        /// </summary>
        [JsonPropertyName("team2")]
        public TeamInfo? Team2 { get; set; }

        /// <summary>
        /// Match players (згідно з документацією DatHost)
        /// </summary>
        [JsonPropertyName("players")]
        public List<MatchPlayer> Players { get; set; } = new();

        /// <summary>
        /// Match maps
        /// </summary>
        [JsonPropertyName("maps")]
        public List<MatchMap> Maps { get; set; } = new();

        /// <summary>
        /// Match spectators
        /// </summary>
        [JsonPropertyName("spectators")]
        public List<MatchPlayer> Spectators { get; set; } = new();

        /// <summary>
        /// Match settings (згідно з документацією DatHost)
        /// </summary>
        [JsonPropertyName("settings")]
        public MatchSettings? Settings { get; set; }

        /// <summary>
        /// Match webhooks
        /// </summary>
        [JsonPropertyName("webhooks")]
        public WebhookSettings? Webhooks { get; set; }

        /// <summary>
        /// Current round number
        /// </summary>
        [JsonPropertyName("current_round")]
        public int CurrentRound { get; set; }

        /// <summary>
        /// Current map number
        /// </summary>
        [JsonPropertyName("current_map")]
        public int CurrentMap { get; set; }

        /// <summary>
        /// Match result
        /// </summary>
        [JsonPropertyName("result")]
        public MatchResult? Result { get; set; }

        /// <summary>
        /// Number of rounds played
        /// </summary>
        [JsonPropertyName("rounds_played")]
        public int RoundsPlayed { get; set; }

        /// <summary>
        /// Whether match is finished
        /// </summary>
        [JsonPropertyName("finished")]
        public bool Finished { get; set; }

        /// <summary>
        /// Cancel reason if any
        /// </summary>
        [JsonPropertyName("cancel_reason")]
        public string? CancelReason { get; set; }
    }

    /// <summary>
    /// Match playback information
    /// </summary>
    public class MatchPlayback
    {
        /// <summary>
        /// Demo URL
        /// </summary>
        [JsonPropertyName("demo_url")]
        public string? DemoUrl { get; set; }

        /// <summary>
        /// GOTV URL
        /// </summary>
        [JsonPropertyName("gotv_url")]
        public string? GotvUrl { get; set; }
    }

    /// <summary>
    /// Match connect information
    /// </summary>
    public class MatchConnect
    {
        /// <summary>
        /// Connect string
        /// </summary>
        [JsonPropertyName("connect_string")]
        public string ConnectString { get; set; } = string.Empty;

        /// <summary>
        /// Server IP
        /// </summary>
        [JsonPropertyName("ip")]
        public string Ip { get; set; } = string.Empty;

        /// <summary>
        /// Server port
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; }

        /// <summary>
        /// Server password
        /// </summary>
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Team information (тільки ім'я та прапор)
    /// </summary>
    public class TeamInfo
    {
        /// <summary>
        /// Team name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Team flag (country code)
        /// </summary>
        [JsonPropertyName("flag")]
        public string? Flag { get; set; }

        /// <summary>
        /// Team statistics
        /// </summary>
        [JsonPropertyName("stats")]
        public TeamStats? Stats { get; set; }
    }

    /// <summary>
    /// Team statistics
    /// </summary>
    public class TeamStats
    {
        /// <summary>
        /// Team score
        /// </summary>
        [JsonPropertyName("score")]
        public int Score { get; set; }
    }

    /// <summary>
    /// Player information for match
    /// </summary>
    public class MatchPlayer
    {
        /// <summary>
        /// Player Steam ID (64-bit)
        /// </summary>
        [JsonPropertyName("steam_id_64")]
        public string SteamId64 { get; set; } = string.Empty;

        /// <summary>
        /// Player team assignment
        /// </summary>
        [JsonPropertyName("team")]
        public string Team { get; set; } = string.Empty; // "team1", "team2", або "spectator"

        /// <summary>
        /// Override player nickname
        /// </summary>
        [JsonPropertyName("nickname_override")]
        public string? NicknameOverride { get; set; }

        /// <summary>
        /// Whether player is connected
        /// </summary>
        [JsonPropertyName("connected")]
        public bool Connected { get; set; }

        /// <summary>
        /// Whether player was kicked
        /// </summary>
        [JsonPropertyName("kicked")]
        public bool Kicked { get; set; }

        /// <summary>
        /// Player statistics
        /// </summary>
        [JsonPropertyName("stats")]
        public PlayerStats? Stats { get; set; }

        /// <summary>
        /// Match ID (for webhook responses)
        /// </summary>
        [JsonPropertyName("match_id")]
        public string? MatchId { get; set; }
    }


    public class CreateMatchPlayer
    {
        [JsonPropertyName("steam_id_64")]
        public string SteamId64 { get; set; } = string.Empty;

        [JsonPropertyName("team")]
        public string Team { get; set; } = string.Empty;

        [JsonPropertyName("nickname_override")]
        public string? NicknameOverride { get; set; }
    }

    /// <summary>
    /// Player statistics
    /// </summary>
    public class PlayerStats
    {
        /// <summary>
        /// Number of kills
        /// </summary>
        [JsonPropertyName("kills")]
        public int Kills { get; set; }

        /// <summary>
        /// Number of deaths
        /// </summary>
        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }

        /// <summary>
        /// Number of assists
        /// </summary>
        [JsonPropertyName("assists")]
        public int Assists { get; set; }

        /// <summary>
        /// Number of MVPs
        /// </summary>
        [JsonPropertyName("mvps")]
        public int Mvps { get; set; }

        /// <summary>
        /// Player score
        /// </summary>
        [JsonPropertyName("score")]
        public int Score { get; set; }

        /// <summary>
        /// 2-kill rounds
        /// </summary>
        [JsonPropertyName("2ks")]
        public int TwoKs { get; set; }

        /// <summary>
        /// 3-kill rounds
        /// </summary>
        [JsonPropertyName("3ks")]
        public int ThreeKs { get; set; }

        /// <summary>
        /// 4-kill rounds
        /// </summary>
        [JsonPropertyName("4ks")]
        public int FourKs { get; set; }

        /// <summary>
        /// 5-kill rounds (aces)
        /// </summary>
        [JsonPropertyName("5ks")]
        public int FiveKs { get; set; }

        /// <summary>
        /// Kills with headshot
        /// </summary>
        [JsonPropertyName("kills_with_headshot")]
        public int KillsWithHeadshot { get; set; }

        /// <summary>
        /// Kills with pistol
        /// </summary>
        [JsonPropertyName("kills_with_pistol")]
        public int KillsWithPistol { get; set; }

        /// <summary>
        /// Kills with sniper
        /// </summary>
        [JsonPropertyName("kills_with_sniper")]
        public int KillsWithSniper { get; set; }

        /// <summary>
        /// Total damage dealt
        /// </summary>
        [JsonPropertyName("damage_dealt")]
        public int DamageDealt { get; set; }

        /// <summary>
        /// Flashbangs thrown
        /// </summary>
        [JsonPropertyName("flashes_thrown")]
        public int FlashesThrown { get; set; }

        /// <summary>
        /// Successful flashbangs
        /// </summary>
        [JsonPropertyName("flashes_successful")]
        public int FlashesSuccessful { get; set; }

        /// <summary>
        /// Enemies blinded by flashes
        /// </summary>
        [JsonPropertyName("flashes_enemies_blinded")]
        public int FlashesEnemiesBlinded { get; set; }

        /// <summary>
        /// Utility thrown
        /// </summary>
        [JsonPropertyName("utility_thrown")]
        public int UtilityThrown { get; set; }

        /// <summary>
        /// Damage from utility
        /// </summary>
        [JsonPropertyName("utility_damage")]
        public int UtilityDamage { get; set; }

        /// <summary>
        /// Entry attempts
        /// </summary>
        [JsonPropertyName("entry_attempts")]
        public int EntryAttempts { get; set; }

        /// <summary>
        /// Successful entries
        /// </summary>
        [JsonPropertyName("entry_successes")]
        public int EntrySuccesses { get; set; }

        /// <summary>
        /// 1vX attempts
        /// </summary>
        [JsonPropertyName("1vX_attempts")]
        public int OneVsXAttempts { get; set; }

        /// <summary>
        /// 1vX wins
        /// </summary>
        [JsonPropertyName("1vX_wins")]
        public int OneVsXWins { get; set; }
    }

    /// <summary>
    /// Match map information
    /// </summary>
    public class MatchMap
    {
        /// <summary>
        /// Map name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Map pick type
        /// </summary>
        [JsonPropertyName("pick_type")]
        public string PickType { get; set; } = string.Empty;

        /// <summary>
        /// Team that picked the map
        /// </summary>
        [JsonPropertyName("picked_by")]
        public string PickedBy { get; set; } = string.Empty;

        /// <summary>
        /// Map status
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Map result
        /// </summary>
        [JsonPropertyName("result")]
        public MapResult? Result { get; set; }
    }

    /// <summary>
    /// Map result information
    /// </summary>
    public class MapResult
    {
        /// <summary>
        /// Team 1 score
        /// </summary>
        [JsonPropertyName("team1_score")]
        public int Team1Score { get; set; }

        /// <summary>
        /// Team 2 score
        /// </summary>
        [JsonPropertyName("team2_score")]
        public int Team2Score { get; set; }

        /// <summary>
        /// Winning team
        /// </summary>
        [JsonPropertyName("winner")]
        public string Winner { get; set; } = string.Empty;
    }

    /// <summary>
    /// Match result information
    /// </summary>
    public class MatchResult
    {
        /// <summary>
        /// Team 1 score
        /// </summary>
        [JsonPropertyName("team1_score")]
        public int Team1Score { get; set; }

        /// <summary>
        /// Team 2 score
        /// </summary>
        [JsonPropertyName("team2_score")]
        public int Team2Score { get; set; }

        /// <summary>
        /// Winning team
        /// </summary>
        [JsonPropertyName("winner")]
        public string Winner { get; set; } = string.Empty;
    }

    /// <summary>
    /// Match settings (згідно з документацією DatHost)
    /// </summary>
    public class MatchSettings
    {
        /// <summary>
        /// Map name
        /// </summary>
        [JsonPropertyName("map")]
        public string? Map { get; set; }

        /// <summary>
        /// Server password
        /// </summary>
        [JsonPropertyName("password")]
        public string? Password { get; set; }

        /// <summary>
        /// Connection time in seconds
        /// </summary>
        [JsonPropertyName("connect_time")]
        public int ConnectTime { get; set; } = 300;

        /// <summary>
        /// Match begin countdown in seconds
        /// </summary>
        [JsonPropertyName("match_begin_countdown")]
        public int MatchBeginCountdown { get; set; } = 30;

        /// <summary>
        /// Team size (can be null for auto-detection)
        /// </summary>
        [JsonPropertyName("team_size")]
        public int? TeamSize { get; set; }

        /// <summary>
        /// Wait for GOTV before ending match
        /// </summary>
        [JsonPropertyName("wait_for_gotv")]
        public bool WaitForGotv { get; set; } = false;

        /// <summary>
        /// Enable plugin
        /// </summary>
        [JsonPropertyName("enable_plugin")]
        public bool EnablePlugin { get; set; } = true;

        /// <summary>
        /// Enable technical pause
        /// </summary>
        [JsonPropertyName("enable_tech_pause")]
        public bool EnableTechPause { get; set; } = true;
    }

    /// <summary>
    /// Webhook settings (згідно з документацією DatHost)
    /// </summary>
    public class WebhookSettings
    {
        /// <summary>
        /// Webhook URL for match end events
        /// </summary>
        [JsonPropertyName("match_end_url")]
        public string? MatchEndUrl { get; set; }

        /// <summary>
        /// Webhook URL for round end events
        /// </summary>
        [JsonPropertyName("round_end_url")]
        public string? RoundEndUrl { get; set; }

        /// <summary>
        /// Webhook URL for player votekick success events
        /// </summary>
        [JsonPropertyName("player_votekick_success_url")]
        public string? PlayerVotekickSuccessUrl { get; set; }

        /// <summary>
        /// URL that will receive webhooks for all event types specified by enabled_events
        /// </summary>
        [JsonPropertyName("event_url")]
        public string? EventUrl { get; set; }

        /// <summary>
        /// Array of event types to subscribe to, use ["*"] to subscribe to all events
        /// </summary>
        [JsonPropertyName("enabled_events")]
        public List<string>? EnabledEvents { get; set; }

        /// <summary>
        /// Authorization header added to all webhook requests
        /// </summary>
        [JsonPropertyName("authorization_header")]
        public string? AuthorizationHeader { get; set; }
    }

    /// <summary>
    /// Request model for creating a CS2 match (згідно з документацією DatHost)
    /// </summary>
    public class CreateCs2MatchRequest
    {
        /// <summary>
        /// ОБОВ'ЯЗКОВЕ: Game server ID
        /// </summary>
        [JsonPropertyName("game_server_id")]
        public string GameServerId { get; set; } = string.Empty;

        /// <summary>
        /// ОБОВ'ЯЗКОВЕ: Players array з командами
        /// </summary>
        [JsonPropertyName("players")]
        public List<CreateMatchPlayer> Players { get; set; } = new();

        /// <summary>
        /// Опціонально: Team 1 information
        /// </summary>
        [JsonPropertyName("team1")]
        public TeamInfo? Team1 { get; set; }

        /// <summary>
        /// Опціонально: Team 2 information
        /// </summary>
        [JsonPropertyName("team2")]
        public TeamInfo? Team2 { get; set; }

        /// <summary>
        /// Опціонально: Match settings
        /// </summary>
        [JsonPropertyName("settings")]
        public MatchSettings? Settings { get; set; }

        /// <summary>
        /// Опціонально: Webhooks configuration
        /// </summary>
        [JsonPropertyName("webhooks")]
        public WebhookSettings? Webhooks { get; set; }
    }

    /// <summary>
    /// Request model for adding a player to a match
    /// </summary>
    public class AddPlayerToMatchRequest
    {
        /// <summary>
        /// Player Steam ID
        /// </summary>
        [JsonPropertyName("steam_id")]
        public string SteamId { get; set; } = string.Empty;

        /// <summary>
        /// Player nickname
        /// </summary>
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        /// <summary>
        /// Target team (team1, team2, spectator)
        /// </summary>
        [JsonPropertyName("team")]
        public string Team { get; set; } = string.Empty;

        /// <summary>
        /// Captain status
        /// </summary>
        [JsonPropertyName("captain")]
        public bool Captain { get; set; }
    }

    // ===== LEGACY MODELS ДЛЯ ЗВОРОТНОЇ СУМІСНОСТІ =====

    /// <summary>
    /// Legacy Team model (для зворотної сумісності)
    /// </summary>
    public class Team
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("players")]
        public List<Player> Players { get; set; } = new();

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; } = string.Empty;
    }

    /// <summary>
    /// Legacy Player model (для зворотної сумісності)
    /// </summary>
    public class Player
    {
        [JsonPropertyName("steam_id")]
        public string SteamId { get; set; } = string.Empty;

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonPropertyName("captain")]
        public bool Captain { get; set; }

        [JsonPropertyName("stats")]
        public PlayerStats? Stats { get; set; }
    }

    /// <summary>
    /// Legacy GameSettings model (для зворотної сумісності)
    /// </summary>
    public class GameSettings
    {
        [JsonPropertyName("knife_round")]
        public bool KnifeRound { get; set; }

        [JsonPropertyName("warmup")]
        public bool Warmup { get; set; }

        [JsonPropertyName("overtime")]
        public bool Overtime { get; set; }

        [JsonPropertyName("map_veto")]
        public bool MapVeto { get; set; }

        [JsonPropertyName("auto_start")]
        public bool AutoStart { get; set; }

        [JsonPropertyName("auto_ready")]
        public bool AutoReady { get; set; }
    }

    /// <summary>
    /// Legacy MatchWebhooks model (для зворотної сумісності)
    /// </summary>
    public class MatchWebhooks
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("secret")]
        public string Secret { get; set; } = string.Empty;
    }
}