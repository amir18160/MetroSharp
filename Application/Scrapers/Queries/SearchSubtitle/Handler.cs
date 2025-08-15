using Application.Core;
using Application.Interfaces;
using Domain.Models.Scrapers.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scrapers.Queries.SearchSubtitle
{
    public class Handler : IRequestHandler<Query, Result<List<SubtitleSearch>>>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IScraperFacade _scraperFacade;
        public Handler(IScraperFacade scraperFacade, ILogger<Handler> logger)
        {
            _scraperFacade = scraperFacade;
            _logger = logger;
        }

        public async Task<Result<List<SubtitleSearch>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = new List<SubtitleSearch>();

            try
            {
                var subf2mResult = await _scraperFacade.SearchSubF2mAsync(request.SearchQuery);
                result.AddRange(subf2mResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scraper from subf2m. message: {message}", ex.Message);
            }

            try
            {
                var subsourceResult = await _scraperFacade.SearchSubsourceAsync(request.SearchQuery);
                result.AddRange(subsourceResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scraper from subsource. message: {message}", ex.Message);
            }

            return Result<List<SubtitleSearch>>.Success(result);       
        }
    }
}