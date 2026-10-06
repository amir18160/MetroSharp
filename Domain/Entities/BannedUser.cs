using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class BannedUser
{
    [Key] public string UserId { get; set; }
    public User User { get; set; }
    public string Reason { get; set; }
    public DateTime BannedAt { get; set; }
    public DateTime? FreeAt { get; set; }

    public override string ToString()
    {
        var freedAtDays = 999;
        if (FreeAt.HasValue)
        {
            freedAtDays =  (DateTime.Now - FreeAt.Value).Days;
        }
        return $"{User}|{Reason}|{freedAtDays}|{BannedAt:M/d/yy}";
    }
}