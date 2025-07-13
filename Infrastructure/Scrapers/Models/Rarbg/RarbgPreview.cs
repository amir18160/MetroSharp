namespace Infrastructure.Scrapers.Models.Rarbg
{
    public class RarbgPreview
    {
        public string Title { get; set; }
        public string TitleHref { get; set; }
        public string Category { get; set; }
        public string CategoryHref { get; set; }
        public string Date { get; set; }
        public string Size { get; set; }
        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public string MagnetLink { get; set; }
    }
}