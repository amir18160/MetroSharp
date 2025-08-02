using System.Collections.Generic;

using Domain.Models.TMDb.General;

namespace Domain.Models.TMDb.TvShows
{
    public class TvEpisode : TvEpisodeBase
    {
        
        public TvAccountState AccountStates { get; set; }

        
        public CreditsWithGuestStars Credits { get; set; }

        
        public List<Crew> Crew { get; set; }

        
        public ExternalIdsTvEpisode ExternalIds { get; set; }

        
        public List<Cast> GuestStars { get; set; }

        
        public StillImages Images { get; set; }

        
        public ResultContainer<Video> Videos { get; set; }

        
        public TranslationsContainer Translations { get; set; }
    }
}
