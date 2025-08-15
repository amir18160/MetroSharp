using Application.Core;
using Application.Interfaces;
using Domain.Models.Scrapers.X1337;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scrapers.Queries.Search1337x
{
    public class Handler : IRequestHandler<Query, Result<List<X1337Preview>>>
    {
        private readonly IScraperFacade _scraperFacade;
        private readonly ILogger<Handler> _logger;
        public Handler(IScraperFacade scraperFacade, ILogger<Handler> logger)
        {
            _logger = logger;
            _scraperFacade = scraperFacade;

        }

        public async Task<Result<List<X1337Preview>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _scraperFacade.SearchX1337MoviesAsync(request.SearchTerm);
                if (result == null || result.Count == 0)
                {
                    return Result<List<X1337Preview>>.Failure("No results found for the given search term.");
                }

                return Result<List<X1337Preview>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling Search1337x Query");
                return Result<List<X1337Preview>>.Failure("Failed to Search perform a 1337x search");
            }
        }
    }
}