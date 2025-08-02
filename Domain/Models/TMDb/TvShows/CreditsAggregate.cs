using System.Collections.Generic;

using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.TvShows
{
    public class CreditsAggregate
    {
        
        public List<CastAggregate> Cast { get; set; }

        
        public List<CrewAggregate> Crew { get; set; }

        
        public int Id { get; set; }
    }
}