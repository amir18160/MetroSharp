using Domain.Enums;

namespace Domain.Entities
{
    public class OmdbItem
    {
        public Guid Id { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string Rated { get; set; }
        public string Released { get; set; }
        public string Runtime { get; set; }
        public ICollection<string> Genres { get; set; }
        public ICollection<string> Actors { get; set; }
        public string Plot { get; set; }
        public string PlotFa { get; set; }
        public ICollection<string> Languages { get; set; }
        public ICollection<string> Countries { get; set; }
        public string Awards { get; set; }
        public string Poster { get; set; }
        public int? Metascore { get; set; }
        public int? RottenTomatoesScore { get; set; }
        public double? ImdbRating { get; set; }
        public double? ImdbVotes { get; set; }
        public OmdbItemType Type { get; set; }
        public string BoxOffice { get; set; }
        public int? TotalSeasons { get; set; }
        public ICollection<string> Directors { get; set; }
        public ICollection<string> Writers { get; set; }
        public int? Year { get; set; }

        /**********************/
        /***** Relations ******/
        /**********************/
        public ICollection<Season> Seasons { get; set; } = new List<Season>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}