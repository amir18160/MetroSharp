using Application.Core;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OMDb.Queries.QueryOMDbItems
{
    public class Handler : IRequestHandler<Query, Result<PagedList<OmdbItem>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<PagedList<OmdbItem>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.OmdbItems
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                query = query.Where(x => EF.Functions.Like(x.Title, $"{request.Title}%"));
            }

            if (!string.IsNullOrWhiteSpace(request.ImdbId))
            {
                query = query.Where(x => x.ImdbId == request.ImdbId);
            }

            if (request.IncludeTags)
            {
                query = query.Include(x => x.Tags);
            }

            if (request.IncludeDocuments)
            {
                query = query.Include(x => x.Documents);
            }

            if (request.IncludeSeasons)
            {
                query = query.Include(x => x.Seasons);
            }

            var pagedList = await PagedList<OmdbItem>.CreateAsync(
                query,
                request.PageNumber,
                request.PageSize);

            return Result<PagedList<OmdbItem>>.Success(pagedList);
        }
    }
}