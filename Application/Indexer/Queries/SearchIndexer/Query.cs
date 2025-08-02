using Application.Core;
using MediatR;

namespace Application.Indexer.Queries.SearchIndexer
{
    public class Query : IRequest<Result<List<Domain.Models.Prowlarr.SearchResult>>>
    {
        public string SearchQuery { get; set; }
        public string Type { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<int> IndexerIds { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
    }
}