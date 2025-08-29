using System.Diagnostics;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TaskCleaner
    {
        private readonly DownloadContext _context;
        private readonly IQbitClient _qbitClient;
        private readonly ILogger<TaskCleaner> _logger;

        public TaskCleaner(DownloadContext context, IQbitClient qbitClient, ILogger<TaskCleaner> logger)
        {
            _context = context;
            _qbitClient = qbitClient;
            _logger = logger;
        }


        public async Task CleanUpAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            // Load the task (allow cancellation here, but proceed with best-effort if token already cancelled)
            var task = await _context.TorrentTasks
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

            if (task == null || task.State == TorrentTaskState.Cancelled)
            {
                // Already cleaned up or doesn't exist
                return;
            }

            _logger.LogInformation("Starting cleanup for task {taskId}.", taskId);

            // 0. Kill FFmpeg process (best-effort). Use a synchronous helper that persists using a non-cancel token.
            try
            {
                await KillFFmpegProcessAsync(taskId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "KillFFmpegProcessAsync failed for task {TaskId}", taskId);
            }

            // 1. Delete torrent from qBittorrent client
            if (!string.IsNullOrEmpty(task.TorrentHash))
            {
                try
                {
                    bool deleted = await _qbitClient.DeleteTorrentAsync(task.TorrentHash, true);
                    if (deleted)
                        _logger.LogInformation("Successfully deleted torrent with hash {hash} from qBittorrent.", task.TorrentHash);
                    else
                        _logger.LogWarning("Failed to delete torrent with hash {hash} from qBittorrent.", task.TorrentHash);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting torrent with hash {hash} from qBittorrent.", task.TorrentHash);
                }
            }

            // 2. Delete downloaded content directory (best-effort)
            if (!string.IsNullOrEmpty(task.FileSavingPath) && Directory.Exists(task.FileSavingPath))
            {
                try
                {
                    Directory.Delete(task.FileSavingPath, true);
                    _logger.LogInformation("Successfully deleted content directory: {path}", task.FileSavingPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting content directory: {path}", task.FileSavingPath);
                }
            }

            // 3. Delete subtitle directory (best-effort)
            if (!string.IsNullOrEmpty(task.SubtitleSavingPath) && Directory.Exists(task.SubtitleSavingPath))
            {
                try
                {
                    Directory.Delete(task.SubtitleSavingPath, true);
                    _logger.LogInformation("Successfully deleted subtitle directory: {path}", task.SubtitleSavingPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting subtitle directory: {path}", task.SubtitleSavingPath);
                }
            }

            // Optionally reset task state/fields if desired (use non-cancel token to ensure persistence)
            try
            {
                var tracked = await _context.TorrentTasks.FirstOrDefaultAsync(x => x.Id == taskId, CancellationToken.None);
                if (tracked != null)
                {
                    // Set Cancelled state only if task isn't completed already
                    if (tracked.State != Domain.Enums.TorrentTaskState.Completed)
                    {
                        tracked.State = Domain.Enums.TorrentTaskState.Cancelled;
                        tracked.ErrorMessage ??= "Task cleaned up after cancellation/stop.";
                        tracked.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync(CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist final cleaned state for task {TaskId}", taskId);
            }

            _logger.LogInformation("Cleanup for task {taskId} completed.", taskId);
        }

        private async Task KillFFmpegProcessAsync(Guid taskId)
        {
            // Use a non-cancel token for this persistence so we can clear PID even if cancellation already requested
            try
            {
                var task = await _context.TorrentTasks.FirstOrDefaultAsync(x => x.Id == taskId, CancellationToken.None);
                if (task?.FfmpegPID.HasValue == true)
                {
                    try
                    {
                        var process = Process.GetProcessById(task.FfmpegPID.Value);
                        try
                        {
                            process.Kill(entireProcessTree: true);
                            _logger.LogInformation("Killed ffmpeg process {ProcessId} for task {TaskId}", process.Id, taskId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not kill ffmpeg process with PID {pid} for task {TaskId}", task.FfmpegPID.Value, taskId);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Process already exited.
                    }
                    finally
                    {
                        task.FfmpegPID = null;
                        // persist with CancellationToken.None to ensure the write is attempted regardless of job token
                        await _context.SaveChangesAsync(CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error while attempting to kill ffmpeg process for task {TaskId}", taskId);
            }
        }
    }
}
