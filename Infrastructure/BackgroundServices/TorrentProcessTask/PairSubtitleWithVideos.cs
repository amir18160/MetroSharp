using System.Text.Json;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.GeminiWrapper.Prompts;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class PairSubtitleWithVideos
    {
        private readonly DownloadContext _context;
        private readonly ILogger<PairSubtitleWithVideos> _logger;
        private readonly IGeminiService _geminiService;
        public PairSubtitleWithVideos(DownloadContext context, ILogger<PairSubtitleWithVideos> logger, IGeminiService geminiService)
        {
            _geminiService = geminiService;
            _logger = logger;
            _context = context;
        }

        public async Task<bool> Pair(Guid taskId, CancellationToken cancellationToken = default)
        {
            // early cancellation
            cancellationToken.ThrowIfCancellationRequested();

            var task = await _context.TorrentTasks
                .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

            if (task == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(task.SubtitleSavingPath))
            {
                task.ErrorMessage = $"Failed to get SubtitleSavingPath for task with id <{taskId}>";
                return false;
            }

            // perform pairing (may throw OperationCanceledException)
            var pairedResult = await GeminiPair(task, cancellationToken);

            // respect cancellation after pairing
            cancellationToken.ThrowIfCancellationRequested();

            if (pairedResult.Count == 0)
            {
                var message = "Failed to pair any video with matching subtitles because no pair created.";
                _logger.LogError(message);
                task.ErrorMessage = message;
                task.UpdatedAt = DateTime.UtcNow;
                task.State = TorrentTaskState.Error;
                await _context.SaveChangesAsync(cancellationToken);
                return false;
            }

            task.State = TorrentTaskState.InParingSubtitlesWithVideo;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SubtitleVideoPairs.AddRangeAsync(pairedResult, cancellationToken);

            var res = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!res)
            {
                task.State = TorrentTaskState.Error;
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogError("Failed to pair any video with matching subtitles.");
                return false;
            }

            return true;
        }

        private async Task<List<SubtitleVideoPair>> GeminiPair(TorrentTask task, CancellationToken cancellationToken)
        {
            // get files (synchronous) — check cancellation before and after to be responsive
            cancellationToken.ThrowIfCancellationRequested();
            var Subtitles = GetSubtitles(task);
            cancellationToken.ThrowIfCancellationRequested();
            var Videos = GetVideos(task);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // call external service
                var rawResult = await _geminiService.GenerateContentAsync(AiPrompts.GeneratePairJson(Videos, Subtitles));

                cancellationToken.ThrowIfCancellationRequested();

                var cleanedJson = rawResult
                    .Trim()
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                List<SubtitleVideoPair> pairs = JsonSerializer.Deserialize<List<SubtitleVideoPair>>(cleanedJson);

                if (pairs == null) return new List<SubtitleVideoPair>();

                foreach (var p in pairs)
                {
                    p.Id = Guid.NewGuid();
                    p.TorrentTask = task;
                    p.TorrentTaskId = task.Id;
                }

                return pairs;
            }
            catch (OperationCanceledException)
            {
                // propagate so outer caller can handle cancellation
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Pair response from gemini api: {message}", ex.Message);
                return new List<SubtitleVideoPair>();
            }
        }

        private static List<string> GetSubtitles(TorrentTask task)
        {
            return FileScanner.GetFilesByExtensions(task.SubtitleSavingPath, ".srt", ".ass", ".sub", ".vtt");
        }

        private static List<string> GetVideos(TorrentTask task)
        {
            return FileScanner.GetFilesByExtensions(task.FileSavingPath, ".mp4", ".mkv", ".avi", ".mov", ".flv");
        }

    }
}
