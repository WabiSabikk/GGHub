using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace NgrokSetup;

class Program
{
    private static string? BotAppsettingsPath;
    private static string? ApiAppsettingsPath;

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    NgrokSetup v2.0                         ║");
        Console.WriteLine("║              Автоматичне налаштування ngrok                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        if (!InitializePaths())
        {
            PrintError("Не вдалося знайти шляхи до проектів. Переконайтеся, що запускаєте з директорії GGHub.");
            WaitForExit();
            return;
        }

        if (!IsDebugMode())
        {
            PrintWarning("Не запущено в Debug режимі. Пропускаємо налаштування ngrok.");
            WaitForExit();
            return;
        }

        if (await IsNgrokRunning())
        {
            PrintInfo("Ngrok вже запущений. Перевіряємо існуючі тунелі...");

            var existingDomains = await GetExistingDomains();

            if (existingDomains.Count >= 2 && existingDomains.ContainsKey("5000") && existingDomains.ContainsKey("5276"))
            {
                PrintSuccess("Знайдено валідні існуючі тунелі. Використовуємо їх...");

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\nВикористовуємо існуючі {existingDomains.Count} домени:");
                foreach (var domain in existingDomains)
                {
                    Console.WriteLine($"  🌐 Порт {domain.Key}: {domain.Value}");
                }
                Console.ResetColor();

                await UpdateBotAppsettings(existingDomains);
                await UpdateApiAppsettings(existingDomains);

                PrintSuccess("Налаштування ngrok завершено успішно!");
                PrintInfo($"Bot домен (порт 5000): {existingDomains["5000"]}");
                PrintInfo($"API домен (порт 5276): {existingDomains["5276"]}");
                WaitForExit();
                return;
            }
            else
            {
                PrintError($"Існуючі тунелі невалідні (знайдено {existingDomains.Count}, потрібні порти 5000 та 5276).");
                PrintWarning("Перезапускаємо ngrok...");
                await StopNgrok();

                Console.Write("Очікуємо зупинення ngrok");
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    if (!await IsNgrokRunning())
                    {
                        PrintSuccess("\nNgrok зупинено успішно.");
                        break;
                    }
                    Console.Write(".");
                    if (i == 9)
                    {
                        PrintWarning("\nПопередження: Ngrok можливо все ще працює.");
                    }
                }
            }
        }

        try
        {
            var domains = await StartNgrokAndGetDomains();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\nОтримано {domains.Count} доменів:");
            foreach (var domain in domains)
            {
                Console.WriteLine($"  🌐 Порт {domain.Key}: {domain.Value}");
            }
            Console.ResetColor();

            if (domains.Count < 2)
            {
                PrintError("Не вдалося отримати обидва домени з виводу ngrok.");
                WaitForExit();
                return;
            }

            await UpdateBotAppsettings(domains);
            await UpdateApiAppsettings(domains);

            Console.WriteLine();
            PrintSuccess("Налаштування ngrok завершено успішно!");
            PrintInfo($"Bot домен (порт 5000): {domains.GetValueOrDefault("5000", "Не знайдено")}");
            PrintInfo($"API домен (порт 5276): {domains.GetValueOrDefault("5276", "Не знайдено")}");
        }
        catch (Exception ex)
        {
            PrintError($"Помилка при налаштуванні ngrok: {ex.Message}");
        }

