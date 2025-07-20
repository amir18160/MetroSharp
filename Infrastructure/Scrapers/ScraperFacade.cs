using Application.Interfaces;
using Domain.Models.Scrapers.Rarbg;
using Domain.Models.Scrapers.ScreenRent;
using Domain.Models.Scrapers.Sub2fm;
using Domain.Models.Scrapers.Subsource;
using Domain.Models.Scrapers.X1337;
using Domain.Models.Scrapers.Yts;
using Infrastructure.Scrapers.Sites;


namespace Infrastructure.Scrapers
{

    public class ScraperFacade : IScraperFacade
    {
        public ScraperFacade()
        {

        }

        #region Rarbg Methods

        public Task<List<RarbgPreview>> SearchRarbgByImdbAsync(string imdbId)
        {
            return RarbgScraper.SearchMoviesAsync(imdbId);
        }


        public Task<List<RarbgPreview>> GetLatestRarbgMoviesAsync(int page = 1)
        {
            return RarbgScraper.GetLatestMoviesAsync(page);
        }

        public Task<RarbgDetails> GetRarbgTitleDetailsAsync(string href)
        {
            return RarbgScraper.GetTitleDetailsByHrefAsync(href);
        }


        public Task<string> GetBestRarbgMagnetAsync(string imdbId)
        {
            return RarbgScraper.GetMostSuitedMagnetAsync(imdbId);
        }

        #endregion

        #region X1337 Methods


        public Task<List<X1337Preview>> SearchX1337MoviesAsync(string query)
        {
            return X1337Scraper.SearchMoviesAsync(query);
        }

        public Task<X1337Details> GetX1337DetailsAsync(string refLink)
        {
            return X1337Scraper.GetDetailsAsync(refLink);
        }

        public Task<List<X1337Preview>> GetX1337Top100MoviesAsync()
        {
            return X1337Scraper.GetTop100MoviesAsync();
        }

        #endregion

        #region YTS Methods


        public Task<List<YtsPreview>> SearchYtsMoviesAsync(string query)
        {
            return YtsScraper.SearchMovieAsync(query);
        }

        public Task<List<YtsPreview>> GetYtsPopularMoviesAsync()
        {
            return YtsScraper.GetPopularMoviesAsync();
        }

        public Task<YtsDetails> GetYtsMovieDetailsAsync(string url)
        {
            return YtsScraper.GetMovieDetailsAsync(url);
        }


        public Task<List<YtsPreview>> GetAllMoviesAsync(int page = 1)
        {
            return YtsScraper.GetAllMoviesAsync(page);
        }


        #endregion

        #region Subtitle Methods


        public Task<List<SubF2mSubtitleSearchResult>> SearchSubF2mAsync(string query)
        {
            return SubF2mScraper.SearchSubtitleAsync(query);
        }


        public Task<SubF2SubtitleDownload> GetSubF2mDownloadLinkAsync(string url)
        {
            return SubF2mScraper.GetDownloadLinkAsync(url);
        }

        public Task<ICollection<SubsourceSearchResult>> SearchSubsourceAsync(string query)
        {
            return SubsourceScraper.SearchSubtitle(query);
        }


        public Task<ICollection<SubsourceTableEntity>> GetSubsourceSubtitlesForUrlAsync(string url)
        {
            return SubsourceScraper.GetSubtitlesForUrl(url);
        }


        public Task<string> GetSubsourceDownloadLinkAsync(string url)
        {
            return SubsourceScraper.GetDownloadLinkFromUrl(url);
        }

        #endregion

        #region ScreenRant News Methods

        public Task<List<ScreenRantArticle>> GetScreenRantArticlesAsync(int page = 1)
        {
            return ScreenRantScraper.GetArticleListAsync(page);
        }

        public Task<ScreenRantArticleDetail> GetScreenRantArticleDetailsAsync(string url)
        {
            return ScreenRantScraper.GetArticleDetailsAsync(url);
        }


        #endregion
    }
}