using Domain.Models.TelegramBot.Messages;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.BackgroundServices.TelegramBot.CallbackQuery
{
    public class CallbackQueryButtons
    {
        private readonly BotMessages _messages;
        public CallbackQueryButtons(BotMessages messages)
        {
            _messages = messages;
        }

        public InlineKeyboardMarkup GetSearchButtons()
        {
            var inlineKeyboard = new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(_messages.CallbackButtons.Search)
            };
            return inlineKeyboard;
        }
    }
}