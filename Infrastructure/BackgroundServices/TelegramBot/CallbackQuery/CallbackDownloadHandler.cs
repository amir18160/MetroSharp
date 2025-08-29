using System.Text;
using Domain.Entities;
using Domain.Models.TelegramBot.Messages;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.BackgroundServices.TelegramBot.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TL;
using TL.Methods;
using WTelegram;
using Document = Domain.Entities.Document;

namespace Infrastructure.BackgroundServices.TelegramBot.CallbackQuery
{
    public class CallbackDownloadHandler
    {
        private readonly WTelegram.Bot _bot;
        private readonly BotMessages _messages;
        private readonly DataContext _context;
        private readonly TelegramBotSettings _settings;
        public CallbackDownloadHandler(BotMessages messages, WTelegram.Bot bot, DataContext context, IOptions<TelegramBotSettings> settings)
        {
            _settings = settings.Value;
            _context = context;
            _messages = messages;
            _bot = bot;
        }

        public async Task HandleDownload(TL.UpdateBotCallbackQuery callbackQuery)
        {
            var data = Encoding.UTF8.GetString(callbackQuery.data);
            var imdbId = CallbackDataSerializer.ParseDownloadFilesParams(data);

            var omdbItem = await _context.OmdbItems
                .Include(x => x.Documents)
                .Include(x => x.Seasons)
                .FirstOrDefaultAsync(x => x.ImdbId == imdbId);

            if (omdbItem.Type == Domain.Enums.OmdbItemType.Movie)
            {
                await _bot.SendMessage(callbackQuery.user_id, GenerateDownloadMessage(MessageType.SelectDocument), Telegram.Bot.Types.Enums.ParseMode.Html, new ReplyParameters
                {
                    AllowSendingWithoutReply = true,
                    ChatId = callbackQuery.user_id,
                    MessageId = callbackQuery.msg_id,
                }, GenerateButtons(omdbItem)
                );
            }
        }

        public async Task HandleDocument(TL.UpdateBotCallbackQuery callbackQuery)
        {
            var data = Encoding.UTF8.GetString(callbackQuery.data);
            var documentId = CallbackDataSerializer.ParseDocumentFileParams(data);
            var document = await _context.Documents.FirstOrDefaultAsync(x => x.Id == Guid.Parse(documentId));
            var messageWithDocument = await _bot.ForwardMessage(_settings.BinChannelChatID, _settings.ChannelFileChatID, document.MessageId);
            var detailsResult = await _bot.SendMessage(callbackQuery.user_id, GenerateDocumentResponseMessage(document), Telegram.Bot.Types.Enums.ParseMode.Html);
            await _bot.SendDocument(callbackQuery.user_id, messageWithDocument.Document.FileId ?? messageWithDocument.Video.FileId, replyParameters: new ReplyParameters
            {
                MessageId = detailsResult.MessageId,
                AllowSendingWithoutReply = true,
                ChatId = callbackQuery.user_id,
            });
        }




        public string GenerateDocumentResponseMessage(Document document)
        {
            string size;
            double fileSize = document.FileSize;

            if (fileSize < 1_024_000_000)
            {
                size = (fileSize / 1024 / 1024).ToString("0.##") + " MB";
            }
            else
            {
                size = (fileSize / 1024 / 1024 / 1024).ToString("0.##") + " GB";
            }

            var sb = new StringBuilder();


            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🎬", _messages.CallbackQueryResponse.FileName, "\u200F", false);

            sb.AppendLine(document.FileName);
            sb.AppendLine();

            sb.AppendLine($"\u200F┓━━━ℹ️ {_messages.InlineQuerySearchResponse.Information} ━━━━━━━━");

            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🇮🇷", _messages.CallbackQueryResponse.IsSoftSubbed, document.IsSubbed ? _messages.Common.Yes : _messages.Common.No, false);

            MessageGeneratorHelper.AppendIfNotEmpty(sb, "🔹", _messages.CallbackQueryResponse.Size, size, false);

            if (!string.IsNullOrWhiteSpace(document.Resolution)
                && !document.Resolution.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            {
                MessageGeneratorHelper.AppendIfNotEmpty(sb, "🌟", _messages.CallbackQueryResponse.Resolution, $"{document.Resolution ?? "720"}p", false);
            }


            if (!string.IsNullOrWhiteSpace(document.Encoder)
                && !document.Encoder.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            {
                MessageGeneratorHelper.AppendIfNotEmpty(sb, "🔐", _messages.CallbackQueryResponse.Encoder, document.Encoder.ToUpper(), false);
            }

            if (!string.IsNullOrWhiteSpace(document.Codec)
                && document.Codec.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            {
                MessageGeneratorHelper.AppendIfNotEmpty(sb, "🈴", _messages.CallbackQueryResponse.Codec, document.Codec, false);
            }

            sb.AppendLine("\u200F┛━━━━━━━━━━━━━━━━━\n");

            return sb.ToString();
        }

        public InlineKeyboardMarkup GenerateButtons(OmdbItem omdbItem)
        {
            var keyboard = new List<InlineKeyboardButton[]>();

            foreach (var document in omdbItem.Documents)
            {
                keyboard.Add(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData(GenerateButtonTitle(document), CallbackDataSerializer.CreateGetDocumentParams(document.Id.ToString()))
                });
            }

            return new InlineKeyboardMarkup(keyboard);
        }


        private string GenerateButtonTitle(Document document)
        {
            string size;
            double fileSize = document.FileSize;

            if (fileSize < 1_024_000_000)
            {
                size = (fileSize / 1024 / 1024).ToString("0.##") + " MB";
            }
            else
            {
                size = (fileSize / 1024 / 1024 / 1024).ToString("0.##") + " GB";
            }

            var sb = new StringBuilder();

            if (document.IsSubbed)
            {
                sb.Append("🇮🇷");
            }

            sb.Append(size);

            if (!document.Encoder.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($" | {document.Resolution ?? "720"}p");
            }

            if (!string.IsNullOrWhiteSpace(document.Encoder)
                && !document.Encoder.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($" | {document.Encoder.ToUpper()}");
            }
            if (!string.IsNullOrWhiteSpace(document.Codec)
                && !document.Codec.Contains("unknown", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($" | {document.Codec.ToUpper()}");
            }

            return sb.ToString();
        }
        public string GenerateDownloadMessage(MessageType type)
        {
            if (type == MessageType.SelectDocument)
            {
                return _messages.CallbackQueryResponse.SelectDocument;
            }

            throw new Exception("This message type does not exist.");
        }

        public enum MessageType
        {
            SelectDocument,
            SelectSeason,
            SelectEpisode
        }
    }
}
