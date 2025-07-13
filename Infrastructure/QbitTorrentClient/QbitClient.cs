using QBittorrent.Client;
using Infrastructure.QbitTorrentClient.Models;
using System.Text.Json;

namespace Infrastructure.QbitTorrentClient
{
    public class QbitClient
    {
        public QBittorrentClient Client { set; get; }
        private readonly QbitInitOptions _qbitInitOptions;

        public QbitClient(QbitInitOptions qbitInitOptions)
        {
            _qbitInitOptions = qbitInitOptions;
        }

        public async Task InitializeAsync()
        {
            Client = new QBittorrentClient(_qbitInitOptions.ConnectionString);
            await Client.LoginAsync(_qbitInitOptions.Username, _qbitInitOptions.Password);
        }

        static async Task<List<string>> GetLatestTrackersFromSource()
        {
            string cachePath = "trackers_cache.json";

            if (File.Exists(cachePath))
            {
                var fileInfo = new FileInfo(cachePath);
                if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < TimeSpan.FromDays(1))
                {
                    var cachedJson = await File.ReadAllTextAsync(cachePath);
                    var cachedList = JsonSerializer.Deserialize<List<string>>(cachedJson);
                    if (cachedList != null)
                    {
                        return cachedList;
                    }
                }
            }

            var client = new HttpClient();
            var source = new Uri("https://raw.githubusercontent.com/ngosang/trackerslist/master/trackers_all.txt");

            var response = await client.GetAsync(source);
            response.EnsureSuccessStatusCode();

            var rawData = await response.Content.ReadAsStringAsync();

            var list = rawData
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line))
                .ToList();

            var json = JsonSerializer.Serialize(list);
            await File.WriteAllTextAsync(cachePath, json);

            return list;
        }
    }
}