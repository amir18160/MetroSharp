using Application.Core;
using Application.Interfaces;
using Domain.Models.TMDb.TvShows;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.TMDb.Queries.GetTvShowById
{
    public class Handler : IRequestHandler<Query, Result<TvShow>>
    {
        private readonly ITMDbService _tmdbService;
        private readonly ILogger<Handler> _logger;
        public Handler(ITMDbService tmdbService, ILogger<Handler> logger)
        {
            _logger = logger;
            _tmdbService = tmdbService;
        }

        public async Task<Result<TvShow>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var res = await _tmdbService.GetTvShowDetails(request);  
                return Result<TvShow>.Success(res);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "failed to get tmdb data because: {message}", ex.Message);
                return Result<TvShow>.Failure("Failed to get any data from tmdb");
            }
        }
    }
}