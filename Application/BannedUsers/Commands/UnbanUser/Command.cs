using Application.Core;
using MediatR;

namespace Application.BannedUsers.Commands.UnbanUser
{
    public class Command : IRequest<Result<Unit>>
    {
        public string UserId { get; set; }
    }
}