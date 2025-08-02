using System;

namespace Domain.Models.TMDb.Find
{
    public class FindTvSeason
    {
     
        public DateTime? AirDate { get; set; }

   
        public int EpisodeCount { get; set; }

       
        public int Id { get; set; }

        public string Name { get; set; }

    
        public string Overview { get; set; }

        public string PosterPath { get; set; }

      
        public int SeasonNumber { get; set; }

        public int ShowId { get; set; }
    }
}