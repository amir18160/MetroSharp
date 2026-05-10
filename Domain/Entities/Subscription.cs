using Domain.Enums;

namespace Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndAt { get; set; }
    public int PaidPrice { get; set; }
    public ICollection<string> Comments { get; set; } = new List<string>();

    public SubscriptionStatus Status
    {
        get { return EndAt > DateTime.Now ? SubscriptionStatus.Active : SubscriptionStatus.Expired; }
    }
}