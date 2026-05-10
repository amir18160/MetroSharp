using Domain.Models.TelegramBot.Messages;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.BackgroundServices.TelegramBot.Keyboard;

public class DefaultKeyboards
{
    private readonly BotMessages _messages;
    public ReplyMarkup StartKeyboard { private set; get; }
    public ReplyMarkup OldKeyboard { private set; get; }

    public DefaultKeyboards(BotMessages messages)
    {
        _messages = messages;
        InitializeKeyboards();
    }

    private void InitializeKeyboards()
    {
        StartKeyboard = new[]
        {
            new KeyboardButton[] { _messages.KeyboardButtons.SupportUs },
            new KeyboardButton[] { _messages.KeyboardButtons.SearchTitles, _messages.KeyboardButtons.LatestTitles },
            new KeyboardButton[] { _messages.KeyboardButtons.AboutUs }
        };
    }
}