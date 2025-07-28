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

        public async Task<bool> ProcessPairs(Guid taskId)
        {
            var task = await _context.TorrentTasks
                .Include(x => x.SubtitleVideoPairs)
                .FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
            {
                return false;
            }

            task.UpdatedAt = DateTime.UtcNow;

            if (task.SubtitleVideoPairs.Count == 0)
            {
                task.State = TorrentTaskState.Error;
                task.ErrorMessage = "No pair found for this task.";
                await _context.SaveChangesAsync();
                return false;
            }

            _logger.LogInformation("Task {TaskId}: Waiting to enter the FFmpeg processing gate.", taskId);

            task.State = TorrentTaskState.InFfmpegButProcessNotStarted;
            await _context.SaveChangesAsync();

            await _semaphore.WaitAsync();

            try
            {
                _logger.LogInformation("Task {TaskId}: Entered the ffmpeg gate. Starting FFmpeg process.", taskId);

                task.State = TorrentTaskState.InFfmpegAndProcessStarted;
                await _context.SaveChangesAsync();

                await ProcessPairsInternal(task);

                _logger.LogInformation("Task {TaskId}: FFmpeg process finished.", taskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task {TaskId}: An error occurred during FFmpeg processing.", taskId);
                task.State = TorrentTaskState.Error;
                task.ErrorMessage = ex.Message;
                await _context.SaveChangesAsync();
                return false;
            }
            finally
            {
                _semaphore.Release();
                _logger.LogInformation("Task {TaskId}: Exited the ffmpeg gate.", taskId);
            }
        }

        private async Task ProcessPairsInternal(TorrentTask task)
        {
            int counter = 0;
            var orderedPairs = task.SubtitleVideoPairs
                                    .Where(p => !p.Ignore)
                                    .OrderBy(p => p.SeasonNumber)
                                    .ThenBy(p => p.EpisodeNumber)
                                    .ToList();

            foreach (var pair in orderedPairs)
            {
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
                        pair.FinalPath = await AddSoftSubtitlesInPlaceAsync(pair.VideoPath, new[] { pair.SubtitlePath }, task.Id);
                        pair.SubtitlesMerged = true; // Mark as successfully subbed
                        _logger.LogInformation("Successfully merged subtitles for pair {pairId}", pair.Id);
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

            await _context.SaveChangesAsync();
        }

        public async Task<string> AddSoftSubtitlesInPlaceAsync(string inputVideoPath, string[] subtitlePaths, Guid taskId)
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

            string inputExt = Path.GetExtension(inputVideoPath);
            string baseName = Path.GetFileNameWithoutExtension(inputVideoPath);
            string dir = Path.GetDirectoryName(inputVideoPath) ?? throw new InvalidOperationException("Could not determine directory of input video.");

            string workingInputPath = inputVideoPath;

            if (!inputExt.Equals(".mkv", StringComparison.OrdinalIgnoreCase))
            {
                workingInputPath = Path.Combine(dir, $"{baseName}.temp.mkv");
                string convertArgs = $"-i \"{inputVideoPath}\" -c copy -y \"{workingInputPath}\"";

                await RunFfmpegAsync(convertArgs, taskId);

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

            await RunFfmpegAsync(args, taskId);

            try
            {
                File.Delete(workingInputPath);
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

        private async Task RunFfmpegAsync(string arguments, Guid taskId)
        {
            var process = new Process
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

            process.Start();

            var task = await _context.TorrentTasks.FindAsync(taskId);
            if (task != null)
            {
                task.FfmpegPID = process.Id;
                await _context.SaveChangesAsync();
            }


            string stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (task != null)
            {
                task.FfmpegPID = null;
                await _context.SaveChangesAsync();
            }


            if (process.ExitCode != 0)
                throw new Exception($"FFmpeg failed:\n{stderr}");
        }
    }
}