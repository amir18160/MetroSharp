using System.Text;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Command;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.BackgroundServices.TelegramBot.InlineQuery;
using Infrastructure.BackgroundServices.TelegramBot.Keyboard;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly BotMessages _messages;
        private readonly IServiceScopeFactory _scopeFactory;

        public TelegramBot(
            IOptions<TelegramBotSettings> options,
            WTelegram.Bot bot,
            ILogger<TelegramBot> logger,
            BotMessages messages,
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
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

            TelegramLogger.CreateLogger(_settings.LogPath);

            var me = await _bot.GetMe();
            await _bot.DropPendingUpdates();

            _bot.WantUnknownTLUpdates = true;

            _bot.OnUpdate += OnUpdate;
            _bot.OnMessage += OnMessage;
            _bot.OnError += (e, s) => Console.Error.WriteLineAsync(e.ToString());

            _logger.LogInformation("Telegram Bot is initialized as {Username}", me.Username);
        }

        private async Task OnMessage(Message message, UpdateType type)
        {
            if (message.Text == null) return;

            var text = message.Text.ToLower();

            using var scope = _scopeFactory.CreateScope();
            var commandHandler = scope.ServiceProvider.GetRequiredService<CommandHandler>();
            var keyboardHandler = scope.ServiceProvider.GetRequiredService<KeyboardHandler>();

            if (CommandHandler.IsCommand(text))
            {
                await commandHandler.HandleCommand(message);
                return;
            }

            if (keyboardHandler.IsKeyboard(text))
            {
                await keyboardHandler.HandleKeyboard(message);
                return;
            }

        }

        public async Task OnUpdate(WTelegram.Types.Update update)
        {
            using var scope = _scopeFactory.CreateScope();
            var inlineQueryHandler = scope.ServiceProvider.GetRequiredService<InlineQueryHandler>();

            if (update.Type == UpdateType.Unknown)
            {
                if (update.TLUpdate is TL.UpdateBotInlineQuery updateInlineQuery)
                {
                    _logger.LogInformation("This is inline query {query}", updateInlineQuery.query);
                    await inlineQueryHandler.HandleInlineRequest(updateInlineQuery);
                };
            };
        }
    }
}
