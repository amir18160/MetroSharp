using Domain.Enums;

namespace Application.Subscriptions;

public class SubscriptionBase
{
    public Guid Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndAt { get; set; }
    public int PaidPrice { get; set; }
    public ICollection<string> Comments { get; set; } = new List<string>();
    public SubscriptionStatus Status { set; get; }
}