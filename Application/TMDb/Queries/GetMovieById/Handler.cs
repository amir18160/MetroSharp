using Application.Core;
using Application.Interfaces;
using Domain.Models.TMDb.Movies;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.TMDb.Queries.GetMovieById
{
    public class Handler : IRequestHandler<Query, Result<Movie>>
    {
        private readonly ITMDbService _tmdbService;
        private readonly ILogger<Handler> _logger;
        public Handler(ITMDbService tmdbService, ILogger<Handler> logger)
        {
            _logger = logger;
            _tmdbService = tmdbService;
        }

        public async Task<Result<Movie>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var res = await _tmdbService.GetMovieDetails(request);  
                return Result<Movie>.Success(res);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "failed to get tmdb data because: {message}", ex.Message);
                return Result<Movie>.Failure("Failed to get any data from tmdb");
            }
        }
    }
}