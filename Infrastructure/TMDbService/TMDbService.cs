using System.Text.Json;
using Application.Interfaces;
using AutoMapper;
using Domain.Models.TMDb;
using Domain.Models.TMDb.General;
using Infrastructure.TMDbService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TMDbLib.Client;



namespace Infrastructure.TMDbService
{
    public class TMDbService : ITMDbService
    {
        private readonly TMDbClient _client;
        private readonly TMDbSettings _settings;
        private readonly IMapper _mapper;
        private readonly ILogger<TMDbService> _logger;

        public TMDbService(IOptions<TMDbSettings> options, IMapper mapper, ILogger<TMDbService> logger)
        {
            _logger = logger;
            _mapper = mapper;
            _settings = options.Value;
            _client = new TMDbClient(_settings.ApiKey);
        }


        public async Task<List<TMDbMedia>> SearchMulti(MultiSearchParams @params)
        {
            var movieResult = await _client.SearchMovieAsync(@params.Query, @params.Page, year: @params.Year);
            var tvResult = await _client.SearchTvShowAsync(@params.Query, @params.Page, firstAirDateYear: @params.Year);

            var finalResult = new List<TMDbMedia>();

            finalResult.AddRange(_mapper.Map<List<TMDbMedia>>(movieResult.Results));
            finalResult.AddRange(_mapper.Map<List<TMDbMedia>>(tvResult.Results));

            return finalResult.OrderByDescending(x => x.VoteCount).ToList();
        }

        public async Task<Domain.Models.TMDb.Movies.Movie> GetMovieDetails(GetMediaDetailsParams @params)
        {
            TMDbLib.Objects.Movies.MovieMethods methods = TMDbLib.Objects.Movies.MovieMethods.Undefined;

            if (@params.IncludeExternalIds)
            {
                methods |= TMDbLib.Objects.Movies.MovieMethods.ExternalIds;
            }
            if (@params.IncludeImages)
            {
                methods |= TMDbLib.Objects.Movies.MovieMethods.Images;
            }
            if (@params.IncludeCredits)
            {
                methods |= TMDbLib.Objects.Movies.MovieMethods.Credits;
            }
            if (@params.IncludeSimilar)
            {
                methods |= TMDbLib.Objects.Movies.MovieMethods.Similar;
            }

            TMDbLib.Objects.Movies.Movie movie = await _client.GetMovieAsync(@params.Id, methods);

            return _mapper.Map<Domain.Models.TMDb.Movies.Movie>(movie);
        }


        public async Task<Domain.Models.TMDb.TvShows.TvShow> GetTvShowDetails(GetMediaDetailsParams @params)
        {

            TMDbLib.Objects.TvShows.TvShowMethods methods = TMDbLib.Objects.TvShows.TvShowMethods.Undefined;

            if (@params.IncludeExternalIds)
            {
                methods |= TMDbLib.Objects.TvShows.TvShowMethods.ExternalIds;
            }
            if (@params.IncludeImages)
            {
                methods |= TMDbLib.Objects.TvShows.TvShowMethods.Images;
            }
            if (@params.IncludeCredits)
            {
                methods |= TMDbLib.Objects.TvShows.TvShowMethods.Credits;
            }
            if (@params.IncludeSimilar)
            {
                methods |= TMDbLib.Objects.TvShows.TvShowMethods.Similar;
            }
            if (@params.IncludeEpisodeGroups)
            {
                methods |= TMDbLib.Objects.TvShows.TvShowMethods.EpisodeGroups;
            }

            TMDbLib.Objects.TvShows.TvShow tvShow = await _client.GetTvShowAsync(@params.Id, methods);

            return _mapper.Map<Domain.Models.TMDb.TvShows.TvShow>(tvShow);
        }

    }
}