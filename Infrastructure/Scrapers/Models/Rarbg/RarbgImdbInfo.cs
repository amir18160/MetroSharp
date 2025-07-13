namespace Infrastructure.Scrapers.Models.Rarbg
{
    public class RarbgImdbInfo
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public List<string> Genres { get; set; }
        public string Runtime { get; set; }
        public string Rating { get; set; }
        public string Director { get; set; }
        public string Cast { get; set; }
        public string Plot { get; set; }
    }
}