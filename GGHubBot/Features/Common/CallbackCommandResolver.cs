namespace GGHubBot.Features.Common
{
    public class CallbackCommandResolver
    {
        private readonly IEnumerable<ICallbackCommand> _commands;

        public CallbackCommandResolver(IEnumerable<ICallbackCommand> commands)
        {
            _commands = commands;
        }

        public ICallbackCommand? Resolve(string callbackData)
        {
            return _commands.FirstOrDefault(c => c.CanHandle(callbackData));
        }
    }
}
