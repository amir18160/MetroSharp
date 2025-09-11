using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using WTelegram;

namespace Infrastructure.BackgroundServices.TelegramBot.Admin;

public class IsAdmin
{
    private readonly Bot _bot;
    private readonly TelegramBotSettings _settings;

    public IsAdmin(Bot bot, IOptions<TelegramBotSettings> settings)
    {
        _bot = bot;
        _settings = settings.Value;
    }

    public async Task<bool> Check(Message message)
    {
        var admins = await _bot.GetChatAdministrators(_settings.MainChannelID);
        return admins.Any(chatMember => message.Chat.Id == chatMember.User.Id);
    }

    public async Task<bool> Check(TL.UpdateNewChannelMessage updateNewChannelMessage)
    {
        var admins = await _bot.GetChatAdministrators(_settings.MainChannelID);
        var senderId = updateNewChannelMessage.message?.From?.ID;
        return admins.Any(chatMember => senderId == chatMember.User.Id);
    }
}