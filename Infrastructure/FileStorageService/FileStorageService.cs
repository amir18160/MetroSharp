using Application.Interfaces;
using Domain.Enums;
using Infrastructure.FileStorageService.Models;
using Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.FileStorageService;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _fileStorageSettings;
    private readonly ZipDownloader _zipDownloader;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IOptions<FileStorageSettings> fileStorageSettings, ZipDownloader zipDownloader,
        ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _zipDownloader = zipDownloader;
        _fileStorageSettings = fileStorageSettings.Value;
    }

    public async Task<string> SaveFileAsync(IFormFile file, FileStorageType type)
    {
        if (file.Length == 0) throw new Exception("File length is zero!");

        if (file.Length > _fileStorageSettings.MaxFileSize) throw new Exception("File size is too big.");

        if (type == FileStorageType.Subtitle) return await SaveSubtitle(file);


        throw new InvalidDataException("file type does not supported");
    }

    private async Task<string> SaveSubtitle(IFormFile file)
    {
        if (!Path.Exists(_fileStorageSettings.SubtitlesPath))
            Directory.CreateDirectory(_fileStorageSettings.SubtitlesPath);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".zip") throw new InvalidDataException("Only .zip files are allowed.");

        var mimeType = file.ContentType?.ToLowerInvariant();
        if (mimeType != "application/zip" && mimeType != "application/x-zip-compressed")
            throw new InvalidDataException("Invalid file type. Expected ZIP.");

        if (!await ZipTools.IsZipFile(file))
            throw new InvalidDataException("The uploaded file is not a valid ZIP archive.");

        var filePath = Path.Combine(_fileStorageSettings.SubtitlesPath, file.FileName);
        await using Stream fileStream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        return filePath;
    }
}