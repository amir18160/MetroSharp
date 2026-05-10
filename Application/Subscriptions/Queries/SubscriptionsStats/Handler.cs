using Application.Core;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Subscriptions.Queries.SubscriptionsStats;

public class Handler : IRequestHandler<Query, Result<SubscriptionStatsDto>>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public Handler(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionStatsDto>> Handle(Query request, CancellationToken cancellationToken)
    {
        var totalSubscriptions = await _context.Subscriptions.CountAsync(cancellationToken);
        var currentActiveSubscriptions = await _context.Subscriptions
            .Where(s => s.EndAt > DateTime.UtcNow)
            .CountAsync(cancellationToken);
        var expiredSubscriptions = totalSubscriptions - currentActiveSubscriptions;
        var totalMoneyEarned = await _context.Subscriptions.SumAsync(s => s.PaidPrice, cancellationToken);
        var activatedToday = await _context.Subscriptions.Where(s => s.StartedAt.Date == DateTime.UtcNow.Date)
            .CountAsync(cancellationToken);
        var activatedThisWeek = await _context.Subscriptions.Where(s => s.StartedAt >= DateTime.UtcNow.AddDays(-7))
            .CountAsync(cancellationToken);
        var activatedThisMonth = await _context.Subscriptions.Where(s => s.StartedAt >= DateTime.UtcNow.AddDays(-30))
            .CountAsync(cancellationToken);

        var stats = new SubscriptionStatsDto
        {
            TotalSubscriptions = totalSubscriptions,
            CurrentActiveSubscriptions = currentActiveSubscriptions,
            ExpiredSubscriptions = expiredSubscriptions,
            TotalMoneyEarned = totalMoneyEarned,
            ActivatedToday = activatedToday,
            ActivatedThisWeek = activatedThisWeek,
            ActivatedThisMonth = activatedThisMonth,
        };

        return Result<SubscriptionStatsDto>.Success(stats);
    }
}