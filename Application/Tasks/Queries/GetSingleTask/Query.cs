using Application.Core;
using Application.Tasks.Queries.GetTasks;
using MediatR;

namespace Application.Tasks.Queries.GetSingleTask
{
    public class Query : IRequest<Result<TaskDto>>
    {
        public Guid Id { get; set; }
    }
}
