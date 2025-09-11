using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.BackgroundServices.TelegramBot.InlineQuery;
using Infrastructure.BackgroundServices.TelegramBot.Keyboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.BackgroundServices.TelegramBot.Command;

public class StartCommandHandler
{
    private readonly WTelegram.Bot _bot;
    private readonly BotMessages _messages;
    private readonly HandleIMDbId _handleIMDbId;
    private readonly DataContext _context;
    private readonly DefaultKeyboards _keyboards;
    private readonly TelegramBotSettings _settings;

    public StartCommandHandler(WTelegram.Bot bot, BotMessages messages, HandleIMDbId handleIMDbId,
        DataContext context, DefaultKeyboards keyboards, IOptions<TelegramBotSettings> settings)
    {
        _settings = settings.Value;
        _keyboards = keyboards;
        _context = context;
        _handleIMDbId = handleIMDbId;
        _messages = messages;
        _bot = bot;
    }

    public async Task HandleStartCommand(Message message, StartPayload payload)
    {
        if (payload != null && !string.IsNullOrEmpty(payload.ImdbId))
        {
            if (!string.IsNullOrEmpty(payload.ImdbId))
            {
                var omdbItem = await _context.OmdbItems
                    .Include(x => x.Documents)
                    .Include(x => x.Seasons)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(x => x.ImdbId == payload.ImdbId);

                await _bot.SendMessage(message.From.Id, "🎬", ParseMode.Html, null, _keyboards.StartKeyboard);
                await _bot.SendMessage(message.From.Id, _handleIMDbId.GenerateMessage(omdbItem), ParseMode.Html,
                    null, _handleIMDbId.GenerateMarkup(omdbItem));
            }
        }
        else if (payload is { Support: true })
        {
            var messageToSend = _messages.SupportUs
                .Replace("{{COFFEE_URL}}", _settings.BuyCoffeeURL)
                .Replace("{{PAYMENT_URL}}", _settings.RaymitURL);
            await _bot.SendMessage(message.Chat.Id, messageToSend,
                ParseMode.Html);
        }
        else
        {
            var imagePath = Path.Combine(AppContext.BaseDirectory, "Images/latest-movies.jpg");
            await using var stream = File.OpenRead(imagePath);

            await _bot.SendPhoto(
                message.Chat.Id,
                stream,
                _messages.WelcomeMessage,
                ParseMode.Html,
                replyMarkup: _keyboards.StartKeyboard
            );
        }
    }
}