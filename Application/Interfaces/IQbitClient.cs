using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Models.Qbit;

namespace Application.Interfaces
{
    public interface IQbitClient
    {
        Task<List<QbitTorrentInfo>> GetAllQbitTorrentsAsync();
        Task<bool> AddTorrentAsync(string pathOrMagnetOrTorrentUrl);
        Task<QbitTorrentInfo> GetQbitTorrentByHashAsync(string hash);
    }
}