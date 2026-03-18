using GGHubBot.Handlers;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace GGHubBot.Services
{
    public class TelegramBotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly MessageHandler _messageHandler;
        private readonly CallbackHandler _callbackHandler;
        private readonly UserStateService _userStateService;

        public TelegramBotService(
            ITelegramBotClient botClient,
            MessageHandler messageHandler,
            CallbackHandler callbackHandler,
            IServiceProvider serviceProvider,
            UserStateService userStateService)
        {
            _botClient = botClient;
            _messageHandler = messageHandler;
            _callbackHandler = callbackHandler;
            _userStateService = userStateService;
        }

        public async Task StartAsync()
        {
            var me = await _botClient.GetMe();
            Log.Information("Bot started: @{Username}", me.Username);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                UpdateType.Message,
                UpdateType.CallbackQuery,
                UpdateType.InlineQuery
            }
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions
            );

            StartBackgroundTasks();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message when update.Message is not null:
                        await _messageHandler.HandleAsync(update.Message, cancellationToken);
                        break;

                    case UpdateType.CallbackQuery when update.CallbackQuery is not null:
                        await _callbackHandler.HandleAsync(update.CallbackQuery, cancellationToken);
                        break;

                    case UpdateType.InlineQuery when update.InlineQuery is not null:
                        await HandleInlineQueryAsync(update.InlineQuery, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error handling update");
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Log.Error(exception, "Telegram Bot Error: {ErrorMessage}", errorMessage);
            return Task.CompletedTask;
        }

        private async Task HandleInlineQueryAsync(InlineQuery inlineQuery, CancellationToken cancellationToken)
        {
            var results = Array.Empty<InlineQueryResult>();
            await _botClient.AnswerInlineQuery(inlineQuery.Id, results, cancellationToken: cancellationToken);
        }

        private void StartBackgroundTasks()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await _userStateService.CleanupOldStatesAsync();
                        await Task.Delay(TimeSpan.FromHours(6));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error in cleanup background task");
                        await Task.Delay(TimeSpan.FromMinutes(30));
                    }
                }
            });
        }
    }
}
