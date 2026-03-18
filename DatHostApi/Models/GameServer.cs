using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace DatHost.Models;

/// <summary>
/// Game server information returned by DatHost API.
/// </summary>
public class GameServerResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    /// <summary>
    /// Helper to get <see cref="CreatedAt"/> as <see cref="DateTime"/>
    /// </summary>
    public DateTime CreatedAtDateTime => DateTimeOffset.FromUnixTimeSeconds(CreatedAt).UtcDateTime;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("user_data")]
    public string? UserData { get; set; }

    [JsonPropertyName("game")]
    public string Game { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("players_online")]
    public int PlayersOnline { get; set; }

    [JsonPropertyName("status")]
    public List<string> Status { get; set; } = new();

    [JsonPropertyName("booting")]
    public bool Booting { get; set; }

    [JsonPropertyName("server_error")]
    public string? ServerError { get; set; }

    [JsonPropertyName("ip")]
    public string Ip { get; set; } = string.Empty;

    [JsonPropertyName("raw_ip")]
    public string RawIp { get; set; } = string.Empty;

    [JsonPropertyName("private_ip")]
    public string? PrivateIp { get; set; }

    [JsonPropertyName("match_id")]
    public string? MatchId { get; set; }

    [JsonPropertyName("on")]
    public bool On { get; set; }

    [JsonPropertyName("ports")]
    public ServerPorts Ports { get; set; } = new();

    [JsonPropertyName("confirmed")]
    public bool Confirmed { get; set; }

    [JsonPropertyName("max_disk_usage_gb")]
    public int MaxDiskUsageGb { get; set; }

    [JsonPropertyName("cost_per_hour")]
    public decimal CostPerHour { get; set; }

    [JsonPropertyName("max_cost_per_hour")]
    public decimal MaxCostPerHour { get; set; }

    [JsonPropertyName("month_credits")]
    public int MonthCredits { get; set; }

    [JsonPropertyName("month_reset_at")]
    public long MonthResetAt { get; set; }

    /// <summary>
    /// Helper to get <see cref="MonthResetAt"/> as <see cref="DateTime"/>
    /// </summary>
    public DateTime MonthResetAtDateTime => DateTimeOffset.FromUnixTimeSeconds(MonthResetAt).UtcDateTime;

    [JsonPropertyName("max_cost_per_month")]
    public decimal MaxCostPerMonth { get; set; }

    [JsonPropertyName("subscription_cycle_months")]
    public int SubscriptionCycleMonths { get; set; }

    [JsonPropertyName("subscription_state")]
    public string SubscriptionState { get; set; } = string.Empty;

    [JsonPropertyName("subscription_renewal_failed_attempts")]
    public int SubscriptionRenewalFailedAttempts { get; set; }

    [JsonPropertyName("subscription_renewal_next_attempt_at")]
    public long? SubscriptionRenewalNextAttemptAt { get; set; }

    public DateTime? SubscriptionRenewalNextAttemptAtDateTime =>
        SubscriptionRenewalNextAttemptAt.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(SubscriptionRenewalNextAttemptAt.Value).UtcDateTime
            : null;

    [JsonPropertyName("cycle_months_1_discount_percentage")]
    public int CycleMonths1DiscountPercentage { get; set; }

    [JsonPropertyName("cycle_months_3_discount_percentage")]
    public int CycleMonths3DiscountPercentage { get; set; }

    [JsonPropertyName("cycle_months_12_discount_percentage")]
    public int CycleMonths12DiscountPercentage { get; set; }

    [JsonPropertyName("first_month_discount_percentage")]
    public int FirstMonthDiscountPercentage { get; set; }

    [JsonPropertyName("enable_mysql")]
    public bool EnableMysql { get; set; }

    [JsonPropertyName("autostop")]
    public bool Autostop { get; set; }

    [JsonPropertyName("autostop_minutes")]
    public int AutostopMinutes { get; set; }

    [JsonPropertyName("enable_core_dump")]
    public bool EnableCoreDump { get; set; }

    [JsonPropertyName("prefer_dedicated")]
    public bool PreferDedicated { get; set; }

    [JsonPropertyName("enable_syntropy")]
    public bool EnableSyntropy { get; set; }

    [JsonPropertyName("server_image")]
    public string ServerImage { get; set; } = string.Empty;

    [JsonPropertyName("reboot_on_crash")]
    public bool RebootOnCrash { get; set; }

    [JsonPropertyName("manual_sort_order")]
    public long ManualSortOrder { get; set; }

    [JsonPropertyName("mysql_username")]
    public string MysqlUsername { get; set; } = string.Empty;

    [JsonPropertyName("mysql_password")]
    public string MysqlPassword { get; set; } = string.Empty;

    [JsonPropertyName("ftp_password")]
    public string FtpPassword { get; set; } = string.Empty;

    [JsonPropertyName("disk_usage_bytes")]
    public long DiskUsageBytes { get; set; }

    [JsonPropertyName("default_file_locations")]
    public JsonElement? DefaultFileLocations { get; set; }

    [JsonPropertyName("custom_domain")]
    public string CustomDomain { get; set; } = string.Empty;

    [JsonPropertyName("scheduled_commands")]
    public List<JsonElement> ScheduledCommands { get; set; } = new();

    [JsonPropertyName("added_voice_server")]
    public JsonElement? AddedVoiceServer { get; set; }

    [JsonPropertyName("duplicate_source_server")]
    public JsonElement? DuplicateSourceServer { get; set; }

    [JsonPropertyName("deletion_protection")]
    public bool DeletionProtection { get; set; }

    [JsonPropertyName("ongoing_maintenance")]
    public bool OngoingMaintenance { get; set; }

    [JsonPropertyName("ark_settings")]
    public JsonElement? ArkSettings { get; set; }

    [JsonPropertyName("cs2_settings")]
    public Cs2Settings? Cs2Settings { get; set; }

    [JsonPropertyName("csgo_settings")]
    public CsgoSettings? CsgoSettings { get; set; }

    [JsonPropertyName("minecraft_settings")]
    public JsonElement? MinecraftSettings { get; set; }

    [JsonPropertyName("palworld_settings")]
    public JsonElement? PalworldSettings { get; set; }

    [JsonPropertyName("satisfactory_settings")]
    public JsonElement? SatisfactorySettings { get; set; }

    [JsonPropertyName("sevendaystodie_settings")]
    public JsonElement? SevenDaysToDieSettings { get; set; }

    [JsonPropertyName("sonsoftheforest_settings")]
    public JsonElement? SonsOfTheForestSettings { get; set; }

    [JsonPropertyName("soulmask_settings")]
    public JsonElement? SoulmaskSettings { get; set; }

    [JsonPropertyName("teamfortress2_settings")]
    public JsonElement? TeamFortress2Settings { get; set; }

    [JsonPropertyName("teamspeak3_settings")]
    public JsonElement? TeamSpeak3Settings { get; set; }

    [JsonPropertyName("valheim_settings")]
    public JsonElement? ValheimSettings { get; set; }

    [JsonPropertyName("vrising_settings")]
    public JsonElement? VRisingSettings { get; set; }
}

