using Application.Plans.Queries;

namespace Application.Subscriptions;

public class UserSubscriptionDto: SubscriptionBase
{
    public PlanDto Plan { get; set; }
}