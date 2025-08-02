using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Search
{
    public class SearchBase
    {
        
        public int Id { get; set; }

        
        public MediaType MediaType { get; set; }

        
        public double Popularity { get; set; }
    }
}