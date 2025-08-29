using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models.TelegramBot.Messages;
using TL;

namespace Infrastructure.BackgroundServices.TelegramBot.Messages
{
    public class MessageHandler
    {
        private readonly BotMessages _messages;

        public MessageHandler(BotMessages messages)
        {
            _messages = messages;
        }

        public async Task HandleMessages(Message message)
        {

        }

        public void IsInlineMessage()
        {
            
        }

    }
}