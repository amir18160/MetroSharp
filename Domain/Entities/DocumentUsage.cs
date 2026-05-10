namespace Domain.Entities;

public class DocumentUsage
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public string UserId  { get; set; }
    public User User { get; set; }

    public Guid DocumentId { get; set; }
    public Document Document { get; set; }
}