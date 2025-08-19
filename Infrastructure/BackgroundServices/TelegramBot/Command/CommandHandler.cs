using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Keyboard;
using Telegram.Bot.Types.Enums;
using WTelegram.Types;

namespace Infrastructure.BackgroundServices.TelegramBot.Command
{
    public class CommandHandler
    {
        private readonly static List<string> _commands = ["/start"];
        private readonly BotMessages _messages;
        private readonly WTelegram.Bot _bot;
        private readonly DefaultKeyboards _keyboards;

        public CommandHandler(BotMessages messages, WTelegram.Bot bot, DefaultKeyboards keyboards)
        {
            _keyboards = keyboards;
            _bot = bot;
            _messages = messages;
        }

        /* static */
        public static bool IsCommand(string message)
        {
            return message.StartsWith('/');
        }

        private static string SupportedCommand(string command)
        {
            return _commands.FirstOrDefault(c => c.Equals(command, StringComparison.OrdinalIgnoreCase));
        }

        /* not static */
        public async Task HandleCommand(Message message)
        {
            if (SupportedCommand(message.Text) == null)
            {
                await _bot.SendMessage(message.Chat.Id, _messages.UnSupportedCommand);
                return;
            }

            await _bot.SendMessage(message.Chat.Id, _messages.WelcomeMessage,  ParseMode.Html, null, _keyboards.StartKeyboard);
        }

    }
}