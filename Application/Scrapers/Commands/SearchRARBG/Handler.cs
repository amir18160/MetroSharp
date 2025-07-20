using Application.Core;
using Application.Interfaces;
using Domain.Models.Scrapers.Rarbg;
using Domain.Models.Scrapers.X1337;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scrapers.Commands.SearchRarbg
{
    public class Handler : IRequestHandler<Command, Result<List<RarbgPreview>>>
    {
        private readonly IScraperFacade _scraperFacade;
        private readonly ILogger<Handler> _logger;
        public Handler(IScraperFacade scraperFacade, ILogger<Handler> logger)
        {
            _logger = logger;
            _scraperFacade = scraperFacade;

        }

        public async Task<Result<List<RarbgPreview>>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _scraperFacade.SearchRarbgByImdbAsync(request.SearchTerm);
                if (result == null || result.Count == 0)
                {
                    return Result<List<RarbgPreview>>.Failure("No results found for the given search term.");
                }

                return Result<List<RarbgPreview>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling SearchRarbg command");
                return Result<List<RarbgPreview>>.Failure("Failed to Search perform a Rarbg search");
            }
        }
    }
}