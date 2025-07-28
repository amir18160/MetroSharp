using Application.Core;
using MediatR;

namespace Application.Tasks.Commands.RetryTask
{
    public class Command: IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }
}