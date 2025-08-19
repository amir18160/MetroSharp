using Domain.Models.TelegramBot.Messages;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.BackgroundServices.TelegramBot.InlineQuery
{
    public class InlineQueryButtons
    {
        private readonly BotMessages _messages;
        public InlineQueryButtons(BotMessages messages)
        {
            _messages = messages;
        }

        public InlineKeyboardMarkup GetSearchButtons()
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
            [
                [
                    InlineKeyboardButton.WithSwitchInlineQueryCurrentChat(_messages.CallbackButtons.Search, "")
                ]
            ]);
            return inlineKeyboard;
        }

    }
}