using Application.Core;
using Domain.Models.TMDb.General;
using Domain.Models.TMDb.Movies;
using Domain.Models.TMDb.TvShows;
using MediatR;


namespace Application.TMDb.Queries.GetMovieById
{
    public class Query : GetMediaDetailsParams, IRequest<Result<Movie>>
    {
    }
}