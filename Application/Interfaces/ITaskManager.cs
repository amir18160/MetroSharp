using Application.Core;
using MediatR;

namespace Application.Interfaces
{
    public interface ITaskManager
    {
        Task<Result<Unit>> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
        Task<Result<Unit>> RetryTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    }
}