namespace Domain.Entities;

public class Plan
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int Length { get; set; }
    public int Price { get; set; }
    public int DiscountPercent { get; set; } = 0;
    public string DiscountReason { get; set; }
    public int NumberOfDirectLinks { get; set; }
    public int NumberOfFileDownloads { get; set; }
    public int NumberOfRequest { get; set; }
    public bool IsActive { get; set; } =  true;
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public override string ToString()
    {
        return $"{Title}|a:{IsActive}|p:{Price}|l:{Length}";
    }
}