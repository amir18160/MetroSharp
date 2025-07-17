using Application.Core;
using MediatR;

namespace Application.Users.Commands.RequestResetPassword
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Email { get; set; }
    }
}