using System.Text;
using Domain.Entities;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.BackgroundServices.TelegramBot.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Document = Domain.Entities.Document;

namespace Infrastructure.BackgroundServices.TelegramBot.CallbackQuery;

public class CallbackDownloadHandler
{
    private readonly WTelegram.Bot _bot;
    private readonly BotMessages _messages;
    private readonly DataContext _context;
    private readonly ILogger<CallbackDownloadHandler> _logger;
    private readonly TelegramBotSettings _settings;

    public CallbackDownloadHandler(
        BotMessages messages,
        WTelegram.Bot bot,
        DataContext context,
        IOptions<TelegramBotSettings> settings,
        ILogger<CallbackDownloadHandler> logger)
    {
        _messages = messages ?? throw new ArgumentNullException(nameof(messages));
        _bot = bot ?? throw new ArgumentNullException(nameof(bot));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Entry for the "download" callback: shows documents (movie) or seasons (series).
    /// </summary>
    public async Task HandleDownloadAsync(TL.UpdateBotCallbackQuery callbackQuery)
    {
        if (callbackQuery == null) throw new ArgumentNullException(nameof(callbackQuery));

        var data = Encoding.UTF8.GetString(callbackQuery.data);
        var imdbId = CallbackDataSerializer.ParseDownloadFilesParams(data);
        if (string.IsNullOrWhiteSpace(imdbId))
        {
            _logger.LogWarning("Download callback contained empty IMDB id.");
            return;
        }

        var omdbItem = await _context.OmdbItems
            .Include(x => x.Documents)
            .Include(x => x.Seasons)
            .AsSingleQuery()
            .FirstOrDefaultAsync(x => x.ImdbId == imdbId);

        if (omdbItem == null)
        {
            _logger.LogWarning("OMDb item not found for imdbId {ImdbId}.", imdbId);
            await _bot.SendMessage(callbackQuery.user_id, _messages.ErrorMessages.NotFound, ParseMode.Html,
                BuildReplyParameters(callbackQuery));
            return;
        }

        if (omdbItem.Documents != null && omdbItem.Seasons.Count == 0)
        {
            await _bot.SendMessage(callbackQuery.user_id, _messages.CallbackQueryResponse.TitleNotAvailable,
                ParseMode.Html, BuildReplyParameters(callbackQuery));
            return;
        }

        switch (omdbItem.Type)
        {
            case Domain.Enums.OmdbItemType.Movie:
                // Prefix with movie info
                var moviePrefix =
                    $"{_messages.CallbackQueryResponse.SelectedMovie}: {omdbItem.Title ?? omdbItem.ImdbId}";
                await _bot.SendMessage(
                    callbackQuery.user_id,
                    $"{moviePrefix}\n\n{GetPrompt(MessagePrompt.SelectDocument)}",
                    ParseMode.Html,
                    BuildReplyParameters(callbackQuery),
                    BuildDocumentKeyboard(omdbItem.Documents));
                break;

            case Domain.Enums.OmdbItemType.Series:
                // Prefix with series title when listing seasons
                var seriesPrefix =
                    $"{_messages.CallbackQueryResponse.SelectedSeries} {omdbItem.Title ?? omdbItem.ImdbId}";
                await _bot.SendMessage(
                    callbackQuery.user_id,
                    $"{seriesPrefix}\n\n{GetPrompt(MessagePrompt.SelectSeason)}",
                    ParseMode.Html,
                    BuildReplyParameters(callbackQuery),
                    BuildKeyboardFromRows(BuildSeasonKeyboardRows(omdbItem.Seasons, 3)));
                break;

            default:
                _logger.LogWarning("Unsupported OMDb item type for imdbId {ImdbId}: {Type}", imdbId, omdbItem.Type);
                await _bot.SendMessage(callbackQuery.user_id, _messages.ErrorMessages.Unsupported, ParseMode.Html,
                    BuildReplyParameters(callbackQuery));
                break;
        }
    }

    /// <summary>
    /// Handles "select season" callback and returns episode buttons.
    /// </summary>
    public async Task HandleSeasonAsync(TL.UpdateBotCallbackQuery callbackQuery)
    {
        if (callbackQuery == null) throw new ArgumentNullException(nameof(callbackQuery));

        var data = Encoding.UTF8.GetString(callbackQuery.data);
        var seasonId = CallbackDataSerializer.ParseSeasonParams(data);
        if (!Guid.TryParse(seasonId, out var parsedSeasonId))
        {
            _logger.LogWarning("Invalid season id parsed from callback data: {Raw}", seasonId);
            return;
        }

        var season = await _context.Seasons
            .Include(s => s.Episodes)
            .FirstOrDefaultAsync(s => s.Id == parsedSeasonId);

        if (season == null)
        {
            _logger.LogWarning("Season not found: {Id}", parsedSeasonId);
            await _bot.SendMessage(callbackQuery.user_id, _messages.ErrorMessages.NotFound, ParseMode.Html,
                BuildReplyParameters(callbackQuery));
            return;
        }

        var prefix = $"{_messages.CallbackQueryResponse.SelectedSeason}: {season.SeasonNumber:D2}";
        await _bot.SendMessage(
            callbackQuery.user_id,
            $"{prefix}\n\n{GetPrompt(MessagePrompt.SelectEpisode)}",
            ParseMode.Html,
            BuildReplyParameters(callbackQuery),
            BuildKeyboardFromRows(BuildEpisodeKeyboardRows(season.Episodes, 4)));
    }

    /// <summary>
    /// Handles "select episode" callback and returns document buttons for that episode.
    /// </summary>
    public async Task HandleEpisodeAsync(TL.UpdateBotCallbackQuery callbackQuery)
    {
        if (callbackQuery == null) throw new ArgumentNullException(nameof(callbackQuery));

        var data = Encoding.UTF8.GetString(callbackQuery.data);
        var episodeId = CallbackDataSerializer.ParseEpisodeParams(data);
        if (!Guid.TryParse(episodeId, out var parsedEpisodeId))
        {
            _logger.LogWarning("Invalid episode id parsed from callback data: {Raw}", episodeId);
            return;
        }

        // include Season so we can display season number in prefix
        var episode = await _context.Episodes
            .Include(e => e.Documents)
            .Include(e => e.Season)
            .FirstOrDefaultAsync(e => e.Id == parsedEpisodeId);

        if (episode == null)
        {
            _logger.LogWarning("Episode not found: {Id}", parsedEpisodeId);
            await _bot.SendMessage(callbackQuery.user_id, _messages.ErrorMessages.NotFound, ParseMode.Html,
                BuildReplyParameters(callbackQuery));
            return;
        }

        if (episode.Documents != null)
        {
            var keyboard = episode.Documents
                .Select(d => InlineKeyboardButton.WithCallbackData(
                    BuildDocumentButtonLabel(d),
                    CallbackDataSerializer.CreateGetDocumentParams(d.Id.ToString())))
                .ToList();

            // Build prefix with season and episode info (if season available)
            var prefix = episode.Season != null
                ? $"{_messages.CallbackQueryResponse.SelectedEpisode}: S{episode.Season.SeasonNumber:D2}E{episode.EpisodeNumber:D2}"
                : $"{_messages.CallbackQueryResponse.SelectedEpisode}: E{episode.EpisodeNumber:D2}";

            await _bot.SendMessage(
                callbackQuery.user_id,
                $"{prefix}\n\n{GetPrompt(MessagePrompt.SelectDocument)}",
                ParseMode.Html,
                BuildReplyParameters(callbackQuery),
                BuildKeyboardFromButtons(keyboard));
        }
    }

    /// <summary>
    /// Handles "get document" callback: forwards file from channel and then sends file and details to user.
    /// </summary>
    public async Task HandleDocumentAsync(TL.UpdateBotCallbackQuery callbackQuery)
    {
        if (callbackQuery == null) throw new ArgumentNullException(nameof(callbackQuery));

        var data = Encoding.UTF8.GetString(callbackQuery.data);
        var documentId = CallbackDataSerializer.ParseDocumentFileParams(data);
        if (!Guid.TryParse(documentId, out var parsedDocumentId))
        {
            _logger.LogWarning("Invalid document id parsed from callback data: {Raw}", documentId);
            return;
        }

        var document = await _context.Documents.FirstOrDefaultAsync(d => d.Id == parsedDocumentId);
        if (document == null)
        {
            _logger.LogWarning("Document not found: {Id}", parsedDocumentId);
            await _bot.SendMessage(callbackQuery.user_id, _messages.ErrorMessages.NotFound, ParseMode.Html,
                BuildReplyParameters(callbackQuery));
            return;
        }

        // Determine a helpful prefix to tell the user exactly what is being downloaded
        var prefix = await BuildDocumentDownloadPrefixAsync(parsedDocumentId);

        var forwarded = await _bot.ForwardMessage(_settings.BinChannelChatID, _settings.ChannelFileChatID,
            document.MessageId);

        // Send document details first (with prefix)
        var detailsText = string.IsNullOrWhiteSpace(prefix)
            ? BuildDocumentDetailsMessage(document)
            : $"{prefix}\n\n{BuildDocumentDetailsMessage(document)}";

        var detailsResult = await _bot.SendMessage(callbackQuery.user_id, detailsText,
            ParseMode.Html);

        // choose file id from forwarded message (either Document or Video)
        var fileId = forwarded.Document?.FileId ?? forwarded.Video?.FileId;
        if (string.IsNullOrWhiteSpace(fileId))
        {
            _logger.LogError("Forwarded message didn't contain a file for document {DocumentId}", parsedDocumentId);
            await _bot.SendMessage(callbackQuery.user_id, _messages.ErrorMessages.Unknown, ParseMode.Html,
                BuildReplyParameters(callbackQuery));
            return;
        }

        // send the actual file as a document attachment, replying to the details message
        await _bot.SendDocument(
            callbackQuery.user_id,
            fileId,
            replyParameters: new ReplyParameters
            {
                MessageId = detailsResult.MessageId,
                AllowSendingWithoutReply = true,
                ChatId = callbackQuery.user_id
            });
    }

    #region Helpers

    private ReplyParameters BuildReplyParameters(TL.UpdateBotCallbackQuery callbackQuery)
    {
        return new ReplyParameters
        {
            AllowSendingWithoutReply = true,
            ChatId = callbackQuery.user_id,
            MessageId = callbackQuery.msg_id
        };
    }

    private static string FormatFileSize(double bytes)
    {
        // Accepts raw file size in bytes (same semantics as before)
        const double oneKb = 1024;
        const double oneMb = oneKb * 1024;
        const double oneGb = oneMb * 1024;

        if (bytes < oneGb)
            return (bytes / oneMb).ToString("0.##") + " MB";

        return (bytes / oneGb).ToString("0.##") + " GB";
    }

    private string BuildDocumentDetailsMessage(Document document)
    {
        var sb = new StringBuilder();
        var sizeText = FormatFileSize(document.FileSize);

        // Title / filename
        MessageGeneratorHelper.AppendIfNotEmpty(sb, "🎬", _messages.CallbackQueryResponse.FileName, "\u200F",
            false);
        sb.AppendLine(document.FileName);
        sb.AppendLine();

        // information block heading
        sb.AppendLine($"\u200F┓━━━ℹ️ {_messages.InlineQuerySearchResponse.Information} ━━━━━━━━");

        // is soft-subbed
        MessageGeneratorHelper.AppendIfNotEmpty(sb, "🇮🇷", _messages.CallbackQueryResponse.IsSoftSubbed,
            document.IsSubbed ? _messages.Common.Yes : _messages.Common.No, false);

        // size
        MessageGeneratorHelper.AppendIfNotEmpty(sb, "🔹", _messages.CallbackQueryResponse.Size, sizeText, false);

        // resolution
        if (!string.IsNullOrWhiteSpace(document.Resolution)
            && !document.Resolution.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🌟", _messages.CallbackQueryResponse.Resolution,
                $"{document.Resolution}p", false);

        // encoder
        if (!string.IsNullOrWhiteSpace(document.Encoder)
            && !document.Encoder.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🔐", _messages.CallbackQueryResponse.Encoder,
                document.Encoder.ToUpperInvariant(), false);

        // codec
        if (!string.IsNullOrWhiteSpace(document.Codec)
            && !document.Codec.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🈴", _messages.CallbackQueryResponse.Codec,
                document.Codec.ToUpperInvariant(), false);

        sb.AppendLine("\u200F┛━━━━━━━━━━━━━━━━━\n");
        return sb.ToString();
    }

    /// <summary>
    /// Builds a prefix describing what the document belongs to (SxxExx or Movie Title).
    /// </summary>
    private async Task<string> BuildDocumentDownloadPrefixAsync(Guid documentId)
    {
        // Try to find an episode which contains this document (and include season)
        var episode = await _context.Episodes
            .Include(e => e.Season)
            .FirstOrDefaultAsync(e => e.Documents.Any(d => d.Id == documentId));

        if (episode != null)
        {
            if (episode.Season != null)
                return $"Downloading Season {episode.Season.SeasonNumber:D2} Episode {episode.EpisodeNumber:D2}";

            return $"Downloading Episode {episode.EpisodeNumber:D2}";
        }

        // If not an episode-document, try to find an OMDb item (movie) that contains the document
        var omdb = await _context.OmdbItems.FirstOrDefaultAsync(o => o.Documents.Any(d => d.Id == documentId));
        if (omdb != null)
            // If it's a movie, show movie title; if series-level doc (rare), show title as well
            return $"Downloading Movie {omdb.Title ?? omdb.ImdbId}";

        // Fallback: nothing to prefix
        return string.Empty;
    }

    private static InlineKeyboardMarkup BuildDocumentKeyboard(IEnumerable<Document> documents)
    {
        var iEnumerable = documents as Document[] ?? documents.ToArray();
        var rows = iEnumerable
            .Select(d => new[]
            {
                InlineKeyboardButton.WithCallbackData("", "")
            })
            .ToList();

        var keyboard = iEnumerable.Select(d =>
            new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    BuildDocumentButtonLabel(d),
                    CallbackDataSerializer.CreateGetDocumentParams(d.Id.ToString()))
            }).ToList();

