using Application.Core;
using MediatR;

namespace Application.Users.Commands.ChangeUserRole;

public class Command : IRequest<Result<Unit>>
{
    public string Username { get; set; }
    public string NewRole { get; set; }
}