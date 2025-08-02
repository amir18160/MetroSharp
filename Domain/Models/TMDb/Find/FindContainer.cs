using System.Collections.Generic;
using Domain.Models.TMDb.Search;

namespace Domain.Models.TMDb.Find
{
    public class FindContainer
    {
   
        public List<SearchMovie> MovieResults { get; set; }

        
        public List<FindPerson> PersonResults { get; set; } // Unconfirmed type

        public List<SearchTvEpisode> TvEpisode { get; set; }

       
        public List<SearchTv> TvResults { get; set; }

    
        public List<FindTvSeason> TvSeason { get; set; }
    }
}