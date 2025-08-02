
using System;

namespace Domain.Models.TMDb.TvShows
{
    public class TvEpisodeBase : TvEpisodeInfo
    {
        
        public DateTime? AirDate { get; set; }

        
        public string Name { get; set; }

        
        public string Overview { get; set; }

        
        public string ProductionCode { get; set; }

        
        public int ShowId { get; set; }

        
        public string StillPath { get; set; }

        
        public double VoteAverage { get; set; }

        
        public int VoteCount { get; set; }

        
        public int? Runtime { get; set; }

        
        public string EpisodeType { get; set; }

    }
}
