using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.GeminiWrapper;
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

        public async Task<bool> Pair(Guid taskId)
        {
            var task = await _context.TorrentTasks.FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(task.SubtitleSavingPath))
            {
                task.ErrorMessage = $"Failed to get SubtitleSavingPath for task with id <{taskId}>";
                return false;
            }

            var pairedResult = await GeminiPair(task);

            if (pairedResult.Count == 0)
            {
                var message = "Failed to pair any video with matching subtitles.";
                _logger.LogError(message);
                task.ErrorMessage = message;
                task.UpdatedAt = DateTime.UtcNow;
                task.State = TorrentTaskState.Error;
                await _context.SaveChangesAsync();
                return false;
            }

            task.State = TorrentTaskState.InParingSubtitlesWithVideo;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SubtitleVideoPairs.AddRangeAsync(pairedResult);
            await _context.SaveChangesAsync();

            _logger.LogError("Failed to pair any video with matching subtitles.");

            return true;
        }

        private async Task<List<SubtitleVideoPair>> GeminiPair(TorrentTask task)
        {
            var Subtitles = GetSubtitles(task);
            var Videos = GetVideos(task);

            try
            {
                var result = await _geminiService.GenerateContentAsync(AiPrompts.GeneratePairJson(Videos, Subtitles));

                List<SubtitleVideoPair> pairs = JsonSerializer.Deserialize<List<SubtitleVideoPair>>(result);

                foreach (var p in pairs)
                {
                    p.TorrentTask = task;
                    p.TorrentTaskId = task.Id;
                }

                return pairs;
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