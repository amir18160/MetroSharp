using Application.Core;
using MediatR;

namespace Application.Users.Commands.ChangeUserRole;

public class Command : IRequest<Result<Unit>>
{
    public string UserId { get; set; }
    public string NewRole { get; set; }
}