using System.Text.Json;
using Application.Interfaces;
using AutoMapper;
using Domain.Models.Scrapers.Common;
using Domain.Models.Scrapers.Rarbg;
using Domain.Models.Scrapers.ScreenRent;
using Domain.Models.Scrapers.X1337;
using Domain.Models.Scrapers.Yts;
using Infrastructure.Scrapers.Sites;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Scrapers;

public class ScraperFacade : IScraperFacade
{
    private readonly IMapper _mapper;
    private readonly ILogger<ScraperFacade> _logger;

    public ScraperFacade(IMapper mapper, ILogger<ScraperFacade> logger)
    {
        _logger = logger;
        _mapper = mapper;
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

    public async Task<List<YtsPreview>> SearchYtsMoviesAsync(string query)
    {
        try
        {
            var result = await YtsScraper.SearchMovieAsync(query);
            _logger.LogInformation(JsonSerializer.Serialize(result));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
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

    public async Task<List<SubtitleSearch>> SearchSubF2mAsync(string query)
    {
        var result = await SubF2mScraper.SearchSubtitleAsync(query);
        return _mapper.Map<List<SubtitleSearch>>(result);
    }

    public async Task<List<SubtitleListItem>> GetSubF2mSubtitlesAsync(string url)
    {
        var result = await SubF2mScraper.GetSubtitlesAsync(url);
        return _mapper.Map<List<SubtitleListItem>>(result);
    }


    public async Task<string> GetSubF2mDownloadLinkAsync(string url)
    {
        var result = await SubF2mScraper.GetDownloadLinkAsync(url);
        return result.DownloadUrl;
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