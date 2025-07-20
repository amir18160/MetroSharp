using Application.Core;
using MediatR;

namespace Application.Scrapers.Queries.GetTorrentDetails
{
    public class Query : IRequest<Result<object>>
    {
        public string Source { get; set; }
    }
}