        return new InlineKeyboardMarkup(keyboard);
    }

    private static string BuildDocumentButtonLabel(Document document)
    {
        var parts = new List<string>();

        if (document.IsSubbed) parts.Add("🇮🇷");

        parts.Add(FormatFileSize(document.FileSize));

        if (!string.IsNullOrWhiteSpace(document.Resolution) &&
            !document.Resolution.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            parts.Add($"{document.Resolution}p");

        if (!string.IsNullOrWhiteSpace(document.Encoder) &&
            !document.Encoder.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            parts.Add(document.Encoder.ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(document.Codec) &&
            !document.Codec.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            parts.Add(document.Codec.ToUpperInvariant());

        return string.Join(" | ", parts);
    }

    private static IEnumerable<IEnumerable<InlineKeyboardButton>> BuildSeasonKeyboardRows(
        IEnumerable<Season> seasons, int rowSize)
    {
        var buttons = seasons.Select(s =>
            InlineKeyboardButton.WithCallbackData(
                $"S{s.SeasonNumber:D2}",
                CallbackDataSerializer.CreateGetSeasonParams(s.Id.ToString())
            )).ToList();

        return Partition(buttons, rowSize);
    }

    private static IEnumerable<IEnumerable<InlineKeyboardButton>> BuildEpisodeKeyboardRows(
        IEnumerable<Episode> episodes, int rowSize)
    {
        var buttons = episodes.Select(e =>
            InlineKeyboardButton.WithCallbackData(
                $"E{e.EpisodeNumber:D2}",
                CallbackDataSerializer.CreateEpisodeParams(e.Id.ToString())
            )).ToList();

        return Partition(buttons, rowSize);
    }

    private static IEnumerable<IEnumerable<T>> Partition<T>(IList<T> source, int size)
    {
        if (source == null) yield break;
        for (var i = 0; i < source.Count; i += size)
            yield return source.Skip(i).Take(size);
    }

    /// <summary>
    /// Turns an IEnumerable of button rows into InlineKeyboardMarkup.
    /// </summary>
    private static InlineKeyboardMarkup BuildKeyboardFromRows(IEnumerable<IEnumerable<InlineKeyboardButton>> rows)
    {
        var keyboard = rows
            .Where(r => r != null)
            .Select(r => r.ToArray())
            .ToList();

        return new InlineKeyboardMarkup(keyboard);
    }

    /// <summary>
    /// Converts a flat set of InlineKeyboardButton into each button on its own row.
    /// </summary>
    private static InlineKeyboardMarkup BuildKeyboardFromButtons(IEnumerable<InlineKeyboardButton> buttons)
    {
        var keyboard = buttons
            .Select(b => new[] { b })
            .ToList();

        return new InlineKeyboardMarkup(keyboard);
    }

    private string GetPrompt(MessagePrompt prompt)
    {
        return prompt switch
        {
            MessagePrompt.SelectDocument => _messages.CallbackQueryResponse.SelectDocument,
            MessagePrompt.SelectSeason => _messages.CallbackQueryResponse.SelectSeason,
            MessagePrompt.SelectEpisode => _messages.CallbackQueryResponse.SelectEpisode,
            _ => string.Empty
        };
    }

    private enum MessagePrompt
    {
        SelectDocument,
        SelectSeason,
        SelectEpisode
    }

    #endregion
}