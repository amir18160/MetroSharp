using System.Text;
using Domain.Entities;
using Domain.Enums;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Persistence;

namespace Infrastructure.BackgroundServices.TelegramBot.CallbackQuery;

public class CallbackTitleListHandler
{
    private readonly DataContext _context;
    private readonly WTelegram.Bot _bot;
    private readonly BotMessages _messages;
    private readonly TelegramBotSettings _settings;

    public CallbackTitleListHandler(WTelegram.Bot bot, DataContext context, BotMessages messages,
        IOptions<TelegramBotSettings> settings)
    {
        _bot = bot;
        _context = context;
        _messages = messages;
        _settings = settings.Value;
    }

    public async Task HandleRecommendedTitles(TL.UpdateBotCallbackQuery callbackQuery)
    {
        var imdbItems = await _context.OmdbItems
            .Where(x => x.Tags.Any(y => y.Type == TagType.Recommended))
            .Include(x => x.Tags)
            .OrderByDescending(x => x.Year)
            .ToListAsync();

        if (imdbItems.Count == 0)
        {
            await _bot.SendMessage(callbackQuery.user_id, _messages.CallbackQueryResponse.EmptyList,
                Telegram.Bot.Types.Enums.ParseMode.Html);
            return;
        }

        var messages = CreateTitleListMessages(imdbItems);

        foreach (var msg in messages)
            await _bot.SendMessage(callbackQuery.user_id, msg, Telegram.Bot.Types.Enums.ParseMode.Html);
    }

    public async Task HandleNewTitles(TL.UpdateBotCallbackQuery callbackQuery)
    {
        var imdbItems = await _context.OmdbItems
            .Where(x => x.Tags.Any(y => y.Type == TagType.New))
            .Include(x => x.Tags)
            .OrderByDescending(x => x.Year)
            .ToListAsync();

        if (imdbItems.Count == 0)
        {
            await _bot.SendMessage(callbackQuery.user_id, _messages.CallbackQueryResponse.EmptyList,
                Telegram.Bot.Types.Enums.ParseMode.Html);
            return;
        }

        var messages = CreateTitleListMessages(imdbItems);

        foreach (var msg in messages)
            await _bot.SendMessage(callbackQuery.user_id, msg, Telegram.Bot.Types.Enums.ParseMode.Html);
    }


    private List<string> CreateTitleListMessages(List<OmdbItem> items, int batchSize = 5)
    {
        var messages = new List<string>();
        var chunkBuilder = new StringBuilder();
        var counter = 0;

        foreach (var item in items.Select((omdbItem, index) => new { omdbItem, index }))
        {
            var sb = new StringBuilder();

            var yearPart = item.omdbItem.Year > 0 ? $" ({item.omdbItem.Year})" : string.Empty;

            var titleWithYear = $"{item.omdbItem.Title}{yearPart}";

            var link = $"https://t.me/{_settings.BotUsername}?start=imdbId={item.omdbItem.ImdbId}";
            sb.AppendLine($"<b>{item.index + 1}. <a href=\"{link}\">{titleWithYear}</a></b>");

            var rating = item.omdbItem.ImdbRating?.ToString("0.0") ?? "N/A";
            var votes = item.omdbItem.ImdbVotes?.ToString("N0") ?? "N/A";
            sb.AppendLine($"<b>IMDb:</b> {rating} ({votes} votes)");

            if (!string.IsNullOrWhiteSpace(item.omdbItem.Plot))
            {
                var plot = StringUtilities.TruncateAtWord(item.omdbItem.PlotFa ?? item.omdbItem.Plot, 200);

                sb.AppendLine($"<b>{_messages.InlineQuerySearchResponse.Plot}:</b> <i>{plot}</i>");
            }

            chunkBuilder.AppendLine(sb.ToString().Trim());
            chunkBuilder.AppendLine();
            counter++;

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