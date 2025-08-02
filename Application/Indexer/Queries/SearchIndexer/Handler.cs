using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Core;
using Application.Interfaces;
using Domain.Models.Prowlarr;
using MediatR;

namespace Application.Indexer.Queries.SearchIndexer
{
    public class Handler : IRequestHandler<Query, Result<List<Domain.Models.Prowlarr.SearchResult>>>
    {
        private readonly IProwlarr _prowlarr;
        public Handler(IProwlarr prowlarr)
        {
            _prowlarr = prowlarr;
        }

        public async Task<Result<List<Domain.Models.Prowlarr.SearchResult>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var searchOptions = new Domain.Models.Prowlarr.SearchOptions
            {
                Query = request.SearchQuery,
            };

            if (request.Offset.HasValue)
            {
                searchOptions.Offset = (int)request.Offset;
            }
            if (request.Limit.HasValue)
            {
                searchOptions.Limit = (int)request.Limit;
            }
            if (request.CategoryIds?.Count != 0)
            {
                searchOptions.CategoryIds = request.CategoryIds;
            }
            if (request.IndexerIds?.Count != 0)
            {
                searchOptions.IndexerIds = request.IndexerIds;
            }

            try
            {
                var result = await _prowlarr.Search(searchOptions);
                return Result<List<Domain.Models.Prowlarr.SearchResult>>.Success(result);
            }
            catch (System.Exception)
            {
                return Result<List<Domain.Models.Prowlarr.SearchResult>>.Failure("Failed to get providers from indexer");
            }
        }
    }
}