using System.Text;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.BackgroundServices.TelegramBot.Localization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer<BotMessages> _messages;

        public TelegramBot(
            IOptions<TelegramBotSettings> options,
            WTelegram.Bot bot,
            ILogger<TelegramBot> logger,
            IStringLocalizer<BotMessages> messages
            )
        {
            _messages = messages;
            _settings = options.Value;
            _logger = logger;
            _bot = bot;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.IsEnabled)
            {
                _logger.LogInformation("Because of the settings Telegram bot is disabled.");
                return;
            }

            CreateLogger();

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

                await _bot.SendMessage(message.Chat, _messages["Welcome"].Value , replyParameters: message);
            }
        }

        private void CreateLogger()
        {
            var directory = Path.GetDirectoryName(_settings.LogPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var stream = new FileStream(_settings.LogPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            StreamWriter WTelegramLogs = new StreamWriter(stream, Encoding.UTF8)
            {
                AutoFlush = true
            };

            WTelegram.Helpers.Log = (lvl, str) => WTelegramLogs.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{"TDIWE!"[lvl]}] {str}");
        }
    }
}