using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using Hangfire;
using MediatR;
using Persistence;

namespace Infrastructure.BackgroundServices.TorrentProcessTask
{
    public class TaskManager : ITaskManager
    {
        private readonly DownloadContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public TaskManager(DownloadContext context, IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<Result<Unit>> CancelTaskAsync(Guid taskId)
        {
            var task = await _context.TorrentTasks.FindAsync(taskId);

            if (task == null)
            {
                return Result<Unit>.Failure("Task not found.");
            }

            if (task.State == TorrentTaskState.Completed || task.State == TorrentTaskState.Cancelled || task.State == TorrentTaskState.Cancelling)
            {
                return Result<Unit>.Failure("Task is already completed, cancelled or in the process of cancelling.");
            }

            task.State = TorrentTaskState.Cancelling;
            await _context.SaveChangesAsync();

            _backgroundJobClient.Enqueue<TaskCleaner>(cleaner => cleaner.CleanUpAsync(task.Id));

            task.State = TorrentTaskState.Cancelled;
            await _context.SaveChangesAsync();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}