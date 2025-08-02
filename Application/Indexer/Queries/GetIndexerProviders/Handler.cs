using Application.Core;
using Application.Interfaces;
using MediatR;

namespace Application.Indexer.Queries.GetIndexerProviders
{
    public class Handler : IRequestHandler<Query, Result<List<Domain.Models.Prowlarr.Indexer>>>
    {
        private readonly IProwlarr _prowlarr;
        public Handler(IProwlarr prowlarr)
        {
            _prowlarr = prowlarr;
        }

        public async Task<Result<List<Domain.Models.Prowlarr.Indexer>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _prowlarr.GetIndexersAsync();
                return Result<List<Domain.Models.Prowlarr.Indexer>>.Success(result);
            }
            catch
            {
                return Result<List<Domain.Models.Prowlarr.Indexer>>.Failure("Failed to get providers from indexer");
            }
        }
    }
}