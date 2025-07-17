using Application.Core;
using MediatR;

namespace Application.BannedUsers.Commands.BanUser
{
    public class Command : IRequest<Result<Unit>>
    {
        public string UserId { get; set; }
        public string Reason { get; set; }
        public DateTime? FreeAt { get; set; }
    }
}