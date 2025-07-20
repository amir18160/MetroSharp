using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scrapers.Queries.GetTorrentDetails
{
    public class Handler : IRequestHandler<Query, Result<object>>
    {
        private readonly IScraperFacade _scraperFacade;
        private readonly ILogger<Handler> _logger;

        public Handler(IScraperFacade scraperFacade, ILogger<Handler> logger)
        {
            _scraperFacade = scraperFacade;
            _logger = logger;
        }

        public async Task<Result<object>> Handle(Query request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Source))
                return Result<object>.Failure("Source cannot be empty.");

            try
            {
                object result = request.Source switch
                {
                    var s when s.StartsWith("https://therarbg") =>
                        await _scraperFacade.GetRarbgTitleDetailsAsync(s),

                    var s when s.StartsWith("https://yts.mx") =>
                        await _scraperFacade.GetYtsMovieDetailsAsync(s),

                    var s when s.Contains("1337x") =>
                        await _scraperFacade.GetX1337DetailsAsync(s),

                    _ => null
                };

                if (result == null)
                    return Result<object>.Failure("No scraper matched the provided source.");

                if (result is object typedResult)
                    return Result<object>.Success(typedResult);

                return Result<object>.Failure("Result type mismatch.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch data from scraper: {Source}", request.Source);
                return Result<object>.Failure("Failed to retrieve details.");
            }
        }
    }
}
