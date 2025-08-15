using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, FileStorageType type);
        Task<string> DownloadAndSaveSubtitle(string DownloadURL, string FileName, DownloaderType downloaderType = DownloaderType.HttpClient);
    }
}