        WaitForExit();
    }

    private static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ {message}");
        Console.ResetColor();
    }

    private static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ {message}");
        Console.ResetColor();
    }

    private static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️ {message}");
        Console.ResetColor();
    }

    private static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"ℹ️ {message}");
        Console.ResetColor();
    }

    private static void WaitForExit()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
        Console.ResetColor();
        Console.ReadKey();
    }

    private static bool InitializePaths()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var searchDir = new DirectoryInfo(currentDir);

        PrintInfo($"Початок пошуку з: {currentDir}");

        while (searchDir != null)
        {
            var botDir = Path.Combine(searchDir.FullName, "GGHubBot");
            var apiDir = Path.Combine(searchDir.FullName, "GGHubApi");

            Console.WriteLine($"🔍 Перевіряємо: {searchDir.FullName}");

            if (Directory.Exists(botDir) && Directory.Exists(apiDir))
            {
                BotAppsettingsPath = Path.Combine(botDir, "appsettings.json");
                ApiAppsettingsPath = Path.Combine(apiDir, "appsettings.json");

                PrintSuccess($"Знайдено корінь рішення: {searchDir.FullName}");
                Console.WriteLine($"📄 Bot appsettings: {BotAppsettingsPath}");
                Console.WriteLine($"📄 API appsettings: {ApiAppsettingsPath}");

                var botExists = File.Exists(BotAppsettingsPath);
                var apiExists = File.Exists(ApiAppsettingsPath);

                Console.WriteLine($"Bot файл існує: {(botExists ? "✅" : "❌")}");
                Console.WriteLine($"API файл існує: {(apiExists ? "✅" : "❌")}");

                return botExists && apiExists;
            }

            searchDir = searchDir.Parent;
        }

        PrintError("Не вдалося знайти директорію рішення GGHub з папками GGHubBot та GGHubApi.");
        return false;
    }

    private static bool IsDebugMode()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    private static async Task<bool> IsNgrokRunning()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = await client.GetAsync("http://127.0.0.1:4040/api/tunnels");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<Dictionary<string, string>> GetExistingDomains()
    {
        var domains = new Dictionary<string, string>();

        try
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync("http://127.0.0.1:4040/api/tunnels");
            var tunnelData = JsonConvert.DeserializeObject<JObject>(response);

            var tunnels = tunnelData?["tunnels"] as JArray;

            if (tunnels != null)
            {
                foreach (var tunnel in tunnels)
                {
                    var publicUrl = tunnel["public_url"]?.ToString();
                    var configName = tunnel["config"]?["addr"]?.ToString();

                    if (publicUrl != null && configName != null)
                    {
                        var portMatch = Regex.Match(configName, @":(\d+)");
                        if (portMatch.Success)
                        {
                            var port = portMatch.Groups[1].Value;
                            domains[port] = publicUrl;
                            Console.WriteLine($"🔗 Знайдено існуючий тунель: {publicUrl} -> {configName}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            PrintError($"Помилка отримання інформації про існуючі тунелі: {ex.Message}");
        }

        return domains;
    }

    private static async Task StopNgrok()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🔄 Зупиняємо існуючу сесію ngrok...");
        Console.ResetColor();

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var tunnelsResponse = await client.GetStringAsync("http://127.0.0.1:4040/api/tunnels");
            var tunnelData = JsonConvert.DeserializeObject<JObject>(tunnelsResponse);
            var tunnels = tunnelData?["tunnels"] as JArray;

            if (tunnels != null && tunnels.Count > 0)
            {
                Console.WriteLine($"Знайдено {tunnels.Count} активних тунелів. Закриваємо їх...");

                foreach (var tunnel in tunnels)
                {
                    var tunnelName = tunnel["name"]?.ToString();
                    if (!string.IsNullOrEmpty(tunnelName))
                    {
                        try
                        {
                            await client.DeleteAsync($"http://127.0.0.1:4040/api/tunnels/{tunnelName}");
                            PrintSuccess($"Зупинено тунель: {tunnelName}");
                        }
                        catch (Exception ex)
                        {
                            PrintError($"Не вдалося зупинити тунель {tunnelName}: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Не знайдено активних тунелів через API.");
            }
        }
        catch (Exception apiEx)
        {
            PrintWarning($"API метод не спрацював: {apiEx.Message}");
        }

        try
        {
            var ngrokProcesses = Process.GetProcessesByName("ngrok");
            if (ngrokProcesses.Length > 0)
            {
                Console.WriteLine($"Знайдено {ngrokProcesses.Length} процесів ngrok. Завершуємо...");

                foreach (var process in ngrokProcesses)
                {
                    try
                    {
                        Console.WriteLine($"  🔄 Завершуємо процес ngrok {process.Id}...");
                        process.Kill();
                        var exited = process.WaitForExit(5000);
                        if (exited)
                        {
                            PrintSuccess($"Процес {process.Id} завершено успішно.");
                        }
                        else
                        {
                            PrintWarning($"Процес {process.Id} не завершився вчасно.");
                        }
                    }
                    catch (Exception ex)
                    {
                        PrintError($"Не вдалося завершити процес {process.Id}: {ex.Message}");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            else
            {
                Console.WriteLine("Процеси ngrok не знайдено.");
            }
        }
        catch (Exception processEx)
        {
            PrintError($"Завершення процесу не вдалося: {processEx.Message}");
        }
    }

    private static async Task<Dictionary<string, string>> StartNgrokAndGetDomains()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("🚀 Запускаємо ngrok...");
        Console.ResetColor();

        var processInfo = new ProcessStartInfo
        {
            FileName = "ngrok",
            Arguments = "start --all",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);

        if (process == null)
        {
            throw new Exception("Не вдалося запустити процес ngrok");
        }

        Console.WriteLine("⏳ Очікуємо ініціалізації ngrok...");
        await Task.Delay(5000);

        var domains = new Dictionary<string, string>();

        try
        {
            using var client = new HttpClient();
            var response = await client.GetStringAsync("http://127.0.0.1:4040/api/tunnels");
            var tunnelData = JsonConvert.DeserializeObject<JObject>(response);

            var tunnels = tunnelData?["tunnels"] as JArray;

            if (tunnels != null)
            {
                foreach (var tunnel in tunnels)
                {
                    var publicUrl = tunnel["public_url"]?.ToString();
                    var configName = tunnel["config"]?["addr"]?.ToString();

                    if (publicUrl != null && configName != null)
                    {
                        var portMatch = Regex.Match(configName, @":(\d+)");
                        if (portMatch.Success)
                        {
                            var port = portMatch.Groups[1].Value;
                            domains[port] = publicUrl;
                            Console.WriteLine($"🔗 Знайдено тунель: {publicUrl} -> {configName}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            PrintError($"Помилка отримання інформації про тунель з API: {ex.Message}");
            domains = await ParseDomainsFromOutput(process);
        }

        if (domains.Count == 0)
        {
            PrintError("Тунелі не знайдено! Переконайтеся, що конфігурація ngrok правильна.");
        }

        return domains;
    }

    private static async Task<Dictionary<string, string>> ParseDomainsFromOutput(Process process)
    {
        var domains = new Dictionary<string, string>();
        var output = await process.StandardOutput.ReadToEndAsync();

        Console.WriteLine("📝 Парсинг доменів з виводу ngrok...");

        var forwardingPattern = @"Forwarding\s+(\S+)\s+->\s+http://localhost:(\d+)";
        var matches = Regex.Matches(output, forwardingPattern);

        foreach (Match match in matches)
        {
            if (match.Success)
            {
                var domain = match.Groups[1].Value;
                var port = match.Groups[2].Value;
                domains[port] = domain;
                Console.WriteLine($"🔗 Розпарсено: {domain} -> порт {port}");
            }
        }

        return domains;
    }

    private static async Task UpdateBotAppsettings(Dictionary<string, string> domains)
    {
        Console.WriteLine("📝 Оновлюємо bot appsettings.json...");

        if (!domains.ContainsKey("5000") || !domains.ContainsKey("5276"))
        {
            throw new Exception("Відсутні необхідні домени для конфігурації бота");
        }

        var botDomain = domains["5000"];
        var apiDomain = domains["5276"];

        var json = await File.ReadAllTextAsync(BotAppsettingsPath!, Encoding.UTF8);
        var config = JsonConvert.DeserializeObject<JObject>(json);

        if (config == null)
        {
            throw new Exception("Не вдалося розпарсити bot appsettings.json");
        }

        if (config["TelegramBot"] != null)
        {
            config["TelegramBot"]!["WebhookUrl"] = $"{botDomain}/webhook";
            PrintSuccess($"Оновлено TelegramBot WebhookUrl: {botDomain}/webhook");
        }

        if (config["GGHubApi"] != null)
        {
            config["GGHubApi"]!["BaseUrl"] = $"{apiDomain}/api/";
            PrintSuccess($"Оновлено GGHubApi BaseUrl: {apiDomain}/api/");
        }

        if (config["Steam"] != null)
        {
            config["Steam"]!["RedirectUrl"] = $"{botDomain}/steam/callback";
            PrintSuccess($"Оновлено Steam RedirectUrl: {botDomain}/steam/callback");
        }

        var updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
        await File.WriteAllTextAsync(BotAppsettingsPath!, updatedJson, Encoding.UTF8);

        PrintSuccess("Bot appsettings.json оновлено успішно");
    }

    private static async Task UpdateApiAppsettings(Dictionary<string, string> domains)
    {
        Console.WriteLine("📝 Оновлюємо API appsettings.json...");

        if (!domains.ContainsKey("5276"))
        {
            throw new Exception("Відсутній API домен для конфігурації");
        }

        var apiDomain = domains["5276"];

        var json = await File.ReadAllTextAsync(ApiAppsettingsPath!, Encoding.UTF8);
        var config = JsonConvert.DeserializeObject<JObject>(json);

        if (config == null)
        {
            throw new Exception("Не вдалося розпарсити API appsettings.json");
        }

        if (config["DatHost"] != null)
        {
            config["DatHost"]!["WebhookUrl"] = $"{apiDomain}/api/webhook/dathost";
            PrintSuccess($"Оновлено DatHost WebhookUrl: {apiDomain}/api/webhook/dathost");
        }
        else
        {
            PrintWarning("Секція DatHost не знайдена в API appsettings.json");
        }

        if (config["App"] != null)
        {
            config["App"]!["PublicBaseUrl"] = $"{apiDomain}/";
            PrintSuccess($"Оновлено App PublicBaseUrl: {apiDomain}/");
        }
        else
        {
            PrintWarning("Секція App не знайдена в API appsettings.json");
        }

        var updatedJson = JsonConvert.SerializeObject(config, Formatting.Indented);
        await File.WriteAllTextAsync(ApiAppsettingsPath!, updatedJson, Encoding.UTF8);

        PrintSuccess("API appsettings.json оновлено успішно");
    }
}