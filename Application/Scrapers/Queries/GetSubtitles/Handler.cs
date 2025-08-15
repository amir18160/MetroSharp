using Application.Core;
using Application.Interfaces;
using Domain.Models.Scrapers.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Scrapers.Queries.GetSubtitles
{
    public class Handler : IRequestHandler<Query, Result<List<SubtitleListItem>>>
    {
        private readonly ILogger<Handler> _logger;
        private readonly IScraperFacade _scraperFacade;
        public Handler(ILogger<Handler> logger, IScraperFacade scraperFacade)
        {
            _scraperFacade = scraperFacade;
            _logger = logger;

        }

        public async Task<Result<List<SubtitleListItem>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                List<SubtitleListItem> result = null;
                
                if (request.URL.StartsWith("https://api.subsource.net"))
                {
                    result = await _scraperFacade.GetSubsourceSubtitlesForUrlAsync(request.URL);
                }
                else if (request.URL.StartsWith("https://subf2m.co"))
                {
                    result = await _scraperFacade.GetSubF2mSubtitlesAsync(request.URL);
                }

                return Result<List<SubtitleListItem>>.Success(result);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get subtitles list for url: {url}", request.URL);
                return Result<List<SubtitleListItem>>.Failure("Failed to get subtitles list");
            }

        }
    }
}