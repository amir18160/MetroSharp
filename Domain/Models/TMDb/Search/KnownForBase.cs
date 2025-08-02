

using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Search
{
    public abstract class KnownForBase
    {
        
        public string BackdropPath { get; set; }

        
        public List<int> GenreIds { get; set; }

        
        public int Id { get; set; }

        
        public MediaType MediaType { get; set; }

        
        public string OriginalLanguage { get; set; }

        
        public string Overview { get; set; }

        
        public double Popularity { get; set; }

        
        public string PosterPath { get; set; }

        
        public double VoteAverage { get; set; }

        
        public int VoteCount { get; set; }
    }
}