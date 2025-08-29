using Domain.Core;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Utilities;
using Telegram.Bot.Types;

namespace Infrastructure.BackgroundServices.TelegramBot.InlineQuery
{
    public class ChosenInlineHandler
    {
        public string IMDbInline { get; set; } = Constants.IMDbTitleBase;
        public string IMDbInlineAlternative { get; set; } = Constants.IMDbTitleAlternative;

        private readonly BotMessages _messages;
        private readonly WTelegram.Bot _bot;
        private readonly HandleIMDbId _handleIMDbId;

        public ChosenInlineHandler(BotMessages messages, WTelegram.Bot bot, HandleIMDbId handleIMDbId)
        {
            _handleIMDbId = handleIMDbId;
            _bot = bot;
            _messages = messages;
        }

        public static bool IsChosenInline(Message message)
        {
            return IMDbUtilities.IncludeIMDbURL(message);
        }

        public async Task Handle(Message message)
        {
            var imdbId = IMDbUtilities.GetImdbIdFromMessage(message);

            if (imdbId != null)
            {
                await _handleIMDbId.Handle(message, imdbId);
                return;
            }
        }
    }
}
