using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.Search
{
    public class KnownForTv : KnownForBase
    {
        public KnownForTv()
        {
            MediaType = MediaType.Tv;
        }

        
        public DateTime? FirstAirDate { get; set; }

        
        public string Name { get; set; }

        
        public string OriginalName { get; set; }

        
        public List<string> OriginCountry { get; set; }
    }
}