using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.BannedUsers.Queries.GetBannedUsers
{
    public class Handler : IRequestHandler<Query, Result<PagedList<BannedUserDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedList<BannedUserDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.BannedUsers
                .Include(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.UserId))
                query = query.Where(x => x.UserId == request.UserId);

            if (!string.IsNullOrWhiteSpace(request.UserName))
                query = query.Where(x => x.User.UserName.Contains(request.UserName));

            if (!string.IsNullOrWhiteSpace(request.Name))
                query = query.Where(x => x.User.Name.Contains(request.Name));

            if (!string.IsNullOrWhiteSpace(request.Email))
                query = query.Where(x => x.User.Email.Contains(request.Email));

            if (request.BannedAt != null)
                query = query.ApplyDateFilter(request.BannedAt, x => x.BannedAt);

            if (request.FreeAt != null)
                query = query.ApplyDateFilter(request.FreeAt, x => x.FreeAt);

            if (request.IsIndefinite.HasValue && request.IsIndefinite.Value)
                query = query.Where(x => x.FreeAt == null);

            var projected = query
                .ProjectTo<BannedUserDto>(_mapper.ConfigurationProvider)
                .AsNoTracking();

            var paged = await PagedList<BannedUserDto>.CreateAsync(
                projected,
                request.PageNumber,
                request.PageSize);

            return Result<PagedList<BannedUserDto>>.Success(paged);
        }
    }
}
