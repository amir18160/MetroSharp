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

                await ProcessPairs(task);

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

        private async Task ProcessPairs(TorrentTask task)
        {
            int counter = 0;

            foreach (var pair in task.SubtitleVideoPairs)
            {
                _logger.LogInformation("Processing pair {counter} out of {total}", ++counter, task.SubtitleVideoPairs.Count);

                if (!string.IsNullOrWhiteSpace(pair.VideoPath))
                {
                    if (!string.IsNullOrWhiteSpace(pair.SubtitlePath))
                    {
                        try
                        {
                            pair.FinalPath = await AddSoftSubtitlesInPlaceAsync(pair.VideoPath, [pair.SubtitlePath]);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to process FFmpeg task with pairId {pairId}", pair.Id);
                            _logger.LogInformation("This task will use the initial path as final path.");
                            pair.FinalPath = pair.VideoPath;
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Pair does not have a subtitle path. Using initial path as final path.");
                        pair.FinalPath = pair.VideoPath;
                    }
                }
                else
                {
                    _logger.LogInformation("Invalid pair: missing VideoPath. Marking as ignored.");
                    pair.Ignore = true;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<string> AddSoftSubtitlesInPlaceAsync(string inputVideoPath, string[] subtitlePaths)
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

                await RunFfmpegAsync(convertArgs);

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

            await RunFfmpegAsync(args);

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

        private async Task RunFfmpegAsync(string arguments)
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

            string stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"FFmpeg failed:\n{stderr}");
        }
    }
}
