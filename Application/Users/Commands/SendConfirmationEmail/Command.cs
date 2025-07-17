using Application.Core;
using MediatR;

namespace Application.Users.Commands.SendConfirmationEmail
{
    public class Command:IRequest<Result<Unit>>
    {
        
    }
}