using System.Text.RegularExpressions;

namespace GGHubApi.Services
{
    /// <summary>
    /// Результат аналізу пінгу гравця
    /// </summary>
    public class PingAnalysisResult
    {
        public bool Success { get; set; }
        public int? AveragePing { get; set; }
        public float? PacketLossIn { get; set; }
        public float? PacketLossOut { get; set; }
        public bool IsPingAcceptable { get; set; }
        public string? ErrorMessage { get; set; }
        public List<int> PingSamples { get; set; } = new();
    }

    public interface IPingAnalysisService
    {
        Task<PingAnalysisResult> AnalyzePlayerPingAsync(
            string serverId,
            string playerName,
            string steamId,
            CancellationToken ct = default);
    }

    public class PingAnalysisService : IPingAnalysisService
    {
        private readonly IDathostService _dathostService;
        private readonly ILogger<PingAnalysisService> _logger;

        // Граничні значення
        private const int MAX_ACCEPTABLE_PING = 150;
        private const float MAX_ACCEPTABLE_PACKET_LOSS = 5.0f;

        // Regex для парсингу tick-логів CS2
        // Формат: ['PlayerName' ... ping=49ms, loss in/out = 0.37%/0.37%]
        private static readonly Regex PingFromTickLogRegex = new Regex(
            @"\['(?<playerName>[^']+)'[^\]]*ping=(?<ping>\d+)ms,\s*loss\s+in/out\s*=\s*(?<lossIn>[\d.]+)%/(?<lossOut>[\d.]+)%",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Резервний regex для команди status
        // Формат: # 2 "PlayerName" STEAM_1:0:123456 05:23 45 0 active
        private static readonly Regex StatusOutputRegex = new Regex(
            @"#\s+\d+\s+""(?<name>[^""]+)""\s+\S+\s+[\d:]+\s+(?<ping>\d+)\s+(?<loss>\d+)\s+\w+",
            RegexOptions.Compiled);

        public PingAnalysisService(IDathostService dathostService, ILogger<PingAnalysisService> logger)
        {
            _dathostService = dathostService;
            _logger = logger;
        }

        public async Task<PingAnalysisResult> AnalyzePlayerPingAsync(
            string serverId,
            string playerName,
            string steamId,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Analyzing ping for player {PlayerName} on server {ServerId}", playerName, serverId);

                // 1. Отримати консольні логи
                var consoleOutput = await _dathostService.GetServerConsoleAsync(serverId, maxLines: 1000, ct);

                // 2. Парсити пінг з tick-логів
                var pingData = ParsePingFromLogs(consoleOutput, playerName);

                if (pingData.Count == 0)
                {
                    _logger.LogWarning("No tick logs found, trying status command for {PlayerName}", playerName);

                    // Резервний метод: команда status
                    await _dathostService.SendConsoleCommandAsync(serverId, "status", ct);
                    await Task.Delay(3000, ct); // Чекаємо виконання

                    consoleOutput = await _dathostService.GetServerConsoleAsync(serverId, maxLines: 100, ct);
                    pingData = ParsePingFromStatusCommand(consoleOutput, playerName);
                }

                if (pingData.Count == 0)
                {
                    _logger.LogWarning("Could not retrieve ping data for {PlayerName}", playerName);
                    return new PingAnalysisResult
                    {
                        Success = false,
                        IsPingAcceptable = true, // Benefit of the doubt
                        ErrorMessage = "Не вдалося отримати дані про пінг"
                    };
                }

                // 3. Розрахувати середній пінг
                var avgPing = (int)pingData.Average(p => p.Ping);
                var avgLossIn = pingData.Average(p => p.PacketLossIn);
                var avgLossOut = pingData.Average(p => p.PacketLossOut);

                _logger.LogInformation(
                    "Ping analysis for {PlayerName}: avg={AvgPing}ms, loss in={LossIn}%, out={LossOut}%",
                    playerName, avgPing, avgLossIn, avgLossOut);

                // 4. Визначити чи пінг прийнятний
                bool isAcceptable = avgPing <= MAX_ACCEPTABLE_PING
                                 && avgLossIn <= MAX_ACCEPTABLE_PACKET_LOSS
                                 && avgLossOut <= MAX_ACCEPTABLE_PACKET_LOSS;

                return new PingAnalysisResult
                {
                    Success = true,
                    AveragePing = avgPing,
                    PacketLossIn = avgLossIn,
                    PacketLossOut = avgLossOut,
                    IsPingAcceptable = isAcceptable,
                    PingSamples = pingData.Select(p => p.Ping).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing ping for player {PlayerName}", playerName);
                return new PingAnalysisResult
                {
                    Success = false,
                    IsPingAcceptable = true, // Benefit of the doubt при помилках
                    ErrorMessage = ex.Message
                };
            }
        }

        private List<PingData> ParsePingFromLogs(string consoleLog, string playerName)
        {
            var results = new List<PingData>();
            var matches = PingFromTickLogRegex.Matches(consoleLog);

            foreach (Match match in matches)
            {
                if (match.Groups["playerName"].Value == playerName)
                {
                    results.Add(new PingData
                    {
                        Ping = int.Parse(match.Groups["ping"].Value),
                        PacketLossIn = float.Parse(match.Groups["lossIn"].Value),
                        PacketLossOut = float.Parse(match.Groups["lossOut"].Value)
                    });
                }
            }

            return results;
        }

        private List<PingData> ParsePingFromStatusCommand(string consoleLog, string playerName)
        {
            var results = new List<PingData>();
            var matches = StatusOutputRegex.Matches(consoleLog);

            foreach (Match match in matches)
            {
                if (match.Groups["name"].Value == playerName)
                {
                    var loss = float.Parse(match.Groups["loss"].Value);
                    results.Add(new PingData
                    {
                        Ping = int.Parse(match.Groups["ping"].Value),
                        PacketLossIn = loss,
                        PacketLossOut = loss
                    });
                }
            }

            return results;
        }

        private class PingData
        {
            public int Ping { get; set; }
            public float PacketLossIn { get; set; }
            public float PacketLossOut { get; set; }
        }
    }
}
