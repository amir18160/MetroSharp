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
        private readonly IDbContextFactory<DownloadContext> _contextFactory;
        private readonly TorrentTaskSettings _settings;

        public StartTorrentTaskDownload(
            IQbitClient qbitClient,
            IDbContextFactory<DownloadContext> contextFactory,
            IOptions<TorrentTaskSettings> settings)
        {
            _qbitClient = qbitClient;
            _contextFactory = contextFactory;
            _settings = settings.Value;
        }

        public async Task<bool> ExecuteAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Load task once just to assert it exists (short-lived context)
            await using (var ctx = _contextFactory.CreateDbContext())
            {
                var exists = await ctx.TorrentTasks.AnyAsync(x => x.Id == taskId, cancellationToken);
                if (!exists) return false;
            }

            try
            {
                var started = await StartDownload(taskId, cancellationToken);
                if (!started) return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                // record error on task (use a fresh context)
                await using var ctxErr = _contextFactory.CreateDbContext();
                var t = await ctxErr.TorrentTasks.FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
                if (t != null)
                {
                    t.ErrorMessage = $"Error starting task in Qbit: {ex.Message}";
                    t.State = TorrentTaskState.Error;
                    await ctxErr.SaveChangesAsync(CancellationToken.None); // best-effort
                }
                return false;
            }

            try
            {
                var result = await UpdateProgress(taskId, cancellationToken);
                return result;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                await using var ctxErr = _contextFactory.CreateDbContext();
                var t = await ctxErr.TorrentTasks.FirstOrDefaultAsync(x => x.Id == taskId);
                if (t != null)
                {
                    t.ErrorMessage = $"Error during progress update: {ex.Message}";
                    t.State = TorrentTaskState.Error;
                    await ctxErr.SaveChangesAsync(CancellationToken.None);
                }
            }

            return false;
        }

        private async Task<bool> StartDownload(Guid taskId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // create a fresh context for atomic small updates
            await using var context = _contextFactory.CreateDbContext();

            var task = await context.TorrentTasks.FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
            if (task == null) return false;

            if (string.IsNullOrEmpty(task.Magnet))
                throw new Exception($"Magnet is empty for task {task.Id}");

            var torrentHash = TorrentUtilities.ExtractHashFromMagnet(task.Magnet);
            if (torrentHash == null)
                throw new Exception($"Failed to extract hash from magnet for task {task.Id}");

            task.TorrentHash = torrentHash;
            task.UpdatedAt = DateTime.UtcNow;

            // commit small change BEFORE calling external network
            await context.SaveChangesAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            var isAdded = await _qbitClient.AddTorrentAsync(task.Magnet);
            cancellationToken.ThrowIfCancellationRequested();

            task.DownloadStartTime = DateTime.UtcNow;

            if (!isAdded)
            {
                task.State = TorrentTaskState.Error;
                task.ErrorMessage = $"Failed to add torrent to qBit for task {task.Id}";
                await context.SaveChangesAsync(cancellationToken);
                return false;
            }

            task.State = TorrentTaskState.InQbitButDownloadNotStarted;
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }

        private async Task<bool> UpdateProgress(Guid taskId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Ensure a progress row exists: use short-lived context
            await using (var ctx = _contextFactory.CreateDbContext())
            {
                var progress = await ctx.TaskDownloadProgress
                    .FirstOrDefaultAsync(x => x.TorrentTaskId == taskId, cancellationToken);

                if (progress == null)
                {
                    progress = new TaskDownloadProgress
                    {
                        Id = Guid.NewGuid(),
                        Progress = 0,
                        Size = 0,
                        Speed = 0,
                        TorrentTaskId = taskId
                    };
                    await ctx.TaskDownloadProgress.AddAsync(progress, cancellationToken);
                    await ctx.SaveChangesAsync(cancellationToken);
                }
            }

            // Polling loop — every iteration uses a fresh DbContext
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Get latest from qbit (external call)
                // You could also bail if torrent hash missing; reload it inside the loop below
                // but for clarity we'll reload the entity each iteration.
                await using var ctx = _contextFactory.CreateDbContext();

                var task = await ctx.TorrentTasks
                    .Include(x => x.TaskDownloadProgress)
                    .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

                if (task == null)
                {
                    return false; // task removed
                }

                // refresh latest info from qbit (non-cancellable assumed)
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
                    await ctx.SaveChangesAsync(cancellationToken);
                    return true;
                }

                if (IsDownloadOpportunityOver(task))
                {
                    task.State = TorrentTaskState.TorrentTimedOut;
                    task.ErrorMessage = "Download timed out And Failed to download in time windows.";
                    await ctx.SaveChangesAsync(cancellationToken);
                    return false;
                }

                if (IsDownloadFailedToGetMetaData(task, latest))
                {
                    task.State = TorrentTaskState.Error;
                    task.ErrorMessage = "Failed to get metadata in time.";
                    await ctx.SaveChangesAsync(cancellationToken);
                    return false;
                }

                // regular update
                await ctx.SaveChangesAsync(cancellationToken);

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
