using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.CallbackQuery;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.BackgroundServices.TelegramBot.Keyboard
{
    public class KeyboardHandler
    {
        private readonly BotMessages _messages;
        private readonly WTelegram.Bot _bot;
        private readonly CallbackQueryButtons _callbackQueryButtons;
        private readonly TelegramBotSettings _settings;
        private readonly Dictionary<string, Func<Message, Task>> _keyboardActions;


        public KeyboardHandler(
            BotMessages messages,
            WTelegram.Bot bot,
            CallbackQueryButtons callbackQueryButtons,
            IOptions<TelegramBotSettings> settings)
        {
            _messages = messages;
            _bot = bot;
            _callbackQueryButtons = callbackQueryButtons;
            _settings = settings.Value;

            _keyboardActions = new Dictionary<string, Func<Message, Task>>(StringComparer.OrdinalIgnoreCase)
            {
                { _messages.KeyboardButtons.LatestTitles, HandleLatestTitlesAsync },
                { _messages.KeyboardButtons.AboutUs, HandleAboutUsAsync },
                { _messages.KeyboardButtons.SearchTitles, HandleSearchTitlesAsync },
                { _messages.KeyboardButtons.SupportUs, HandleSupportUsAsync }
            };
        }

        public bool IsKeyboard(string message) =>
            _keyboardActions.ContainsKey(message);

        public async Task HandleKeyboard(Message message)
        {
            if (_keyboardActions.TryGetValue(message.Text, out var action))
            {
                await action(message);
            }
            else
            {
                await _bot.SendMessage(message.Chat.Id, _messages.ErrorMessages.Unknown);
            }
        }

        private async Task HandleLatestTitlesAsync(Message message)
        {
            var imagePath = Path.Combine(AppContext.BaseDirectory, "latest-movies.jpg");
            await using var stream = File.OpenRead(imagePath);
            var buttons = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    new InlineKeyboardButton(_messages.CallbackButtons.RecommendedTitles)
                    {
                        CallbackData = CallbackDataSerializer.CreateRecommendedTitlesParams()
                    },
                    new InlineKeyboardButton(_messages.CallbackButtons.NewPopularTitles)
                    {
                        CallbackData = CallbackDataSerializer.CreateNewTitlesParams()
                    }
                }
            });
            await _bot.SendPhoto(message.Chat.Id, stream, _messages.LatestMoviesMessage, Telegram.Bot.Types.Enums.ParseMode.Html, replyMarkup: buttons);
        }

        private async Task HandleAboutUsAsync(Message message)
        {
            await _bot.SendMessage(message.Chat.Id, _messages.AboutUs, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        private async Task HandleSearchTitlesAsync(Message message)
        {
            var imagePath = Path.Combine(AppContext.BaseDirectory, "trending.jpg");
            await using var stream = File.OpenRead(imagePath);
            await _bot.SendPhoto(
                message.Chat.Id,
                stream,
                _messages.SearchTitles,
                Telegram.Bot.Types.Enums.ParseMode.Html,
                replyMarkup: _callbackQueryButtons.GetSearchButtons());
        }

        private async Task HandleSupportUsAsync(Message message)
        {
            var messageToSend = _messages.SupportUs
                .Replace("{{COFFEE_URL}}", _settings.BuyCoffeeURL)
                .Replace("{{PAYMENT_URL}}", _settings.RaymitURL);
            await _bot.SendMessage(message.Chat.Id, messageToSend, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}