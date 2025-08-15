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

        public async Task<bool> StartUploadProcess(Guid taskId, CancellationToken cancellationToken = default)
        {
            // respect cancellation right away
            cancellationToken.ThrowIfCancellationRequested();

            var task = await _downloadContext.TorrentTasks
                .Include(x => x.SubtitleVideoPairs)
                .Include(x => x.TaskUploadProgress)
                .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {taskId} not found for upload.", taskId);
                return false;
            }

            if (!task.SubtitleVideoPairs.Any(p => !p.Ignore && !string.IsNullOrEmpty(p.FinalPath) && File.Exists(p.FinalPath)))
            {
                task.State = TorrentTaskState.Error;
                task.UpdatedAt = DateTime.UtcNow;
                task.ErrorMessage = $"No valid files found to upload for task with id {taskId}";
                await _downloadContext.SaveChangesAsync(cancellationToken);
                return false;
            }

            if (task.TaskUploadProgress.Count != 0)
            {
                _downloadContext.RemoveRange(task.TaskUploadProgress);
                if (!(await _downloadContext.SaveChangesAsync(cancellationToken) > 0))
                {
                    _logger.LogError("Failed to clear previous upload progress for task {taskId}", task.Id);
                    return false;
                }
            }

            var orderedPairs = task.SubtitleVideoPairs
                .Where(p => !p.Ignore && !string.IsNullOrEmpty(p.FinalPath) && File.Exists(p.FinalPath))
                .OrderBy(p => p.SeasonNumber)
                .ThenBy(p => p.EpisodeNumber)
                .ToList();

            bool allUploadsSuccessful = true;
            var failureCount = 0;
            foreach (var pair in orderedPairs)
            {
                // check cancellation between files
                cancellationToken.ThrowIfCancellationRequested();

                bool uploadSuccess = await UploadFileFromPair(task, pair, cancellationToken);
                if (!uploadSuccess)
                {
                    allUploadsSuccessful = false;
                    failureCount++;
                    _logger.LogError("Upload failed for pair {pairId} in task {taskId}. continue remaining uploads.", pair.Id, task.Id);
                }
            }

            if (allUploadsSuccessful)
            {
                task.State = TorrentTaskState.Completed;
                _logger.LogInformation("All files for task {taskId} uploaded successfully.", task.Id);
            }
            else
            {
                task.State = TorrentTaskState.Error;
                _logger.LogInformation("Number of Failed Documents is: {failureCount}", failureCount);
                task.ErrorMessage = $"One or more files failed to upload. number of failed documents: {failureCount}";
            }

            await _downloadContext.SaveChangesAsync(cancellationToken);
            return allUploadsSuccessful;
        }

        private async Task<bool> UploadFileFromPair(TorrentTask task, SubtitleVideoPair pair, CancellationToken cancellationToken)
        {
            // allow early cancellation
            cancellationToken.ThrowIfCancellationRequested();

            var filePath = pair.FinalPath;

            try
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > _settings.MaxAllowedUploadSize)
                {
                    _logger.LogError("File {fileName} is larger than 2GB and cannot be uploaded to Telegram.", Path.GetFileName(filePath));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not get file info for {filePath}.", filePath);
                return false;
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
            if (!(await _downloadContext.SaveChangesAsync(cancellationToken) > 0))
            {
                _logger.LogError("Failed to save initial upload progress for file {fileName}", uploadProgress.FileName);
                return false;
            }

            // Keep a registration so we can close the stream when cancellation is requested
            CancellationTokenRegistration? streamCloseReg = null;

            try
            {
                using var fileStream = File.OpenRead(filePath);

                // register a callback that attempts to close the stream when cancellation is requested
                streamCloseReg = cancellationToken.Register(() =>
                {
                    try
                    {
                        fileStream.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Exception while disposing file stream during cancellation for task {TaskId}", task.Id);
                    }
                });

                var fileLength = fileStream.Length;
                uploadProgress.Total = fileLength;
                task.State = TorrentTaskState.InUploaderAndUploadingStarted;
                await _downloadContext.SaveChangesAsync(cancellationToken);

                long lastSaved = 0;
                long saveIntervalBytes = Math.Max(1, fileLength / 20);

                var progressStream = new ProgressStream(fileStream, (read, total) =>
                {
                    // This callback runs on IO path — check cancellation and abort by throwing
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException(cancellationToken);

                    uploadProgress.Read = read;
                    uploadProgress.Progress = (int)(100.0 * read / total);

                    _logger.LogInformation("Uploading {fileName}: {progress}%", uploadProgress.FileName, uploadProgress.Progress);

                    if (read - lastSaved >= saveIntervalBytes || read == total)
                    {
                        lastSaved = read;
                        try
                        {
                            // keep original synchronous behavior to avoid changing surrounding logic
                            _downloadContext.TaskUploadProgress.Update(uploadProgress);
                            _downloadContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Progress DB update failed.");
                        }
                    }
                });

                // create input for Telegram upload
                var inputFile = Telegram.Bot.Types.InputFile.FromStream(progressStream, Path.GetFileName(filePath));

                // perform upload — if cancellation happens, stream disposal or exception from callback should abort it
                var uploadResult = await _botClient.SendDocument(
                    chatId: _settings.ChannelFileChatID,
                    document: inputFile,
                    caption: GenerateCaption(task, Path.GetFileName(filePath))
                );

                // store document in DB (pass cancellation token)
                return await StoreDocumentInDatabase(uploadResult, task, pair, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Upload was cancelled for file {filePath} in task {taskId}.", filePath, task.Id);
                // mark as cancelled / keep consistent with other flows
                try
                {
                    var t = await _downloadContext.TorrentTasks.FindAsync(new object[] { task.Id }, CancellationToken.None);
                    if (t != null)
                    {
                        t.State = TorrentTaskState.Cancelled;
                        t.ErrorMessage = "Upload cancelled by user.";
                        await _downloadContext.SaveChangesAsync(CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to persist cancellation state for task {TaskId}", task.Id);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed for file {filePath} in task {taskId}.", filePath, task.Id);
                task.State = TorrentTaskState.Error;
                task.ErrorMessage = $"Upload failed for {Path.GetFileName(filePath)}: {ex.Message}";
                await _downloadContext.SaveChangesAsync(cancellationToken);
                return false;
            }
            finally
            {
                // dispose registration if created
                streamCloseReg?.Dispose();
            }
        }

        private async Task<bool> StoreDocumentInDatabase(Message message, TorrentTask task, SubtitleVideoPair pair, CancellationToken cancellationToken)
        {
            // respect cancellation
            cancellationToken.ThrowIfCancellationRequested();

            var fileName = message.Document?.FileName ?? message.Video?.FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogError("Could not determine filename from uploaded Telegram message for task {taskId}", task.Id);
                return false;
            }

            var document = new Document
            {
                ChatId = _settings.ChannelFileChatID,
                ChatName = message.Chat.Username,
                MimeType = message.Document?.MimeType ?? message.Video?.MimeType ?? "unknown",
                FileName = fileName,
                FileSize = message.Document?.FileSize ?? message.Video?.FileSize ?? -1,
                FileId = message.Document?.FileId ?? message.Video?.FileId,
                UniqueFileId = message.Document?.FileUniqueId ?? message.Video?.FileUniqueId,
                MessageId = message.MessageId,
                IsSubbed = pair.SubtitlesMerged,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Codec = ReleaseInfo.GetCodec(fileName),
                Encoder = ReleaseInfo.GetEncoder(fileName),
                Resolution = ReleaseInfo.GetResolution(fileName)
            };

            var omdbItem = await _dataContext.OmdbItems
                .Include(i => i.Seasons)
                .ThenInclude(s => s.Episodes)
                .FirstOrDefaultAsync(x => x.ImdbId == task.ImdbId, cancellationToken);

            if (omdbItem == null)
            {
                _logger.LogError("OMDb item with IMDB ID {imdbId} not found.", task.ImdbId);
                return false;
            }

            document.OmdbItem = omdbItem;

            if (pair.IsMovie)
            {
                if (omdbItem.Type != OmdbItemType.Movie)
                {
                    _logger.LogError("OMDb item type mismatch. Expected Movie, got {type} for IMDB ID {imdbId}.", omdbItem.Type, task.ImdbId);
                    return false;
                }
                document.Type = DocumentType.Movie;
            }
            else // It's a series
            {
                if (omdbItem.Type != OmdbItemType.Series)
                {
                    _logger.LogError("OMDb item type mismatch. Expected Series, got {type} for IMDB ID {imdbId}.", omdbItem.Type, task.ImdbId);
                    return false;
                }

                if (pair.SeasonNumber.HasValue && pair.EpisodeNumber.HasValue)
                {
                    var season = omdbItem.Seasons.FirstOrDefault(s => s.SeasonNumber == pair.SeasonNumber.Value);
                    if (season == null)
                    {
                        _logger.LogInformation("Season {seasonNumber} not found for series {imdbId}. Creating it.", pair.SeasonNumber.Value, task.ImdbId);
                        season = new Season { SeasonNumber = pair.SeasonNumber.Value, OmdbItem = omdbItem };
                        omdbItem.Seasons.Add(season);
                    }

                    var episode = season.Episodes.FirstOrDefault(e => e.EpisodeNumber == pair.EpisodeNumber.Value);
                    if (episode == null)
                    {
                        _logger.LogInformation("Episode {episodeNumber} of Season {seasonNumber} not found for series {imdbId}. Creating it.", pair.EpisodeNumber.Value, pair.SeasonNumber.Value, task.ImdbId);
                        episode = new Episode { EpisodeNumber = pair.EpisodeNumber.Value, Season = season };
                        season.Episodes.Add(episode);
                    }

                    document.Type = DocumentType.Episode;
                    document.Episode = episode;
                }
                else
                {
                    _logger.LogError("Cannot process series document for task {taskId} because SeasonNumber or EpisodeNumber is null.", task.Id);
                    return false;
                }
            }

            _dataContext.Documents.Add(document);
            var res = await _dataContext.SaveChangesAsync(cancellationToken) > 0;

            if (res)
            {
                _logger.LogInformation("Successfully stored document for file {fileName} in task {taskId}.", fileName, task.Id);
                return true;
            }

            _logger.LogError("Failed to save document to database for file {fileName} in task {taskId}.", fileName, task.Id);
            return false;
        }

        private static string GenerateCaption(TorrentTask task, string fileName)
        {
            return $$"""
            __start__
            taskId:{{task.Id}}
            imdbId:{{task.ImdbId}}
            __end__
            name:{{fileName}}
            """;
        }
    }
}
