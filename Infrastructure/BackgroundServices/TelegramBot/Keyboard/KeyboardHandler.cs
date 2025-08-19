using System.Runtime.CompilerServices;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.CallbackQuery;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.BackgroundServices.TelegramBot.Keyboard
{
    public class KeyboardHandler
    {
        private readonly BotMessages _messages;
        private readonly WTelegram.Bot _bot;
        private readonly CallbackQueryButtons _callbackQueryButtons;
        public KeyboardHandler(BotMessages messages, WTelegram.Bot bot, CallbackQueryButtons callbackQueryButtons)
        {
            _callbackQueryButtons = callbackQueryButtons;
            _bot = bot;
            _messages = messages;
        }

        public bool IsKeyboard(string message)
        {
            var keyboards = new List<string>()
            {
                _messages.KeyboardButtons.SearchTitles,
                _messages.KeyboardButtons.LatestTitles,
                _messages.KeyboardButtons.AboutUs,
                _messages.KeyboardButtons.SupportUs,
            };

            return keyboards.Any(x => string.Equals(x, message, StringComparison.OrdinalIgnoreCase));
        }


        public async Task HandleKeyboard(Message message)
        {
            var text = message.Text;

            if (text.Equals(_messages.KeyboardButtons.LatestTitles, StringComparison.CurrentCultureIgnoreCase))
            {
                await _bot.SendMessage(message.Chat.Id, _messages.LatestMoviesMessage);
            }
            else if (text.Equals(_messages.KeyboardButtons.AboutUs, StringComparison.CurrentCultureIgnoreCase))
            {
                await _bot.SendMessage(message.Chat.Id, _messages.AboutUs);
            }
            else if (text.Equals(_messages.KeyboardButtons.SearchTitles, StringComparison.CurrentCultureIgnoreCase))
            {
                await HandleSearchRequest(message);
            }
            else if (text.Equals(_messages.KeyboardButtons.SupportUs, StringComparison.CurrentCultureIgnoreCase))
            {
                await _bot.SendMessage(message.Chat.Id, _messages.SupportUs);
            }
            else
            {
                await _bot.SendMessage(message.Chat.Id, _messages.ErrorMessages.Unknown);
            }
        }

        private async Task HandleSearchRequest(Message message)
        {
            await _bot.SendMessage(message.Chat.Id, _messages.SearchTitles, replyMarkup: _callbackQueryButtons.GetSearchButtons()

             );
        }

    }
}