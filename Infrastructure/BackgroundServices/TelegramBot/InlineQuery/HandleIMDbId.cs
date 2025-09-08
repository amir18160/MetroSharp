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

            await _bot.SendMessage(message.From.Id, GenerateMessage(omdbItem), Telegram.Bot.Types.Enums.ParseMode.Html,
                null, GenerateMarkup(omdbItem));
        }

        public string GenerateMessage(OmdbItem item, MessageGenerationOptions options = null)
        {
            options ??= new MessageGenerationOptions(); // fallback to "show all"

            var sb = new StringBuilder();
            var imdbUrl = StringUtilities.GetIMDbURL(item.ImdbId);

            // ===== Header =====
            if (options.ShowHeader)
            {
                var typeLabel = item.Type == Domain.Enums.OmdbItemType.Movie
                    ? _messages.Common.Movie
                    : _messages.Common.Series;
                var yearPart = item.Year > 0 ? $" {item.Year}" : string.Empty;
                var titleTxt = $"{item.Title}{yearPart} · {typeLabel}".Trim();

                sb.AppendLine("╔══════════════════════╗");
                sb.AppendLine(
                    $"🎬 <a href=\"{MessageGeneratorHelper.E(imdbUrl)}\"><b>{MessageGeneratorHelper.E(titleTxt)}</b></a>");
                sb.AppendLine("╚══════════════════════╝\n");
            }

            // ===== Ratings =====
            if (options.ShowRatings)
            {
                sb.AppendLine($"\u200F┓━━━📖 {_messages.InlineQuerySearchResponse.Scores.Score} ━━━━━━━");
                string imdbVotes = item.ImdbVotes.HasValue ? $" ({item.ImdbVotes})" : "";
                string imdbRating = item.ImdbRating?.ToString("F1") + imdbVotes;


                MessageGeneratorHelper.AppendIfNotEmpty(sb, "⭐", _messages.InlineQuerySearchResponse.Scores.Imdb,
                    imdbRating, false);

                if (item.RottenTomatoesScore is > 0)
                {
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "🍅", _messages.InlineQuerySearchResponse.Scores.Rotten,
                        item.RottenTomatoesScore?.ToString(), false);
                }

                if (item.RottenTomatoesScore is > 0)
                {
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "📊",
                        _messages.InlineQuerySearchResponse.Scores.Metascore,
                        item.Metascore?.ToString(), false);
                }

                sb.AppendLine("\u200F┛━━━━━━━━━━━━━━━━━\n");
            }

            // ===== Meta =====
            if (options.ShowMeta)
            {
                sb.AppendLine($"\u200F┓━━━ℹ️ {_messages.InlineQuerySearchResponse.Information} ━━━━━━━━");

                if (options.Meta.Rated)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "🔞", _messages.InlineQuerySearchResponse.Rated,
                        item.Rated, false);

                if (options.Meta.Released)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "📅", _messages.InlineQuerySearchResponse.Released,
                        item.Released, false);

                if (options.Meta.BoxOffice)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "💰", _messages.InlineQuerySearchResponse.BoxOffice,
                        item.BoxOffice, false);

                if (options.Meta.Genres)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "🏷️", _messages.InlineQuerySearchResponse.Genres,
                        MessageGeneratorHelper.JoinOrEmpty(item.Genres), false);

                if (options.Meta.Directors)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "🎬", _messages.InlineQuerySearchResponse.Directors,
                        MessageGeneratorHelper.JoinOrEmpty(item.Directors), false);

                if (options.Meta.Writers)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "✍️", _messages.InlineQuerySearchResponse.Writers,
                        MessageGeneratorHelper.JoinOrEmpty(item.Writers), false);

                if (options.Meta.Actors)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "🎭", _messages.InlineQuerySearchResponse.Actors,
                        MessageGeneratorHelper.JoinOrEmpty(item.Actors), false);

                if (options.Meta.Runtime)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "⏰", _messages.InlineQuerySearchResponse.Runtime,
                        item.Runtime, false);

                if (options.Meta.Languages)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "🗣️", _messages.InlineQuerySearchResponse.Languages,
                        MessageGeneratorHelper.JoinOrEmpty(item.Languages)?.Replace(",", ", "), false);

                if (options.Meta.Countries)
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "🌍", _messages.InlineQuerySearchResponse.Countries,
                        MessageGeneratorHelper.JoinOrEmpty(item.Countries), false);

                if (options.Meta.Awards && MessageGeneratorHelper.HasValue(item.Awards))
                    MessageGeneratorHelper.AppendIfNotEmpty(sb, "🏆", _messages.InlineQuerySearchResponse.Awards,
                        item.Awards);

                sb.AppendLine("\u200F┛━━━━━━━━━━━━━━━━━\n");
            }

            // ===== Plot =====
            if (options.ShowPlot)
            {
                var plotRaw = StringUtilities.TruncateAtWord(item.PlotFa ?? item.Plot, 300) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(plotRaw))
                {
                    sb.AppendLine($"\u200F┓━━━📖 {_messages.InlineQuerySearchResponse.Plot} ━━━━━━━━");
                    sb.AppendLine($"\u200F<i>{MessageGeneratorHelper.E(plotRaw)}</i>");
                    sb.AppendLine("\u200F┛━━━━━━━━━━━━━━━━━\n");
                }
            }

            // ===== Footer =====
            if (options.ShowFooter)
            {
                sb.AppendLine("\n━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine($"🔎 IMDb ID: <code>{MessageGeneratorHelper.E(item.ImdbId)}</code>");
                sb.AppendLine($"🔗 {MessageGeneratorHelper.E(imdbUrl)}");
                sb.AppendLine("📎 <i>Powered by MetroMoviez</i>");
            }

            return sb.ToString().TrimEnd();
        }


        public class MessageGenerationOptions
        {
            public bool ShowHeader { get; set; } = true;
            public bool ShowRatings { get; set; } = true;
            public bool ShowMeta { get; set; } = true;
            public bool ShowPlot { get; set; } = true;
            public bool ShowFooter { get; set; } = true;

            public MetaOptions Meta { get; set; } = new();

            public class MetaOptions
            {
                public bool Rated { get; set; } = true;
                public bool Released { get; set; } = true;
                public bool BoxOffice { get; set; } = true;
                public bool Genres { get; set; } = true;
                public bool Directors { get; set; } = true;
                public bool Writers { get; set; } = true;
                public bool Actors { get; set; } = true;
                public bool Runtime { get; set; } = true;
                public bool Languages { get; set; } = true;
                public bool Countries { get; set; } = true;
                public bool Awards { get; set; } = true;
            }
        }


        public InlineKeyboardMarkup GenerateMarkup(OmdbItem item)
        {
            var inlineKeyboard = new List<InlineKeyboardButton>()
            {
                InlineKeyboardButton.WithSwitchInlineQuery(_messages.Common.Share, item.ImdbId),
                InlineKeyboardButton.WithCallbackData(_messages.CallbackButtons.Download,
                    CallbackDataSerializer.CreateDownloadFiles(item.ImdbId))
            };
            return inlineKeyboard.ToArray();
        }
    }
}