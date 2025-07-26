using Domain.Entities;
using Domain.Enums;
using Infrastructure.BackgroundServices.Models;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;
using WTelegram.Types;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class FileUploadProcessor
    {
        private readonly WTelegram.Bot _botClient;
        private readonly ILogger<FileUploadProcessor> _logger;
        private readonly DownloadContext _downloadContext;
        private readonly DataContext _dataContext;
        private readonly TelegramBotSettings _settings;
        public FileUploadProcessor(WTelegram.Bot botClient, IOptions<TelegramBotSettings> settings, DownloadContext downloadContext, DataContext dataContext, ILogger<FileUploadProcessor> logger)
        {
            _dataContext = dataContext;
            _settings = settings.Value;
            _downloadContext = downloadContext;
            _logger = logger;
            _botClient = botClient;
        }

        public async Task<bool> StartUploadProcess(Guid taskId)
        {
            var task = await _downloadContext.TorrentTasks
                .Include(x => x.SubtitleVideoPairs)
                .Include(x => x.TaskUploadProgress)
                .FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
            {
                return false;
            }

            if (task.SubtitleVideoPairs.Count == 0)
            {
                task.State = TorrentTaskState.Error;
                task.UpdatedAt = DateTime.UtcNow;
                task.ErrorMessage = $"No SubtitleVideoPairs found for task with id {taskId}";
                await _downloadContext.SaveChangesAsync();
                return false;
            }

            if (task.TaskType == TorrentTaskType.Movie || task.TaskType == TorrentTaskType.SingleEpisode)
            {
                var message = UploadSingleFile(task);
                return true;
            }
            else
            {
                // return all other cases
                return true;
            }
        }

        private async Task<bool> UploadSingleFile(TorrentTask task)
        {
            var filePath = task.SubtitleVideoPairs.FirstOrDefault()?.FinalPath;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            if (task.TaskUploadProgress.Count != 0)
            {
                task.UpdatedAt = DateTime.UtcNow;
                task.State = TorrentTaskState.InUploaderButUploadingNotStarted;
                _downloadContext.RemoveRange(task.TaskUploadProgress);
                var removeRes = await _downloadContext.SaveChangesAsync() > 0;
                if (!removeRes) return false;
            }

            var uploadProgress = new TaskUploadProgress
            {
                FileName = Path.GetFileName(filePath),
                Progress = 0,
                TorrentTask = task,
                Read = 0,
                Total = 0
            };

            _downloadContext.TaskUploadProgress.Add(uploadProgress);
            var saveInit = await _downloadContext.SaveChangesAsync() > 0;
            if (!saveInit) return false;

            using var fileStream = File.OpenRead(filePath);

            var fileLength = fileStream.Length;
            uploadProgress.Total = fileLength;
            task.State = TorrentTaskState.InUploaderAndUploadingStarted;

            long lastSaved = 0;
            long saveIntervalBytes = fileLength / 20;

            var progressStream = new ProgressStream(fileStream, async (read, total) =>
            {
                uploadProgress.Read = read;
                uploadProgress.Progress = (int)(100.0 * read / total);

                _logger.LogInformation("Uploading {uploadProgress.FileName}: {uploadProgress.Progress}%", uploadProgress.FileName, uploadProgress.Progress);

                if (read - lastSaved >= saveIntervalBytes || read == total)
                {
                    lastSaved = read;
                    try
                    {
                        _downloadContext.TaskUploadProgress.Update(uploadProgress);
                        await _downloadContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Progress DB update failed: {ex.Message}");
                    }
                }
            });

            try
            {
                var uploadResult = await _botClient.SendDocument(
                    chatId: _settings.ChannelFileChatID,
                    document: progressStream,
                    caption: GenerateCaption(task, Path.GetFileName(filePath))
                );

                var storeResult = await StoreDocumentInDatabase(uploadResult);




            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload failed: {ex.Message}");
                return false;
            }
        }


        private async Task<bool> StoreDocumentInDatabase(Message message, TorrentTask task)
        {
            if (task.TaskType == TorrentTaskType.Movie)
            {
                var OMDbItem = await _dataContext.OmdbItems.FirstOrDefaultAsync(x => x.ImdbId == task.ImdbId);

                if (OMDbItem == null)
                {
                    throw new NotImplementedException();
                }

                if (OMDbItem.Type != OmdbItemType.Movie)
                {
                    throw new NotImplementedException();
                }

                var fileName = message.Document.FileName ?? message.Video.FileName;
                var document = new Document
                {
                    ChatId = _settings.ChannelFileChatID,
                    ChatName = message.Chat.Username,
                    MimeType = message.Document.MimeType ?? message.Video.MimeType ?? "unknown",
                    FileName = fileName,
                    FileSize = message.Document.FileSize ?? message.Video.FileSize ?? -1,
                    FileId = message.Document.FileId ?? message.Video.FileId,
                    UniqueFileId = message.Document.FileUniqueId ?? message.Video.FileUniqueId,
                    MessageId = message.MessageId,
                    Type = DocumentType.Movie,
                    OmdbItem = OMDbItem,
                    Codec = ReleaseInfo.GetCodec(fileName),
                    Encoder = ReleaseInfo.GetEncoder(fileName),
                    Resolution = ReleaseInfo.GetResolution(fileName),
                    IsSubbed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                _dataContext.Documents.Add(document);
                var res = await _dataContext.SaveChangesAsync() > 0;
                if (res)
                {
                    return true;
                }

                return false;
            }

            throw new NotImplementedException();
        }

        private async Task<bool> UpdateTask(Message message)
        {
            return true;
        }

        private static string GenerateCaption(TorrentTask task, string FileName)
        {
            return $$"""
            __start__
            taskId:{{task.Id}}
            imdbId:{{task.ImdbId}}
            __end__
            name:{{FileName}}
            """;
        }

        private Task UpdateUploadProgress(TaskUploadProgress progress, TorrentTask task)
        {
            throw new NotImplementedException();
        }
    }
}