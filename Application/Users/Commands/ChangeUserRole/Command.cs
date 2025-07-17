using MediatR;
using Domain.Enums;
using Application.Core;

namespace Application.Users.Commands.ChangeUserRole
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Username { get; set; }
        public string NewRole { get; set; }
    }
}
