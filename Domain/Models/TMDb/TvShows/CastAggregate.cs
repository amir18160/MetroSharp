using System.Collections.Generic;

using Domain.Models.TMDb.People;

namespace Domain.Models.TMDb.TvShows
{
    public class CastAggregate : CastBase
    {
        
        public List<CastRole> Roles { get; set; }

        
        public int TotalEpisodeCount { get; set; }
    }
}