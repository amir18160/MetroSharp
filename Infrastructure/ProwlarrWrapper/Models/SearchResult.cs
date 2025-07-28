namespace Infrastructure.ProwlarrWrapper.Models
{
    public class SearchResult
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public int? Age { get; set; }
        public double? AgeHours { get; set; }
        public double? AgeMinutes { get; set; }
        public long? Size { get; set; }
        public int? Files { get; set; }
        public int? Grabs { get; set; }
        public int? IndexerId { get; set; }
        public string Indexer { get; set; }
        public string SubGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public int? ImdbId { get; set; }
        public int? TmdbId { get; set; }
        public int? TvdbId { get; set; }
        public int? TvMazeId { get; set; }
        public DateTime? PublishDate { get; set; }
        public string CommentUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string InfoUrl { get; set; }
        public string PosterUrl { get; set; }
        public List<string> IndexerFlags { get; set; } = [];
        public List<SearchCategory> Categories { get; set; } = [];
        public string MagnetUrl { get; set; }
        public string InfoHash { get; set; }
        public int? Seeders { get; set; }
        public int? Leechers { get; set; }
        public string Protocol { get; set; }
        public int? DownloadClientId { get; set; }
    }

}