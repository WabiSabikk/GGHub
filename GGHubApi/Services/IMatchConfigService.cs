using GGHubDb.Models;
using GGHubDb.Repos;
using GGHubShared.Enums;
using GGHubShared.Models;

namespace GGHubApi.Services
{
    public interface IMatchConfigService
    {
        Task<DuelFormatConfig> GetDuelConfigAsync(DuelFormat format, CancellationToken cancellationToken = default);
        Task<string> GenerateServerConfigAsync(
            DuelFormat format,
            bool primeOnly,
            int? customWarmupMinutes = null,
            int? customTickrate = null,
            int? customMaxRounds = null,
            bool? customOvertimeEnabled = null,
            CancellationToken cancellationToken = default);
        Task<ApiResponse<List<DuelFormatConfig>>> GetAvailableFormatsAsync(CancellationToken cancellationToken = default);
        Task<ApiResponse<bool>> ValidateCustomSettingsAsync(DuelFormat format, CreateDuelRequest request, CancellationToken cancellationToken = default);
    
    }

    public class MatchConfigService : IMatchConfigService
    {
        private readonly IMatchConfigRepository _configRepository;
        private readonly ILogger<MatchConfigService> _logger;

        public MatchConfigService(IMatchConfigRepository configRepository, ILogger<MatchConfigService> logger)
        {
            _configRepository = configRepository;
            _logger = logger;
        }

        public async Task<DuelFormatConfig> GetDuelConfigAsync(DuelFormat format, CancellationToken cancellationToken = default)
        {
            var config = await _configRepository.GetByFormatAsync(format, cancellationToken);
            if (config == null)
            {
                _logger.LogWarning("No configuration found for format: {Format}", format);
                return new DuelFormatConfig { Format = format };
            }

            return config;
        }

        public async Task<string> GenerateServerConfigAsync(
            DuelFormat format,
            bool primeOnly,
            int? customWarmupMinutes = null,
            int? customTickrate = null,
            int? customMaxRounds = null,
            bool? customOvertimeEnabled = null,
            CancellationToken cancellationToken = default)
        {
            var config = await GetDuelConfigAsync(format, cancellationToken);

            var warmupMinutes = customWarmupMinutes ?? config.DefaultWarmupMinutes;
            var tickrate = customTickrate ?? config.DefaultTickrate;
            var maxRounds = customMaxRounds ?? config.DefaultMaxRounds;
            var overtimeEnabled = customOvertimeEnabled ?? config.DefaultOvertimeEnabled;

            var serverConfig = "mp_warmup_start 1\n" +                        // Запускає розминку автоматично
                               "mp_warmup_pausetimer 0\n" +                   // Дозволяє таймеру розминки йти нормально (0 = таймер працює)
                               $"mp_warmuptime {warmupMinutes * 60}\n" +      // Встановлює час розминки в секундах
                               $"mp_team_timeout_time {config.TeamTimeoutTime}\n" +  // Час тайм-ауту команди
                               $"mp_freezetime {config.FreezeTime}\n" +       // Час заморозки на початку раунду для покупок
                               $"mp_round_restart_delay {config.RoundRestartDelay}\n" +  // Затримка перед перезапуском раунду
                               $"mp_maxrounds {maxRounds}\n" +                // Максимальна кількість раундів у матчі
                               $"mp_overtime_enable {(overtimeEnabled ? 1 : 0)}\n" +  // Увімкнути/вимкнути овертайм
                               $"mp_overtime_maxrounds {config.OvertimeMaxRounds}\n" +  // Максимальна кількість раундів в овертаймі
                               $"mp_overtime_startmoney {config.OvertimeStartMoney}\n" +  // Стартові гроші в овертаймі
                               "sv_hibernate_when_empty 0\n" +                // Вимикає сплячий режим сервера при відсутності гравців
                               "mp_force_pick_time 0\n" +                     // Автоматичне призначення команди (0 = без вибору)
                               "mp_spectators_max 0\n" +                      // Заборонити спостерігачів (0 = максимум 0 спостерігачів)
                               "mp_autoteambalance 1\n" +                     // Автоматичне балансування команд у кінці раунду
                               "mp_limitteams 1";                             // Обмежує різницю гравців між командами до 1

            if (primeOnly)
            {
                serverConfig += "\nsv_prime_accounts_only 1";
            }


            if (format == DuelFormat.OneVsOne)
            {
                serverConfig += "\nmp_teammates_are_enemies 1"; 
            }

            if (!string.IsNullOrEmpty(config.CustomConfig))
            {
                serverConfig += $"\n{config.CustomConfig}";
            }

            return serverConfig;
        }

        public async Task<ApiResponse<List<DuelFormatConfig>>> GetAvailableFormatsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var configs = await _configRepository.GetEnabledFormatsAsync(cancellationToken);
                return new ApiResponse<List<DuelFormatConfig>> { Success = true, Data = configs };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available formats");
                return new ApiResponse<List<DuelFormatConfig>> { Success = false, Code = ErrorCode.ServerError, Errors = { ex.Message } };
            }
        }

        public async Task<ApiResponse<bool>> ValidateCustomSettingsAsync(DuelFormat format, CreateDuelRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var config = await GetDuelConfigAsync(format, cancellationToken);

                if (request.CustomTickrate.HasValue && !config.AllowCustomTickrate)
                {
                    return new ApiResponse<bool> { Success = false, Code = ErrorCode.ValidationFailed, Message = "Custom tickrate not allowed for this format" };
                }

                if (request.CustomMaxRounds.HasValue && !config.AllowCustomRounds)
                {
                    return new ApiResponse<bool> { Success = false, Code = ErrorCode.ValidationFailed, Message = "Custom rounds not allowed for this format" };
                }

                if (request.CustomTickrate.HasValue)
                {
                    var allowed = config.AllowedTickrates.Split(',').Select(int.Parse).ToArray();
                    if (!allowed.Contains(request.CustomTickrate.Value))
                    {
                        return new ApiResponse<bool> { Success = false, Code = ErrorCode.ValidationFailed, Message = $"Tickrate must be one of: {string.Join(", ", allowed)}" };
                    }
                }

                if (request.CustomMaxRounds.HasValue)
                {
                    if (request.CustomMaxRounds.Value < config.MinRounds || request.CustomMaxRounds.Value > config.MaxRounds)
                    {
                        return new ApiResponse<bool> { Success = false, Code = ErrorCode.ValidationFailed, Message = $"Rounds must be between {config.MinRounds} and {config.MaxRounds}" };
                    }
                }

                return new ApiResponse<bool> { Success = true, Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating custom settings");
                return new ApiResponse<bool> { Success = false, Code = ErrorCode.ServerError, Errors = { ex.Message } };
            }
        }

       
    }
}
