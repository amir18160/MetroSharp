using System;

using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Search
{
    public class SearchMovie : SearchMovieTvBase
    {
        public SearchMovie()
        {
            MediaType = MediaType.Movie;
        }

        
        public bool Adult { get; set; }

        
        public string OriginalTitle { get; set; }

        
        public DateTime? ReleaseDate { get; set; }

        
        public string Title { get; set; }

        
        public bool Video { get; set; }
    }
}