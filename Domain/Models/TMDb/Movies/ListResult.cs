
using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Movies
{
    public class ListResult
    {
                public string Description { get; set; }

        
        public int FavoriteCount { get; set; }

        public string Id { get; set; }

 
        public string Iso_639_1 { get; set; }
        

        public int ItemCount { get; set; }

        
        public MediaType ListType { get; set; }

      
        public string Name { get; set; }

        
        public string PosterPath { get; set; }
    }
}