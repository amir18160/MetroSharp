
using Application.Interfaces;
using Infrastructure.QbitTorrentClient;

namespace Infrastructure.Utilities
{
    public class UtilitiesFacade : IUtilitiesFacade
    {
        public string GetMagnetHash(string magnet)
        {
            return TorrentUtilities.ExtractHashFromMagnet(magnet);
        }
    }
}