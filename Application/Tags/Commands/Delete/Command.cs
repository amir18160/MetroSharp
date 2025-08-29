using Application.Core;
using MediatR;

namespace Application.Tags.Commands.Delete
{
    public class Command : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }
}