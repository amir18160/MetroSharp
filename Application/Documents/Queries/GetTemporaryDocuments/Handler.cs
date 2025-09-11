using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Persistence;

namespace Application.Documents.Queries.GetTemporaryDocuments;

public class Handler : IRequestHandler<Query, Result<PagedList<DocumentDto>>>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public Handler(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<DocumentDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = _context.Documents
            .Where(x => x.UniqueFileId.StartsWith("temp_WTelegramBot"))
            .OrderByDescending(x => x.CreatedAt)
            .ProjectTo<DocumentDto>(_mapper.ConfigurationProvider)
            .AsQueryable();

        var pagedList = await PagedList<DocumentDto>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize);

        return Result<PagedList<DocumentDto>>.Success(pagedList);
    }
}