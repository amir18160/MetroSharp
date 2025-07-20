using Domain.Models.Scrapers.Rarbg;
using Domain.Models.Scrapers.ScreenRent;
using Domain.Models.Scrapers.Sub2fm;
using Domain.Models.Scrapers.Subsource;
using Domain.Models.Scrapers.X1337;
using Domain.Models.Scrapers.Yts;

namespace Application.Interfaces
{
    /// <summary>
    /// Defines a simplified, unified interface for all scraper functionalities.
    /// This contract abstracts the complexity of the underlying scraper subsystems.
    /// </summary>
    public interface IScraperFacade
    {
        #region Rarbg Methods

        /// <summary>
        /// Searches for movies on Rarbg using an IMDb ID.
        /// </summary>
        Task<List<RarbgPreview>> SearchRarbgByImdbAsync(string imdbId);

        /// <summary>
        /// Gets the latest movie releases from Rarbg.
        /// </summary>
        Task<List<RarbgPreview>> GetLatestRarbgMoviesAsync(int page = 1);

        /// <summary>
        /// Gets detailed information for a specific Rarbg title by its href.
        /// </summary>
        Task<RarbgDetails> GetRarbgTitleDetailsAsync(string href);

        /// <summary>
        /// Finds the most suitable magnet link from Rarbg for a given IMDb ID, prioritizing smaller file sizes.
        /// </summary>
        Task<string> GetBestRarbgMagnetAsync(string imdbId);

        #endregion

        #region X1337 Methods

        /// <summary>
        /// Searches for movies on 1337x.
        /// </summary>
        Task<List<X1337Preview>> SearchX1337MoviesAsync(string query);

        /// <summary>
        /// Gets detailed information for a specific 1337x torrent.
        /// </summary>
        Task<X1337Details> GetX1337DetailsAsync(string refLink);
        
        /// <summary>
        /// Gets the top 100 movies from 1337x.
        /// </summary>
        Task<List<X1337Preview>> GetX1337Top100MoviesAsync();

        #endregion

        #region YTS Methods

        /// <summary>
        /// Searches for movies on YTS.
        /// </summary>
        Task<List<YtsPreview>> SearchYtsMoviesAsync(string query);
        
        /// <summary>
        /// Gets popular movies from YTS.
        /// </summary>
        Task<List<YtsPreview>> GetYtsPopularMoviesAsync();

        /// <summary>
        /// Gets detailed movie information from a YTS URL.
        /// </summary>
        Task<YtsDetails> GetYtsMovieDetailsAsync(string url);

        /// <summary>
        /// Gets all movies information from a YTS URL.
        /// </summary>
        Task<List<YtsPreview>> GetAllMoviesAsync(int page = 1);

        #endregion

        #region Subtitle Methods

        /// <summary>
        /// Searches for subtitles on SubF2m.
        /// </summary>
        Task<List<SubF2mSubtitleSearchResult>> SearchSubF2mAsync(string query);

        /// <summary>
        /// Gets the final download link for a subtitle from SubF2m.
        /// </summary>
        Task<SubF2SubtitleDownload> GetSubF2mDownloadLinkAsync(string url);
        
        /// <summary>
        /// Searches for subtitles on Subsource.
        /// </summary>
        Task<ICollection<SubsourceSearchResult>> SearchSubsourceAsync(string query);

        /// <summary>
        /// Gets all subtitles available for a given Subsource show URL.
        /// </summary>
        Task<ICollection<SubsourceTableEntity>> GetSubsourceSubtitlesForUrlAsync(string url);

        /// <summary>
        /// Gets the final download link for a subtitle from Subsource.
        /// </summary>
        Task<string> GetSubsourceDownloadLinkAsync(string url);

        #endregion

        #region ScreenRant News Methods

        /// <summary>
        /// Gets a list of news articles from ScreenRant.
        /// </summary>
        Task<List<ScreenRantArticle>> GetScreenRantArticlesAsync(int page = 1);

        /// <summary>
        /// Gets the full details of a specific ScreenRant article.
        /// </summary>
        Task<ScreenRantArticleDetail> GetScreenRantArticleDetailsAsync(string url);

        #endregion
    }
}