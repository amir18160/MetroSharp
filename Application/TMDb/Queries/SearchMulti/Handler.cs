using Application.Core;
using Application.Interfaces;
using Domain.Models.TMDb;
using Domain.Models.TMDb.General;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.TMDb.Queries.SearchMulti
{
    public class Handler : IRequestHandler<Query, Result<List<TMDbMedia>>>
    {
        private readonly ITMDbService _tmdbService;
        private readonly ILogger<Handler> _logger;
        public Handler(ITMDbService tmdbService, ILogger<Handler> logger)
        {
            _logger = logger;
            _tmdbService = tmdbService;
        }

        public async Task<Result<List<TMDbMedia>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var searchQuery = new MultiSearchParams
                {
                    Query = request.SearchQuery
                };


                if (request.Page > 0)
                {
                    searchQuery.Page = request.Page;
                }
                if (request.Year > 0)
                {
                    searchQuery.Year = request.Year;
                }

                var res = await _tmdbService.SearchMulti(searchQuery);  
                return Result<List<TMDbMedia>>.Success(res);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "failed to get tmdb data because: {message}", ex.Message);
                return Result<List<TMDbMedia>>.Failure("Failed to get any data from tmdb");
            }
        }
    }
}