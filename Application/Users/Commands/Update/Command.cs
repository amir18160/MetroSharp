using Application.Core;
using MediatR;

namespace Application.Users.Commands.Update
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
