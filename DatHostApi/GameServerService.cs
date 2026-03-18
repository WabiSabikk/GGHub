using DatHost.Api.Client;
using DatHost.Models;

namespace DatHostApi
{
    /// <summary>
    /// Interface for game server management operations
    /// </summary>
    public interface IGameServerService
    {
        /// <summary>
        /// Get list of all game servers
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of game servers</returns>
        Task<List<GameServerResponse>> GetGameServersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get specific game server by ID
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Game server information</returns>
        Task<GameServerResponse> GetGameServerAsync(string serverId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new game server
        /// </summary>
        /// <param name="request">Create server request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created game server</returns>
        Task<GameServerResponse> CreateGameServerAsync(CreateGameServerRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update an existing game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="request">Update server request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated game server</returns>
        Task<GameServerResponse> UpdateGameServerAsync(string serverId, CreateGameServerRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteGameServerAsync(string serverId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Start a game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task StartGameServerAsync(string serverId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop a game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task StopGameServerAsync(string serverId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reset a game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ResetGameServerAsync(string serverId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Duplicate a game server
        /// </summary>
        /// <param name="serverId">Source server ID</param>
        /// <param name="request">Duplicate request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Duplicated game server</returns>
        Task<GameServerResponse> DuplicateGameServerAsync(string serverId, DuplicateGameServerRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sync files between API cache and game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SyncFilesAsync(string serverId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get list of files on game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="path">Optional path to list files from</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of server files</returns>
        Task<List<ServerFile>> GetServerFilesAsync(string serverId, string? path = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get console output from game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="maxLines">Maximum number of lines to retrieve (default 500)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Console output as string</returns>
        Task<string> GetServerConsoleAsync(string serverId, int maxLines = 500, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send RCON command to server console
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="command">Command to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task SendConsoleCommandAsync(string serverId, string command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service for managing game servers through DatHost API
    /// </summary>
    public class GameServerService : IGameServerService
    {
        private readonly IDatHostApiClient _apiClient;

        public GameServerService(IDatHostApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        /// <summary>
        /// Get list of all game servers
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of game servers</returns>
        public async Task<List<GameServerResponse>> GetGameServersAsync(CancellationToken cancellationToken = default)
        {
            return await _apiClient.GetAsync<List<GameServerResponse>>("game-servers", cancellationToken);
        }

        /// <summary>
        /// Get specific game server by ID
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Game server information</returns>
        public async Task<GameServerResponse> GetGameServerAsync(string serverId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));

            return await _apiClient.GetAsync<GameServerResponse>($"game-servers/{serverId}", cancellationToken);
        }

        /// <summary>
        /// Create a new game server
        /// </summary>
        /// <param name="request">Create server request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created game server</returns>
        public async Task<GameServerResponse> CreateGameServerAsync(CreateGameServerRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Create multipart form data for server creation
            var formData = new MultipartFormDataContent();

            // Required fields
            formData.Add(new StringContent(request.Name), "name");
            formData.Add(new StringContent(request.Game), "game");
            formData.Add(new StringContent(request.Location), "location");

            // Add general server settings
            AddGeneralServerSettingsToForm(formData, request);

            // Add CS2 settings if provided
            if (request.Cs2Settings != null)
            {
                AddCs2SettingsToForm(formData, request.Cs2Settings);
            }

            //// Add CSGO settings if provided
            //if (request.CsgoSettings != null)
            //{
            //    AddCsgoSettingsToForm(formData, request.CsgoSettings);
            //}

            return await _apiClient.PostAsync<GameServerResponse>("game-servers", formData, cancellationToken);
        }

        /// <summary>
        /// Update an existing game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="request">Update server request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated game server</returns>
        public async Task<GameServerResponse> UpdateGameServerAsync(string serverId, CreateGameServerRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Create multipart form data for server update
            var formData = new MultipartFormDataContent();

            // Required fields
            formData.Add(new StringContent(request.Name), "name");
            formData.Add(new StringContent(request.Game), "game");
            formData.Add(new StringContent(request.Location), "location");

            // Add general server settings
            AddGeneralServerSettingsToForm(formData, request);

            // Add CS2 settings if provided
            if (request.Cs2Settings != null)
            {
                AddCs2SettingsToForm(formData, request.Cs2Settings);
            }

            // Add CSGO settings if provided
            //if (request.CsgoSettings != null)
            //{
            //    AddCsgoSettingsToForm(formData, request.CsgoSettings);
            //}

            //return await _apiClient.PostAsync<GameServerResponse>($"game-servers/{serverId}", formData, cancellationToken);
            return await _apiClient.PutAsync<GameServerResponse>($"game-servers/{serverId}", formData, cancellationToken);
        }

        /// <summary>
        /// Delete a game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task DeleteGameServerAsync(string serverId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));

            await _apiClient.PostAsync($"game-servers/{serverId}/delete", null, cancellationToken);
        }

        /// <summary>
        /// Start a game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task StartGameServerAsync(string serverId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));

            await _apiClient.PostAsync($"game-servers/{serverId}/start", null, cancellationToken);
        }

        /// <summary>
        /// Stop a game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task StopGameServerAsync(string serverId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));

            await _apiClient.PostAsync($"game-servers/{serverId}/stop", null, cancellationToken);
        }

        /// <summary>
        /// Reset a game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task ResetGameServerAsync(string serverId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));

            await _apiClient.PostAsync($"game-servers/{serverId}/reset", null, cancellationToken);
        }

        /// <summary>
        /// Duplicate a game server
        /// </summary>
        /// <param name="serverId">Source server ID</param>
        /// <param name="request">Duplicate request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Duplicated game server</returns>
        public async Task<GameServerResponse> DuplicateGameServerAsync(string serverId, DuplicateGameServerRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(request.Location), "location");

            if (!string.IsNullOrEmpty(request.DestinationServerId))
            {
                formData.Add(new StringContent(request.DestinationServerId), "destination_server_id");
            }

            return await _apiClient.PostAsync<GameServerResponse>($"game-servers/{serverId}/duplicate", formData, cancellationToken);
        }

        /// <summary>
        /// Sync files between API cache and game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SyncFilesAsync(string serverId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));

            await _apiClient.PostAsync($"game-servers/{serverId}/sync-files", null, cancellationToken);
        }

        /// <summary>
        /// Get list of files on game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="path">Optional path to list files from</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of server files</returns>
        public async Task<List<ServerFile>> GetServerFilesAsync(string serverId, string? path = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));

            var endpoint = $"game-servers/{serverId}/files";
            if (!string.IsNullOrEmpty(path))
            {
                endpoint += $"?path={Uri.EscapeDataString(path)}";
            }

            return await _apiClient.GetAsync<List<ServerFile>>(endpoint, cancellationToken);
        }

        /// <summary>
        /// Get console output from game server
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="maxLines">Maximum number of lines to retrieve (default 500)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Console output as string</returns>
        public async Task<string> GetServerConsoleAsync(string serverId, int maxLines = 500, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));

            var endpoint = $"game-servers/{serverId}/console?max_lines={maxLines}";
            var result = await _apiClient.GetAsync<ConsoleOutput>(endpoint, cancellationToken);

            return string.Join("\n", result.Lines);
        }

        /// <summary>
        /// Send RCON command to server console
        /// </summary>
        /// <param name="serverId">Server ID</param>
        /// <param name="command">Command to execute</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SendConsoleCommandAsync(string serverId, string command, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serverId))
                throw new ArgumentException("Server ID cannot be null or empty", nameof(serverId));
            if (string.IsNullOrEmpty(command))
                throw new ArgumentException("Command cannot be null or empty", nameof(command));

            var endpoint = $"game-servers/{serverId}/console";
            var data = new ConsoleCommandRequest { Line = command };

            await _apiClient.PostAsync(endpoint, data, cancellationToken);
        }

        /// <summary>
        /// Add general server settings to form data
        /// </summary>
        /// <param name="formData">Form data</param>
        /// <param name="request">Server request</param>
        private static void AddGeneralServerSettingsToForm(MultipartFormDataContent formData, CreateGameServerRequest request)
        {
            // Optional string fields
            if (!string.IsNullOrEmpty(request.AddedVoiceServer))
            {
                formData.Add(new StringContent(request.AddedVoiceServer), "added_voice_server");
            }

            if (!string.IsNullOrEmpty(request.CustomDomain))
            {
                formData.Add(new StringContent(request.CustomDomain), "custom_domain");
            }

            if (!string.IsNullOrEmpty(request.ScheduledCommands))
            {
                formData.Add(new StringContent(request.ScheduledCommands), "scheduled_commands");
            }

            if (!string.IsNullOrEmpty(request.ServerImage))
            {
                formData.Add(new StringContent(request.ServerImage), "server_image");
            }

            if (!string.IsNullOrEmpty(request.UserData))
            {
                formData.Add(new StringContent(request.UserData), "user_data");
            }

            // Boolean fields with optional values
            if (request.Autostop.HasValue)
            {
                formData.Add(new StringContent(request.Autostop.Value.ToString().ToLower()), "autostop");
            }

            if (request.Confirmed.HasValue)
            {
                formData.Add(new StringContent(request.Confirmed.Value.ToString().ToLower()), "confirmed");
            }

            if (request.DeletionProtection.HasValue)
            {
                formData.Add(new StringContent(request.DeletionProtection.Value.ToString().ToLower()), "deletion_protection");
            }

            if (request.EnableCoreDump.HasValue)
            {
                formData.Add(new StringContent(request.EnableCoreDump.Value.ToString().ToLower()), "enable_core_dump");
            }

            if (request.EnableMysql.HasValue)
            {
                formData.Add(new StringContent(request.EnableMysql.Value.ToString().ToLower()), "enable_mysql");
            }

            if (request.EnableSyntropy.HasValue)
            {
                formData.Add(new StringContent(request.EnableSyntropy.Value.ToString().ToLower()), "enable_syntropy");
            }

            if (request.PreferDedicated.HasValue)
            {
                formData.Add(new StringContent(request.PreferDedicated.Value.ToString().ToLower()), "prefer_dedicated");
            }

            if (request.RebootOnCrash.HasValue)
            {
                formData.Add(new StringContent(request.RebootOnCrash.Value.ToString().ToLower()), "reboot_on_crash");
            }

            // Numeric fields
            if (request.AutostopMinutes.HasValue)
            {
                formData.Add(new StringContent(request.AutostopMinutes.Value.ToString()), "autostop_minutes");
            }

            if (request.ManualSortOrder.HasValue)
            {
                formData.Add(new StringContent(request.ManualSortOrder.Value.ToString()), "manual_sort_order");
            }

            if (request.MaxDiskUsageGb.HasValue)
            {
                formData.Add(new StringContent(request.MaxDiskUsageGb.Value.ToString()), "max_disk_usage_gb");
            }
        }

        /// <summary>
        /// Add CS2 settings to form data
        /// </summary>
        /// <param name="formData">Form data</param>
        /// <param name="settings">CS2 settings</param>
        private static void AddCs2SettingsToForm(MultipartFormDataContent formData, Cs2Settings settings)
        {
            formData.Add(new StringContent(settings.Rcon), "cs2_settings.rcon");
            formData.Add(new StringContent(settings.SteamGameServerLoginToken), "cs2_settings.steam_game_server_login_token");
            formData.Add(new StringContent(settings.Slots.ToString()), "cs2_settings.slots");
            formData.Add(new StringContent(settings.Password), "cs2_settings.password");
            formData.Add(new StringContent(settings.MapsSource), "cs2_settings.maps_source");
            formData.Add(new StringContent(settings.Mapgroup), "cs2_settings.mapgroup");
            formData.Add(new StringContent(settings.MapgroupStartMap), "cs2_settings.mapgroup_start_map");
            formData.Add(new StringContent(settings.WorkshopCollectionId), "cs2_settings.workshop_collection_id");
            formData.Add(new StringContent(settings.WorkshopCollectionStartMapId), "cs2_settings.workshop_collection_start_map_id");
            formData.Add(new StringContent(settings.WorkshopSingleMapId), "cs2_settings.workshop_single_map_id");
            formData.Add(new StringContent(settings.Insecure.ToString().ToLower()), "cs2_settings.insecure");
            formData.Add(new StringContent(settings.EnableGotv.ToString().ToLower()), "cs2_settings.enable_gotv");
            formData.Add(new StringContent(settings.EnableGotvSecondary.ToString().ToLower()), "cs2_settings.enable_gotv_secondary");
            formData.Add(new StringContent(settings.DisableBots.ToString().ToLower()), "cs2_settings.disable_bots");
            formData.Add(new StringContent(settings.GameMode), "cs2_settings.game_mode");
            formData.Add(new StringContent(settings.EnableMetamod.ToString().ToLower()), "cs2_settings.enable_metamod");
            foreach (var plugin in settings.MetamodPlugins)
            {
                formData.Add(new StringContent(plugin), "cs2_settings.metamod_plugins[]");
            }
            formData.Add(new StringContent(settings.PrivateServer.ToString().ToLower()), "cs2_settings.private_server");

            // Optional fields that are present in current code but not in API docs
            if (!string.IsNullOrEmpty(settings.MapName))
            {
                formData.Add(new StringContent(settings.MapName), "cs2_settings.mapname");
            }
            if (settings.Players.HasValue)
            {
                formData.Add(new StringContent(settings.Players.Value.ToString()), "cs2_settings.players");
            }
            if (settings.Tickrate.HasValue)
            {
                formData.Add(new StringContent(settings.Tickrate.Value.ToString()), "cs2_settings.tickrate");
            }
            if (!string.IsNullOrEmpty(settings.Config))
            {
                formData.Add(new StringContent(settings.Config), "cs2_settings.config");
            }
        }

        ///// <summary>
        ///// Add CSGO settings to form data
        ///// </summary>
        ///// <param name="formData">Form data</param>
        ///// <param name="settings">CSGO settings</param>
        //private static void AddCsgoSettingsToForm(MultipartFormDataContent formData, CsgoSettings settings)
        //{
        //    formData.Add(new StringContent(settings.Rcon), "csgo_settings.rcon");
        //    formData.Add(new StringContent(settings.SteamGameServerLoginToken), "csgo_settings.steam_game_server_login_token");
        //    formData.Add(new StringContent(settings.MapName), "csgo_settings.mapname");
        //    formData.Add(new StringContent(settings.GameMode), "csgo_settings.game_mode");
        //    formData.Add(new StringContent(settings.Players.ToString()), "csgo_settings.players");
        //    formData.Add(new StringContent(settings.Tickrate.ToString()), "csgo_settings.tickrate");
        //    formData.Add(new StringContent(settings.Autostart.ToString().ToLower()), "csgo_settings.autostart");
        //    formData.Add(new StringContent(settings.Autostop.ToString().ToLower()), "csgo_settings.autostop");
        //    formData.Add(new StringContent(settings.AutostopMinutes.ToString()), "csgo_settings.autostop_minutes");

        //    if (!string.IsNullOrEmpty(settings.Config))
        //    {
        //        formData.Add(new StringContent(settings.Config), "csgo_settings.config");
        //    }
        //}
    }
}