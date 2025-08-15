// ExtractSubtitleForTorrent.cs
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.BackgroundServices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;
using System;
using System.IO;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class ExtractSubtitleForTorrent
    {
        private readonly DownloadContext _context;
        private readonly ILogger<ExtractSubtitleForTorrent> _logger;
        private readonly TorrentTaskSettings _settings;

        public ExtractSubtitleForTorrent(
            DownloadContext context,
            ILogger<ExtractSubtitleForTorrent> logger,
            IOptions<TorrentTaskSettings> settings
        )
        {
            _logger = logger;
            _context = context;
            _settings = settings.Value;
        }

        public async Task<bool> GetSubtitleForTorrent(Guid taskId, CancellationToken cancellationToken = default)
        {
            // Respect cancellation early
            cancellationToken.ThrowIfCancellationRequested();

            var task = await _context.TorrentTasks
                .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);

            if (task == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(task.FileSavingPath))
            {
                task.State = TorrentTaskState.Error;
                task.UpdatedAt = DateTime.UtcNow;
                task.ErrorMessage = $"No file path found for task with id <{taskId}>";
                await _context.SaveChangesAsync(cancellationToken);
                return false;
            }

            if (string.IsNullOrWhiteSpace(task.SubtitleStoredPath) || !Path.Exists(task.SubtitleStoredPath))
            {
                task.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return false;
            }

            // mark extracting
            task.State = TorrentTaskState.InExtractingSubtitle;
            task.UpdatedAt = DateTime.UtcNow;

            // check cancellation before extraction
            cancellationToken.ThrowIfCancellationRequested();

            var isSubtitleExtracted = await ExtractSubtitleFromSubtitleStoredPathAsync(task, cancellationToken);

            if (!isSubtitleExtracted)
            {
                await _context.SaveChangesAsync(cancellationToken);
                return false;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }


        private async Task<bool> ExtractSubtitleFromSubtitleStoredPathAsync(TorrentTask task, CancellationToken cancellationToken)
        {
            var uniqueName = $"{task.TorrentHash}-{DateTime.UtcNow.Ticks}";
            var extractPath = Path.Combine(_settings.SubtitleDownloadPath, uniqueName);

            try
            {
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                // run extraction off the worker thread; note: this does not forcibly abort the extraction
                // once it started — it does allow early cancellation before starting and prevents blocking the worker.
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(task.SubtitleStoredPath, extractPath);
                }, cancellationToken);

                // check cancellation after extraction
                cancellationToken.ThrowIfCancellationRequested();

                task.SubtitleSavingPath = extractPath;
                return true;
            }
            catch (OperationCanceledException)
            {
                // propagate so caller/processor can handle it and set state if desired
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract subtitle ZIP.");
                return false;
            }
        }

    }
}
