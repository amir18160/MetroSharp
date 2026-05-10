using Application.Plans.Queries;
using Application.Users.Queries;

namespace Application.Subscriptions;

public class SubscriptionDto: SubscriptionBase
{
    public UserBase User { get; set; }
    public PlanDto Plan { get; set; }
}