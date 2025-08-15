using Application.Core;
using Application.Interfaces;
using Domain.Enums;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
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

            // Mark as cancelling
            task.State = TorrentTaskState.Cancelling;
            await _context.SaveChangesAsync();

            // Enqueue TaskCleaner into the dedicated "cleaners" queue with CancellationToken.None
            try
            {
                var job = Job.FromExpression<TaskCleaner>(cleaner => cleaner.CleanUpAsync(task.Id, CancellationToken.None));
                // Create the job in the "cleaners" queue so the cleaner-server will pick it up immediately.
                _backgroundJobClient.Create(job, new EnqueuedState("cleaners"));
            }
            catch (Exception)
            {
                // best-effort: if enqueue fails, don't break cancellation flow — cleanup will still be attempted from running job finally.
            }

            // reflect user requested cancellation
            task.State = TorrentTaskState.Cancelled;
            await _context.SaveChangesAsync();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
