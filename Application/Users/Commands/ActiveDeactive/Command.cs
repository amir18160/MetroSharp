using Application.Core;
using MediatR;

namespace Application.Users.Commands.ActiveDeactive;

public class Command : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
    public bool Activate { get; set; }
}