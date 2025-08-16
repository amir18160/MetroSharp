using Application.Interfaces;
using Domain.Enums;
using Infrastructure.FileStorageService.Models;
using Infrastructure.Scrapers.Core;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.FileStorageService
{
    public class FileStorageService : IFileStorageService
    {
        private readonly FileStorageSettings _fileStorageSettings;
        private readonly ZipDownloader _zipDownloader;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(IOptions<FileStorageSettings> fileStorageSettings, ZipDownloader zipDownloader, ILogger<FileStorageService> logger)
        {
            _logger = logger;
            _zipDownloader = zipDownloader;
            _fileStorageSettings = fileStorageSettings.Value;
        }

        public async Task<string> SaveFileAsync(IFormFile file, FileStorageType type)
        {
            if (file.Length == 0)
            {
                throw new Exception("File length is zero!");
            }

            if (file.Length > _fileStorageSettings.MaxFileSize)
            {
                throw new Exception("File size is too big.");
            }

            if (type == FileStorageType.Subtitle)
            {
                return await SaveSubtitle(file);
            }


            throw new InvalidDataException("file type does not supported");
        }

        public async Task<string> DownloadAndSaveSubtitle(
            string downloadUrl,
            string fileName,
            DownloaderType downloaderType = DownloaderType.HttpClient)
        {
            var destination = Path.Combine(
                _fileStorageSettings.SubtitlesPath,
                $"{fileName}-{DateTime.UtcNow.Ticks}.zip"
            );

            if (downloaderType == DownloaderType.HttpClient)
            {
                await _zipDownloader.DownloadZipAsync(downloadUrl, destination);
            }
            else
            {           
                var tempPath = await ScraperBase.BrowserDownloadFileAsync(downloadUrl, headless: false);
                await FileMoveToAsync.MoveFileAsync(tempPath, destination);
            }

            if (!await ZipTools.IsZipFile(destination))
            {
                try
                {
                    File.Delete(destination);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to delete an invalid subtitle zip: {message}", ex.Message);
                }
                throw new InvalidDataException("The downloaded file is not a valid ZIP archive.");
            }

            return destination;
        }

        private async Task<string> SaveSubtitle(IFormFile file)
        {
            if (!Path.Exists(_fileStorageSettings.SubtitlesPath))
            {
                Directory.CreateDirectory(_fileStorageSettings.SubtitlesPath);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".zip")
            {
                throw new InvalidDataException("Only .zip files are allowed.");
            }

            var mimeType = file.ContentType?.ToLowerInvariant();
            if (mimeType != "application/zip" && mimeType != "application/x-zip-compressed")
            {
                throw new InvalidDataException("Invalid file type. Expected ZIP.");
            }

            if (!await ZipTools.IsZipFile(file))
            {
                throw new InvalidDataException("The uploaded file is not a valid ZIP archive.");
            }

            string filePath = Path.Combine(_fileStorageSettings.SubtitlesPath, file.FileName);
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return filePath;
        }

    }
}