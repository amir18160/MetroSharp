namespace Domain.Models.Scrapers.Yts
{
    public class YtsTorrent
    {
        public string Quality { get; set; }
        public double Size { get; set; }
        public string MagnetLink { get; set; }
        public string TorrentFileLink { get; set; }
    }
}