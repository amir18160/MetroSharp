using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Models.Qbit;
using Infrastructure.BackgroundServices.Models;
using Infrastructure.QbitTorrentClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class StartTorrentTaskDownload
    {
        private readonly IQbitClient _qbitClient;
        private readonly DownloadContext _context;
        private readonly TorrentTaskSettings _settings;

        public StartTorrentTaskDownload(
            IQbitClient qbitClient,
            DownloadContext context,
            IOptions<TorrentTaskSettings> settings)
        {
            _context = context;
            _qbitClient = qbitClient;
            _settings = settings.Value;
        }

        public async Task<bool> ExecuteAsync(Guid taskId)
        {
            var task = await _context.TorrentTasks.FirstOrDefaultAsync(x => x.Id == taskId);
            if (task == null)
            {
                return false;
            }

            try
            {
                await StartDownload(task);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                task.ErrorMessage = $"Error starting task in Qbit: {ex.Message}";
                task.State = TorrentTaskState.Error;
                await _context.SaveChangesAsync();
                return false;
            }

            try
            {
                await UpdateProgress(task);
            }
            catch (Exception ex)
            {
                task.ErrorMessage = $"Error during progress update: {ex.Message}";
                task.State = TorrentTaskState.Error;
                await _context.SaveChangesAsync();
                return false;
            }

            return true;
        }

        private async Task StartDownload(TorrentTask task)
        {
            if (string.IsNullOrEmpty(task.Magnet))
            {
                throw new Exception($"Magnet is empty for task {task.Id}");
            }

            var torrentHash = TorrentUtilities.ExtractHashFromMagnet(task.Magnet);
            if (torrentHash == null)
            {
                throw new Exception($"Failed to extract hash from magnet for task {task.Id}");
            }

            task.TorrentHash = torrentHash;
            task.UpdatedAt = DateTime.UtcNow;

            var isAdded = await _qbitClient.AddTorrentAsync(task.Magnet);
            task.DownloadStartTime = DateTime.UtcNow;

            if (!isAdded)
            {
                throw new Exception($"Failed to add torrent to qBit for task {task.Id}");
            }

            task.State = TorrentTaskState.InQbitButDownloadNotStarted;
        }

        private async Task UpdateProgress(TorrentTask task)
        {
            var progress = await _context.TaskDownloadProgress
                .FirstOrDefaultAsync(x => x.TorrentTaskId == task.Id);

            if (progress == null)
            {
                progress = new TaskDownloadProgress
                {
                    Id = Guid.NewGuid(),
                    Progress = 0,
                    Size = 0,
                    Speed = 0,
                    TorrentTaskId = task.Id
                };
                await _context.TaskDownloadProgress.AddAsync(progress);
                await _context.SaveChangesAsync();
            }

            task = await _context.TorrentTasks
                .Include(x => x.TaskDownloadProgress)
                .FirstOrDefaultAsync(x => x.Id == task.Id);

            while (true)
            {
                var latest = await _qbitClient.GetQbitTorrentByHashAsync(task.TorrentHash);
                task.TaskDownloadProgress.Progress = (decimal)latest.Progress;
                task.TaskDownloadProgress.Size = latest.Size;
                task.TaskDownloadProgress.Speed = latest.DownloadSpeed;
                task.UpdatedAt = DateTime.UtcNow;

                if (IsDownloadFinished(task, latest))
                {
                    task.State = TorrentTaskState.TorrentWasDownloaded;
                    task.DownloadEndTime = DateTime.UtcNow;
                    task.FileSavingPath = latest.SavePath;
                    await _context.SaveChangesAsync();
                    break;
                }

                if (IsDownloadOpportunityOver(task))
                {
                    task.State = TorrentTaskState.TorrentTimedOut;
                    await _context.SaveChangesAsync();
                    break;
                }

                if (IsDownloadFailedToGetMetaData(task, latest))
                {
                    task.State = TorrentTaskState.Error;
                    task.ErrorMessage = "Failed to get metadata in time.";
                    await _context.SaveChangesAsync();
                    break;
                }

                await _context.SaveChangesAsync();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private bool IsDownloadFailedToGetMetaData(TorrentTask task, QbitTorrentInfo torrentDetails)
        {
            bool isMetaState =
                torrentDetails.State == QbitTorrentState.FetchingMetadata ||
                torrentDetails.State == QbitTorrentState.ForcedFetchingMetadata;

            bool exceeded = DateTime.UtcNow > task.DownloadStartTime?.AddMinutes(_settings.MaxAllowedTimeToGetMetaData);

            return isMetaState && exceeded;
        }

        private bool IsDownloadOpportunityOver(TorrentTask task)
        {
            return DateTime.UtcNow > task.DownloadStartTime?.AddMinutes(_settings.MaxAllowedDownloadTimeInMinutes);
        }

        private bool IsDownloadFinished(TorrentTask task, QbitTorrentInfo torrentDetails)
        {
            bool isComplete =
                torrentDetails.State == QbitTorrentState.CheckingUpload ||
                torrentDetails.State == QbitTorrentState.PausedUpload ||
                torrentDetails.Progress == 1;

            if (isComplete)
            {
                task.TorrentTitle = torrentDetails.Name;
                return true;
            }

            return false;
        }
    }
}
