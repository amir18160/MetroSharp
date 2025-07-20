using Domain.Enums;

namespace Application.Scrapers.Queries.GetLatestTorrents
{
    public class AbstractedLatestTorrent
    {
        public string Title { get; set; }
        public string DetailUrl { get; set; }
        public string Category { get; set; }
        public string CategoryUrl { get; set; }
        public string Year { get; set; }
        public string Size { get; set; }
        public int? Seeders { get; set; }
        public int? Leechers { get; set; }
        public string MagnetLink { get; set; }
        public string ImageUrl { get; set; }
        public string Rating { get; set; }
        public List<string> Genres { get; set; }
        public TorrentSource Type { get; set; }
    }
}
