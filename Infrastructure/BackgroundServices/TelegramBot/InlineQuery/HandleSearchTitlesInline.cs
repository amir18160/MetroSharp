using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Telegram.Bot.Types.InlineQueryResults;

namespace Infrastructure.BackgroundServices.TelegramBot.InlineQuery
{
    public class HandleSearchTitlesInline
    {
        private readonly WTelegram.Bot _bot;
        private readonly DataContext _context;
        public HandleSearchTitlesInline(WTelegram.Bot bot, DataContext context)
        {
            _bot = bot;
            _context = context;
        }
        public async Task Search(TL.UpdateBotInlineQuery inlineQuery)
        {
            var results = await _context.OmdbItems
                .Where(x => x.Title.Contains(inlineQuery.query))
                .Take(10)
                .OrderByDescending(x => x.ImdbVotes)
                .ToListAsync();

            var response = new List<InlineQueryResult>();

            System.Console.WriteLine(response.Count);

            foreach (var item in results)
            {
                var qr = new InlineQueryResultArticle
                {
                    Id = item.ImdbId,
                    Description = StringUtilities.TruncateAtWord(item.PlotFa, 50),
                    Title = item.Title + " " + item.Year,
                    ThumbnailUrl = item.Poster ?? "https://images.unsplash.com/photo-1536440136628-849c177e76a1?q=80&w=1025&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                    InputMessageContent = new InputTextMessageContent(item.Plot ?? item.PlotFa)
                };
                response.Add(qr);
            }

            try
            {
                await _bot.AnswerInlineQuery(inlineQuery.query_id.ToString(), response, 0);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("error");
            }

        }
    }
}