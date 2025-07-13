namespace Infrastructure.Scrapers.Models.Rarbg
{
    public class RarbgDetails
    {
        public string Torrent { get; set; }
        public string MagnetLink { get; set; }
        public string Thumbnail { get; set; }
        public string Trailer { get; set; }
        public string Uploader { get; set; }
        public string Downloads { get; set; }
        public string Type { get; set; }
        public List<string> Genre { get; set; }
        public string InfoHash { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Size { get; set; }
        public string Added { get; set; }
        public RarbgPeerInfo Peers { get; set; }
        public bool MultipleQualityAvailable { get; set; }
        public RarbgImdbInfo Imdb { get; set; }
    }

}