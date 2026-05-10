using Application.Core;
using Domain.Enums;
using MediatR;

namespace Application.Subscriptions.Queries.QuerySubscriptions;

public class Query : PagingParams, IRequest<Result<PagedList<SubscriptionDto>>>
{
    public Guid? Id { get; set; }
    public DateFilter StartedAt { get; set; }
    public DateFilter EndAt { get; set; }
    public Guid? UserId { get; set; }
    public Guid? PlanId { get; set; }
    public SubscriptionStatus? Status { get; set; }
}