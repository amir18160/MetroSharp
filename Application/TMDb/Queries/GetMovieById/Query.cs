using Application.Core;
using Domain.Models.TMDb.General;
using Domain.Models.TMDb.Movies;
using MediatR;

namespace Application.TMDb.Queries.GetMovieById;

public class Query : GetMediaDetailsParams, IRequest<Result<Movie>>
{
}