using Application.Core;
using MediatR;

namespace Application.Users.Commands.UpdateUsersByAdmin
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Bio { get; set; }
        public string RemoveImage { get; set; }
        public string NewPassword { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsActive { get; set; }
        public string TelegramId { get; set; }
    }

}
