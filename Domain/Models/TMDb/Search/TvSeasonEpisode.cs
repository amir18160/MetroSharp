using Domain.Models.TMDb.General;
using Domain.Models.TMDb.TvShows;

namespace Domain.Models.TMDb.Search
{
    public class TvSeasonEpisode
    {
        
        public DateTime? AirDate { get; set; }

        
        public List<Crew> Crew { get; set; }

        
        public int EpisodeNumber { get; set; }

        
        public string EpisodeType { get; set; }

        
        public List<Cast> GuestStars { get; set; }

        
        public int Id { get; set; }

        
        public string Name { get; set; }

        
        public string Overview { get; set; }

        
        public string ProductionCode { get; set; }

        
        public int? Runtime { get; set; }

        
        public int SeasonNumber { get; set; }

        
        public string StillPath { get; set; }

        
        public double VoteAverage { get; set; }

        
        public int VoteCount { get; set; }
    }
}
