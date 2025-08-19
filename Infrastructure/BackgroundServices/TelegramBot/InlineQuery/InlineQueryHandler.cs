using Domain.Models.TelegramBot.Messages;
using Telegram.Bot.Types.InlineQueryResults;

namespace Infrastructure.BackgroundServices.TelegramBot.InlineQuery
{
    public class InlineQueryHandler
    {
        private readonly BotMessages _messages;
        private readonly WTelegram.Bot _bot;
        private readonly HandleSearchTitlesInline _handleSearchTitles;
        public InlineQueryHandler(BotMessages messages, WTelegram.Bot bot, HandleSearchTitlesInline handleSearchTitles)
        {
            _handleSearchTitles = handleSearchTitles;
            _bot = bot;
            _messages = messages;
        }

        public async Task HandleInlineRequest(TL.UpdateBotInlineQuery inlineQuery)
        {
            if (string.IsNullOrWhiteSpace(inlineQuery.query))
            {
                await HandleEmptyInlineQuery(inlineQuery);
                return;
            }

            await _handleSearchTitles.Search(inlineQuery);
        }

        public async Task HandleEmptyInlineQuery(TL.UpdateBotInlineQuery inlineQuery)
        {
            var defaultResponse = new List<InlineQueryResult>()
            {
                new InlineQueryResultArticle("default_result", _messages.InlineQueryMessages.TypeToSearch, new InputTextMessageContent(_messages.InlineQueryMessages.TypeToSearchContent) )
            };

            await _bot.AnswerInlineQuery(inlineQuery.query_id.ToString(), defaultResponse);       
        }
    }
}