using QBittorrent.Client;
using Infrastructure.QbitTorrentClient.Models;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Domain.Models.Qbit;
using Application.Interfaces;
using AutoMapper;



namespace Infrastructure.QbitTorrentClient
{
    public class QbitClient : IQbitClient
    {
        public QBittorrentClient Client { set; get; }
        private readonly QbitTorrentSettings _qbitTorrentSettings;
        private readonly ILogger<QbitClient> _logger;
        private readonly IMapper _mapper;

        public QbitClient(IOptions<QbitTorrentSettings> qbitTorrentSettings, ILogger<QbitClient> logger, IMapper mapper)
        {
            _mapper = mapper;
            _qbitTorrentSettings = qbitTorrentSettings.Value;
            _logger = logger;

            _logger.LogInformation("Initializing QbitClient with settings: {@Settings}", _qbitTorrentSettings);
            InitializeClient();
            _logger.LogInformation("QbitClient initialized successfully.");
        }

        private void InitializeClient()
        {
            Client = new QBittorrentClient(_qbitTorrentSettings.ConnectionString);
            Task.Run(async () =>
            {
                await Client.LoginAsync(_qbitTorrentSettings.Username, _qbitTorrentSettings.Password);
            }).GetAwaiter().GetResult();
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

        public async Task<List<QbitTorrentInfo>> GetAllQbitTorrentsAsync()
        {
            var torrents = await Client.GetTorrentListAsync();
            var qbitTorrents = _mapper.Map<List<QbitTorrentInfo>>(torrents).ToList();
            return qbitTorrents;
        }
        public async Task<QbitTorrentInfo> GetQbitTorrentByHashAsync(string hash)
        {
            var options = new TorrentListQuery
            {
                Hashes = new List<string> { hash }
            };
            var torrent = await Client.GetTorrentListAsync(options);
            if (torrent == null || !torrent.Any()) return null;
            return _mapper.Map<QbitTorrentInfo>(torrent[0]);
        }

        public async Task<bool> AddTorrentAsync(string pathOrMagnetOrTorrentUrl)
        {
            var options = new AddTorrentUrlsRequest(new Uri(pathOrMagnetOrTorrentUrl))
            {
                CreateRootFolder = true,
                DownloadFolder = _qbitTorrentSettings.DownloadFolder ?? null,
            };
            try
            {
                await Client.AddTorrentsAsync(options);
                await UpdateTrackersOfTorrent(pathOrMagnetOrTorrentUrl);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add torrent: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteTorrentAsync(string hash, bool deleteFiles = true)
        {
            try
            {
                await Client.DeleteAsync(hash, deleteFiles);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete torrent: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateTrackersOfTorrent(string magnetLink)
        {
            try
            {
                if (!magnetLink.StartsWith("magnet:"))
                {
                    throw new InvalidDataException("provided string must be a magnet link");
                }

                var hash = TorrentUtilities.ExtractHashFromMagnet(magnetLink);
                var trackers = await GetLatestTrackersFromSource();
                var uriTrackers = trackers.Select(t => new Uri(t)).ToList();
                await Client.AddTrackersAsync(hash, uriTrackers);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add torrents to a hash: {Message}", ex.Message);
                return false;
            }
        }
    }
}