

namespace Domain.Models.TMDb.Search
{
    public class SearchList
    {
        
        public string Description { get; set; }

        
        public int FavoriteCount { get; set; }

        
        public string Id { get; set; }

        /// <summary>
        /// A language code, e.g. en
        /// </summary>
        
        public string Iso_639_1 { get; set; }

        
        public int ItemCount { get; set; }

        
        public string ListType { get; set; }

        
        public string Name { get; set; }

        
        public string PosterPath { get; set; }
    }
}