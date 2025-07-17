namespace Application.Users.Queries.GetSingleUser
{
    public class UserDetailsDto : UserBase
    {
        public string Bio { get; set; }
        public string TelegramProfileImage { get; set; }
        public string PhoneNumber { get; set; }
        public string TelegramId { get; set; }
        public string UpdatedAt { get; set; }
        public bool IsBanned { get; set; }
        public bool HasActiveSubscription { get; set; }
    }
}