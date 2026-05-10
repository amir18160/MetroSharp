namespace Application.Subscriptions.Queries.SubscriptionsStats;

public class SubscriptionStatsDto
{
    public int TotalSubscriptions { get; set; }
    public int CurrentActiveSubscriptions { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public int TotalMoneyEarned { get; set; }
    public int ActivatedToday { get; set; }
    public int ActivatedThisWeek { get; set; }
    public int ActivatedThisMonth { get; set; }
}