/// <summary>
/// Ports used by the game server.
/// </summary>
public class ServerPorts
{
    [JsonPropertyName("game")]
    public int Game { get; set; }

    [JsonPropertyName("gotv")]
    public int Gotv { get; set; }

    [JsonPropertyName("gotv_secondary")]
    public int? GotvSecondary { get; set; }
}

/// <summary>
/// CS2 server settings used during creation.
/// Includes both modern and legacy field names returned by DatHost API.
/// </summary>
public class Cs2Settings
{
    // New fields returned by DatHost API
    [JsonPropertyName("slots")]
    public int Slots { get; set; } = 14;

    [JsonPropertyName("steam_game_server_login_token")]
    public string SteamGameServerLoginToken { get; set; } = string.Empty;

    [JsonPropertyName("rcon")]
    public string Rcon { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("maps_source")]
    public string MapsSource { get; set; } = "mapgroup";

    [JsonPropertyName("mapgroup")]
    public string Mapgroup { get; set; } = string.Empty;

    [JsonPropertyName("mapgroup_start_map")]
    public string MapgroupStartMap { get; set; } = "de_dust2";

    [JsonPropertyName("workshop_collection_id")]
    public string WorkshopCollectionId { get; set; } = string.Empty;

    [JsonPropertyName("workshop_collection_start_map_id")]
    public string WorkshopCollectionStartMapId { get; set; } = string.Empty;

