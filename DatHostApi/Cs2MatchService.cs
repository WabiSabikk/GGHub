using DatHost.Api.Client;
using DatHostApi.Models;

namespace DatHostApi
{
    /// <summary>
    /// Interface for CS2 match management operations
    /// </summary>
    public interface ICs2MatchService
    {
        /// <summary>
        /// Get specific CS2 match by ID
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>CS2 match information</returns>
        Task<Cs2Match> GetCs2MatchAsync(string matchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Start a new CS2 match
        /// </summary>
        /// <param name="request">Create match request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created CS2 match</returns>
        Task<Cs2Match> StartCs2MatchAsync(CreateCs2MatchRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel a CS2 match in progress
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task CancelCs2MatchAsync(string matchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add a player to a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="request">Add player request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        Task<Cs2Match> AddPlayerToCs2MatchAsync(string matchId, AddPlayerToMatchRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove a player from a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="steamId">Player Steam ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        Task<Cs2Match> RemovePlayerFromCs2MatchAsync(string matchId, string steamId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ready a player in a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="steamId">Player Steam ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        Task<Cs2Match> ReadyPlayerInCs2MatchAsync(string matchId, string steamId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unready a player in a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="steamId">Player Steam ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        Task<Cs2Match> UnreadyPlayerInCs2MatchAsync(string matchId, string steamId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Start knife round in a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        Task<Cs2Match> StartKnifeRoundAsync(string matchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pause a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        Task<Cs2Match> PauseCs2MatchAsync(string matchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unpause a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        Task<Cs2Match> UnpauseCs2MatchAsync(string matchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reset a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        Task<Cs2Match> ResetCs2MatchAsync(string matchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get match statistics
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Match statistics</returns>
        Task<Cs2Match> GetMatchStatisticsAsync(string matchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get match demo download URL
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Demo download URL</returns>
        Task<string> GetMatchDemoUrlAsync(string matchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get match GOTV URL
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>GOTV URL</returns>
        Task<string> GetMatchGotvUrlAsync(string matchId, CancellationToken cancellationToken = default);
    }
    /// <summary>
    /// Service for managing CS2 matches through DatHost API
    /// </summary>
    public class Cs2MatchService : ICs2MatchService
    {
        private readonly IDatHostApiClient _apiClient;

        public Cs2MatchService(IDatHostApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        /// <summary>
        /// Get specific CS2 match by ID
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>CS2 match information</returns>
        public async Task<Cs2Match> GetCs2MatchAsync(string matchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));

            return await _apiClient.GetAsync<Cs2Match>($"cs2-matches/{matchId}", cancellationToken);
        }

        /// <summary>
        /// Start a new CS2 match
        /// </summary>
        /// <param name="request">Create match request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created CS2 match</returns>
        public async Task<Cs2Match> StartCs2MatchAsync(CreateCs2MatchRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await _apiClient.PostAsync<Cs2Match>("cs2-matches", request, cancellationToken);
        }

        /// <summary>
        /// Cancel a CS2 match in progress
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task CancelCs2MatchAsync(string matchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));

            await _apiClient.PostAsync($"cs2-matches/{matchId}/cancel", null, cancellationToken);
        }

        /// <summary>
        /// Add a player to a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="request">Add player request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        public async Task<Cs2Match> AddPlayerToCs2MatchAsync(string matchId, AddPlayerToMatchRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return await _apiClient.PostAsync<Cs2Match>($"cs2-matches/{matchId}/players", request, cancellationToken);
        }

        /// <summary>
        /// Remove a player from a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="steamId">Player Steam ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        public async Task<Cs2Match> RemovePlayerFromCs2MatchAsync(string matchId, string steamId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));
            if (string.IsNullOrEmpty(steamId))
                throw new ArgumentException("Steam ID cannot be null or empty", nameof(steamId));

            return await _apiClient.PostAsync<Cs2Match>($"cs2-matches/{matchId}/players/{steamId}/remove", null, cancellationToken);
        }

        /// <summary>
        /// Ready a player in a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="steamId">Player Steam ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        public async Task<Cs2Match> ReadyPlayerInCs2MatchAsync(string matchId, string steamId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));
            if (string.IsNullOrEmpty(steamId))
                throw new ArgumentException("Steam ID cannot be null or empty", nameof(steamId));

            return await _apiClient.PostAsync<Cs2Match>($"cs2-matches/{matchId}/players/{steamId}/ready", null, cancellationToken);
        }

        /// <summary>
        /// Unready a player in a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="steamId">Player Steam ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        public async Task<Cs2Match> UnreadyPlayerInCs2MatchAsync(string matchId, string steamId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));
            if (string.IsNullOrEmpty(steamId))
                throw new ArgumentException("Steam ID cannot be null or empty", nameof(steamId));

            return await _apiClient.PostAsync<Cs2Match>($"cs2-matches/{matchId}/players/{steamId}/unready", null, cancellationToken);
        }

        /// <summary>
        /// Start knife round in a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        public async Task<Cs2Match> StartKnifeRoundAsync(string matchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));

            return await _apiClient.PostAsync<Cs2Match>($"cs2-matches/{matchId}/knife", null, cancellationToken);
        }

        /// <summary>
        /// Pause a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        public async Task<Cs2Match> PauseCs2MatchAsync(string matchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));

            return await _apiClient.PostAsync<Cs2Match>($"cs2-matches/{matchId}/pause", null, cancellationToken);
        }

        /// <summary>
        /// Unpause a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        public async Task<Cs2Match> UnpauseCs2MatchAsync(string matchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));

            return await _apiClient.PostAsync<Cs2Match>($"cs2-matches/{matchId}/unpause", null, cancellationToken);
        }

        /// <summary>
        /// Reset a CS2 match
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated CS2 match</returns>
        public async Task<Cs2Match> ResetCs2MatchAsync(string matchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));

            return await _apiClient.PostAsync<Cs2Match>($"cs2-matches/{matchId}/reset", null, cancellationToken);
        }

        /// <summary>
        /// Get match statistics
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Match statistics</returns>
        public async Task<Cs2Match> GetMatchStatisticsAsync(string matchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));

            return await _apiClient.GetAsync<Cs2Match>($"cs2-matches/{matchId}/stats", cancellationToken);
        }

        /// <summary>
        /// Get match demo download URL
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Demo download URL</returns>
        public async Task<string> GetMatchDemoUrlAsync(string matchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));

            var match = await _apiClient.GetAsync<Cs2Match>($"cs2-matches/{matchId}", cancellationToken);

            return match.Playback?.DemoUrl ?? throw new InvalidOperationException("Demo URL not available for this match");
        }

        /// <summary>
        /// Get match GOTV URL
        /// </summary>
        /// <param name="matchId">Match ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>GOTV URL</returns>
        public async Task<string> GetMatchGotvUrlAsync(string matchId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(matchId))
                throw new ArgumentException("Match ID cannot be null or empty", nameof(matchId));

            var match = await _apiClient.GetAsync<Cs2Match>($"cs2-matches/{matchId}", cancellationToken);

            return match.Playback?.GotvUrl ?? throw new InvalidOperationException("GOTV URL not available for this match");
        }
    }
}
