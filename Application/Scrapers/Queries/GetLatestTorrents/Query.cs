using Application.Core;
using MediatR;

namespace Application.Scrapers.Queries.GetLatestTorrents
{
    public class Query : IRequest<Result<List<AbstractedLatestTorrent>>>
    {

    }
}