using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class SubtitleEditor
    {
        private readonly DownloadContext _context;
        private readonly ILogger<SubtitleEditor> _logger;

        public SubtitleEditor(DownloadContext context, ILogger<SubtitleEditor> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task EditSubtitle(Guid taskId, CancellationToken cancellationToken = default)
        {
            // Respect cancellation immediately
            cancellationToken.ThrowIfCancellationRequested();

            var task = await _context.TorrentTasks
                .Include(x => x.SubtitleVideoPairs)
                .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

            if (task == null)
            {
                _logger.LogWarning("No task found with ID {taskId}", taskId);
                return;
            }

            if (task.SubtitleVideoPairs.Count == 0)
            {
                _logger.LogInformation("No subtitle/video pairs found to edit.");
                return;
            }

            _logger.LogInformation("Found {count} subtitle/video pairs to process.", task.SubtitleVideoPairs.Count);

            // Process pairs with cancellation token
            await ProcessPairs(task, cancellationToken);
        }

        private async Task ProcessPairs(TorrentTask task, CancellationToken cancellationToken)
        {
            int counter = 1;

            // iterate through pairs and check cancellation between iterations
            foreach (var pair in task.SubtitleVideoPairs)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation("Processing subtitle {counter} of {total}", counter++, task.SubtitleVideoPairs.Count);

                if (string.IsNullOrWhiteSpace(pair.SubtitlePath))
                {
                    _logger.LogWarning("Subtitle path is missing for video: {videoPath}", pair.VideoPath);
                    continue;
                }

                if (!File.Exists(pair.SubtitlePath))
                {
                    _logger.LogWarning("Subtitle file not found: {subtitlePath}", pair.SubtitlePath);
                    continue;
                }

                try
                {
                    // Offload heavy parsing/editing to a background task to avoid blocking worker thread.
                    await EditAndSaveSubtitleAsync(pair.SubtitlePath, cancellationToken);
                    _logger.LogInformation("Successfully edited subtitle: {subtitlePath}", pair.SubtitlePath);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Subtitle editing was cancelled for path {subtitlePath}", pair.SubtitlePath);
                    // propagate so upstream can handle cancellation consistently
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to edit subtitle at: {subtitlePath}", pair.SubtitlePath);
                }
            }
        }

        private async Task EditAndSaveSubtitleAsync(string path, CancellationToken cancellationToken)
        {
            // allow early cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // Parse subtitle on threadpool; Task.Run accepts token to avoid scheduling if already cancelled.
            var subtitle = await Task.Run(() => Subtitle.Parse(path), cancellationToken);

            // Check after parse in case cancellation happened during parse.
            cancellationToken.ThrowIfCancellationRequested();

            if (subtitle.Paragraphs.Count == 0)
            {
                throw new Exception("Subtitle is empty");
            }

            var updated = InsertMetroMovieSpread(subtitle.Paragraphs);

            // Replace paragraphs
            subtitle.Paragraphs.Clear();
            subtitle.Paragraphs.AddRange(updated);

            // Persist file write on threadpool; not cancellable mid-write, but token prevents scheduling if already cancelled.
            await Task.Run(() =>
            {
                File.WriteAllText(path, new SubRip().ToText(subtitle, "untitled"));
            }, cancellationToken);
        }

        public static List<Paragraph> InsertMetroMovieSpread(List<Paragraph> originalParagraphs, int insertCount = 3)
        {
            const string MetroMovieText = "<i><b><font color=\"#ff0000\">.:: مترو موویز ::.</font></b></i>\n<font color=\"#ffff00\"><i>.:: @MetroMovieBot :: @MetroMoviez ::.</i></font>";
            const double InsertDurationMs = 10000;

            if (originalParagraphs == null || originalParagraphs.Count == 0)
                return new List<Paragraph>();

            var sorted = originalParagraphs.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
            var result = new List<Paragraph>(originalParagraphs);

            var firstStart = sorted.First().StartTime.TotalMilliseconds;
            var lastEnd = sorted.Last().EndTime.TotalMilliseconds;
            var totalDuration = lastEnd - firstStart;

            var interval = totalDuration / (insertCount + 1);

            var inserts = new List<Paragraph>();

            for (int i = 0; i < insertCount; i++)
            {
                var start = firstStart + interval * (i + 1) - InsertDurationMs / 2;
                var end = start + InsertDurationMs;

                if (IsOverlapping(result, start, end))
                {
                    const double shiftMs = 1500;
                    int attempts = 5;
                    bool placed = false;

                    for (int j = 0; j < attempts; j++)
                    {
                        start += shiftMs;
                        end = start + InsertDurationMs;

                        if (!IsOverlapping(result, start, end))
                        {
                            placed = true;
                            break;
                        }
                    }

                    if (!placed) continue;
                }

                inserts.Add(new Paragraph(MetroMovieText, start, end));
            }

            // Optional: intro and outro insertions
            var introStart = Math.Max(0, firstStart - InsertDurationMs - 1000);
            inserts.Insert(0, new Paragraph(MetroMovieText, introStart, introStart + InsertDurationMs));

            inserts.Add(new Paragraph(MetroMovieText, lastEnd + 1000, lastEnd + 1000 + InsertDurationMs));

            result.AddRange(inserts);

            var final = result.OrderBy(p => p.StartTime.TotalMilliseconds).ToList();
            for (int i = 0; i < final.Count; i++)
                final[i].Number = i + 1;

            return final;
        }

        private static bool IsOverlapping(List<Paragraph> existing, double start, double end)
        {
            return existing.Any(p =>
                !(end <= p.StartTime.TotalMilliseconds || start >= p.EndTime.TotalMilliseconds));
        }
    }
}
