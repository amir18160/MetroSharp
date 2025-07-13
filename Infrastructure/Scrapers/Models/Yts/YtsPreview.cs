namespace Infrastructure.Scrapers.Models.Yts
{
    public class YtsPreview
    {

        public string Title { get; set; }
        public string Year { get; set; }
        public string DetailUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Rating { get; set; }
        public List<string> Genres { get; set; }

    }
}