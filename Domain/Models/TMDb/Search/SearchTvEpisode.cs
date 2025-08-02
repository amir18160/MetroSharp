using Domain.Models.TMDb.General;
namespace Domain.Models.TMDb.Search
{
    public class SearchTvEpisode : SearchBase
    {
        public SearchTvEpisode()
        {
            MediaType = MediaType.Episode;
        }


        public DateTime? AirDate { get; set; }


        public int EpisodeNumber { get; set; }


        public string Name { get; set; }


        public string Overview { get; set; }


        public string ProductionCode { get; set; }


        public int SeasonNumber { get; set; }


        public int ShowId { get; set; }


        public string StillPath { get; set; }


        public double VoteAverage { get; set; }


        public int VoteCount { get; set; }
    }
}