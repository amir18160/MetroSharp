using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Subscriptions.Queries.QuerySubscriptions;

public class Handler : IRequestHandler<Query, Result<PagedList<SubscriptionDto>>>
{
    private readonly UserManager<User> _userManager;
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public Handler(UserManager<User> userManager, DataContext context, IMapper mapper)
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PagedList<SubscriptionDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = _context.Subscriptions
            .Include(x => x.Plan)
            .Include(x => x.User)
            .AsQueryable();


        if (request.PlanId.HasValue)
            query = query.Where(s => s.PlanId == request.PlanId);

        if (!string.IsNullOrWhiteSpace(request.UserId.ToString()))
            query = query.Where(s => s.UserId == request.UserId.ToString());

        if (request.StartedAt != null)
            query = query.ApplyDateFilter(request.StartedAt, t => t.StartedAt);

        if (request.EndAt != null)
            query = query.ApplyDateFilter(request.EndAt, t => t.EndAt);


        var pagedTasks = await query.Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<SubscriptionDto>(_mapper.ConfigurationProvider)
            .OrderByDescending(x=> x.StartedAt)
            .ToListAsync(cancellationToken);
        
        var count = await _context.Subscriptions.CountAsync(cancellationToken);


        var result = new PagedList<SubscriptionDto>(pagedTasks, count, request.PageSize, request.PageSize);
        return Result<PagedList<SubscriptionDto>>.Success(result);
    }
}