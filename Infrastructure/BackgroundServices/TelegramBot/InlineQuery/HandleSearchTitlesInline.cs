using System.Net;
using System.Text;
using Domain.Entities;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.Utilities;
using Infrastructure.BackgroundServices.TelegramBot.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using WTelegram;
using Telegram.Bot.Types.ReplyMarkups;
using Infrastructure.BackgroundServices.TelegramBot.CallbackQuery;

namespace Infrastructure.BackgroundServices.TelegramBot.InlineQuery
{
    public class HandleSearchTitlesInline
    {
        private readonly WTelegram.Bot _bot;
        private readonly DataContext _context;
        private readonly BotMessages _messages;
        private readonly ILogger<HandleSearchTitlesInline> _logger;

        public HandleSearchTitlesInline(
            WTelegram.Bot bot,
            DataContext context,
            BotMessages messages,
            ILogger<HandleSearchTitlesInline> logger)
        {
            _logger = logger;
            _messages = messages;
            _bot = bot;
            _context = context;
        }

        public async Task Search(TL.UpdateBotInlineQuery inlineQuery)
        {
            var results = await _context.OmdbItems
                .Where(x => EF.Functions.Like(x.Title, $"{inlineQuery.query}%") || x.ImdbId == inlineQuery.query)
                .Include(x => x.Tags)
                .OrderByDescending(x => x.ImdbVotes)
                .Take(20)
                .ToListAsync();

            var response = new List<InlineQueryResult>();

            foreach (var item in results)
            {
                var qr = new InlineQueryResultArticle
                {
                    Id = item.ImdbId,
                    Description = MessageGeneratorHelper.E(
                        StringUtilities.TruncateAtWord(item.PlotFa, 50)
                    ),
                    Title = $"{item.Title} {item.Year}",
                    ThumbnailUrl = item.Poster,
                    InputMessageContent = GenerateDescription(item),
                    // ReplyMarkup = new InlineKeyboardMarkup(new List<InlineKeyboardButton>()
                    // {
                    //     new InlineKeyboardButton {
                    //         Text = _messages.CallbackButtons.MoreInfo,
                    //         CallbackData = CallbackDataSerializer.(item.ImdbId)
                    //     },
                    // })
                };
                response.Add(qr);
            }

            try
            {
                await _bot.AnswerInlineQuery(inlineQuery.query_id.ToString(), response, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to answer inline query with Telegram bot");
            }
        }

        private InputTextMessageContent GenerateDescription(OmdbItem item)
        {

            var imdbUrl = StringUtilities.GetIMDbURL(item.ImdbId);
            var titleTxt = $"{item.Title} {item.Year} ({(item.Type == Domain.Enums.OmdbItemType.Movie ? _messages.Common.Movie : _messages.Common.Series)})";

            var sb = new StringBuilder();

            // Title + IMDb link
            sb.AppendLine($@"🎬 <a href=""{MessageGeneratorHelper.E(imdbUrl)}""><b>{MessageGeneratorHelper.E(titleTxt)}</b></a>");

            // Ratings
            var ratingParts = new List<string>();
            if (item.ImdbRating.HasValue)
                ratingParts.Add($"⭐ <b>{MessageGeneratorHelper.E(_messages.InlineQuerySearchResponse.Scores.Imdb)}</b>: {item.ImdbRating:F1} ({item.ImdbVotes})");

            if (item.RottenTomatoesScore.HasValue)
                ratingParts.Add($"🍅 <b>{MessageGeneratorHelper.E(_messages.InlineQuerySearchResponse.Scores.Rotten)}</b>: {item.RottenTomatoesScore}");

            if (item.Metascore.HasValue)
                ratingParts.Add($"📊 <b>{MessageGeneratorHelper.E(_messages.InlineQuerySearchResponse.Scores.Metascore)}</b>: {item.Metascore}");

            if (ratingParts.Count != 0)
            {
                sb.AppendLine();
                sb.AppendLine(string.Join("\n", ratingParts));
            }

            // Genres
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🏷️", _messages.InlineQuerySearchResponse.Genres, MessageGeneratorHelper.JoinOrEmpty(item.Genres));

            // Actors
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🎭", _messages.InlineQuerySearchResponse.Actors, MessageGeneratorHelper.JoinOrEmpty(item.Actors));

            // Directors
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🎬", _messages.InlineQuerySearchResponse.Directors, MessageGeneratorHelper.JoinOrEmpty(item.Directors));

            // Writers
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "✍️", _messages.InlineQuerySearchResponse.Writers, MessageGeneratorHelper.JoinOrEmpty(item.Writers));

            // Plot
            var plotRaw = StringUtilities.TruncateAtWord(item.PlotFa ?? item.Plot, 300) ?? "";
            if (!string.IsNullOrWhiteSpace(plotRaw))
            {
                sb.AppendLine();
                sb.AppendLine($"📖 <b>{MessageGeneratorHelper.E(_messages.InlineQuerySearchResponse.Plot)}</b>:");
                sb.AppendLine($"- {MessageGeneratorHelper.E(plotRaw)}");
                sb.AppendLine();
            }

            // Languages
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🗣️", _messages.InlineQuerySearchResponse.Languages, MessageGeneratorHelper.JoinOrEmpty(item.Languages)?.Replace(",", ", "));

            // BoxOffice
            if (MessageGeneratorHelper.HasValue(item.BoxOffice))
                sb.AppendLine($"💰 <b>{MessageGeneratorHelper.E(_messages.InlineQuerySearchResponse.BoxOffice)}</b>: {MessageGeneratorHelper.E(item.BoxOffice)}");

            // Awards
            if (MessageGeneratorHelper.HasValue(item.Awards))
                MessageGeneratorHelper.AppendIfNotEmpty(sb, "🏆", _messages.InlineQuerySearchResponse.Awards, item.Awards);

            sb.AppendLine();
            sb.AppendLine($"🔎 <i>IMDb</i> • <code>{item.ImdbId}</code>");

            return new InputTextMessageContent(sb.ToString().TrimEnd())
            {
                ParseMode = ParseMode.Html
            };
        }

        
    }
}
