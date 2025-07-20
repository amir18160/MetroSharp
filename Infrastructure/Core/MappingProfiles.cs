using AutoMapper;
using Domain.Models.Qbit;
using QBittorrent.Client;

namespace Infrastructure.Core
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<TorrentInfo, QbitTorrentInfo>();
        }
    }
}