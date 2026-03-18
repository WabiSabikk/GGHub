using Microsoft.Extensions.Configuration;
using Serilog;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace GGHubBot.Services
{
    public class MediaService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly string _mediaPath;
        private readonly Dictionary<string, string> _mapImages;

        public MediaService(ITelegramBotClient botClient, IConfiguration configuration)
        {
            _botClient = botClient;
            _mediaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media");

            Directory.CreateDirectory(_mediaPath);
            Directory.CreateDirectory(Path.Combine(_mediaPath, "maps"));
            Directory.CreateDirectory(Path.Combine(_mediaPath, "generated"));

            _mapImages = new Dictionary<string, string>
            {
                ["de_dust2"] = "https://i.imgur.com/dust2_preview.jpg",
                ["de_mirage"] = "https://i.imgur.com/mirage_preview.jpg",
                ["de_inferno"] = "https://i.imgur.com/inferno_preview.jpg",
                ["de_cache"] = "https://i.imgur.com/cache_preview.jpg",
                ["de_overpass"] = "https://i.imgur.com/overpass_preview.jpg",
                ["de_train"] = "https://i.imgur.com/train_preview.jpg",
                ["de_nuke"] = "https://i.imgur.com/nuke_preview.jpg",
                ["de_vertigo"] = "https://i.imgur.com/vertigo_preview.jpg",
                ["de_ancient"] = "https://i.imgur.com/ancient_preview.jpg",
                ["de_anubis"] = "https://i.imgur.com/anubis_preview.jpg"
            };
        }

        public async Task<InputFile?> GetMapImageAsync(string mapName)
        {
            try
            {
                var mapImagePath = Path.Combine(_mediaPath, "maps", $"{mapName}.jpg");

                if (File.Exists(mapImagePath))
                {
                    return InputFile.FromStream(File.OpenRead(mapImagePath), $"{mapName}.jpg");
                }

                if (_mapImages.ContainsKey(mapName))
                {
                    using var httpClient = new HttpClient();
                    var imageBytes = await httpClient.GetByteArrayAsync(_mapImages[mapName]);
                    await File.WriteAllBytesAsync(mapImagePath, imageBytes);

                    return InputFile.FromStream(new MemoryStream(imageBytes), $"{mapName}.jpg");
                }

                return await GenerateMapPlaceholderAsync(mapName);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error getting map image for {MapName}", mapName);
                return null;
            }
        }

        public async Task<InputFile?> GenerateMapPlaceholderAsync(string mapName)
        {
            try
            {
                var bitmap = new Bitmap(400, 300);
                using var graphics = Graphics.FromImage(bitmap);

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.FromArgb(34, 34, 34));

                using var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 400, 300),
                    Color.FromArgb(64, 64, 64),
                    Color.FromArgb(24, 24, 24),
                    LinearGradientMode.Vertical);

                graphics.FillRectangle(brush, 0, 0, 400, 300);

                using var font = new Font("Arial", 24, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);

                var text = mapName.Replace("de_", "").ToUpperInvariant();
                var textSize = graphics.MeasureString(text, font);
                var x = (400 - textSize.Width) / 2;
                var y = (300 - textSize.Height) / 2;

                graphics.DrawString(text, font, textBrush, x, y);

                using var borderPen = new Pen(Color.Orange, 3);
                graphics.DrawRectangle(borderPen, 0, 0, 399, 299);

                var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Jpeg);
                stream.Position = 0;

                return InputFile.FromStream(stream, $"{mapName}_placeholder.jpg");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating map placeholder for {MapName}", mapName);
                return null;
            }
        }

        public async Task<InputFile?> GenerateUserStatsImageAsync(string username, int rating, int wins, int losses, decimal balance)
        {
            try
            {
                var bitmap = new Bitmap(600, 400);
                using var graphics = Graphics.FromImage(bitmap);

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(System.Drawing.Color.FromArgb(30, 30, 30));

                using var headerBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, 600, 80),
                    Color.FromArgb(255, 165, 0),
                    Color.FromArgb(255, 140, 0),
                    LinearGradientMode.Horizontal);

                graphics.FillRectangle(headerBrush, 0, 0, 600, 80);

                using var titleFont = new Font("Arial", 20, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);

                graphics.DrawString("CS2 Duels Profile", titleFont, textBrush, 20, 25);

                using var usernameFont = new Font("Arial", 16, FontStyle.Bold);
                graphics.DrawString(username, usernameFont, textBrush, 20, 100);

                var stats = new[]
                {
                ($"Rating: {rating}", 140),
                ($"Wins: {wins}", 180),
                ($"Losses: {losses}", 220),
                ($"Win Rate: {(wins + losses > 0 ? (wins * 100.0 / (wins + losses)):0):F1}%", 260),
                ($"Balance: €{balance:F2}", 300)
            };

                using var statsFont = new Font("Arial", 14);
                foreach (var (text, y) in stats)
                {
                    graphics.DrawString(text, statsFont, textBrush, 20, y);
                }

                if (wins + losses > 0)
                {
                    var winPercentage = wins * 100.0 / (wins + losses);
                    await DrawProgressBarAsync(graphics, 300, 180, 250, 20, winPercentage, "Win Rate");
                }

                var ratingColor = rating switch
                {
                    >= 1500 => Color.Gold,
                    >= 1200 => Color.Silver,
                    >= 1000 => Color.Orange,
                    _ => Color.Gray
                };

                using var ratingBrush = new SolidBrush(ratingColor);
                graphics.FillEllipse(ratingBrush, 500, 140, 20, 20);

                var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                return InputFile.FromStream(stream, "user_stats.png");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating user stats image for {Username}", username);
                return null;
            }
        }

        public async Task<InputFile?> GenerateTournamentBracketImageAsync(string tournamentTitle, int totalTeams, int currentRound)
        {
            try
            {
                var width = Math.Max(800, totalTeams * 100);
                var height = 600;

                var bitmap = new Bitmap(width, height);
                using var graphics = Graphics.FromImage(bitmap);

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.FromArgb(25, 25, 25));

                using var headerBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, width, 60),
                    Color.FromArgb(0, 123, 255),
                    Color.FromArgb(0, 86, 179),
                    LinearGradientMode.Horizontal);

                graphics.FillRectangle(headerBrush, 0, 0, width, 60);

                using var titleFont = new Font("Arial", 18, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);

                graphics.DrawString($"🏆 {tournamentTitle}", titleFont, textBrush, 20, 20);

                using var infoFont = new Font("Arial", 12);
                graphics.DrawString($"Teams: {totalTeams} | Current Round: {currentRound}", infoFont, textBrush, 20, 80);

                var totalRounds = (int)Math.Ceiling(Math.Log2(totalTeams));
                var roundWidth = width / totalRounds;

                for (int round = 1; round <= totalRounds; round++)
                {
                    var x = (round - 1) * roundWidth + 20;
                    var y = 120;

                    var roundName = round switch
                    {
                        var r when r == totalRounds => "Final",
                        var r when r == totalRounds - 1 => "Semi-Final",
                        var r when r == totalRounds - 2 => "Quarter-Final",
                        _ => $"Round {round}"
                    };

                    graphics.DrawString(roundName, infoFont, textBrush, x, y);

                    var matchesInRound = totalTeams / (int)Math.Pow(2, round);
                    var matchHeight = (height - 200) / Math.Max(matchesInRound, 1);

                    for (int match = 0; match < matchesInRound; match++)
                    {
                        var matchY = y + 40 + match * matchHeight;
                        var matchRect = new Rectangle(x, matchY, roundWidth - 40, 40);

                        var matchColor = round <= currentRound ? Color.FromArgb(40, 167, 69) : Color.FromArgb(108, 117, 125);
                        using var matchBrush = new SolidBrush(matchColor);
                        graphics.FillRectangle(matchBrush, matchRect);

                        using var borderPen = new Pen(Color.White, 1);
                        graphics.DrawRectangle(borderPen, matchRect);

                        var matchText = round <= currentRound ? "Completed" : "Pending";
                        var textSize = graphics.MeasureString(matchText, infoFont);
                        var textX = matchRect.X + (matchRect.Width - textSize.Width) / 2;
                        var textY = matchRect.Y + (matchRect.Height - textSize.Height) / 2;

                        graphics.DrawString(matchText, infoFont, textBrush, textX, textY);
                    }
                }

                var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                return InputFile.FromStream(stream, "tournament_bracket.png");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating tournament bracket image");
                return null;
            }
        }

        public async Task<InputFile?> GenerateDuelPreviewImageAsync(string format, decimal entryFee, List<string> maps, string status)
        {
            try
            {
                var bitmap = new Bitmap(500, 300);
                using var graphics = Graphics.FromImage(bitmap);

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.FromArgb(35, 35, 35));

                var statusColor = status switch
                {
                    "WaitingForPlayers" => Color.FromArgb(255, 193, 7),
                    "PaymentPending" => Color.FromArgb(255, 165, 0),
                    "InProgress" => Color.FromArgb(40, 167, 69),
                    "Completed" => Color.FromArgb(108, 117, 125),
                    _ => Color.FromArgb(220, 53, 69)
                };

                using var headerBrush = new SolidBrush(statusColor);
                graphics.FillRectangle(headerBrush, 0, 0, 500, 60);

                using var titleFont = new Font("Arial", 18, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);

                graphics.DrawString($"⚔️ {format} Duel", titleFont, textBrush, 20, 20);

                using var bodyFont = new Font("Arial", 14);
                graphics.DrawString($"💰 Entry Fee: €{entryFee:F2}", bodyFont, textBrush, 20, 80);
                graphics.DrawString($"📊 Status: {status}", bodyFont, textBrush, 20, 110);

                var mapsText = maps.Any() ? string.Join(", ", maps.Take(3)) : "TBD";
                if (maps.Count > 3) mapsText += "...";
                graphics.DrawString($"🗺️ Maps: {mapsText}", bodyFont, textBrush, 20, 140);

                var prizeFund = entryFee * GetPlayersCount(format) * 0.9m;
                graphics.DrawString($"🏆 Prize: €{prizeFund:F2}", bodyFont, textBrush, 20, 170);

                using var borderPen = new Pen(statusColor, 3);
                graphics.DrawRectangle(borderPen, 0, 0, 499, 299);

                var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Png);
                stream.Position = 0;

                return InputFile.FromStream(stream, "duel_preview.png");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating duel preview image");
                return null;
            }
        }

        public async Task SendMapSelectionWithImagesAsync(long chatId, string[] maps, string language, CancellationToken cancellationToken)
        {
            try
            {
                var mediaGroup = new List<IAlbumInputMedia>();

                foreach (var map in maps.Take(10))
                {
                    var mapImage = await GetMapImageAsync(map);
                    if (mapImage != null)
                    {
                        mediaGroup.Add(new InputMediaPhoto(mapImage)
                        {
                            Caption = map.Replace("de_", "").ToUpperInvariant()
                        });
                    }
                }

                if (mediaGroup.Any())
                {
                    await _botClient.SendMediaGroup(chatId, mediaGroup, cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error sending map selection with images");
            }
        }

        private static async Task DrawProgressBarAsync(Graphics graphics, int x, int y, int width, int height, double percentage, string label)
        {
            using var backgroundBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
            graphics.FillRectangle(backgroundBrush, x, y, width, height);

            var progressWidth = (int)(width * percentage / 100.0);
            var progressColor = percentage switch
            {
                >= 70 => Color.FromArgb(40, 167, 69),
                >= 50 => Color.FromArgb(255, 193, 7),
                _ => Color.FromArgb(220, 53, 69)
            };

            using var progressBrush = new SolidBrush(progressColor);
            graphics.FillRectangle(progressBrush, x, y, progressWidth, height);

            using var borderPen = new Pen(Color.White, 1);
            graphics.DrawRectangle(borderPen, x, y, width, height);
        }

        private static int GetPlayersCount(string format) => format switch
        {
            "1v1" => 2,
            "2v2" => 4,
            "5v5" => 10,
            _ => 2
        };
    }
}
