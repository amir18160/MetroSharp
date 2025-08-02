using System.Collections.Generic;

namespace Domain.Models.TMDb.Search
{
    public class SearchMovieTvBase : SearchBase
    {
        
        public string BackdropPath { get; set; }

        public List<int> GenreIds { get; set; }

        
        public string OriginalLanguage { get; set; }

        
        public string Overview { get; set; }

        
        public string PosterPath { get; set; }

        
        public double VoteAverage { get; set; }

        
        public int VoteCount { get; set; }
    }
}