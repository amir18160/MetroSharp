using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Subscriptions.Commands.ExtendSubscriptions;

public class Handler : IRequestHandler<Command, Result<Unit>>
{
    private readonly DataContext _context;

    public Handler(DataContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
    {
        var subscriptionGoesDownDate = DateTime.UtcNow.AddDays(request.ExpirationMargin * (-1));

        var mustBeUpdated =
            await _context.Subscriptions.Where(x => x.EndAt > subscriptionGoesDownDate)
                .ToListAsync(cancellationToken);

        if (mustBeUpdated.Count == 0)
        {
            return Result<Unit>.Failure("There are no subscribed in that margin days.");
        }

        mustBeUpdated.ForEach(s =>
        {
            // currentDay - margin = the date that subscription start going off      * diff 1
            // endDate - the date that subscription start going off =>               * diff 2
            //                                                     if positive => days to add + absolute(diff 2) 
            //                                                     if negative => days to add + absolute(diff 2)

            var differenceWithEndDate = s.EndAt.Subtract(subscriptionGoesDownDate).TotalDays;
            s.EndAt = DateTime.UtcNow.AddDays(Math.Abs(differenceWithEndDate) + request.DaysToAdd);
        });

        _context.Subscriptions.UpdateRange(mustBeUpdated);
        var success = await _context.SaveChangesAsync(cancellationToken) > 0;
        return success ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Failed to Update subscriptions.");
    }
}