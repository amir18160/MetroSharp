using MediatR;
using Application.Core;

namespace Application.Users.Commands.ActiveDeactive
{

    public class Command : IRequest<Result<Unit>>
    {
   
        public string Username { get; set; }
        public bool Activate { get; set; }
    }
}
