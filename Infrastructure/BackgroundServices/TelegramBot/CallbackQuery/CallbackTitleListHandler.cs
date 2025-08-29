using System.Text;
using Domain.Entities;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.BackgroundServices.TelegramBot.CallbackQuery
{
    public class CallbackTitleListHandler
    {
        private readonly DataContext _context;
        private readonly WTelegram.Bot _bot;
        private readonly BotMessages _messages;
        public CallbackTitleListHandler(WTelegram.Bot bot, DataContext context, BotMessages messages)
        {
            _bot = bot;
            _context = context;
            _messages = messages;
        }

        public async Task HandleRecommendedTitles(TL.UpdateBotCallbackQuery callbackQuery)
        {
            var imdbItems = await _context.OmdbItems
                .OrderByDescending(x => x.ImdbVotes)
                .Where(x => x.ImdbVotes > 25000)
                .Take(20)
                .ToListAsync();

            var messages = CreateTitleListMessages(imdbItems);

            foreach (var msg in messages)
            {
                await _bot.SendMessage(callbackQuery.user_id, msg, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }

        public async Task HandleNewTitles(TL.UpdateBotCallbackQuery callbackQuery)
        {
            var imdbItems = await _context.OmdbItems
                .OrderByDescending(x => x.ImdbVotes)
                .Where(x => x.ImdbVotes > 25000)
                .Take(20)
                .ToListAsync();

            var messages = CreateTitleListMessages(imdbItems);

            foreach (var msg in messages)
            {
                await _bot.SendMessage(callbackQuery.user_id, msg, Telegram.Bot.Types.Enums.ParseMode.Html);
            }
        }


        public List<string> CreateTitleListMessages(List<OmdbItem> items, int batchSize = 5)
        {
            var messages = new List<string>();
            var chunkBuilder = new StringBuilder();
            int counter = 0;

            foreach (var item in items.Select((omdbItem, index) => new { omdbItem, index }))
            {
                var sb = new StringBuilder();

                // Index. Title + Year
                var titleWithYear = $"{item.omdbItem.Title} ({item.omdbItem.Year?.ToString() ?? "N/A"})";
                var link = $"https://t.me/MetroDBBot?start=imdbId={item.omdbItem.ImdbId}";
                sb.AppendLine($"<b>{item.index + 1}. <a href=\"{link}\">{titleWithYear}</a></b>");

                // IMDb Rating + Votes
                var rating = item.omdbItem.ImdbRating?.ToString("0.0") ?? "N/A";
                var votes = item.omdbItem.ImdbVotes?.ToString("N0") ?? "N/A";
                sb.AppendLine($"<b>IMDb:</b> {rating} ({votes} votes)");

                // Plot (truncate to avoid Telegram flood)
                if (!string.IsNullOrWhiteSpace(item.omdbItem.Plot))
                {
                    var plot = StringUtilities.TruncateAtWord(item.omdbItem.PlotFa ?? item.omdbItem.Plot, 200);

                    sb.AppendLine($"<b>{_messages.InlineQuerySearchResponse.Plot}:</b> <i>{plot}</i>");
                }

                chunkBuilder.AppendLine(sb.ToString().Trim());
                chunkBuilder.AppendLine(); // spacing
                counter++;

                // Flush batch
                if (counter % batchSize == 0)
                {
                    messages.Add(chunkBuilder.ToString().Trim());
                    chunkBuilder.Clear();
                }
            }

            // Add remaining items
            if (chunkBuilder.Length > 0)
                messages.Add(chunkBuilder.ToString().Trim());

            return messages;
        }


    }
}