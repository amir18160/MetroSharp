using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public bool IsConfirmed { get; set; }
        public string Bio { get; set; }
        public string TelegramProfileImage { get; set; }
        public string TelegramId { get; set; }
        public string Image { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } =  new List<Subscription>();
        public ICollection<EmailVerification> EmailVerifications { get; set; } =  new List<EmailVerification>();

        public BannedUser BannedUser { get; set; }
    }
}
