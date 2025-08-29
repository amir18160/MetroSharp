using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.InlineQuery;
using Infrastructure.BackgroundServices.TelegramBot.Keyboard;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.BackgroundServices.TelegramBot.Command
{
    public class StartCommandHandler
    {
        private readonly WTelegram.Bot _bot;
        private readonly BotMessages _messages;
        private readonly HandleIMDbId _handleIMDbId;
        private readonly DataContext _context;
        private readonly DefaultKeyboards _keyboards;
        public StartCommandHandler(WTelegram.Bot bot, BotMessages messages, HandleIMDbId handleIMDbId, DataContext context, DefaultKeyboards keyboards )
        {
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
                    await _bot.SendMessage(message.From.Id, _handleIMDbId.GenerateMessage(omdbItem), ParseMode.Html, null, _handleIMDbId.GenerateMarkup(omdbItem));
                }
            }
            else
            {
                await _bot.SendMessage(message.Chat.Id,
                    _messages.WelcomeMessage,
                    ParseMode.Html,
                    null,
                    _keyboards.StartKeyboard);
            }
        }
    }
}