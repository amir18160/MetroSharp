using Domain.Models.TMDb.Changes;
using Domain.Models.TMDb.General;
using Domain.Models.TMDb.Reviews;
using Domain.Models.TMDb.Search;

namespace Domain.Models.TMDb.Movies
{
    
    public class Movie
    {
        public AccountState AccountStates { get; set; }


        public bool Adult { get; set; }

        public string BackdropPath { get; set; }


        public SearchCollection BelongsToCollection { get; set; }


        public long Budget { get; set; }


        public ChangesContainer Changes { get; set; }


        public Credits Credits { get; set; }


        public List<Genre> Genres { get; set; }


        public string Homepage { get; set; }


        public int Id { get; set; }


        public Images Images { get; set; }


        public string ImdbId { get; set; }


        public KeywordsContainer Keywords { get; set; }


        public SearchContainer<ListResult> Lists { get; set; }


        public string OriginalLanguage { get; set; }


        public string OriginalTitle { get; set; }


        public string Overview { get; set; }


        public double? Popularity { get; set; }


        public string PosterPath { get; set; }


        public List<ProductionCompany> ProductionCompanies { get; set; }


        public List<ProductionCountry> ProductionCountries { get; set; }


        public DateTime? ReleaseDate { get; set; }


        public ResultContainer<ReleaseDatesContainer> ReleaseDates { get; set; }


        public ExternalIdsMovie ExternalIds { get; set; }


        public Releases Releases { get; set; }


        public long Revenue { get; set; }


        public SearchContainer<ReviewBase> Reviews { get; set; }


        public int? Runtime { get; set; }


        public SearchContainer<SearchMovie> Similar { get; set; }


        public SearchContainer<SearchMovie> Recommendations { get; set; }


        public List<SpokenLanguage> SpokenLanguages { get; set; }


        public string Status { get; set; }


        public string Tagline { get; set; }


        public string Title { get; set; }


        public TranslationsContainer Translations { get; set; }


        public bool Video { get; set; }


        public ResultContainer<Video> Videos { get; set; }


        public SingleResultContainer<Dictionary<string, WatchProviders>> WatchProviders { get; set; }


        public double VoteAverage { get; set; }


        public int VoteCount { get; set; }
    }
}