using Application.Core;
using MediatR;

namespace Application.Tasks.Commands.CancelTask
{
    public class Command: IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }
}