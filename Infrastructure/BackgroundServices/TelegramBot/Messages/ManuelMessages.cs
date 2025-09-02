using Application.Interfaces;
using Domain.Entities;
using Domain.Models.TelegramBot.ManuelMessages;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.BackgroundServices.TelegramBot.InlineQuery;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;

namespace Infrastructure.BackgroundServices.TelegramBot.Messages
{
    public class ManuelMessages : ITelegramManuelMessages
    {
        private readonly Bot _bot;
        private readonly DataContext _context;
        private readonly HandleIMDbId _handleImdbId;
        private readonly TelegramBotSettings _settings;

        public ManuelMessages(WTelegram.Bot bot, IOptions<TelegramBotSettings> settings, DataContext context,
            HandleIMDbId handleImdbId)
        {
            _settings = settings.Value;
            _bot = bot;
            _context = context;
            _handleImdbId = handleImdbId;
        }

        public async Task SendMovieSeriesNotificationPost(MovieSeriesNotificationParams @params)
        {
            var omdbItem = await _context.OmdbItems
                .FirstOrDefaultAsync(x => x.ImdbId == @params.ImdbId);

            if (omdbItem == null)
                throw new Exception("OMDb Item not found");

            var messageBody = GenerateMovieSeriesNotificationBody(
                omdbItem,
                @params.NotificationType,
                @params.UpdateDetails
            );

            var inlineButtons = new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithUrl("📥 دانلود",
                        $"https://t.me/{_settings.BotUsername}?start=imdbId={omdbItem.ImdbId}"),
                    InlineKeyboardButton.WithUrl("💬 حمایت",
                        $"https://t.me/{_settings.BotUsername}?start=support=true")
                }
            };

            try
            {
                await _bot.SendPhoto(_settings.MainChannelID, omdbItem.Poster, messageBody, ParseMode.Html,
                    replyMarkup: inlineButtons);
            }
            catch
            {
                await _bot.SendMessage(
                    _settings.MainChannelID,
                    messageBody,
                    replyMarkup: inlineButtons,
                    parseMode: ParseMode.Html
                );
            }
        }

        private string GenerateMovieSeriesNotificationBody(
            OmdbItem omdbItem,
            MovieSeriesNotificationType notificationType,
            UpdateDetails updateDetails)
        {
            var plot = StringUtilities.TruncateAtWord(
                omdbItem.PlotFa ?? omdbItem.Plot, 400
            );

            string headline = notificationType switch
            {
                MovieSeriesNotificationType.NewMovie => "🎬 کیفیت خوب منتشر شد",
                MovieSeriesNotificationType.NewAndHot => "🔥 کیفیت خوب منتشر شد - داغ!",
                MovieSeriesNotificationType.NewSeries => "📺 سریال جدید منتشر شد",
                MovieSeriesNotificationType.UpdatedSeries =>
                    $"📡 قسمت جدید منتشر شد (S{updateDetails.Season}E{updateDetails.Episode})",
                _ => "🎥 محتوای جدید"
            };

            const string rlm = "\u200f";
            var message = $"""
                           {rlm}┓━━━━━━━━━━━━━━━
                           <b>{headline}</b>
                           {rlm}┛━━━━━━━━━━━━━━━

                           {_handleImdbId.GenerateMessage(omdbItem, new HandleIMDbId.MessageGenerationOptions
                           {
                               ShowFooter = false,
                               ShowHeader = false,
                               Meta = new HandleIMDbId.MessageGenerationOptions.MetaOptions
                               {
                                   BoxOffice = false,
                                   Writers = false,
                                   Released = false,
                                   Awards = false,
                                   Countries = false,
                                   Runtime = false,
                                   Languages = false,
                                   Rated = false,
                               }
                           })}

                           """;

            return message.Trim();
        }
    }
}