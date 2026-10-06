using Application.Plans.DTOs;
using Application.Plans.Queries;
using Application.Users.DTOs;
using Application.Users.Queries;

namespace Application.Subscriptions.DTOs;

public class SubscriptionDto: SubscriptionBase
{
    public UserBase User { get; set; }
    public PlanDto Plan { get; set; }
}