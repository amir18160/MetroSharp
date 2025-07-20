using Application.Core;
using Application.Interfaces;
using Domain.Models.Scrapers.Yts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scrapers.Queries.GetAllYTSMovies
{
    public class Handler : IRequestHandler<Query, Result<List<YtsPreview>>>
    {
        private readonly IScraperFacade _scraperFacade;
        private readonly ILogger<Handler> _logger;

        public Handler(IScraperFacade scraperFacade, ILogger<Handler> logger)
        {
            _scraperFacade = scraperFacade;
            _logger = logger;
        }

        public async Task<Result<List<YtsPreview>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _scraperFacade.GetAllMoviesAsync(request.Page);
                if (result == null || result.Count == 0 )
                {
                    return Result<List<YtsPreview>>.Failure("No movies found.");
                }

                return Result<List<YtsPreview>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching YTS movies");
                return Result<List<YtsPreview>>.Failure("An error occurred while fetching movies.");
            }
        }
    }
}