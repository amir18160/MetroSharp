

using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Reviews
{
    public class Review : ReviewBase
    {
        
        public string Iso_639_1 { get; set; }

        
        public int MediaId { get; set; }

        
        public string MediaTitle { get; set; }

        
        public MediaType MediaType { get; set; }
    }
}
