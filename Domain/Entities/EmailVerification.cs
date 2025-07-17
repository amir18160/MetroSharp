using Domain.Enums;

namespace Domain.Entities
{
    public class EmailVerification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public string Code { get; set; }
        public EmailPurpose Purpose { get; set; }
        public DateTime Expiration { get; set; }
        public bool Used { get; set; } = false;
        public User User { get; set; }
    }
}
