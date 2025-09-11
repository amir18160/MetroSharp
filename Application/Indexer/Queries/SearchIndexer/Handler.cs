using Application.Core;
using Application.Interfaces;
using Domain.Models.Prowlarr;
using MediatR;

namespace Application.Indexer.Queries.SearchIndexer;

public class Handler : IRequestHandler<Query, Result<List<SearchResult>>>
{
    private readonly IProwlarr _prowlarr;

    public Handler(IProwlarr prowlarr)
    {
        _prowlarr = prowlarr;
    }

    public async Task<Result<List<SearchResult>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var searchOptions = new SearchOptions
        {
            Query = request.SearchQuery
        };

        if (request.Offset.HasValue) searchOptions.Offset = (int)request.Offset;
        if (request.Limit.HasValue) searchOptions.Limit = (int)request.Limit;
        if (request.CategoryIds?.Count != 0) searchOptions.CategoryIds = request.CategoryIds;
        if (request.IndexerIds?.Count != 0) searchOptions.IndexerIds = request.IndexerIds;

        try
        {
            var result = await _prowlarr.Search(searchOptions);
            return Result<List<SearchResult>>.Success(result);
        }
        catch (Exception)
        {
            return Result<List<SearchResult>>.Failure("Failed to get providers from indexer");
        }
    }
}