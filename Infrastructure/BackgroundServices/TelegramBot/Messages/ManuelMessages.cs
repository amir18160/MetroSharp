using Application.Interfaces;
using Domain.Entities;
using Domain.Models.TelegramBot.ManuelMessages;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.BackgroundServices.TelegramBot.InlineQuery;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;

namespace Infrastructure.BackgroundServices.TelegramBot.Messages;

public class ManuelMessages : ITelegramManuelMessages
{
    private readonly Bot _bot;
    private readonly DataContext _context;
    private readonly BotMessages _messages;
    private readonly IHostEnvironment _environment;
    private readonly ImageEditorService.ImageEditorService _imageEditorService;
    private readonly ILogger<ManuelMessages> _logger;
    private readonly HandleIMDbId _handleImdbId;
    private readonly TelegramBotSettings _settings;


    public ManuelMessages(Bot bot, IOptions<TelegramBotSettings> settings, DataContext context,
        HandleIMDbId handleImdbId, BotMessages messages, IHostEnvironment environment,
        ImageEditorService.ImageEditorService imageEditorService, ILogger<ManuelMessages> logger)
    {
        _settings = settings.Value;
        _bot = bot;
        _context = context;
        _messages = messages;
        _environment = environment;
        _imageEditorService = imageEditorService;
        _logger = logger;
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
                InlineKeyboardButton.WithUrl(_messages.CallbackButtons.Download,
                    $"https://t.me/{_settings.BotUsername}?start=imdbId={omdbItem.ImdbId}"),
                InlineKeyboardButton.WithUrl(_messages.KeyboardButtons.SupportUs,
                    $"https://t.me/{_settings.BotUsername}?start=support=true")
            }
        };

        var channelId = _environment.IsDevelopment() ? _settings.BinChannelChatID : _settings.MainChannelID;


        try
        {
            MemoryStream generatedPoster = null;
            try
            {
                generatedPoster = await _imageEditorService.GenerateMediaWatermark(omdbItem.Poster);
            }
            catch (Exception e)
            {
                _logger.LogWarning("failed to generate media watermark");
            }

            if (generatedPoster != null)
            {
                await _bot.SendPhoto(channelId, generatedPoster, messageBody, ParseMode.Html,
                    replyMarkup: inlineButtons);
            }
            else
            {
                await _bot.SendPhoto(channelId, omdbItem.Poster, messageBody, ParseMode.Html,
                    replyMarkup: inlineButtons);
            }

            return;
        }
        catch
        {
            _logger.LogWarning("failed to send a media with photo. trying to send without photo...");
        }

        try
        {
            await _bot.SendMessage(
                channelId,
                messageBody,
                replyMarkup: inlineButtons,
                parseMode: ParseMode.Html
            );
        }
        catch (Exception e)
        {
            _logger.LogWarning("failed to send a media notification and all its fallbacks.");
            throw;
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


        var seasonText = _messages.ManuelMessagesResponse.UpdatedSeries.Replace("{HEADER}",
            updateDetails.IsFullSeason
                ? $"{omdbItem.Title} S{updateDetails.Season:D2}"
                : $"{omdbItem.Title} S{updateDetails.Season:D2}E{updateDetails.Episode:D2}");

        var headline = notificationType switch
        {
            MovieSeriesNotificationType.NewMovie => _messages.ManuelMessagesResponse.NewMovie,
            MovieSeriesNotificationType.NewAndHotMovie => _messages.ManuelMessagesResponse.NewAndHotMovie,
            MovieSeriesNotificationType.NewSeries => _messages.ManuelMessagesResponse.NewSeries,
            MovieSeriesNotificationType.Recommended => _messages.ManuelMessagesResponse.Recommended,
            MovieSeriesNotificationType.UpdatedSeries => seasonText,
            _ => ""
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
                               Rated = false
                           }
                       })}

                       """;

        return message.Trim();
    }
}