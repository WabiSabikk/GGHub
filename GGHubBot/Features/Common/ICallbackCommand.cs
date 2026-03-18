using GGHubBot.Models;
using Telegram.Bot.Types;

namespace GGHubBot.Features.Common
{
    public interface ICallbackCommand
    {
        bool CanHandle(string callbackData);
        Task HandleAsync(CallbackQuery callbackQuery, UserState userState, CancellationToken cancellationToken);
    }
}
