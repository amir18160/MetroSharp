using Application.Core;
using MediatR;

namespace Application.Indexer.Queries.GetIndexerProviders
{
    public class Query: IRequest<Result<List<Domain.Models.Prowlarr.Indexer>>>
    {
    }
}