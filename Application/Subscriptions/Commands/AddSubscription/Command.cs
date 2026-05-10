using Application.Core;
using MediatR;

namespace Application.Subscriptions.Commands.AddSubscription;

public class Command: IRequest<Result<Unit>>
{
    public string UserId { get; set; }
    public Guid PlanId { get; set; }
}