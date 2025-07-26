using Infrastructure.BackgroundServices.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.Enums;
using WTelegram.Types;

namespace Infrastructure.BackgroundServices.TelegramBot
{

    public class TelegramBot : BackgroundService
    {
        private readonly TelegramBotSettings _settings;
        private readonly ILogger<TelegramBot> _logger;
        private readonly WTelegram.Bot _bot;

        public TelegramBot(
            IOptions<TelegramBotSettings> options,
            WTelegram.Bot bot,
            ILogger<TelegramBot> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _bot = bot;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Required to initialize bot session
            var me = await _bot.GetMe();

            _bot.OnMessage += OnMessage;
        
            _logger.LogInformation("Telegram Bot is initialized as {Username}", me.Username);

        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text == null) return;
            var text = message.Text.ToLower();
            if (text == "/start")
            {

                await _bot.SendMessage(message.Chat, $"Hello, {message.From}!\nTry commands /pic /react /lastseen /getchat /setphoto", replyParameters: message);
            }
        }
    }
}