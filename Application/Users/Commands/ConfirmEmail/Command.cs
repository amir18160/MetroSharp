using Application.Core;
using MediatR;

namespace Application.Users.Commands.ConfirmEmail
{
    public class Command: IRequest<Result<Unit>>
    {
        public Guid Code { get; set; }
    }
}