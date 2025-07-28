using Application.Core;
using Application.Interfaces;
using MediatR;

namespace Application.Tasks.Commands.CancelTask
{
    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly ITaskManager _taskManager;

        public Handler(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _taskManager.CancelTaskAsync(request.Id);
        }
    }
}