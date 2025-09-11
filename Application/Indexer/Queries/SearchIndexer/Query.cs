using Application.Core;
using Domain.Models.Prowlarr;
using MediatR;

namespace Application.Indexer.Queries.SearchIndexer;

public class Query : IRequest<Result<List<SearchResult>>>
{
    public string SearchQuery { get; set; }
    public string Type { get; set; }
    public List<int> CategoryIds { get; set; }
    public List<int> IndexerIds { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
}