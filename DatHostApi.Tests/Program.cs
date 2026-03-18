using DatHost.Api.Client;
using DatHost.Models;
using DatHostApi;
using DatHostApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using static DatHost.Api.Client.IDatHostApiClient;

namespace DatHostApiTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Налаштування кодування консолі для українських символів
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Встановлення заголовка консолі
            Console.Title = "🎮 CS2 Server Manager | DatHost API";

            try
            {
                PrintHeader();

                var serverConfig = BuildServerConfig();

                LogInfo("🔧 Налаштування API клієнта...");
                var options = new DatHostApiOptions
                {
                    BaseUrl = "https://dathost.net/api/0.1/",
                    Email = "your-dathost-email@example.com",
                    Password = "YOUR_DATHOST_PASSWORD",
                    TimeoutSeconds = 30
                };

                LogInfo("⚙️ Ініціалізація сервісів...");
                var services = new ServiceCollection();

                var configurationBuilder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddEnvironmentVariables();
                var configuration = configurationBuilder.Build();

                services.AddSingleton<IConfiguration>(configuration);
                services.AddLogging(builder => builder.AddConsole());

                var serviceProvider = services.BuildServiceProvider();
                var _webhookUrl = configuration["DatHost:WebhookUrl"] ?? "https://yourdomain.com/api/webhook/dathost";
                var _webhookSecret = configuration["DatHost:WebhookSecret"] ?? "default_secret_key";

                LogInfo("🌐 Створення HTTP клієнта...");
                var httpClient = new HttpClient();
                var apiClient = new DatHostApiClient(httpClient, Options.Create(options));
                var gameServerService = new GameServerService(apiClient);
                var cs2MatchService = new Cs2MatchService(apiClient);

                LogInfo("📋 Підготовка конфігурації сервера...");
                var createRequest = new CreateGameServerRequest
                {
                    Name = "🔥 Test CS2 Duel Server",
                    Game = "cs2",
                    Autostop = true,
                    AutostopMinutes = 30,
                    Location = "stockholm",
                    Cs2Settings = new Cs2Settings
                    {
                        Slots = 5,
                        MapgroupStartMap = "de_inferno",
                        GameMode = "competitive",
                        Tickrate = 128,
                        Rcon = "password",
                        MapsSource = "mapgroup",
                        Players = 2,
                        Config = serverConfig
                    }
                };

                LogProcess("🚀 Створюємо новий CS2 сервер...");

                // Створення сервера
                var createdServer = await gameServerService.CreateGameServerAsync(createRequest);

                LogSuccess("✅ Сервер успішно створено!");
                PrintServerInfo(createdServer);

                // Запуск сервера
                if (!createdServer.On)
                {
                    LogProcess("⚡ Запускаємо сервер...");
                    await gameServerService.StartGameServerAsync(createdServer.Id);
                    LogSuccess("✅ Сервер успішно запущено!");
                }

                LogInfo("👥 Підготовка списку гравців...");
                var players = CreatePlayersList();
                PrintPlayersInfo(players);

                LogProcess("🎯 Створюємо матч...");
                var cs2MatchRequest = new CreateCs2MatchRequest
                {
                    GameServerId = createdServer.Id,
                    Players = players,
                    Webhooks = new WebhookSettings
                    {
                        EventUrl = _webhookUrl,
                        EnabledEvents = new List<string>
                        {
                            "server_ready_for_players",
                            "match_started",
                            "player_connected",
                            "player_disconnected",
                            "match_ended",
                            "round_end"
                        },
                        AuthorizationHeader = "Bearer " + _webhookSecret
                    }
                };

                var cs2Match = await cs2MatchService.StartCs2MatchAsync(cs2MatchRequest);
                LogSuccess("🎉 Матч успішно створено та запущено!");

                LogInfo($"🔗 Connect команда: connect {createdServer.Ip}:{createdServer.Ports.Game}");
            }
            catch (DatHostApiException ex)
            {
                LogError($"❌ Помилка DatHost API: {ex.Message}");
                LogError($"📊 Статус код: {ex.StatusCode}");
                if (!string.IsNullOrEmpty(ex.ResponseContent))
                {
                    LogError($"📄 Відповідь сервера: {ex.ResponseContent}");
                }
            }
            catch (Exception ex)
            {
                LogError($"💥 Критична помилка: {ex.Message}");
                LogError($"🔍 Деталі: {ex.StackTrace}");
            }

            Console.WriteLine();
            LogInfo("🔚 Натисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }

        private static string BuildServerConfig()
        {
            var serverConfig = "mp_warmup_start 1\n" +                        // Запускає розминку автоматично
                           "mp_warmup_pausetimer 0\n" +                   // Дозволяє таймеру розминки йти нормально (0 = таймер працює)
                           $"mp_warmuptime {2 * 60}\n" +      // Встановлює час розминки в секундах
                           $"mp_team_timeout_time {5}\n" +  // Час тайм-ауту команди
                           $"mp_freezetime {5}\n" +       // Час заморозки на початку раунду для покупок
                           $"mp_round_restart_delay {5}\n" +  // Затримка перед перезапуском раунду
                           $"mp_maxrounds {3}\n" +                // Максимальна кількість раундів у матчі
                           $"mp_overtime_enable {0}\n" +  // Увімкнути/вимкнути овертайм
                           $"mp_overtime_maxrounds {1}\n" +  // Максимальна кількість раундів в овертаймі
                           $"mp_overtime_startmoney {1000}\n" +  // Стартові гроші в овертаймі
                           "sv_hibernate_when_empty 0\n" +                // Вимикає сплячий режим сервера при відсутності гравців
                           "mp_force_pick_time 0\n" +                     // Автоматичне призначення команди (0 = без вибору)
                           "mp_spectators_max 0\n" +                      // Заборонити спостерігачів (0 = максимум 0 спостерігачів)
                           "mp_autoteambalance 1\n" +                     // Автоматичне балансування команд у кінці раунду
                           "mp_limitteams 1";         // Закупка всюди

            return serverConfig;
        }

        private static List<CreateMatchPlayer> CreatePlayersList()
        {
            var players = new List<CreateMatchPlayer>
            {
                new CreateMatchPlayer
                {
                    SteamId64 = "76561198448976345",
                    Team = "team1",
                    NicknameOverride = "Player1"
                },
                new CreateMatchPlayer
                {
                    SteamId64 = "76561199765649710",
                    Team = "team2",
                    NicknameOverride = "imagegen123"
                }
            };

            return players;
        }

        // Методи для стилізованого логування
        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    🎮 CS2 Server Manager                     ║");
            Console.WriteLine("║                      DatHost API Client                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void LogSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            Console.ResetColor();
        }

        private static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            Console.ResetColor();
        }

        private static void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            Console.ResetColor();
        }

        private static void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            Console.ResetColor();
        }

        private static void LogProcess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            Console.ResetColor();
        }

        private static void PrintServerInfo(GameServerResponse server)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════ ІНФОРМАЦІЯ ПРО СЕРВЕР ══════════════════════╗");
            Console.WriteLine($"║ 🆔 ID сервера:    {server.Id,-44} ║");
            Console.WriteLine($"║ 📝 Назва:         {server.Name,-44} ║");
            Console.WriteLine($"║ 📊 Статус:        {string.Join(", ", server.Status),-44} ║");
            Console.WriteLine($"║ 🌍 IP адреса:     {server.Ip,-44} ║");
            Console.WriteLine($"║ 🎮 Порт гри:      {server.Ports.Game,-44} ║");
            Console.WriteLine($"║ 📺 GOTV порт:     {server.Ports.Gotv,-44} ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        private static void PrintPlayersInfo(List<CreateMatchPlayer> players)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("╔══════════════════════ ГРАВЦІ ═══════════════════════════════╗");
            foreach (var player in players)
            {
                Console.WriteLine($"║ 👤 {player.NicknameOverride,-15} | Команда: {player.Team,-8} ║");
                Console.WriteLine($"║    SteamID: {player.SteamId64,-43} ║");
            }
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}