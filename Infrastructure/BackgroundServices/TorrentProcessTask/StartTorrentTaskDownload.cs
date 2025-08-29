using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Models.Qbit;
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
        
        public async Task<bool> ExecuteAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            // Respect cancellation at the entry point
            Console.WriteLine($"Download TTT With Id {taskId}");
            cancellationToken.ThrowIfCancellationRequested();

            var task = await _context.TorrentTasks
                .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
            if (task == null)
            {
                return false;
            }

            try
            {
                await StartDownload(task, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // cancellation requested — bail out gracefully
                return false;
            }
            catch (Exception ex)
            {
                task.ErrorMessage = $"Error starting task in Qbit: {ex.Message}";
                task.State = TorrentTaskState.Error;
                await _context.SaveChangesAsync(cancellationToken);
                return false;
            }

            try
            {
                var result = await UpdateProgress(task, cancellationToken);
                if (result)
                {
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                // cancellation requested during progress loop
                return false;
            }
            catch (Exception ex)
            {
                task.ErrorMessage = $"Error during progress update: {ex.Message}";
                task.State = TorrentTaskState.Error;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return false;
        }

        private async Task StartDownload(TorrentTask task, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            // call to qbit client (assumed non-cancellable); check token before/after
            cancellationToken.ThrowIfCancellationRequested();
            var isAdded = await _qbitClient.AddTorrentAsync(task.Magnet);
            cancellationToken.ThrowIfCancellationRequested();

            task.DownloadStartTime = DateTime.UtcNow;

            if (!isAdded)
            {
                throw new Exception($"Failed to add torrent to qBit for task {task.Id}");
            }

            task.State = TorrentTaskState.InQbitButDownloadNotStarted;
        }

        private async Task<bool> UpdateProgress(TorrentTask task, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progress = await _context.TaskDownloadProgress
                .FirstOrDefaultAsync(x => x.TorrentTaskId == task.Id, cancellationToken);

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
                await _context.TaskDownloadProgress.AddAsync(progress, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            task = await _context.TorrentTasks
                .Include(x => x.TaskDownloadProgress)
                .FirstOrDefaultAsync(x => x.Id == task.Id, cancellationToken);

            // Polling loop — respect cancellation and use cancellable delay
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // fetch latest torrent info from qbit (non-cancellable call assumed)
                var latest = await _qbitClient.GetQbitTorrentByHashAsync(task.TorrentHash);

                // apply latest values
                task.TaskDownloadProgress.Progress = (decimal)latest.Progress;
                task.TaskDownloadProgress.Size = latest.Size;
                task.TaskDownloadProgress.Speed = latest.DownloadSpeed;
                task.UpdatedAt = DateTime.UtcNow;

                if (IsDownloadFinished(task, latest))
                {
                    task.State = TorrentTaskState.TorrentWasDownloaded;
                    task.DownloadEndTime = DateTime.UtcNow;
                    task.FileSavingPath = latest.ContentPath;
                    await _context.SaveChangesAsync(cancellationToken);
                    return true;
                }

                if (IsDownloadOpportunityOver(task))
                {
                    task.State = TorrentTaskState.TorrentTimedOut;
                    task.ErrorMessage = "Download timed out And Failed to download in time windows.";
                    await _context.SaveChangesAsync(cancellationToken);
                    return false;
                }

                if (IsDownloadFailedToGetMetaData(task, latest))
                {
                    task.State = TorrentTaskState.Error;
                    task.ErrorMessage = "Failed to get metadata in time.";
                    await _context.SaveChangesAsync(cancellationToken);
                    return false;
                }

                await _context.SaveChangesAsync(cancellationToken);

                // cancellable delay
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private bool IsDownloadFailedToGetMetaData(TorrentTask task, QbitTorrentInfo torrentDetails)
        {
            bool isMetaState =
                torrentDetails.State == QbitTorrentState.FetchingMetadata ||
                torrentDetails.State == QbitTorrentState.ForcedFetchingMetadata;

            if (!isMetaState && task.State == TorrentTaskState.InQbitButDownloadNotStarted)
            {
                task.State = TorrentTaskState.InQbitAndDownloadStarted;
            }
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
