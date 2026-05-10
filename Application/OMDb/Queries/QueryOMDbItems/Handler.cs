using Application.Core;
using Application.OMDb.DTOs;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.OMDb.Queries.QueryOMDbItems;

public class Handler : IRequestHandler<Query, Result<PagedList<OmdbItemDto>>>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public Handler(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<OmdbItemDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = _context.OmdbItems
            .AsQueryable();

        if (request.TagType.HasValue)
        {
            query = query
                .Where(x => x.Tags.Any(t => t.Type == request.TagType.Value))
                .Include(x => x.Tags);
        }

        if (!string.IsNullOrWhiteSpace(request.ImdbId))
        {
            query = query.Where(x => x.ImdbId == request.ImdbId);
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            query = query.Where(x => EF.Functions.Like(x.Title, $"%{request.Title}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Genre))
        {
            query = query.Where(x => x.Genres.Any(g => g.ToLower() == request.Genre.ToLower()));
        }
        
        if (request.IncludeTags) query = query.Include(x => x.Tags);
        if (request.IncludeDocuments) query = query.Include(x => x.Documents);
        if (request.IncludeSeasons) query = query.Include(x => x.Seasons);
        
        query = query.OrderByDescending(x => x.Year);
        
        var pagedList = await PagedList<OmdbItem>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize
        );
        
        var dtoPagedList = pagedList.Map<OmdbItemDto>(x => _mapper.Map<OmdbItemDto>(x));


        return Result<PagedList<OmdbItemDto>>.Success(dtoPagedList);

    }
}