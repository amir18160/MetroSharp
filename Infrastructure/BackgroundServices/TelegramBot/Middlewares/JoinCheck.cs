using System.Text;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TL;
using WTelegram;

namespace Infrastructure.BackgroundServices.TelegramBot.Middlewares;

public class JoinCheck
{
    private readonly ILogger<JoinCheck> _logger;
    private readonly Bot _bot;
    private readonly BotMessages _messages;
    private readonly TelegramBotSettings _settings;

    public JoinCheck(ILogger<JoinCheck> logger, Bot bot, IOptions<TelegramBotSettings> settings, BotMessages messages)
    {
        _logger = logger;
        _bot = bot;
        _messages = messages;
        _settings = settings.Value;
    }

    public async Task<bool> Check(long userId, long chatId, string callbackQueryData = null, int? messageId = null)
    {
        var isUserInChat = true;

        try
        {
            await _bot.GetChatMember(_settings.MainChannelID, userId);
        }
        catch (Exception e)
        {
            _logger.LogError("Exception: {ex}", e.Message);
            if (e.Message == "USER_NOT_PARTICIPANT")
            {
                isUserInChat = false;
            }
        }

        if (isUserInChat) return true;

        ReplyParameters replyParams = null;
        if (messageId.HasValue)
        {
            replyParams = new ReplyParameters()
            {
                AllowSendingWithoutReply = true,
                ChatId = chatId,
                MessageId = messageId.Value,
            };
        }

        var channelsToJoin = new List<ChannelToJoin>
        {
            new()
            {
                ChannelName = _settings.MailChannelUsername,
                ChannelUsername = _settings.MailChannelUsername,
            }
        };

        var inlineButtons = GenerateJoinButtons(channelsToJoin);
        if (!string.IsNullOrWhiteSpace(callbackQueryData))
        {
            inlineButtons.AddNewRow(new InlineKeyboardButton
            {
                CallbackData = callbackQueryData,
                Text = _messages.Common.Continue,
            });
        }

        await _bot.SendMessage(chatId, _messages.Middlewares.PleaseJoin, ParseMode.Html,
            replyParams, inlineButtons);
        return false;
    }

    public Task<bool> Check(UpdateBotCallbackQuery callbackQuery)
    {
        var data = Encoding.UTF8.GetString(callbackQuery.data);
        return Check(callbackQuery.user_id, callbackQuery.user_id, data, callbackQuery.msg_id);
    }


    private InlineKeyboardMarkup GenerateJoinButtons(List<ChannelToJoin> channels)
    {
        var inlineKeyboard = new List<InlineKeyboardButton>();

        foreach (var channel in channels)
        {
            inlineKeyboard.Add(InlineKeyboardButton.WithUrl(channel.ChannelName, GenerateUrl(channel.ChannelUsername)));
        }

        return inlineKeyboard.ToArray();
    }

    private static string GenerateUrl(string channelUsername)
    {
        return $"https://t.me/{channelUsername}";
    }
}

public class ChannelToJoin
{
    public string ChannelName { get; set; }
    public string ChannelUsername { get; set; }
}