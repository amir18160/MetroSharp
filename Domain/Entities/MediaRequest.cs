namespace Domain.Entities;

public class MediaRequest
{
    public Guid Id { get; set; }

    public string CreatorUserId { get; set; }
    public User CreatorUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public string RequestMessage { get; set; }
    
    public DateTime ResponseToAt { get; set; }
    public User ResponserUser { get; set; }
    public string ResponserUserId { get; set; }
    public string ResponseMessage { get; set; }

    public bool UserNotified { get; set; }
    public bool MarkAsSuccess { get; set; }
}