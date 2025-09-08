using WTelegram;

namespace Infrastructure.BackgroundServices.TelegramBot.CallbackQuery
{
    public class CallbackHandler
    {
        private readonly CallbackDownloadHandler _callbackDownloadHandler;
        private readonly WTelegram.Bot _bot;
        private readonly CallbackTitleListHandler _callbackTitleListHandler;
        public CallbackHandler(WTelegram.Bot bot, CallbackDownloadHandler callbackDownloadHandler, CallbackTitleListHandler callbackTitleListHandler)
        {
            _callbackTitleListHandler = callbackTitleListHandler;
            _bot = bot;
            _callbackDownloadHandler = callbackDownloadHandler;
        }

        public async Task Handle(TL.UpdateBotCallbackQuery callbackQuery)
        {
            var callbackType = CallbackDataSerializer.GetCallbackType(callbackQuery);
            if (callbackType == CallbackDataSerializer.CallbackType.Unknown)
            {
                // send message that this is not supported yet.
            }
            else if (callbackType == CallbackDataSerializer.CallbackType.Download)
            {
                await _callbackDownloadHandler.HandleDownloadAsync(callbackQuery);
            }
            else if (callbackType == CallbackDataSerializer.CallbackType.Document)
            {
                await _callbackDownloadHandler.HandleDocumentAsync(callbackQuery);
            }
            else if (callbackType == CallbackDataSerializer.CallbackType.Season)
            {
                await  _callbackDownloadHandler.HandleSeasonAsync(callbackQuery);
            }
            else if (callbackType == CallbackDataSerializer.CallbackType.Episode)
            {
                await  _callbackDownloadHandler.HandleEpisodeAsync(callbackQuery);
            }
            else if (callbackType == CallbackDataSerializer.CallbackType.RecommendedTitles)
            {
                await _callbackTitleListHandler.HandleRecommendedTitles(callbackQuery);
            }
            else if (callbackType == CallbackDataSerializer.CallbackType.NewTitles)
            {
                await _callbackTitleListHandler.HandleNewTitles(callbackQuery);
            }
            await _bot.AnswerCallbackQuery(callbackQuery.query_id.ToString());
        }
    }
}