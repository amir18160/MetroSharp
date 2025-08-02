

using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Movies
{
    public class KeywordsContainer
    {
     
        public int Id { get; set; }

        
        public List<Keyword> Keywords { get; set; }
    }
}