    [JsonPropertyName("workshop_single_map_id")]
    public string WorkshopSingleMapId { get; set; } = string.Empty;

    [JsonPropertyName("insecure")]
    public bool Insecure { get; set; } = false;

    [JsonPropertyName("enable_gotv")]
    public bool EnableGotv { get; set; } = false;

    [JsonPropertyName("enable_gotv_secondary")]
    public bool EnableGotvSecondary { get; set; } = false;

    [JsonPropertyName("disable_bots")]
    public bool DisableBots { get; set; } = false;

    [JsonPropertyName("game_mode")]
    public string GameMode { get; set; } = "competitive";

    [JsonPropertyName("enable_metamod")]
    public bool EnableMetamod { get; set; } = false;

    [JsonPropertyName("metamod_plugins")]
    public List<string> MetamodPlugins { get; set; } = new();

    [JsonPropertyName("private_server")]
    public bool PrivateServer { get; set; } = false;

    // Legacy fields still used when creating a server (not in official API docs but supported)
    [JsonPropertyName("mapname")]
    public string? MapName { get; set; }

    [JsonPropertyName("players")]
    public int? Players { get; set; }

    [JsonPropertyName("tickrate")]
    public int? Tickrate { get; set; }

    [JsonPropertyName("config")]
    public string? Config { get; set; }
}

/// <summary>
/// CSGO server settings used during creation.
/// </summary>
public class CsgoSettings
{
    [JsonPropertyName("rcon")]
    public string Rcon { get; set; } = string.Empty;

    [JsonPropertyName("steam_game_server_login_token")]
    public string SteamGameServerLoginToken { get; set; } = string.Empty;

    [JsonPropertyName("slots")]
    public int Slots { get; set; } = 12;

    [JsonPropertyName("tickrate")]
    public decimal Tickrate { get; set; } = 128;

    [JsonPropertyName("autostart")]
    public bool Autostart { get; set; } = true;

    [JsonPropertyName("autostop")]
    public bool Autostop { get; set; } = true;

    [JsonPropertyName("autostop_minutes")]
    public int AutostopMinutes { get; set; } = 20;

    [JsonPropertyName("mapname")]
    public string MapName { get; set; } = "de_dust2";

    [JsonPropertyName("game_mode")]
    public string GameMode { get; set; } = "classic_competitive";

    [JsonPropertyName("insecure")]
    public bool Insecure { get; set; } = false;

    [JsonPropertyName("disable_bots")]
    public bool DisableBots { get; set; } = false;

    [JsonPropertyName("enable_gotv")]
    public bool EnableGotv { get; set; } = false;

    [JsonPropertyName("enable_gotv_secondary")]
    public bool EnableGotvSecondary { get; set; } = false;

    [JsonPropertyName("enable_sourcemod")]
    public bool EnableSourcemod { get; set; } = false;

    [JsonPropertyName("enable_csay_plugin")]
    public bool EnableCsayPlugin { get; set; } = true;

    [JsonPropertyName("disable_1v1_warmup_arenas")]
    public bool Disable1v1WarmupArenas { get; set; } = false;

    [JsonPropertyName("maps_source")]
    public string MapsSource { get; set; } = "mapgroup";

    [JsonPropertyName("mapgroup")]
    public string Mapgroup { get; set; } = "mg_active";

