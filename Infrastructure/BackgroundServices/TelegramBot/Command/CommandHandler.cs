using System.Reflection;
using System.Web;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.InlineQuery;
using Infrastructure.BackgroundServices.TelegramBot.Keyboard;
using Telegram.Bot.Types.Enums;
using WTelegram.Types;

namespace Infrastructure.BackgroundServices.TelegramBot.Command
{
    public class CommandHandler
    {
        private readonly BotMessages _messages;
        private readonly WTelegram.Bot _bot;
        private readonly DefaultKeyboards _keyboards;

        // Each command maps to a handler that accepts a payload object
        private readonly Dictionary<string, Func<Message, object, Task>> _commandRegistry;
        private readonly StartCommandHandler _startCommandHandler;

        public CommandHandler(BotMessages messages, WTelegram.Bot bot, DefaultKeyboards keyboards, StartCommandHandler startCommandHandler)
        {
            _startCommandHandler = startCommandHandler;
            _messages = messages;
            _bot = bot;
            _keyboards = keyboards;

            _commandRegistry = new Dictionary<string, Func<Message, object, Task>>(StringComparer.OrdinalIgnoreCase)
            {
                { "/start", async (msg, payload) => await _startCommandHandler.HandleStartCommand(msg, (StartPayload)payload) }
            };
        }

        public static bool IsCommand(string message) =>
            !string.IsNullOrWhiteSpace(message) && message.StartsWith('/');

        private static ParsedCommand ParseCommand(string messageText)
        {
            var parts = messageText.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            var commandText = parts[0];
            var queryString = parts.Length > 1 ? parts[1] : "";


            var query = HttpUtility.ParseQueryString(queryString);

            var payloadPairs = query.AllKeys!
                .Where(k => k != null)
                .ToDictionary(k => k!, k => query[k] ?? "", StringComparer.OrdinalIgnoreCase);

            return new ParsedCommand
            {
                CommandText = commandText,
                PayloadPairs = payloadPairs
            };
        }

         public async Task HandleCommand(Message message)
        {
            var parsed = ParseCommand(message.Text);

            if (_commandRegistry.TryGetValue(parsed.CommandText, out var handler))
            {
                Type payloadType = parsed.CommandText switch
                {
                    "/start" => typeof(StartPayload),
                    _ => null
                };

                object payload = payloadType != null
                    ? BindPayload(payloadType, parsed.PayloadPairs)
                    : null;

                await handler(message, payload);
            }
            else
            {
                await _bot.SendMessage(message.Chat.Id, _messages.UnSupportedCommand);
            }
        }

        /* ------------------- Payload Binding ------------------- */

        private static object BindPayload(Type payloadType, Dictionary<string, string> values)
        {
            var instance = Activator.CreateInstance(payloadType)!;

            foreach (var prop in payloadType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (values.TryGetValue(prop.Name, out var value))
                {
                    object converted = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(instance, converted);
                }
            }

            return instance;
        }
    }

    public class ParsedCommand
    {
        public string CommandText { get; set; } = "";
        public Dictionary<string, string> PayloadPairs { get; set; } = new();
    }

    public class StartPayload
    {
        public string ImdbId { get; set; } = "";
        public string Name { get; set; }
    }
}
