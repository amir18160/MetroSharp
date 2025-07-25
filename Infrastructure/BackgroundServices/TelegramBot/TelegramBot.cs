using Infrastructure.BackgroundServices.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using WTelegram;

public class TelegramBot : BackgroundService
{
    private readonly TelegramBotSettings _settings;
    private readonly IUpdateHandler _updateHandler;
    private readonly ILogger<TelegramBot> _logger;
    private readonly ITelegramBotClient _bot;

    public TelegramBot(
        IOptions<TelegramBotSettings> options,
        IUpdateHandler updateHandler,
        ITelegramBotClient bot,
        ILogger<TelegramBot> logger)
    {
        _settings = options.Value;
        _updateHandler = updateHandler;
        _logger = logger;
        _bot = bot;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message],
            DropPendingUpdates = true
        };

        // Required to initialize bot session
        var me = await _bot.GetMe(stoppingToken);
        _logger.LogInformation("Bot initialized as {Username}", me.Username);

        await _bot.ReceiveAsync(_updateHandler, receiverOptions, stoppingToken);
    }
}
