namespace Domain.Models.Scrapers.Common
{
    public class SubtitleListItem
    {
        public List<string> Names { get; set; }
        public SubtitleSource Source { get; set; }
        public string Caption { get; set; }
        public string Link { get; set; }
        public string Translator { get; set; }
    }
}