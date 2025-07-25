// ZipDownloader.cs
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Utilities
{
    public class ZipDownloader
    {
        private readonly HttpClient _httpClient;

        public ZipDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task DownloadZipAsync(string url, string destinationPath, int maxRetries = 3, int delayMilliseconds = 1000)
        {
            int attempt = 0;

            while (true)
            {
                try
                {
                    attempt++;

                    using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                        throw new HttpRequestException($"Failed with status code: {response.StatusCode}");

                    if (response.Content.Headers.ContentType?.MediaType != "application/zip")
                        throw new InvalidDataException("The file is not a valid zip file (unexpected content type).");

                    await using var stream = await response.Content.ReadAsStreamAsync();
                    await using var fileStream = File.Create(destinationPath);
                    await stream.CopyToAsync(fileStream);

                    return;
                }
                catch when (attempt <= maxRetries)
                {
                    if (attempt == maxRetries)
                        throw;

                    await Task.Delay(delayMilliseconds * attempt);
                }
            }
        }
    }
}
