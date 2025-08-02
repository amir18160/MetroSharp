using Domain.Models.TMDb.General;
using Domain.Models.TMDb.Search;

namespace Domain.Models.TMDb.Collections
{
    public class Collection
    {
        public string BackdropPath { get; set; }
        public int Id { get; set; }
        public Images Images { get; set; }      
        public TranslationsContainer Translations { get; set; }
        public string Name { get; set; } 
        public string Overview { get; set; }
        public List<SearchMovie> Parts { get; set; }
        public string PosterPath { get; set; }
    }
}
