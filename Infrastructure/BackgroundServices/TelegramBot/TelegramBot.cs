using System.Text;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Admin;
using Infrastructure.BackgroundServices.TelegramBot.CallbackQuery;
using Infrastructure.BackgroundServices.TelegramBot.Command;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.BackgroundServices.TelegramBot.InlineQuery;
using Infrastructure.BackgroundServices.TelegramBot.Keyboard;
using Infrastructure.BackgroundServices.TelegramBot.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using WTelegram.Types;

namespace Infrastructure.BackgroundServices.TelegramBot;

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
        _logger.LogInformation("Bot Want unknown TL updates: {wantUnknown}", _bot.WantUnknownTLUpdates);

        _bot.OnUpdate += OnUpdate;
        _bot.OnMessage += OnMessage;
        _bot.OnError += OnError;

        _logger.LogInformation("Telegram Bot is initialized as {Username}", me.Username);
    }

    private async Task OnMessage(Message message, UpdateType type)
    {
        if (message.Text == null) return;


        var text = message.Text.ToLower();

        using var scope = _scopeFactory.CreateScope();
        var commandHandler = scope.ServiceProvider.GetRequiredService<CommandHandler>();
        var keyboardHandler = scope.ServiceProvider.GetRequiredService<KeyboardHandler>();
        var chosenInlineHandler = scope.ServiceProvider.GetRequiredService<ChosenInlineHandler>();


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

        if (keyboardHandler.IsOldKeyboard(text))
        {
            await keyboardHandler.HandleAskUserToUpdate(message);
            return;
        }
        
        if (ChosenInlineHandler.IsChosenInline(message)) await chosenInlineHandler.Handle(message);
    }

    private async Task OnUpdate(Update update)
    {
        using var scope = _scopeFactory.CreateScope();
        var inlineQueryHandler = scope.ServiceProvider.GetRequiredService<InlineQueryHandler>();
        var callbackHandler = scope.ServiceProvider.GetRequiredService<CallbackHandler>();
        var joinCheck = scope.ServiceProvider.GetRequiredService<JoinCheck>();
        
        _logger.LogInformation("Telegram bot update received. Update type is {type}", update.Type.ToString());
        _logger.LogInformation("TLUpdate type: {updateType}", update.TLUpdate);
        
        switch (update.TLUpdate)
        {
            case TL.UpdateBotInlineQuery updateInlineQuery:
                _logger.LogInformation("This is inline query {query}", updateInlineQuery.query);
                await inlineQueryHandler.HandleInlineRequest(updateInlineQuery);
                break;
            case TL.UpdateBotCallbackQuery updateCallbackQuery:
                _logger.LogInformation("This is callback query data {data}: ",
                    Encoding.UTF8.GetString(updateCallbackQuery.data));
                if (!await joinCheck.Check(updateCallbackQuery))
                {
                    break;
                }
                await callbackHandler.Handle(updateCallbackQuery);
                break;
            case TL.UpdateNewChannelMessage updateNewChannelMessage:
            {
                var handleForwardedMessages = scope.ServiceProvider.GetRequiredService<HandleForwardedFiles>();
                var shouldHandleForwardedMessage =
                    await handleForwardedMessages.ShouldHandleMessage(updateNewChannelMessage);
                if (shouldHandleForwardedMessage)
                    await handleForwardedMessages.HandleAsync(updateNewChannelMessage);
                break;
            }
            default:
                _logger.LogInformation("Unknown/Unhandled update type {updateType}", update.Type.ToString());
                break;
        }
    }

    private Task OnError(Exception error, HandleErrorSource source)
    {
        _logger.LogError("Error inside Bot error handler: ");
        _logger.LogError(error.Message);

        return Task.CompletedTask;
    }
}