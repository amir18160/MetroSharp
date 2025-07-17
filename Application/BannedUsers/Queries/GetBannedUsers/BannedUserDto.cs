namespace Application.BannedUsers.Queries.GetBannedUsers
{
    public class BannedUserDto
    {
        public string UserId { get; set; }
        public DateTime BannedAt { get; set; }
        public string FreeAt { get; set; }
        public string Reason { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Email { get; set; }
    }
}