
using Domain.Models.TMDb.People;

namespace Domain.Models.TMDb.General
{
    public class CrewAggregate : CrewBase
    {
       
        public List<CrewJob> Jobs { get; set; }
        
    
        public int TotalEpisodeCount { get; set; }
    }
}