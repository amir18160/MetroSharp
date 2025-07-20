using Application.Core;
using Application.Interfaces;
using Domain.Models.Scrapers.X1337;
using Domain.Models.Scrapers.Yts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scrapers.Commands.SearchYTS
{
    public class Handler : IRequestHandler<Command, Result<List<YtsPreview>>>
    {
        private readonly IScraperFacade _scraperFacade;
        private readonly ILogger<Handler> _logger;
        public Handler(IScraperFacade scraperFacade, ILogger<Handler> logger)
        {
            _logger = logger;
            _scraperFacade = scraperFacade;

        }

        public async Task<Result<List<YtsPreview>>> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _scraperFacade.SearchYtsMoviesAsync(request.SearchTerm);
                if (result == null || result.Count == 0)
                {
                    return Result<List<YtsPreview>>.Failure("No results found for the given search term.");
                }

                return Result<List<YtsPreview>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while handling SearchYts command");
                return Result<List<YtsPreview>>.Failure("Failed to Search perform a Yts search");
            }
        }
    }
}