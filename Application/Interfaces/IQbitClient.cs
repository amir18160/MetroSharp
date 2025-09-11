using Domain.Models.Qbit;

namespace Application.Interfaces;

public interface IQbitClient
{
    Task<List<QbitTorrentInfo>> GetAllQbitTorrentsAsync();
    Task<bool> AddTorrentAsync(string pathOrMagnetOrTorrentUrl);
    Task<bool> DeleteTorrentAsync(string hash, bool deleteFiles = true);
    Task<QbitTorrentInfo> GetQbitTorrentByHashAsync(string hash);
    Task<bool> UpdateTrackersOfTorrent(string magnet);
}