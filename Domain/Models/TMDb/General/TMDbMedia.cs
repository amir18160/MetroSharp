namespace Domain.Models.TMDb.General
{
    public class TMDbMedia
    {
        public int Id { get; set; }
        public bool Adult { get; set; }
        public string OriginalTitle { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Title { get; set; }
        public MediaType MediaType { get; set; }
        public bool Video { get; set; }
        public string Backdrop { get; set; }
        public List<int> GenreIds { get; set; }
        public string OriginalLanguage { get; set; }
        public string Overview { get; set; }
        public string Poster { get; set; }
        public double VoteAverage { get; set; }
        public int VoteCount { get; set; }
        public DateTime? FirstAirDate { get; set; }
        public List<string> OriginCountry { get; set; }
    }
}