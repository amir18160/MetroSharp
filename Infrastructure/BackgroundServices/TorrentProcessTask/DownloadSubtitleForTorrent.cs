using System.IO.Compression;
using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.BackgroundServices.Models;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class DownloadSubtitleForTorrent
    {
        private readonly IScraperFacade _scraperFacade;
        private readonly DownloadContext _context;
        private readonly ZipDownloader _zipDownloader;
        private readonly ILogger<DownloadSubtitleForTorrent> _logger;
        private readonly TorrentTaskSettings _settings;

        public DownloadSubtitleForTorrent(
            IScraperFacade scraperFacade,
            DownloadContext context,
            ILogger<DownloadSubtitleForTorrent> logger,
            ZipDownloader zipDownloader,
            IOptions<TorrentTaskSettings> settings
        )
        {
            _logger = logger;
            _context = context;
            _scraperFacade = scraperFacade;
            _zipDownloader = zipDownloader;
            _settings = settings.Value;
        }

        public async Task<bool> GetSubtitleForTorrent(Guid taskId)
        {
            var task = await _context.TorrentTasks.FirstOrDefaultAsync(x => x.Id == taskId);
            if (task == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(task.FileSavingPath))
            {
                task.State = TorrentTaskState.Error;
                task.UpdatedAt = DateTime.UtcNow;
                task.ErrorMessage = $"No file path found for task with id <{taskId}>";
                await _context.SaveChangesAsync();
                return false;
            }
            if (string.IsNullOrWhiteSpace(task.SubtitleUrl))
            {
                task.State = TorrentTaskState.Error;
                task.UpdatedAt = DateTime.UtcNow;
                task.ErrorMessage = $"No SubtitleUrl found for task with id <{taskId}>";
                await _context.SaveChangesAsync();
                return false;
            }

            task.State = TorrentTaskState.InDownloadingSubtitle;
            task.UpdatedAt = DateTime.UtcNow;

            var isSubtitleDownloaded = await DownloadSubtitleWithDownloadLink(task);

            if (!isSubtitleDownloaded)
            {
                await _context.SaveChangesAsync();
                return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }


        private async Task<bool> DownloadSubtitleWithDownloadLink(TorrentTask task)
        {
            var uniqueName = $"{task.TorrentHash}-{DateTime.UtcNow.Ticks}";
            var zipFileName = $"{uniqueName}.zip";
            var downloadZipPath = Path.Combine(_settings.SubtitleDownloadPath, zipFileName);
            var extractPath = Path.Combine(_settings.SubtitleDownloadPath, uniqueName);

            try
            {
                await _zipDownloader.DownloadZipAsync(task.SubtitleUrl, downloadZipPath);

                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                ZipFile.ExtractToDirectory(downloadZipPath, extractPath);

                File.Delete(downloadZipPath);

                task.SubtitleSavingPath = extractPath;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download and extract subtitle ZIP.");
                return false;
            }
        }

    }
}