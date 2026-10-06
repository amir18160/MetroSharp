using Application.Plans.DTOs;
using Application.Plans.Queries;

namespace Application.Subscriptions.DTOs;

public class UserSubscriptionDto: SubscriptionBase
{
    public PlanDto Plan { get; set; }
}