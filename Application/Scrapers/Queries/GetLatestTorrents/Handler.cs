using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scrapers.Queries.GetLatestTorrents
{
    public class Handler : IRequestHandler<Query, Result<List<AbstractedLatestTorrent>>>
    {
        private readonly IScraperFacade _scraperFacade;
        private readonly IMapper _mapper;
        private readonly ILogger<Handler> _logger;
        public Handler(IScraperFacade scraperFacade, IMapper mapper, ILogger<Handler> logger)
        {
            _logger = logger;
            _mapper = mapper;
            _scraperFacade = scraperFacade;
        }

        public async Task<Result<List<AbstractedLatestTorrent>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var torrentList = new List<AbstractedLatestTorrent>();

            try
            {
                var ytsPopular = await _scraperFacade.GetYtsPopularMoviesAsync();
                torrentList.AddRange(_mapper.Map<List<AbstractedLatestTorrent>>(ytsPopular));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to scrape data from yts");
            }

            try
            {
                var rarbgPopular = await _scraperFacade.GetLatestRarbgMoviesAsync();
                torrentList.AddRange(_mapper.Map<List<AbstractedLatestTorrent>>(rarbgPopular));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to scrape data from rarbg");
            }

            try
            {
                var x1337Popular = await _scraperFacade.GetX1337Top100MoviesAsync();
                torrentList.AddRange(_mapper.Map<List<AbstractedLatestTorrent>>(x1337Popular));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "failed to scrape data from 1337x");
            }

            if (torrentList.Count == 0)
            {
                return Result<List<AbstractedLatestTorrent>>.Failure("Failed to scrape any torrent at All.");
            }

            return Result<List<AbstractedLatestTorrent>>.Success(torrentList);
        }
    }
}