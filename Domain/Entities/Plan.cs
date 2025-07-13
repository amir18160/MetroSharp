namespace Domain.Entities
{
    public class Plan
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public int Length { get; set; }
        public int Price { get; set; }
        public int Discount { get; set; }
        public string DiscountReason { get; set; }
        public int NumOfDownloads { get; set; }
        public int NumOfRequest { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; }
    }
}
