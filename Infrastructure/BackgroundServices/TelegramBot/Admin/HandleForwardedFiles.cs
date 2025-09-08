using Domain.Entities;
using Infrastructure.BackgroundServices.TelegramBot.Configs;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;
using WTelegram;

namespace Infrastructure.BackgroundServices.TelegramBot.Admin
{
    public class HandleForwardedFiles
    {
        private readonly Bot _bot;
        private readonly IsAdmin _isAdmin;
        private readonly ILogger<HandleForwardedFiles> _logger;
        private readonly DataContext _context;
        private readonly TelegramBotSettings _settings;

        public HandleForwardedFiles(WTelegram.Bot bot, IOptions<TelegramBotSettings> settings, IsAdmin isAdmin,
            ILogger<HandleForwardedFiles> logger, DataContext context)
        {
            _bot = bot;
            _isAdmin = isAdmin;
            _logger = logger;
            _context = context;
            _settings = settings.Value;
        }

        public async Task<bool> ShouldHandleMessage(TL.UpdateNewChannelMessage updateNewChannelMessage)
        {
            var isForwardChannel = updateNewChannelMessage.message.Peer.ID == _settings.ForwardChannelPeerID;
            var isAdmin = await _isAdmin.Check(updateNewChannelMessage);

            return isForwardChannel && isAdmin;
        }

        public async Task HandleAsync(TL.UpdateNewChannelMessage updateNewChannelMessage)
        {
            var message = await _bot.GetMessagesById(_settings.ForwardChannelID, [updateNewChannelMessage.message.ID]);
            if (message.Count == 0)
            {
                _logger.LogWarning("No messages found for updateNewChannelMessage in handleForwardedFiles");
                return;
            }

            var forwardResult =
                await _bot.ForwardMessage(_settings.ChannelFileChatID, _settings.ForwardChannelID, message[0].Id);

            string fileName;
            string fileId;
            string uniqueFileId;
            long? fileSize;
            string mimeType;
            if (forwardResult.Document is { FileName: not null } doc)
            {
                fileName = doc.FileName;
                fileId = doc.FileId;
                uniqueFileId = doc.FileUniqueId;
                fileSize = doc.FileSize;
                mimeType = doc.MimeType;
            }
            else if (forwardResult.Video is { FileName: not null } vid)
            {
                fileName = vid.FileName;
                fileId = vid.FileId;
                uniqueFileId = vid.FileUniqueId;
                fileSize = vid.FileSize;
                mimeType = vid.MimeType;
            }
            else
            {
                _logger.LogWarning("No video nor document found...");
                return;
            }

            var tempFileId = $"temp_WTelegramBot.{fileId}";
            var tempUniqueFileId = $"temp_WTelegramBot.{uniqueFileId}";

            var alreadyExisting =
                await _context.Documents.Where(x =>
                        x.UniqueFileId == uniqueFileId || x.UniqueFileId == tempUniqueFileId)
                    .ToListAsync();


            if (alreadyExisting.Count != 0)
            {
                foreach (var existingDocument in alreadyExisting)
                {
                    existingDocument.UpdatedAt = DateTime.UtcNow;
                }
                
                await _context.SaveChangesAsync();
                _logger.LogError("forwarded file was not unique at uniqueFileId {fileName}", fileName);
                return;
            }

            var document = new Document
            {
                ChatId = _settings.ChannelFileChatID,
                ChatName = forwardResult.Chat.Title,
                Codec = ReleaseInfo.GetCodec(fileName),
                Encoder = ReleaseInfo.GetEncoder(fileName),
                Resolution = ReleaseInfo.GetResolution(fileName),
                FileName = fileName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                FileId = tempFileId,
                UniqueFileId = tempUniqueFileId,
                FileSize = fileSize.GetValueOrDefault(),
                MimeType = mimeType,
                MessageId = forwardResult.MessageId,
            };
            _context.Documents.Add(document);

            var storeResult = await _context.SaveChangesAsync() > 0;

            if (!storeResult)
            {
                _logger.LogError("Failed to store new files in database.");
                return;
            }

            _logger.LogInformation("new temporary documents were added to the database.");
        }
    }
}