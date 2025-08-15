using System.Diagnostics;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.BackgroundServices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class FFmpegTaskProcessor
    {
        private static readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly DownloadContext _context;
        private readonly ILogger<FFmpegTaskProcessor> _logger;
        private readonly TorrentTaskSettings _settings;

        public FFmpegTaskProcessor(DownloadContext context, ILogger<FFmpegTaskProcessor> logger, IOptions<TorrentTaskSettings> settings)
        {
            _settings = settings.Value;
            _logger = logger;
            _context = context;
        }

        public async Task<bool> ProcessPairs(Guid taskId, CancellationToken cancellationToken = default)
        {
            // Respect cancellation at the entry
            cancellationToken.ThrowIfCancellationRequested();

            var task = await _context.TorrentTasks
                .Include(x => x.SubtitleVideoPairs)
                .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

            if (task == null)
            {
                return false;
            }

            task.UpdatedAt = DateTime.UtcNow;

            if (task.SubtitleVideoPairs.Count == 0)
            {
                task.State = TorrentTaskState.Error;
                task.ErrorMessage = "No pair found for this task.";
                await _context.SaveChangesAsync(cancellationToken);
                return false;
            }

            _logger.LogInformation("Task {TaskId}: Waiting to enter the FFmpeg processing gate.", taskId);

            task.State = TorrentTaskState.InFfmpegButProcessNotStarted;
            await _context.SaveChangesAsync(cancellationToken);

            // Wait for gate with cancellation support
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                _logger.LogInformation("Task {TaskId}: Entered the ffmpeg gate. Starting FFmpeg process.", taskId);

                // Check cancellation before heavy work
                cancellationToken.ThrowIfCancellationRequested();

                task.State = TorrentTaskState.InFfmpegAndProcessStarted;
                await _context.SaveChangesAsync(cancellationToken);

                await ProcessPairsInternal(task, cancellationToken);

                _logger.LogInformation("Task {TaskId}: FFmpeg process finished.", taskId);
                return true;
            }
            catch (OperationCanceledException)
            {
                // Propagate so upper-level job can set state/cleanup as needed
                _logger.LogInformation("Task {TaskId}: FFmpeg processing was cancelled.", taskId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task {TaskId}: An error occurred during FFmpeg processing.", taskId);

                try
                {
                    // attempt to mark error in DB; swallow exceptions to avoid masking original error
                    var t = await _context.TorrentTasks.FindAsync(new object[] { taskId }, CancellationToken.None);
                    if (t != null)
                    {
                        t.State = TorrentTaskState.Error;
                        t.ErrorMessage = ex.Message;
                        await _context.SaveChangesAsync(CancellationToken.None);
                    }
                }
                catch (Exception saveEx)
                {
                    _logger.LogWarning(saveEx, "Failed to persist error state for task {TaskId}", taskId);
                }

                return false;
            }
            finally
            {
                try
                {
                    _semaphore.Release();
                }
                catch (SemaphoreFullException)
                {
                    // ignore - guard against double release
                }

                _logger.LogInformation("Task {TaskId}: Exited the ffmpeg gate.", taskId);
            }
        }

        private async Task ProcessPairsInternal(TorrentTask task, CancellationToken cancellationToken)
        {
            int counter = 0;

            // re-query pairs in case the passed-in task is stale
            var orderedPairs = task.SubtitleVideoPairs
                                    .Where(p => !p.Ignore)
                                    .OrderBy(p => p.SeasonNumber)
                                    .ThenBy(p => p.EpisodeNumber)
                                    .ToList();

            foreach (var pair in orderedPairs)
            {
                // check cancellation between iterations
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Processing pair {counter} out of {total} for task {taskId}", ++counter, orderedPairs.Count, task.Id);

                if (string.IsNullOrWhiteSpace(pair.VideoPath) || !File.Exists(pair.VideoPath))
                {
                    _logger.LogWarning("Invalid or missing VideoPath for pair {pairId}. Marking as ignored.", pair.Id);
                    pair.Ignore = true;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(pair.SubtitlePath) && File.Exists(pair.SubtitlePath))
                {
                    try
                    {
                        pair.FinalPath = await AddSoftSubtitlesInPlaceAsync(pair.VideoPath, new[] { pair.SubtitlePath }, task.Id, cancellationToken);
                        pair.SubtitlesMerged = true; // Mark as successfully subbed
                        _logger.LogInformation("Successfully merged subtitles for pair {pairId}", pair.Id);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Cancelled while processing pair {pairId}. Leaving pair state as-is.", pair.Id);
                        // rethrow to allow upstream handling and to ensure we don't continue processing more pairs
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process FFmpeg task for pairId {pairId}. Using original video path.", pair.Id);
                        pair.FinalPath = pair.VideoPath;
                        pair.SubtitlesMerged = false; // Explicitly set to false on failure
                    }
                }
                else
                {
                    _logger.LogInformation("Pair {pairId} does not have a valid subtitle path. Using original video path.", pair.Id);
                    pair.FinalPath = pair.VideoPath;
                    pair.SubtitlesMerged = false; // No subtitles were merged
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> AddSoftSubtitlesInPlaceAsync(string inputVideoPath, string[] subtitlePaths, Guid taskId, CancellationToken cancellationToken)
        {
            if (!File.Exists(inputVideoPath))
            {
                throw new FileNotFoundException("Input video not found", inputVideoPath);
            }

            foreach (var sub in subtitlePaths)
            {
                if (!File.Exists(sub))
                {
                    throw new FileNotFoundException("Subtitle file not found", sub);
                }
            }

            // Respect cancellation before starting heavy work
            cancellationToken.ThrowIfCancellationRequested();

            string inputExt = Path.GetExtension(inputVideoPath);
            string baseName = Path.GetFileNameWithoutExtension(inputVideoPath);
            string dir = Path.GetDirectoryName(inputVideoPath) ?? throw new InvalidOperationException("Could not determine directory of input video.");

            string workingInputPath = inputVideoPath;

            if (!inputExt.Equals(".mkv", StringComparison.OrdinalIgnoreCase))
            {
                workingInputPath = Path.Combine(dir, $"{baseName}.temp.mkv");
                string convertArgs = $"-i \"{inputVideoPath}\" -c copy -y \"{workingInputPath}\"";

                await RunFfmpegAsync(convertArgs, taskId, cancellationToken);

                try { File.Delete(inputVideoPath); }
                catch (Exception ex) { _logger.LogWarning(ex, "Could not delete original input video: {path}", inputVideoPath); }
            }

            string tempOutputPath = Path.Combine(dir, $"{baseName}.temp.subbed.mkv");

            var subtitleInputs = string.Join(" ", subtitlePaths.Select(p => $"-i \"{p}\""));
            var args = $"-i \"{workingInputPath}\" {subtitleInputs} -map 0 -map -0:s";

            for (int i = 0; i < subtitlePaths.Length; i++)
            {
                args += $" -map {i + 1} -c:s:{i} srt -disposition:s:{i} default";
            }

            args += $" -c:v copy -c:a copy -y \"{tempOutputPath}\"";

            await RunFfmpegAsync(args, taskId, cancellationToken);

            try
            {
                // attempt to clean intermediate file (ignore failures)
                if (workingInputPath != inputVideoPath && File.Exists(workingInputPath))
                {
                    File.Delete(workingInputPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete intermediate MKV: {path}", workingInputPath);
            }

            try
            {
                File.Move(tempOutputPath, inputVideoPath, overwrite: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not replace original video with processed one.");
                throw;
            }

            return inputVideoPath;
        }

        private async Task RunFfmpegAsync(string arguments, Guid taskId, CancellationToken cancellationToken)
        {
            // start process
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _settings.FFmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            // start and persist PID
            process.Start();

            // update DB with PID (best-effort - avoid failing on DB issues)
            try
            {
                var taskEntity = await _context.TorrentTasks.FindAsync(new object[] { taskId }, CancellationToken.None);
                if (taskEntity != null)
                {
                    taskEntity.FfmpegPID = process.Id;
                    await _context.SaveChangesAsync(CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to persist FFmpeg PID for task {TaskId}", taskId);
            }

            // register cancellation to kill the process tree when requested
            using var reg = cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                    {
                        // kill whole process tree if supported
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to kill ffmpeg process for task {TaskId} during cancellation.", taskId);
                }
            });

            // read stderr concurrently
            var stderrTask = process.StandardError.ReadToEndAsync();

            try
            {
                // Wait for exit with cancellation support. If cancellation triggers, the registered callback
                // should attempt to kill the process which will cause WaitForExitAsync to complete.
                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // If cancellation token triggered and process still hasn't exited, try to ensure it's killed
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch { /* swallow */ }

                // propagate cancellation so the caller can handle/rollback as needed
                throw;
            }

            // gather stderr result (if the process was killed, this still attempts to read whatever is available)
            string stderr = string.Empty;
            try
            {
                stderr = await stderrTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read ffmpeg stderr for task {TaskId}", taskId);
            }

            // clear PID in DB (best-effort)
            try
            {
                var t = await _context.TorrentTasks.FindAsync(new object[] { taskId }, CancellationToken.None);
                if (t != null)
                {
                    t.FfmpegPID = null;
                    await _context.SaveChangesAsync(CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clear FFmpeg PID for task {TaskId}", taskId);
            }

            // check exit code
            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg failed (exit code {process.ExitCode}):\n{stderr}");
            }
        }
    }
}
