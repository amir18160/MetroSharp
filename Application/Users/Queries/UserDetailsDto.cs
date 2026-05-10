using Application.Subscriptions;
using Application.Subscriptions.Queries;

namespace Application.Users.Queries;

public class UserDetailsDto : UserBase
{
    public string Bio { get; set; }
    public string TelegramProfileImage { get; set; }
    public string PhoneNumber { get; set; }
    public string TelegramId { get; set; }
    public string UpdatedAt { get; set; }
    public bool IsBanned { get; set; }
    public UserSubscriptionDto CurrentActiveSubscription { get; set; }
}