using Application.Core;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Subscriptions.Commands.AddSubscription;

public class Handler : IRequestHandler<Command, Result<Unit>>
{
    private readonly DataContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IUserAccessor _userAccessor;

    public Handler(DataContext context, UserManager<User> userManager, IUserAccessor userAccessor)
    {
        _context = context;
        _userManager = userManager;
        _userAccessor = userAccessor;
    }

    public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
    {
        var plan = await _context.Plans.FindAsync(request.PlanId, cancellationToken);
        if (plan == null)
        {
            return Result<Unit>.Failure("Plan with provided id was not found.");
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<Unit>.Failure("User with provided id was not found.");
        }

        var existingSubscription = await _context.Subscriptions.FirstOrDefaultAsync(s =>
            s.UserId == user.Id && s.PlanId == request.PlanId && s.UserId == request.UserId, cancellationToken);

        if (existingSubscription != null)
        {
            return Result<Unit>.Failure("User already has a subscription.");
        }

        var addedBy =
            await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUserName(), cancellationToken);
        _context.Subscriptions.Add(new Subscription
        {
            User = user,
            Plan = plan,
            StartedAt = DateTime.Now,
            EndAt = DateTime.UtcNow.AddDays(plan.Length),
            PaidPrice = 0,
            Comments = new List<string>()
            {
                $"[{DateTime.UtcNow.ToShortDateString()}] [{addedBy?.UserName ?? "unknown"}] [{addedBy?.Id}]:  Subscription added by admin."
            }
        });

        var success = await _context.SaveChangesAsync(cancellationToken) > 0;

        return success ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure("Failed to add subscription.");
    }
}