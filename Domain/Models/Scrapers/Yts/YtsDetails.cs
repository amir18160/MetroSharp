namespace Domain.Models.Scrapers.Yts
{
    public class YtsDetails
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string DetailUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Description { set; get; }
        public string ImdbId { get; set; }
        public string ImdbUrl { get; set; }
        public string ImdbRating { get; set; }
        
        
        public List<string> Genres { get; set; }
        public ICollection<YtsTorrent> AvailableTorrents { get; set; }
    }
}