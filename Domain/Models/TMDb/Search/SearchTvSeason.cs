using Domain.Models.TMDb.General;
namespace Domain.Models.TMDb.Search
{
    public class SearchTvSeason : SearchBase
    {
        public SearchTvSeason()
        {
            MediaType = MediaType.Season;
        }


        public DateTime? AirDate { get; set; }


        public int EpisodeCount { get; set; }


        public string Name { get; set; }


        public string Overview { get; set; }


        public string PosterPath { get; set; }


        public int SeasonNumber { get; set; }
    }
}