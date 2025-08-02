using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Search
{
    public class KnownForMovie : KnownForBase
    {
        public KnownForMovie()
        {
            MediaType = MediaType.Movie;
        }

        
        public bool Adult { get; set; }

        
        public string OriginalTitle { get; set; }

        
        public DateTime? ReleaseDate { get; set; }

        
        public string Title { get; set; }

        
        public bool Vide { get; set; }
    }
}