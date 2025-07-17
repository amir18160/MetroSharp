namespace Application.Users.Queries
{
    public class UserBase
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public bool IsActive { get; set; }
        public bool IsConfirmed { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}