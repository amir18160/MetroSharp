using Application.Core;
using MediatR;

namespace Application.Users.Commands.ResetPassword
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}