    [JsonPropertyName("mapgroup_start_map")]
    public string MapgroupStartMap { get; set; } = "de_dust2";

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("private_server")]
    public bool PrivateServer { get; set; } = false;

    [JsonPropertyName("pure_server")]
    public bool PureServer { get; set; } = false;

    [JsonPropertyName("sourcemod_admins")]
    public string? SourcemodAdmins { get; set; }

    [JsonPropertyName("sourcemod_plugins")]
    public string? SourcemodPlugins { get; set; }

    [JsonPropertyName("workshop_authkey")]
    public string? WorkshopAuthkey { get; set; }

    [JsonPropertyName("workshop_id")]
    public string WorkshopId { get; set; } = "435587093";

    [JsonPropertyName("workshop_start_map_id")]
    public string WorkshopStartMapId { get; set; } = "125438255";

    [JsonPropertyName("autoload_configs")]
    public string? AutoloadConfigs { get; set; }

    [JsonPropertyName("config")]
    public string? Config { get; set; }
}

/// <summary>
/// Request model for creating a game server.
/// </summary>
public class CreateGameServerRequest
{
    // Required fields
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("game")]
    public string Game { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    // Optional general server settings
    [JsonPropertyName("added_voice_server")]
    public string? AddedVoiceServer { get; set; }

    [JsonPropertyName("autostop")]
    public bool? Autostop { get; set; }

    [JsonPropertyName("autostop_minutes")]
    public int? AutostopMinutes { get; set; }

    [JsonPropertyName("confirmed")]
    public bool? Confirmed { get; set; }

    [JsonPropertyName("custom_domain")]
    public string? CustomDomain { get; set; }

    [JsonPropertyName("deletion_protection")]
    public bool? DeletionProtection { get; set; }

    [JsonPropertyName("enable_core_dump")]
    public bool? EnableCoreDump { get; set; }

    [JsonPropertyName("enable_mysql")]
    public bool? EnableMysql { get; set; }

    [JsonPropertyName("enable_syntropy")]
    public bool? EnableSyntropy { get; set; }

    [JsonPropertyName("manual_sort_order")]
    public decimal? ManualSortOrder { get; set; }

    [JsonPropertyName("max_disk_usage_gb")]
    public int? MaxDiskUsageGb { get; set; }

    [JsonPropertyName("prefer_dedicated")]
    public bool? PreferDedicated { get; set; }

    [JsonPropertyName("reboot_on_crash")]
    public bool? RebootOnCrash { get; set; }

    [JsonPropertyName("scheduled_commands")]
    public string? ScheduledCommands { get; set; }

    [JsonPropertyName("server_image")]
    public string? ServerImage { get; set; }

    [JsonPropertyName("user_data")]
    public string? UserData { get; set; }

    // Game-specific settings
    [JsonPropertyName("cs2_settings")]
    public Cs2Settings? Cs2Settings { get; set; }

    [JsonPropertyName("csgo_settings")]
    public CsgoSettings? CsgoSettings { get; set; }

    // Other game settings as JsonElement for flexibility
    [JsonPropertyName("ark_settings")]
    public JsonElement? ArkSettings { get; set; }

    [JsonPropertyName("teamfortress2_settings")]
    public JsonElement? TeamFortress2Settings { get; set; }

    [JsonPropertyName("teamspeak3_settings")]
    public JsonElement? TeamSpeak3Settings { get; set; }

    [JsonPropertyName("valheim_settings")]
    public JsonElement? ValheimSettings { get; set; }
}

/// <summary>
/// Request model for duplicating a game server.
/// </summary>
public class DuplicateGameServerRequest
{
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("destination_server_id")]
    public string? DestinationServerId { get; set; }
}

/// <summary>
/// Information about a file on the game server.
/// </summary>
public class ServerFile
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("modified")]
    public DateTime Modified { get; set; }

    [JsonPropertyName("is_directory")]
    public bool IsDirectory { get; set; }
}

/// <summary>
/// Console output from game server.
/// </summary>
public class ConsoleOutput
{
    [JsonPropertyName("lines")]
    public List<string> Lines { get; set; } = new();
}

/// <summary>
/// Request to send command to console.
/// </summary>
public class ConsoleCommandRequest
{
    [JsonPropertyName("line")]
    public string Line { get; set; } = string.Empty;
}