


namespace Domain.Models.TMDb.People
{
    public class TvCredits
    {
        
        public List<TvRole> Cast { get; set; }

        
        public List<TvJob> Crew { get; set; }

        
        public int Id { get; set; }
    }
}