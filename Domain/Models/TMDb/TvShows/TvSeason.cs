using System;
using System.Collections.Generic;

using Domain.Models.TMDb.General;
using Domain.Models.TMDb.Search;

namespace Domain.Models.TMDb.TvShows
{
    public class TvSeason
    {
        
        public ResultContainer<TvEpisodeAccountStateWithNumber> AccountStates { get; set; }

        
        public DateTime? AirDate { get; set; }

        
        public Credits Credits { get; set; }

        
        public List<TvSeasonEpisode> Episodes { get; set; }

        
        public ExternalIdsTvSeason ExternalIds { get; set; }


        public int? Id { get; set; }

        
        public PosterImages Images { get; set; }

        
        public string Name { get; set; }

        
        public string Overview { get; set; }

        
        public string PosterPath { get; set; }

        
        public int SeasonNumber { get; set; }

        
        public double VoteAverage { get; set; }

        
        public ResultContainer<Video> Videos { get; set; }

        
        public TranslationsContainer Translations { get; set; }
    }
}
