using Application.Core;
using MediatR;

namespace Application.Users.Update
{
    public class Command : IRequest<Result<Unit>>
    {
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
    }
}
