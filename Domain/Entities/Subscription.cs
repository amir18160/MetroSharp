using Domain.Enums;

namespace Domain.Entities
{
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
        public SubscriptionStatus Status { get; set; }
    }
}
