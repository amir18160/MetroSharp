using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Diagnostics;
using System.Linq;


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

        public async Task CleanUpAsync(Guid taskId)
        {
            var task = await _context.TorrentTasks.FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
            {
                _logger.LogWarning("Task with ID {taskId} not found for cleanup.", taskId);
                return;
            }

            _logger.LogInformation("Starting cleanup for task {taskId}.", taskId);

            // 0. Kill FFmpeg process
            KillFFmpegProcess(taskId);


            // 1. Delete torrent from qBittorrent client
            if (!string.IsNullOrEmpty(task.TorrentHash))
            {
                try
                {
                    bool deleted = await _qbitClient.DeleteTorrentAsync(task.TorrentHash, true);
                    if (deleted)
                    {
                        _logger.LogInformation("Successfully deleted torrent with hash {hash} from qBittorrent.", task.TorrentHash);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to delete torrent with hash {hash} from qBittorrent.", task.TorrentHash);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting torrent with hash {hash} from qBittorrent.", task.TorrentHash);
                }
            }

            // 2. Delete downloaded content directory
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

            // 3. Delete subtitle directory
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
            
            _logger.LogInformation("Cleanup for task {taskId} completed.", taskId);
        }
        
        private void KillFFmpegProcess(Guid taskId)
        {
            var task = _context.TorrentTasks.Find(taskId);
            if (task?.FfmpegPID.HasValue == true)
            {
                try
                {
                    var process = Process.GetProcessById(task.FfmpegPID.Value);
                    process.Kill();
                    _logger.LogInformation("Killed ffmpeg process {ProcessId} for task {TaskId}", process.Id, taskId);
                }
                catch (ArgumentException)
                {
                    // Process already exited.
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not kill ffmpeg process with PID {pid} for task {TaskId}", task.FfmpegPID.Value, taskId);
                }
                finally
                {
                    task.FfmpegPID = null;
                    _context.SaveChanges();
                }
            }
        }
    }
}