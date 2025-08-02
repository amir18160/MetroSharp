using Application.Core;
using Domain.Models.TMDb.General;
using Domain.Models.TMDb.TvShows;
using MediatR;


namespace Application.TMDb.Queries.GetTvShowById
{
    public class Query : GetMediaDetailsParams, IRequest<Result<TvShow>>
    {
    }
}