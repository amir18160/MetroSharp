using System.Text;
using Domain.Entities;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.Utilities;
using Infrastructure.BackgroundServices.TelegramBot.Utilities;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Telegram.Bot.Types;
using WTelegram;
using Telegram.Bot.Types.ReplyMarkups;
using Infrastructure.BackgroundServices.TelegramBot.CallbackQuery;

namespace Infrastructure.BackgroundServices.TelegramBot.InlineQuery
{
    public class HandleIMDbId
    {
        private readonly BotMessages _messages;
        private readonly WTelegram.Bot _bot;
        private readonly DataContext _context;

        public HandleIMDbId(BotMessages messages, WTelegram.Bot bot, DataContext context)
        {
            _context = context;
            _bot = bot;
            _messages = messages;
        }

        public async Task Handle(Message message, string imdbId)
        {
            var omdbItem = await _context.OmdbItems
                .Include(x => x.Seasons)
                .Include(x => x.Documents)
                .FirstOrDefaultAsync(x => x.ImdbId == imdbId);

            if (omdbItem == null)
            {
                await _bot.SendMessage(message.From.Id, _messages.InlineQuerySearchResponse.NotFoundMessage);
                return;
            }

            await _bot.SendMessage(message.From.Id, GenerateMessage(omdbItem), Telegram.Bot.Types.Enums.ParseMode.Html, null, GenerateMarkup(omdbItem));
        }

        public string GenerateMessage(OmdbItem item)
        {
            var sb = new StringBuilder();
            var imdbUrl = StringUtilities.GetIMDbURL(item.ImdbId);
            var typeLabel = item.Type == Domain.Enums.OmdbItemType.Movie
                ? _messages.Common.Movie
                : _messages.Common.Series;

            var titleTxt = $"{item.Title} {(item.Year.HasValue ? item.Year.ToString() : "")} · {typeLabel}".Trim();

            // ===== Header (LTR) =====
            sb.AppendLine("╔══════════════════════╗");
            sb.AppendLine($"🎬 <a href=\"{MessageGeneratorHelper.E(imdbUrl)}\"><b>{MessageGeneratorHelper.E(titleTxt)}</b></a>");
            sb.AppendLine("╚══════════════════════╝\n");

            // ===== Ratings (LTR) =====
            sb.AppendLine($"\u200F┓━━━📖 {_messages.InlineQuerySearchResponse.Scores.Score} ━━━━━━━");
            string imdbVotes = item.ImdbVotes.HasValue ? $" ({item.ImdbVotes})" : "";
            string ImdbRating = item.ImdbRating?.ToString("F1") + imdbVotes;
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "⭐", _messages.InlineQuerySearchResponse.Scores.Imdb, ImdbRating, false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🍅", _messages.InlineQuerySearchResponse.Scores.Rotten, item.RottenTomatoesScore?.ToString(), false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "📊", _messages.InlineQuerySearchResponse.Scores.Metascore, item.Metascore?.ToString(), false);
            sb.AppendLine("\u200F┛━━━━━━━━━━━━━━━━━\n");

            // ===== Meta (RTL) =====
            sb.AppendLine($"\u200F┓━━━ℹ️ {_messages.InlineQuerySearchResponse.Information} ━━━━━━━━");

            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🔞", _messages.InlineQuerySearchResponse.Rated, item.Rated, false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "📅", _messages.InlineQuerySearchResponse.Released, item.Released, false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "💰", _messages.InlineQuerySearchResponse.BoxOffice, item.BoxOffice, false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🏷️", _messages.InlineQuerySearchResponse.Genres, MessageGeneratorHelper.JoinOrEmpty(item.Genres), false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🎬", _messages.InlineQuerySearchResponse.Directors, MessageGeneratorHelper.JoinOrEmpty(item.Directors), false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "✍️", _messages.InlineQuerySearchResponse.Writers, MessageGeneratorHelper.JoinOrEmpty(item.Writers), false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🎭", _messages.InlineQuerySearchResponse.Actors, MessageGeneratorHelper.JoinOrEmpty(item.Actors), false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "⏰", _messages.InlineQuerySearchResponse.Runtime, item.Runtime, false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🗣️", _messages.InlineQuerySearchResponse.Languages, MessageGeneratorHelper.JoinOrEmpty(item.Languages)?.Replace(",", ", "), false);
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🌍", _messages.InlineQuerySearchResponse.Countries, MessageGeneratorHelper.JoinOrEmpty(item.Countries), false);
            if (MessageGeneratorHelper.HasValue(item.Awards))
            {
                MessageGeneratorHelper.AppendIfNotEmpty(sb, "🏆", _messages.InlineQuerySearchResponse.Awards, item.Awards);
            }

            sb.AppendLine("\u200F┛━━━━━━━━━━━━━━━━━\n");

            // ===== Plot (RTL) =====
            var plotRaw = StringUtilities.TruncateAtWord(item.PlotFa ?? item.Plot, 300) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(plotRaw))
            {
                sb.AppendLine($"\u200F┓━━━📖 {_messages.InlineQuerySearchResponse.Plot} ━━━━━━━━");
                sb.AppendLine($"\u200F<i>{MessageGeneratorHelper.E(plotRaw)}</i>");
                sb.AppendLine("\u200F┛━━━━━━━━━━━━━━━━━\n");
            }

            // ===== Languages / Countries =====

            // ===== Awards =====
            if (MessageGeneratorHelper.HasValue(item.Awards))
            {
                MessageGeneratorHelper.AppendIfNotEmpty(sb, "🏆", _messages.InlineQuerySearchResponse.Awards, item.Awards);
            }

            // ===== Footer (LTR) =====
            sb.AppendLine("\n━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine($"🔎 IMDb ID: <code>{MessageGeneratorHelper.E(item.ImdbId)}</code>");
            sb.AppendLine($"🔗 {MessageGeneratorHelper.E(imdbUrl)}");
            sb.AppendLine("📎 <i>Powered by MetroMoviez</i>");

            return sb.ToString().TrimEnd();
        }

        public InlineKeyboardMarkup GenerateMarkup(OmdbItem item)
        {
            var inlineKeyboard = new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithSwitchInlineQuery(_messages.Common.Share, item.ImdbId),
                InlineKeyboardButton.WithCallbackData(_messages.CallbackButtons.Download, CallbackDataSerializer.CreateDownloadFiles(item.ImdbId))
            };
            return inlineKeyboard.ToArray();
        }

    }
}
