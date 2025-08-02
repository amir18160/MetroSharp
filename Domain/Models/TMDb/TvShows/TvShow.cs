

using Domain.Models.TMDb.Changes;
using Domain.Models.TMDb.General;
using Domain.Models.TMDb.Movies;
using Domain.Models.TMDb.Reviews;
using Domain.Models.TMDb.Search;


namespace Domain.Models.TMDb.TvShows
{
    public class TvShow
    {
        
        public bool Adult { get; set; }

        
        public AccountState AccountStates { get; set; }

        
        public ResultContainer<AlternativeTitle> AlternativeTitles { get; set; }

        
        public string BackdropPath { get; set; }

        
        public ChangesContainer Changes { get; set; }

        
        public ResultContainer<ContentRating> ContentRatings { get; set; }

        
        public List<CreatedBy> CreatedBy { get; set; }

        
        public Credits Credits { get; set; }

        
        public CreditsAggregate AggregateCredits { get; set; }

        
        public ResultContainer<TvGroupCollection> EpisodeGroups { get; set; }

        
        public List<int> EpisodeRunTime { get; set; }

        
        public ExternalIdsTvShow ExternalIds { get; set; }

        
        public DateTime? FirstAirDate { get; set; }

        
    
        public List<int> GenreIds { get; set; }

        
        public List<Genre> Genres { get; set; }

        
        public string Homepage { get; set; }

        
        public int Id { get; set; }

        
        public Images Images { get; set; }

        
        public bool InProduction { get; set; }

        
        public ResultContainer<Keyword> Keywords { get; set; }

        
        public List<string> Languages { get; set; }

        
        public DateTime? LastAirDate { get; set; }

        
        public TvEpisodeBase LastEpisodeToAir { get; set; }

        
        public string Name { get; set; }

        
        public TvEpisodeBase NextEpisodeToAir { get; set; }

        
        public List<NetworkWithLogo> Networks { get; set; }

        
       
        public int NumberOfEpisodes { get; set; }

        
  
        public int NumberOfSeasons { get; set; }

        
        public string OriginalLanguage { get; set; }

        
        public string OriginalName { get; set; }


        public List<string> OriginCountry { get; set; }

        
        public string Overview { get; set; }

        
        public double Popularity { get; set; }

        
        public string PosterPath { get; set; }

        
        public List<ProductionCompany> ProductionCompanies { get; set; }

        
        public List<ProductionCountry> ProductionCountries { get; set; }

        
        public SearchContainer<SearchTv> Recommendations { get; set; }

        
        public SearchContainer<ReviewBase> Reviews { get; set; }

        
        public List<SearchTvSeason> Seasons { get; set; }

        
        public SearchContainer<SearchTv> Similar { get; set; }

        
        public List<SpokenLanguage> SpokenLanguages { get; set; }

        
        public string Status { get; set; }

        
        public string Tagline { get; set; }

        
        public TranslationsContainer Translations { get; set; }

        
        public string Type { get; set; }

        
        public ResultContainer<Video> Videos { get; set; }

        
        public SingleResultContainer<Dictionary<string, WatchProviders>> WatchProviders { get; set; }

        
        public double VoteAverage { get; set; }

        
        public int VoteCount { get; set; }
    }
}
